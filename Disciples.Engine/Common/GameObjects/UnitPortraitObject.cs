using System;
using System.Collections.Generic;
using System.Linq;

using Disciples.Engine.Base;
using Disciples.Engine.Battle.Enums;
using Disciples.Engine.Battle.Models;
using Disciples.Engine.Battle.Providers;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Common.SceneObjects;

namespace Disciples.Engine.Common.GameObjects
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
        private readonly ISceneController _sceneController;
        private readonly IBattleInterfaceProvider _battleInterfaceProvider;
        private readonly bool _rightToLeft;

        /// <summary>
        /// Картинка с портретом юнита.
        /// </summary>
        private IImageSceneObject _unitPortrait;
        /// <summary>
        /// Изображение, отображающий полученный юнитом урон (красный прямоугольник поверх портрета).
        /// </summary>
        private IImageSceneObject _unitDamageImage;
        /// <summary>
        /// Иконка умершего юнита.
        /// </summary>
        private IImageSceneObject _deathIcon;
        /// <summary>
        /// Изображение, отображающий моментальный эффект.
        /// </summary>
        private IImageSceneObject _instantaneousEffectImage;
        /// <summary>
        /// Изображение с текстом моментального эффекта.
        /// </summary>
        private ITextSceneObject _instantaneousEffectText;
        /// <summary>
        /// Текст, отображающий текущее количество здоровья и максимальное.
        /// </summary>
        private ITextSceneObject _unitHitpoints;
        /// <summary>
        /// Картинка-разделитель на панели для больших существ.
        /// </summary>
        private IImageSceneObject _unitPanelSeparator;
        /// <summary>
        /// Иконки эффектов, которые воздействуют на юнита.
        /// </summary>
        private Dictionary<UnitBattleEffectType, IImageSceneObject> _battleEffectsIcons;
        /// <summary>
        /// Количество ОЗ, которое было при предыдущей проверке.
        /// </summary>
        private int _lastUnitHitPoints;


        public UnitPortraitObject(
            ITextProvider textProvider,
            ISceneController sceneController,
            IBattleInterfaceProvider battleInterfaceProvider,
            Unit unit,
            bool rightToLeft,
            double x,
            double y) : base(x, y)
        {
            _textProvider = textProvider;
            _sceneController = sceneController;
            _battleInterfaceProvider = battleInterfaceProvider;
            _rightToLeft = rightToLeft;

            Unit = unit;

            Width = Unit.UnitType.Face.Width;
            Height = Unit.UnitType.Face.Height;

            _battleEffectsIcons = new Dictionary<UnitBattleEffectType, IImageSceneObject>();
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

            _unitPortrait = _sceneController.AddImage(Unit.UnitType.Face, X, Y, INTERFACE_LAYER + 2);
            _unitPortrait.IsReflected = _rightToLeft;

            _unitHitpoints = _sceneController.AddText(string.Empty, 11, X, Y + Height + 3, INTERFACE_LAYER + 3, Width, isBold: true);
            // Если юнит большой, то необходимо "закрасить" область между двумя клетками на панели.
            if (!Unit.UnitType.SizeSmall) {
                _unitPanelSeparator = _sceneController.AddImage(
                    _battleInterfaceProvider.PanelSeparator,
                    X + (Width - _battleInterfaceProvider.PanelSeparator.Width) / 2 - 1,
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
            RemoveSceneObject(ref _unitPortrait);
            RemoveSceneObject(ref _unitHitpoints);
            RemoveSceneObject(ref _deathIcon);
            RemoveSceneObject(ref _instantaneousEffectImage);
            RemoveSceneObject(ref _instantaneousEffectText);
            RemoveSceneObject(ref _unitDamageImage);
            RemoveSceneObject(ref _unitPanelSeparator);

            foreach (var battleEffectsIcon in _battleEffectsIcons) {
                _sceneController.RemoveSceneObject(battleEffectsIcon.Value);
            }

            base.Destroy();
        }

        /// <summary>
        /// Обновить состояние юнита.
        /// </summary>
        private void UpdateUnitEffects()
        {
            ProcessBattleEffects();
            ProcessInstantaneousEffects();

            // Если сейчас обрабатывается моментальный эффект, то рамку размещать не нужно.
            if (Unit.Effects.CurrentInstantaneousEffect != null && !Unit.Effects.InstantaneousEffectEnded)
                return;

            if (Unit.IsDead) {
                if (_deathIcon == null) {
                    // Картинку выравниваем по ширине.
                    var deathScull = _battleInterfaceProvider.DeathSkull;
                    var widthOffset = (Width - deathScull.Width) / 2;
                    _deathIcon = _sceneController.AddImage(_battleInterfaceProvider.DeathSkull, X + widthOffset, Y, INTERFACE_LAYER + 3);
                    _unitHitpoints.Text = $"0/{Unit.UnitType.HitPoints}";

                    RemoveSceneObject(ref _unitDamageImage);
                }
            }
            else if (_lastUnitHitPoints != Unit.HitPoints) {
                _lastUnitHitPoints = Unit.HitPoints;
                _unitHitpoints.Text = $"{_lastUnitHitPoints}/{Unit.UnitType.HitPoints}";

                RemoveSceneObject(ref _unitDamageImage);

                var height = (1 - ((double)_lastUnitHitPoints / Unit.UnitType.HitPoints)) * Height;
                if (height > 0) {
                    var width = Width;
                    var x = _unitPortrait.X;
                    var y = _unitPortrait.Y + (Height - height);

                    _unitDamageImage = _sceneController.AddColorImage(GameColor.Red, width, height, x, y, INTERFACE_LAYER + 3);
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
                _battleEffectsIcons.Add(battleEffect.EffectType, _sceneController.AddImage(
                    icon,
                    X + (Width - icon.Width) / 2,
                    Y + Height - icon.Height,
                    INTERFACE_LAYER + 4));
            }

            // Удаляем иконки тех эффектов, действие которых закончилось.
            var expiredEffects = _battleEffectsIcons
                .Where(bei => battleEffects.All(be => be.EffectType != bei.Key))
                .ToList();
            foreach (var expiredEffect in expiredEffects) {
                var effectIcon = _battleEffectsIcons[expiredEffect.Key];
                _sceneController.RemoveSceneObject(effectIcon);
                _battleEffectsIcons.Remove(expiredEffect.Key);
            }
        }

        /// <summary>
        /// Обработать моментальные эффекты - обработать новые и завершенные.
        /// </summary>
        private void ProcessInstantaneousEffects()
        {
            // Обрабатываем новый мгновенный эффект.
            if (Unit.Effects.InstantaneousEffectBegin) {
                ProcessNewInstantaneousEffect(Unit.Effects.CurrentInstantaneousEffect);
                return;
            }

            // Обрабатываем действующий мгновенный эффект.
            if (Unit.Effects.InstantaneousEffectEnded) {
                RemoveSceneObject(ref _instantaneousEffectImage);
                RemoveSceneObject(ref _instantaneousEffectText);

                // Сбрасываем количество ХП, чтобы обновить рамку.
                _lastUnitHitPoints = int.MaxValue;
            }
        }

        /// <summary>
        /// Обработать новый мгновенный эффект.
        /// </summary>
        private void ProcessNewInstantaneousEffect(UnitInstantaneousEffect instantaneousEffect)
        {
            switch (instantaneousEffect.EffectType) {
                case UnitInstantaneousEffectType.Damaged:
                    _instantaneousEffectImage = AddColorImage(GameColor.Red);
                    _instantaneousEffectText = AddText($"-{instantaneousEffect.Power}");
                    break;
                case UnitInstantaneousEffectType.Healed:
                    var hitPointsDiff = Unit.HitPoints - _lastUnitHitPoints;
                    if (hitPointsDiff > 0) {
                        _instantaneousEffectImage = AddColorImage(GameColor.Blue);
                        _instantaneousEffectText = AddText($"+{instantaneousEffect.Power}");
                    }
                    break;
                case UnitInstantaneousEffectType.Miss:
                    _instantaneousEffectImage = AddColorImage(GameColor.Yellow);
                    _instantaneousEffectText = AddText(_textProvider.GetText(MISS_TEXT_ID));
                    break;
                case UnitInstantaneousEffectType.Defended:
                    _instantaneousEffectText = AddText(_textProvider.GetText(DEFEND_TEXT_ID));
                    break;
                case UnitInstantaneousEffectType.Waiting:
                    _instantaneousEffectText = AddText(_textProvider.GetText(WAIT_TEXT_ID));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _unitHitpoints.Text = $"{Unit.HitPoints}/{Unit.UnitType.HitPoints}";
        }

        /// <summary>
        /// Добавить на портрет изображение указанного цвета.
        /// </summary>
        private IImageSceneObject AddColorImage(GameColor color)
        {
            // Если мы добавляем изображение поверх портрета, то должны на время очистить изображение с % здоровья.
            RemoveSceneObject(ref _unitDamageImage);

            return _sceneController.AddColorImage(color, Width, Height, X, Y, INTERFACE_LAYER + 2);
        }

        /// <summary>
        /// Добавить на портрет указанный текст.
        /// </summary>
        private ITextSceneObject AddText(string text)
        {
            return _sceneController.AddText(text, 12, X - 3, Y + Height / 2 - 6, INTERFACE_LAYER + 3, Width, isBold: true, foregroundColor: GameColor.White);
        }

        /// <summary>
        /// Удалить визуальный объект со сцены и очистить ссылку.
        /// </summary>
        /// <typeparam name="T">Тип объекта.</typeparam>
        /// <param name="sceneObject">Объект, который необходимо удалить.</param>
        private void RemoveSceneObject<T>(ref T sceneObject) where T : ISceneObject
        {
            _sceneController.RemoveSceneObject(sceneObject);
            sceneObject = default(T);
        }
    }
}