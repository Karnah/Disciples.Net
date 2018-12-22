using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia.Media;

using Engine.Battle.Enums;
using Engine.Battle.Models;
using Engine.Battle.Providers;
using Engine.Common.Controllers;
using Engine.Common.Enums;
using Engine.Common.Models;
using Engine.Common.Providers;
using Engine.Common.VisualObjects;

namespace Engine.Common.GameObjects
{
    /// <summary>
    /// Потрет юнита с его состоянием.
    /// </summary>
    public class UnitPortraitObject : GameObject
    {
        /// <summary>
        /// Слой для расположения интерфейса.
        /// </summary>
        // todo вынести это в одно место
        private const int INTERFACE_LAYER = 1000;

        /// <summary>
        /// Идентификатор в ресурсах с текстом "Промах".
        /// </summary>
        private const string MISS_TEXT_ID = "X008TA0001";
        /// <summary>
        /// Идентификатор в ресурсах с текстом "Защита".
        /// </summary>
        private const string DEFEND_TEXT_ID = "X008TA0011";
        /// <summary>
        /// Идентификатор в ресурсах с текстом "Ждать".
        /// </summary>
        private const string WAIT_TEXT_ID = "X008TA0020";

        private readonly ITextProvider _textProvider;
        private readonly IVisualSceneController _visualSceneController;
        private readonly IBattleInterfaceProvider _battleInterfaceProvider;
        private readonly bool _rightToLeft;

        /// <summary>
        /// Картинка с портретом юнита.
        /// </summary>
        private ImageVisualObject _unitPortrait;
        /// <summary>
        /// Изображение, отображающий полученный юнитом урон (красный прямоугольник поверх портрета).
        /// </summary>
        private ImageVisualObject _unitDamageImage;
        /// <summary>
        /// Иконка умершего юнита.
        /// </summary>
        private ImageVisualObject _deathIcon;
        /// <summary>
        /// Изображение, отображающий моментальный эффект.
        /// </summary>
        private ImageVisualObject _momentalEffectImage;
        /// <summary>
        /// Изображение с текстом моментального эффекта.
        /// </summary>
        private TextVisualObject _momentalEffectText;
        /// <summary>
        /// Текст, отображающий текущее количество здоровья и максимальное.
        /// </summary>
        private TextVisualObject _unitHitpoints;
        /// <summary>
        /// Картинка-разделитель на панели для больших существ.
        /// </summary>
        private ImageVisualObject _unitPanelSeparator;
        /// <summary>
        /// Иконки эффектов, которые воздействуют на юнита.
        /// </summary>
        private Dictionary<UnitBattleEffectType, ImageVisualObject> _battleEffectsIcons;
        /// <summary>
        /// Количество ОЗ, которое было при предыдущей проверке.
        /// </summary>
        private int _lastUnitHitPoints;


        public UnitPortraitObject(
            ITextProvider textProvider,
            IVisualSceneController visualSceneController,
            IBattleInterfaceProvider battleInterfaceProvider,
            Unit unit,
            bool rightToLeft,
            double x,
            double y) : base(x, y)
        {
            _textProvider = textProvider;
            _visualSceneController = visualSceneController;
            _battleInterfaceProvider = battleInterfaceProvider;
            _rightToLeft = rightToLeft;

            Unit = unit;

            Width = Unit.UnitType.Face.PixelSize.Width;
            Height = Unit.UnitType.Face.PixelSize.Height;

            _battleEffectsIcons = new Dictionary<UnitBattleEffectType, ImageVisualObject>();
        }


        /// <inheritdoc />
        public override bool IsInteractive => true;

        /// <summary>
        /// Юнит.
        /// </summary>
        public Unit Unit { get; }


