﻿using Disciples.Engine;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Common.SceneObjects;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle.GameObjects;

/// <summary>
/// Потрет юнита с его состоянием.
/// </summary>
internal class UnitPortraitObject : GameObject
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
    /// <summary>
    /// Идентификатор в ресурсах с текстом "Защита".
    /// </summary>
    private const string WARD_TEXT_ID = "X008TA0011";
    /// <summary>
    /// Идентификатор в ресурсах с текстом "Иммунитет".
    /// </summary>
    private const string IMMUNITY_TEXT_ID = "X008TA0002";

    private readonly ITextProvider _textProvider;
    private readonly ISceneObjectContainer _sceneObjectContainer;
    private readonly IBattleInterfaceProvider _battleInterfaceProvider;
    private readonly bool _rightToLeft;

    private readonly IBitmap _unitFaceBitmap;

    /// <summary>
    /// Картинка с портретом юнита.
    /// </summary>
    private IImageSceneObject _unitPortrait = null!;
    /// <summary>
    /// Изображение, отображающий полученный юнитом урон (красный прямоугольник поверх портрета).
    /// </summary>
    private IImageSceneObject? _unitDamageForeground;
    /// <summary>
    /// Иконка умершего юнита.
    /// </summary>
    private IImageSceneObject? _deathIcon;
    /// <summary>
    /// Изображение, отображающий моментальный эффект.
    /// </summary>
    private IImageSceneObject? _instantaneousEffectImage;
    /// <summary>
    /// Изображение с текстом моментального эффекта.
    /// </summary>
    private ITextSceneObject? _instantaneousEffectText;
    /// <summary>
    /// Текст, отображающий текущее количество здоровья и максимальное.
    /// </summary>
    private ITextSceneObject _unitHitpoints = null!;
    /// <summary>
    /// Картинка-разделитель на панели для больших существ.
    /// </summary>
    private IImageSceneObject? _unitPanelSeparator;
    /// <summary>
    /// Иконка для юнитов, уровень которых больше базового на 5/10/15.
    /// </summary>
    private IImageSceneObject? _highLevelUnitIcon;
    /// <summary>
    /// Иконки эффектов, которые воздействуют на юнита.
    /// </summary>
    private readonly Dictionary<UnitBattleEffectType, IImageSceneObject> _battleEffectsIcons;
    /// <summary>
    /// Передний фон эффектов, которые воздействуют на юнита.
    /// </summary>
    private readonly Dictionary<UnitBattleEffectType, IImageSceneObject> _battleEffectsForegrounds;
    /// <summary>
    /// Количество ОЗ, которое было при предыдущей проверке.
    /// </summary>
    private int _lastUnitHitPoints;
    /// <summary>
    /// Количество уровней, которые было при предыдущей проверке.
    /// </summary>
    private int _lastLevelDiff;

    /// <summary>
    /// Создать объект класса <see cref="UnitPortraitObject" />.
    /// </summary>
    public UnitPortraitObject(
        ITextProvider textProvider,
        ISceneObjectContainer sceneObjectContainer,
        IBattleInterfaceProvider battleInterfaceProvider,
        IBattleUnitResourceProvider battleUnitResourceProvider,
        Unit unit,
        bool rightToLeft,
        double x,
        double y) : base(x, y)
    {
        _textProvider = textProvider;
        _sceneObjectContainer = sceneObjectContainer;
        _battleInterfaceProvider = battleInterfaceProvider;
        _rightToLeft = rightToLeft;

        _unitFaceBitmap = battleUnitResourceProvider.GetUnitFace(unit.UnitType);

        Unit = unit;

        Width = _unitFaceBitmap.Width;
        Height = _unitFaceBitmap.Height;

        _battleEffectsIcons = new Dictionary<UnitBattleEffectType, IImageSceneObject>();
        _battleEffectsForegrounds = new Dictionary<UnitBattleEffectType, IImageSceneObject>();
    }

    /// <inheritdoc />
    public override bool IsInteractive => true;

    /// <summary>
    /// Юнит.
    /// </summary>
    public Unit Unit { get; }

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        _unitPortrait = _sceneObjectContainer.AddImage(_unitFaceBitmap, X, Y, INTERFACE_LAYER + 2);
        _unitPortrait.IsReflected = _rightToLeft;

        _unitHitpoints = _sceneObjectContainer.AddText(string.Empty, 11, X, Y + Height + 3, INTERFACE_LAYER + 3, Width, isBold: true);
        // Если юнит большой, то необходимо "закрасить" область между двумя клетками на панели.
        if (!Unit.UnitType.IsSmall) {
            _unitPanelSeparator = _sceneObjectContainer.AddImage(
                _battleInterfaceProvider.PanelSeparator,
                X + (Width - _battleInterfaceProvider.PanelSeparator.Width) / 2 - 1,
                Y + Height - 1,
                INTERFACE_LAYER);
        }

        UpdateUnitEffects();
    }

    public override void Update(long ticksCount)
    {
        base.Update(ticksCount);

        UpdateUnitEffects();
    }

    /// <inheritdoc />
    public override void Destroy()
    {
        _sceneObjectContainer.RemoveSceneObject(_unitPortrait);
        _sceneObjectContainer.RemoveSceneObject(_unitHitpoints);

        RemoveSceneObject(ref _deathIcon);
        RemoveSceneObject(ref _instantaneousEffectImage);
        RemoveSceneObject(ref _instantaneousEffectText);
        RemoveSceneObject(ref _unitDamageForeground);
        RemoveSceneObject(ref _unitPanelSeparator);
        RemoveSceneObject(ref _highLevelUnitIcon);

        foreach (var battleEffectsIcon in _battleEffectsIcons)
            _sceneObjectContainer.RemoveSceneObject(battleEffectsIcon.Value);

        foreach (var battleEffectsForeground in _battleEffectsForegrounds)
            _sceneObjectContainer.RemoveSceneObject(battleEffectsForeground.Value);

        base.Destroy();
    }

    /// <summary>
    /// Обработать событие, произошедшее с юнитом.
    /// </summary>
    public void ProcessBeginUnitPortraitEvent(BattleUnitPortraitEventData eventData)
    {
        if (_instantaneousEffectText != null || _instantaneousEffectImage != null)
        {
            // TODO Fatal в лог.
            RemoveSceneObject(ref _instantaneousEffectImage);
            RemoveSceneObject(ref _instantaneousEffectText);
        }

        switch (eventData.UnitActionType)
        {
            case UnitActionType.Damaged:
                _instantaneousEffectImage = AddColorImage(GameColor.Red);
                _instantaneousEffectText = AddText($"-{eventData.Power!.Value}");
                _unitHitpoints.Text = $"{Unit.HitPoints}/{Unit.MaxHitPoints}";
                break;

            case UnitActionType.Healed:
                _instantaneousEffectImage = AddColorImage(GameColor.Blue);
                _instantaneousEffectText = AddText($"+{eventData.Power!.Value}");
                _unitHitpoints.Text = $"{Unit.HitPoints}/{Unit.MaxHitPoints}";
                break;

            case UnitActionType.Dodge:
                _instantaneousEffectImage = AddColorImage(GameColor.Yellow);
                _instantaneousEffectText = AddText(_textProvider.GetText(MISS_TEXT_ID));
                break;

            case UnitActionType.Defend:
                _instantaneousEffectText = AddText(_textProvider.GetText(DEFEND_TEXT_ID));
                break;

            case UnitActionType.Waiting:
                _instantaneousEffectText = AddText(_textProvider.GetText(WAIT_TEXT_ID));
                break;

            case UnitActionType.Dying:
                break;

            case UnitActionType.UnderEffect:
            {
                var effectColor = GetEffectTypeColor((UnitBattleEffectType)eventData.AttackType!.Value);
                if (effectColor != null)
                    _instantaneousEffectImage = AddColorImage(effectColor.Value, false);

                var underEffectText = _textProvider.GetText(GetEffectText(eventData.AttackType!.Value));
                _instantaneousEffectText = AddText(underEffectText);

                break;
            }

            case UnitActionType.TriggeredEffect:
            {
                var triggeredEffectText = _textProvider.GetText(GetEffectText(eventData.AttackType!.Value));
                _instantaneousEffectText = AddText(eventData.Power == null
                    ? triggeredEffectText
                    : $"{triggeredEffectText} (-{eventData.Power})");

                break;
            }

            case UnitActionType.Ward:
                _instantaneousEffectText = AddText(_textProvider.GetText(WARD_TEXT_ID));
                break;

            case UnitActionType.Immunity:
                _instantaneousEffectText = AddText(_textProvider.GetText(IMMUNITY_TEXT_ID));
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Обработать завершение действия юнита.
    /// </summary>
    public void ProcessCompletedUnitPortraitEvent()
    {
        RemoveSceneObject(ref _instantaneousEffectImage);
        RemoveSceneObject(ref _instantaneousEffectText);

        // Сбрасываем количество ХП, чтобы обновить рамку.
        _lastUnitHitPoints = int.MaxValue;
        UpdateUnitEffects();
    }

    /// <summary>
    /// Обновить состояние юнита.
    /// </summary>
    private void UpdateUnitEffects()
    {
        var levelDiff = Unit.Level - Unit.UnitType.Level;
        if (levelDiff != _lastLevelDiff)
        {
            RemoveSceneObject(ref _highLevelUnitIcon);
            _highLevelUnitIcon = GetHighLevelUnitIcon(levelDiff);
            _lastLevelDiff = levelDiff;
        }

        // Если сейчас обрабатывается моментальный эффект, то рамку размещать не нужно.
        if (_instantaneousEffectImage != null || _instantaneousEffectText != null)
            return;

        ProcessBattleEffects();

        if (Unit.IsDead)
        {
            if (_deathIcon == null)
            {
                var deathScull = Unit.UnitType.IsSmall
                    ? _battleInterfaceProvider.DeathSkullSmall
                    : _battleInterfaceProvider.DeathSkullBig;
                _deathIcon = _sceneObjectContainer.AddImage(deathScull, X, Y, INTERFACE_LAYER + 3);
                _unitHitpoints.Text = $"0/{Unit.MaxHitPoints}";

                RemoveSceneObject(ref _unitDamageForeground);
                RemoveBattleEffectsForegrounds();
            }
        }
        else if (_lastUnitHitPoints != Unit.HitPoints)
        {
            _lastUnitHitPoints = Unit.HitPoints;
            _unitHitpoints.Text = $"{_lastUnitHitPoints}/{Unit.MaxHitPoints}";

            RemoveSceneObject(ref _unitDamageForeground);

            var height = (1 - ((double)_lastUnitHitPoints / Unit.MaxHitPoints)) * Height;
            if (height > 0)
            {
                var width = Width;
                var x = _unitPortrait.X;
                var y = _unitPortrait.Y + (Height - height);

                _unitDamageForeground = _sceneObjectContainer.AddColorImage(GameColor.Red, width, height, x, y, INTERFACE_LAYER + 3);
            }
        }
    }

    /// <summary>
    /// Обработать эффекты битвы - добавить новые и удалить старые.
    /// </summary>
    private void ProcessBattleEffects()
    {
        var battleEffects = Unit.Effects.GetBattleEffects();

        // Удаляем иконки тех эффектов, действие которых закончилось.
        var expiredEffects = _battleEffectsIcons
            .Where(bei => battleEffects.All(be => be.EffectType != bei.Key))
            .ToList();
        foreach (var expiredEffect in expiredEffects)
        {
            if (_battleEffectsIcons.TryGetValue(expiredEffect.Key, out var effectIcon))
            {
                _sceneObjectContainer.RemoveSceneObject(effectIcon);
                _battleEffectsIcons.Remove(expiredEffect.Key);
            }

            if (_battleEffectsForegrounds.TryGetValue(expiredEffect.Key, out var effectForeground))
            {
                _sceneObjectContainer.RemoveSceneObject(effectForeground);
                _battleEffectsForegrounds.Remove(expiredEffect.Key);
            }
        }

        // Добавляем иконки новых эффектов.
        foreach (var battleEffect in battleEffects)
        {
            if (!_battleEffectsIcons.ContainsKey(battleEffect.EffectType))
            {
                if (_battleInterfaceProvider.UnitBattleEffectsIcon.TryGetValue(battleEffect.EffectType, out var icon))
                {
                    // Иконку "Защиты" располагаем по центру.
                    if (battleEffect.EffectType == UnitBattleEffectType.Defend)
                    {
                        _battleEffectsIcons.Add(battleEffect.EffectType, _sceneObjectContainer.AddImage(
                            icon,
                            X + (Width - icon.Width) / 2,
                            Y + Height - icon.Height,
                            INTERFACE_LAYER + 4));
                    }
                    else
                    {
                        // Иконки остальных эффектов располагаются справа.
                        var iconsCount = _battleEffectsIcons.Keys
                            .Where(be => be != UnitBattleEffectType.Defend)
                            .GroupBy(be => be)
                            .Count();
                        _battleEffectsIcons.Add(battleEffect.EffectType, _sceneObjectContainer.AddImage(
                            icon,
                            X + Width - icon.Width,
                            Y + Height - icon.Height * (iconsCount + 1),
                            INTERFACE_LAYER + 4));
                    }
                }
            }

            // Добавляем фон, связанный с воздействием эффекта яда и заморозки.
            if (!_battleEffectsForegrounds.ContainsKey(battleEffect.EffectType))
            {
                var effectTypeColor = GetEffectTypeColor(battleEffect.EffectType);
                if (effectTypeColor != null)
                {
                    _battleEffectsForegrounds.Add(
                        battleEffect.EffectType,
                        AddColorImage(effectTypeColor.Value, false));
                }
            }
        }
    }

    /// <summary>
    /// Получить цвет эффекта.
    /// </summary>
    private static GameColor? GetEffectTypeColor(UnitBattleEffectType unitBattleEffectType)
    {
        return unitBattleEffectType switch
        {
            UnitBattleEffectType.Poison => GameColor.Green,
            UnitBattleEffectType.Frostbite => GameColor.Blue,
            UnitBattleEffectType.Blister => GameColor.Orange,
            _ => null
        };
    }

    /// <summary>
    /// Получить наименования эффекта, что воздействует на юнита.
    /// </summary>
    private static string GetEffectText(UnitAttackType attackType)
    {
        // TODO Взял из описания типов атак. Возможно, что неправильно.
        return attackType switch
        {
            UnitAttackType.Damage => "X005TA0791",
            UnitAttackType.Drain => "X005TA0792",
            UnitAttackType.Paralyze => "X005TA0789",
            UnitAttackType.Heal => "X005TA0802",
            UnitAttackType.Fear => "X005TA0794",
            UnitAttackType.BoostDamage => "X005TA0795",
            UnitAttackType.Petrify => "X005TA0790",
            UnitAttackType.LowerDamage => "X005TA0796",
            UnitAttackType.LowerInitiative => "X005TA0797",
            UnitAttackType.Poison => "X005TA0798",
            UnitAttackType.Frostbite => "X005TA0799",
            UnitAttackType.Revive => "X005TA0800",
            UnitAttackType.DrainOverflow => "X005TA0801", // TODO перепроверить.
            UnitAttackType.Cure => "X005TA0793",
            UnitAttackType.Summon => "X005TA0803",
            UnitAttackType.DrainLevel => "X005TA0804",
            UnitAttackType.GiveAttack => "X005TA0805",
            UnitAttackType.Doppelganger => "X005TA0806",
            UnitAttackType.TransformSelf => "X005TA0807",
            UnitAttackType.TransformOther => "X005TA0808",
            UnitAttackType.Blister => "X160TA0012",
            UnitAttackType.BestowWards => "X160TA0014",
            UnitAttackType.Shatter => "X160TA0020",
            _ => throw new ArgumentOutOfRangeException(nameof(attackType), attackType, null)
        };
    }

    /// <summary>
    /// Добавить на портрет изображение указанного цвета.
    /// </summary>
    private IImageSceneObject AddColorImage(GameColor color, bool shouldRemoveDamageImage = true)
    {
        // Если мы добавляем изображение поверх портрета, то в некоторых случаях должны на время очистить изображение с % здоровья.
        if (shouldRemoveDamageImage)
        {
            RemoveSceneObject(ref _unitDamageForeground);
            RemoveBattleEffectsForegrounds();
        }

        return _sceneObjectContainer.AddColorImage(color, Width, Height, X, Y, INTERFACE_LAYER + 2);
    }

    /// <summary>
    /// Добавить на портрет указанный текст.
    /// </summary>
    private ITextSceneObject AddText(string text)
    {
        return _sceneObjectContainer.AddText(text, 12, X - 3, Y + Height / 2 - 6, INTERFACE_LAYER + 3, Width, isBold: true, foregroundColor: GameColor.White);
    }

    /// <summary>
    /// Получить иконку соответствующую уровню юнита.
    /// </summary>
    private IImageSceneObject? GetHighLevelUnitIcon(int levelDiff)
    {
        if (levelDiff < 5)
            return null;

        var icon = levelDiff switch
        {
            >= 15 => _battleInterfaceProvider.RedLevelIcon,
            >= 10 => _battleInterfaceProvider.OrangeLevelIcon,
            _ => _battleInterfaceProvider.BlueLevelIcon
        };

        return _sceneObjectContainer.AddImage(
            icon,
            X + (Width - icon.Width) / 2,
            Y + icon.Height / 2,
            INTERFACE_LAYER + 4);
    }

    /// <summary>
    /// Очистить фоны эффектов битвы.
    /// </summary>
    private void RemoveBattleEffectsForegrounds()
    {
        foreach (var battleEffectsForeground in _battleEffectsForegrounds)
            _sceneObjectContainer.RemoveSceneObject(battleEffectsForeground.Value);

        _battleEffectsForegrounds.Clear();
    }

    /// <summary>
    /// Удалить визуальный объект со сцены и очистить ссылку.
    /// </summary>
    /// <typeparam name="T">Тип объекта.</typeparam>
    /// <param name="sceneObject">Объект, который необходимо удалить.</param>
    private void RemoveSceneObject<T>(ref T? sceneObject) where T : ISceneObject
    {
        _sceneObjectContainer.RemoveSceneObject(sceneObject);
        sceneObject = default(T);
    }
}