        /// <inheritdoc />
        public override void OnInitialize()
        {
            base.OnInitialize();

            _unitPortrait = _visualSceneController.AddImageVisual(Unit.UnitType.Face, X, Y, INTERFACE_LAYER + 2);
            if (_rightToLeft) {
                _unitPortrait.Transform = new ScaleTransform(-1, 1);
            }

            _unitHitpoints = _visualSceneController.AddTextVisual(string.Empty, 11, X, Y + Height + 3, INTERFACE_LAYER + 3, Width, isBold: true);
            // Если юнит большой, то необходимо "закрасить" область между двумя клетками на панели.
            if (!Unit.UnitType.SizeSmall) {
                _unitPanelSeparator = _visualSceneController.AddImageVisual(
                    _battleInterfaceProvider.PanelSeparator,
                    X + (Width - _battleInterfaceProvider.PanelSeparator.PixelSize.Width) / 2 - 1,
                    Y + Height - 1,
                    INTERFACE_LAYER);
            }

            UpdateUnitEffects();
        }

        public override void OnUpdate(long ticksCount)
        {
            base.OnUpdate(ticksCount);

            UpdateUnitEffects();
        }

        /// <inheritdoc />
        public override void Destroy()
        {
            RemoveVisual(ref _unitPortrait);
            RemoveVisual(ref _unitHitpoints);
            RemoveVisual(ref _deathIcon);
            RemoveVisual(ref _momentalEffectImage);
            RemoveVisual(ref _momentalEffectText);
            RemoveVisual(ref _unitDamageImage);
            RemoveVisual(ref _unitPanelSeparator);

            foreach (var battleEffectsIcon in _battleEffectsIcons) {
                _visualSceneController.RemoveVisualObject(battleEffectsIcon.Value);
            }

            base.Destroy();
        }

        /// <summary>
        /// Обновить состояние юнита.
        /// </summary>
        private void UpdateUnitEffects()
        {
            ProcessBattleEffects();
            ProcessMomentalEffects();

            // Если сейчас обрабатывается моментальный эффект, то рамку размещать не нужно.
            if (Unit.Effects.CurrentMomentalEffect != null && !Unit.Effects.MomentalEffectEnded)
                return;

            if (Unit.IsDead) {
                if (_deathIcon == null) {
                    // Картинку выравниваем по ширине.
                    var deathScull = _battleInterfaceProvider.DeathSkull;
                    var widthOffset = (Width - deathScull.PixelSize.Width) / 2;
                    _deathIcon = _visualSceneController.AddImageVisual(_battleInterfaceProvider.DeathSkull, X + widthOffset, Y, INTERFACE_LAYER + 3);
                    _unitHitpoints.Text = $"0/{Unit.UnitType.HitPoints}";

                    RemoveVisual(ref _unitDamageImage);
                }
            }
            else if (_lastUnitHitPoints != Unit.HitPoints) {
                _lastUnitHitPoints = Unit.HitPoints;
                _unitHitpoints.Text = $"{_lastUnitHitPoints}/{Unit.UnitType.HitPoints}";

                RemoveVisual(ref _unitDamageImage);

                var height = (1 - ((double)_lastUnitHitPoints / Unit.UnitType.HitPoints)) * Height;
                if (height > 0) {
                    var width = Width;
                    var x = _unitPortrait.X;
                    var y = _unitPortrait.Y + (Height - height);

                    _unitDamageImage = _visualSceneController.AddColorImageVisual(GameColor.Red, width, height, x, y, INTERFACE_LAYER + 3);
                }
            }
        }

        /// <summary>
        /// Обработать эффекты битвы - добавить новые и удалить старые.
        /// </summary>
        private void ProcessBattleEffects()
        {
            var battleEffects = Unit.Effects.GetBattleEffects();

            // Добавляем иконки новых эффектов.
            foreach (var battleEffect in battleEffects) {
                if (_battleEffectsIcons.ContainsKey(battleEffect.EffectType))
                    continue;

                var icon = _battleInterfaceProvider.UnitBattleEffectsIcon[battleEffect.EffectType];
                _battleEffectsIcons.Add(battleEffect.EffectType, _visualSceneController.AddImageVisual(
                    icon,
                    X + (Width - icon.PixelSize.Width) / 2,
                    Y + Height - icon.PixelSize.Height,
                    INTERFACE_LAYER + 4));
            }

            // Удаляем иконки тех эффектов, действие которых закончилось.
            var expiredEffects = _battleEffectsIcons
                .Where(bei => battleEffects.All(be => be.EffectType != bei.Key))
                .ToList();
            foreach (var expiredEffect in expiredEffects) {
                var effectIcon = _battleEffectsIcons[expiredEffect.Key];
                _visualSceneController.RemoveVisualObject(effectIcon);
                _battleEffectsIcons.Remove(expiredEffect.Key);
            }
        }

        /// <summary>
        /// Обработать моментальные эффекты - обработать новые и завершенные.
        /// </summary>
        private void ProcessMomentalEffects()
        {
            // Обрабатываем новый мгновенный эффект.
            if (Unit.Effects.MomentalEffectBegin) {
                ProcessNewMomentalEffect(Unit.Effects.CurrentMomentalEffect);
                return;
            }

            // Обрабатываем действующий мгновенный эффект.
            if (Unit.Effects.MomentalEffectEnded) {
                RemoveVisual(ref _momentalEffectImage);
                RemoveVisual(ref _momentalEffectText);

                // Сбрасываем количество ХП, чтобы обновить рамку.
                _lastUnitHitPoints = int.MaxValue;
            }
        }

        /// <summary>
        /// Обработать новый мгновенный эффект.
        /// </summary>
        private void ProcessNewMomentalEffect(UnitMomentalEffect momentalEffect)
        {
            switch (momentalEffect.EffectType) {
                case UnitMomentalEffectType.Damaged:
                    _momentalEffectImage = AddColorImage(GameColor.Red);
                    _momentalEffectText = AddText($"-{momentalEffect.Power}");
                    break;
                case UnitMomentalEffectType.Healed:
                    var hitPointsDiff = Unit.HitPoints - _lastUnitHitPoints;
                    if (hitPointsDiff > 0) {
                        _momentalEffectImage = AddColorImage(GameColor.Blue);
                        _momentalEffectText = AddText($"+{momentalEffect.Power}");
                    }
                    break;
                case UnitMomentalEffectType.Miss:
                    _momentalEffectImage = AddColorImage(GameColor.Yellow);
                    _momentalEffectText = AddText(_textProvider.GetText(MISS_TEXT_ID));
                    break;
                case UnitMomentalEffectType.Defended:
                    _momentalEffectText = AddText(_textProvider.GetText(DEFEND_TEXT_ID));
                    break;
                case UnitMomentalEffectType.Waiting:
                    _momentalEffectText = AddText(_textProvider.GetText(WAIT_TEXT_ID));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _unitHitpoints.Text = $"{Unit.HitPoints}/{Unit.UnitType.HitPoints}";
        }

        /// <summary>
        /// Добавить на портрет изображение указанного цвета.
        /// </summary>
        private ImageVisualObject AddColorImage(GameColor color)
        {
            // Если мы добавляем изображение поверх портрета, то должны на время очистить изображение с % здоровья.
            RemoveVisual(ref _unitDamageImage);

            return _visualSceneController.AddColorImageVisual(color, Width, Height, X, Y, INTERFACE_LAYER + 2);
        }

        /// <summary>
        /// Добавить на портрет указанный текст.
        /// </summary>
        private TextVisualObject AddText(string text)
        {
            return _visualSceneController.AddTextVisual(text, 12, X - 3, Y + Height / 2 - 6, INTERFACE_LAYER + 3, Width, isBold: true, foregroundColor: Colors.White);
        }

        /// <summary>
        /// Удалить визуальный объект со сцены и очистить ссылку.
        /// </summary>
        /// <typeparam name="T">Тип объекта.</typeparam>
        /// <param name="visualObject">Объект, который необходимо удалить.</param>
        private void RemoveVisual<T>(ref T visualObject) where T : VisualObject
        {
            _visualSceneController.RemoveVisualObject(visualObject);
            visualObject = null;
        }
    }
}
