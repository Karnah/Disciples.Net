﻿using System.Drawing;
using Disciples.Common.Models;
using Disciples.Engine;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Components;
using Disciples.Engine.Common.Constants;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Common.SceneObjects;
using Disciples.Engine.Enums;
using Disciples.Engine.Models;
using Disciples.Scene.Battle.Constants;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle.GameObjects;

/// <summary>
/// Потрет юнита с его состоянием.
/// </summary>
internal class UnitPortraitObject : GameObject
{
    private const int PANEL_SEPARATOR_LAYER_SHIFT = 11;
    private const int PORTRAIT_AND_HP_LAYER_SHIFT = 12;
    private const int FOREGROUND_LAYER_SHIFT = 13;
    private const int TEXT_LAYER_SHIFT = 13;
    private const int EFFECTS_LAYER_SHIFT = 14;

    /// <summary>
    /// Идентификатор в ресурсах для ХП юнита.
    /// </summary>
    private const string UNIT_HIT_POINTS_TEXT_ID = "X005TA0757";
    /// <summary>
    /// Идентификатор в ресурсах для урона и лечения.
    /// </summary>
    private const string DAMAGE_TEXT_ID = "X008TA0004";
    /// <summary>
    /// Идентификатор в ресурсах для начисления опыта.
    /// </summary>
    private const string EXPERIENCE_TEXT_ID = "X008TA0005";
    /// <summary>
    /// Идентификатор в ресурсах для урона с критическим уроном.
    /// </summary>
    private const string CRITICAL_DAMAGE_TEXT_ID = "X160TA0021";
    /// <summary>
    /// Идентификатор в ресурсах с текстом "Промах".
    /// </summary>
    private const string MISS_TEXT_ID = "X008TA0001";
    /// <summary>
    /// Идентификатор в ресурсах с текстом "Защита".
    /// </summary>
    private const string DEFEND_TEXT_ID = "X008TA0021";
    /// <summary>
    /// Идентификатор в ресурсах с текстом "Ждать".
    /// </summary>
    private const string WAIT_TEXT_ID = "X008TA0020";
    /// <summary>
    /// Идентификатор в ресурсах с текстом "Защита" (от какого-то типа атаки).
    /// </summary>
    private const string WARD_TEXT_ID = "X008TA0011";
    /// <summary>
    /// Идентификатор в ресурсах с текстом "Иммунитет".
    /// </summary>
    private const string IMMUNITY_TEXT_ID = "X008TA0002";

    private readonly ITextProvider _textProvider;
    private readonly ISceneObjectContainer _sceneObjectContainer;
    private readonly IBattleInterfaceProvider _battleInterfaceProvider;
    private readonly IBattleUnitResourceProvider _battleUnitResourceProvider;
    private readonly BattleSquadPosition _unitSquadPosition;

    private IBitmap _unitFaceBitmap;
    private string _unitTypeId;

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
    /// Иконка, если юнит защитился.
    /// </summary>
    private IImageSceneObject? _defendIcon;
    /// <summary>
    /// Иконка, если юнит сбежал.
    /// </summary>
    private IImageSceneObject? _retreatedIcon;
    /// <summary>
    /// Иконка положительных модификаторов.
    /// </summary>
    private IImageSceneObject? _positiveModifiersIcon;
    /// <summary>
    /// Иконки эффектов, которые воздействуют на юнита.
    /// </summary>
    private readonly Dictionary<UnitAttackType, IImageSceneObject> _battleEffectsIcons;
    /// <summary>
    /// Передний фон эффектов, которые воздействуют на юнита.
    /// </summary>
    private readonly Dictionary<UnitAttackType, IImageSceneObject> _battleEffectsForegrounds;
    /// <summary>
    /// Количество ОЗ, которое было при предыдущей проверке.
    /// </summary>
    private int _lastUnitHitPoints;
    /// <summary>
    /// Количество уровней, которые было при предыдущей проверке.
    /// </summary>
    private int _lastLevelDiff;

    /// <summary>
    /// Фон сообщения.
    /// </summary>
    private IImageSceneObject? _messageBackground;
    /// <summary>
    /// Тексто сообщения.
    /// </summary>
    private ITextSceneObject? _messageText;

    /// <summary>
    /// Создать объект класса <see cref="UnitPortraitObject" />.
    /// </summary>
    public UnitPortraitObject(ITextProvider textProvider,
        ISceneObjectContainer sceneObjectContainer,
        IBattleInterfaceProvider battleInterfaceProvider,
        IBattleUnitResourceProvider battleUnitResourceProvider,
        Action<UnitPortraitObject> onUnitPortraitSelected,
        Action<UnitPortraitObject> onUnitPortraitRightMouseButtonClicked,
        Action<UnitPortraitObject> onUnitPortraitMouseLeftButtonPressed,
        Unit unit,
        BattleSquadPosition unitSquadPosition,
        RectangleD portraitBounds,
        RectangleD hitPointsBounds) : base(RectangleD.Union(portraitBounds, hitPointsBounds))
    {
        _textProvider = textProvider;
        _sceneObjectContainer = sceneObjectContainer;
        _battleInterfaceProvider = battleInterfaceProvider;
        _battleUnitResourceProvider = battleUnitResourceProvider;
        _unitSquadPosition = unitSquadPosition;

        _unitFaceBitmap = battleUnitResourceProvider.GetUnitFace(unit.UnitType);
        _unitTypeId = unit.UnitType.Id;

        Unit = unit;
        PortraitBounds = portraitBounds;
        HitPointsBounds = hitPointsBounds;

        _battleEffectsIcons = new Dictionary<UnitAttackType, IImageSceneObject>();
        _battleEffectsForegrounds = new Dictionary<UnitAttackType, IImageSceneObject>();

        Components = new IComponent[]
        {
            new SelectionComponent(this, sceneObjectContainer, () => onUnitPortraitSelected.Invoke(this)),
            new MouseLeftButtonClickComponent(this, Array.Empty<KeyboardButton>(), onClickedAction: () => onUnitPortraitRightMouseButtonClicked.Invoke(this)),
            new MouseRightButtonClickComponent(this, () => onUnitPortraitMouseLeftButtonPressed.Invoke(this))
        };
    }

    /// <summary>
    /// Юнит.
    /// </summary>
    public Unit Unit { get; private set; }

    /// <summary>
    /// Расположение портрета.
    /// </summary>
    public RectangleD PortraitBounds { get; }

    /// <summary>
    /// Расположение информации о здоровье.
    /// </summary>
    public RectangleD HitPointsBounds { get; }

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        _unitPortrait = _sceneObjectContainer.AddImage(_unitFaceBitmap, PortraitBounds, BattleLayers.INTERFACE_LAYER + PORTRAIT_AND_HP_LAYER_SHIFT);

        _unitHitpoints = _sceneObjectContainer.AddText(null, HitPointsBounds, BattleLayers.INTERFACE_LAYER + PORTRAIT_AND_HP_LAYER_SHIFT);

        // Если юнит большой, то необходимо "закрасить" область между двумя клетками на панели.
        if (!Unit.UnitType.IsSmall)
        {
            _unitPanelSeparator = _sceneObjectContainer.AddImage(
                _battleInterfaceProvider.PanelSeparator,
                HitPointsBounds.X + (HitPointsBounds.Width - _battleInterfaceProvider.PanelSeparator.ActualSize.Width) / 2,
                HitPointsBounds.Y + (HitPointsBounds.Height - _battleInterfaceProvider.PanelSeparator.ActualSize.Height) / 2 + 1,
                BattleLayers.PANEL_LAYER + PANEL_SEPARATOR_LAYER_SHIFT);
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
        RemoveSceneObject(ref _messageBackground);
        RemoveSceneObject(ref _messageText);
        RemoveSceneObject(ref _unitDamageForeground);
        RemoveSceneObject(ref _unitPanelSeparator);
        RemoveSceneObject(ref _highLevelUnitIcon);
        RemoveSceneObject(ref _defendIcon);
        RemoveSceneObject(ref _retreatedIcon);
        RemoveSceneObject(ref _positiveModifiersIcon);

        foreach (var battleEffectsIcon in _battleEffectsIcons)
            _sceneObjectContainer.RemoveSceneObject(battleEffectsIcon.Value);

        foreach (var battleEffectsForeground in _battleEffectsForegrounds)
            _sceneObjectContainer.RemoveSceneObject(battleEffectsForeground.Value);

        base.Destroy();
    }

    /// <summary>
    /// Отобразить сообщение.
    /// </summary>
    public void ShowMessage(BattleUnitPortraitEventData eventData)
    {
        if (_messageText != null || _messageBackground != null)
            throw new InvalidOperationException($"Портрет юнита {Unit.Id} уже отображает сообщение");

        // Сразу добавляем иконки новых эффектов.
        ProcessBattleEffects();

        _messageBackground = AddColorImage(eventData.UnitActionType, eventData.AttackType, eventData.EffectDuration);
        _messageText = AddText(eventData);
        _unitHitpoints.Text = GetUnitHitPoints();

        // При воскрешении юнита, сразу убираем значок смерти.
        if (eventData.AttackType == UnitAttackType.Revive)
            RemoveSceneObject(ref _deathIcon);
    }

    /// <summary>
    /// Закрыть сообщение.
    /// </summary>
    public void CloseMessage()
    {
        RemoveSceneObject(ref _messageBackground);
        RemoveSceneObject(ref _messageText);

        // Сбрасываем количество ХП, чтобы обновить рамку.
        _lastUnitHitPoints = int.MaxValue;
        UpdateUnitEffects();
    }

    /// <summary>
    /// Заменить юнита на портрете.
    /// </summary>
    public void ChangeUnit(Unit newUnit)
    {
        Unit = newUnit;
        ProcessUnitState();
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

        _unitPortrait.IsReflected = Unit.IsRetreated || Unit.Effects.IsRetreating
            ? _unitSquadPosition == BattleSquadPosition.Attacker
            : _unitSquadPosition != BattleSquadPosition.Attacker;

        // После превращение тип юнита меняется.
        if (_unitTypeId != Unit.UnitType.Id)
        {
            _unitFaceBitmap = _battleUnitResourceProvider.GetUnitFace(Unit.UnitType);
            _unitPortrait.Bitmap = _unitFaceBitmap;
            _unitTypeId = Unit.UnitType.Id;
        }

        // Если сейчас выводятся сообщение, то рамку размещать не нужно.
        if (_messageText != null || _messageBackground != null)
            return;

        ProcessBattleEffects();
        ProcessUnitState();
    }

    /// <summary>
    /// Обработать эффекты битвы - добавить новые и удалить старые.
    /// </summary>
    private void ProcessBattleEffects()
    {
        // Иконку "Защиты" располагаем по центру.
        if (Unit.Effects.IsDefended && _defendIcon == null)
        {
            var icon = _battleInterfaceProvider.UnitPortraitDefendIcon;
            _defendIcon = _sceneObjectContainer.AddImage(
                icon,
                PortraitBounds.X + (PortraitBounds.Width - icon.OriginalSize.Width) / 2,
                PortraitBounds.Y + PortraitBounds.Height - icon.OriginalSize.Height,
                BattleLayers.INTERFACE_LAYER + EFFECTS_LAYER_SHIFT);
        }
        else if (!Unit.Effects.IsDefended && _defendIcon != null)
        {
            RemoveSceneObject(ref _defendIcon);
        }

        // Иконка сбежавшего юнита отображается снизу слева.
        // После того, как юнит сбежал - все эффекты с него удаляются, поэтому не будет пересечений с _battleEffectsIcons.
        if (Unit.IsRetreated && _retreatedIcon == null)
        {
            var icon = _battleInterfaceProvider.UnitPortraitRetreatedIcon;
            _retreatedIcon = _sceneObjectContainer.AddImage(
                icon,
                PortraitBounds.X + PortraitBounds.Width - icon.OriginalSize.Width,
                PortraitBounds.Y + PortraitBounds.Height - icon.OriginalSize.Height,
                BattleLayers.INTERFACE_LAYER + EFFECTS_LAYER_SHIFT);
        }

        var battleEffects = Unit.Effects.GetBattleEffects();

        // Удаляем иконки тех эффектов, действие которых закончилось.
        var expiredEffects = _battleEffectsIcons
            .Where(bei => battleEffects.All(be => be.AttackType != bei.Key));
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
        // TODO Если эффектов слишком много, заполнять соседний столбец.
        foreach (var battleEffect in battleEffects)
        {
            if (!_battleEffectsIcons.ContainsKey(battleEffect.AttackType))
            {
                if (_battleInterfaceProvider.UnitBattleEffectsIcon.TryGetValue(battleEffect.AttackType, out var icon))
                {
                    // Иконки остальных эффектов располагаются справа.
                    var iconsCount = _battleEffectsIcons.Keys
                        .GroupBy(be => be)
                        .Count();
                    _battleEffectsIcons.Add(battleEffect.AttackType, _sceneObjectContainer.AddImage(
                        icon,
                        PortraitBounds.X + PortraitBounds.Width - icon.OriginalSize.Width,
                        PortraitBounds.Y + PortraitBounds.Height - icon.OriginalSize.Height * (iconsCount + 1),
                        BattleLayers.INTERFACE_LAYER + EFFECTS_LAYER_SHIFT));
                }
            }

            // Добавляем фон, если есть связанный с эффектом.
            if (!_battleEffectsForegrounds.ContainsKey(battleEffect.AttackType))
            {
                var effectTypeColor = GetEffectTypeColor(battleEffect.AttackType);
                if (effectTypeColor != null)
                {
                    _battleEffectsForegrounds.Add(
                        battleEffect.AttackType,
                        AddColorImage(effectTypeColor.Value, false));
                }
            }
        }

        if (Unit.Effects.HasPositiveModifiers && _positiveModifiersIcon == null)
        {
            var icon = _battleInterfaceProvider.UnitPortraitPositiveModifierIcon;
            _positiveModifiersIcon = _sceneObjectContainer.AddImage(
                icon,
                PortraitBounds.X,
                PortraitBounds.Y,
                BattleLayers.INTERFACE_LAYER + EFFECTS_LAYER_SHIFT);
        }
        else if (!Unit.Effects.HasPositiveModifiers && _positiveModifiersIcon != null)
        {
            RemoveSceneObject(ref _positiveModifiersIcon);
        }
    }

    /// <summary>
    /// Обработать состояние юнита.
    /// </summary>
    private void ProcessUnitState()
    {
        if (Unit.IsDead)
        {
            if (_deathIcon == null)
            {
                var deathScull = Unit.UnitType.IsSmall
                    ? _battleInterfaceProvider.DeathSkullSmall
                    : _battleInterfaceProvider.DeathSkullBig;
                _deathIcon = _sceneObjectContainer.AddImage(deathScull, X, Y, BattleLayers.INTERFACE_LAYER + FOREGROUND_LAYER_SHIFT);
                _unitHitpoints.Text = GetUnitHitPoints();

                RemoveSceneObject(ref _unitDamageForeground);
                RemoveBattleEffectsForegrounds();
            }
        }
        else if (_lastUnitHitPoints != Unit.HitPoints)
        {
            _lastUnitHitPoints = Unit.HitPoints;
            _unitHitpoints.Text = GetUnitHitPoints();

            RemoveSceneObject(ref _unitDamageForeground);

            var height = (1 - ((double)_lastUnitHitPoints / Unit.MaxHitPoints)) * PortraitBounds.Height;
            if (height > 0)
            {
                var width = PortraitBounds.Width;
                var x = PortraitBounds.X;
                var y = PortraitBounds.Y + (PortraitBounds.Height - height);
                var bounds = new RectangleD(x, y, width, height);

                _unitDamageForeground = _sceneObjectContainer.AddColorImage(BattleColors.Damage, bounds, BattleLayers.INTERFACE_LAYER + FOREGROUND_LAYER_SHIFT);
            }
        }
    }

    /// <summary>
    /// Получить цвет фона для типа атаки.
    /// </summary>
    private static Color? GetUnitAttackTypeColor(UnitAttackType? unitAttackType, bool isCompleted)
    {
        return unitAttackType switch
        {
            UnitAttackType.Damage => BattleColors.Damage,
            UnitAttackType.DrainLife => BattleColors.Damage,
            UnitAttackType.Paralyze => isCompleted
                ? BattleColors.Transparent
                : BattleColors.Paralyze,
            UnitAttackType.Heal => BattleColors.Heal,
            UnitAttackType.Fear => BattleColors.Damage,
            UnitAttackType.IncreaseDamage => BattleColors.Boost,
            UnitAttackType.Poison => BattleColors.Poison,
            UnitAttackType.Frostbite => BattleColors.Frostbite,
            UnitAttackType.DrainLifeOverflow => BattleColors.Damage,
            UnitAttackType.Revive => BattleColors.Heal,
            UnitAttackType.Cure => BattleColors.Heal,
            UnitAttackType.Summon => BattleColors.Summon,
            UnitAttackType.ReduceLevel => BattleColors.ReduceLevel,
            UnitAttackType.Doppelganger => BattleColors.Transform,
            UnitAttackType.TransformSelf => BattleColors.Transform,
            UnitAttackType.TransformEnemy => BattleColors.Transform,
            UnitAttackType.Blister => BattleColors.Blister,
            UnitAttackType.GiveProtection => BattleColors.Boost,
            UnitAttackType.ReduceArmor => BattleColors.ReduceArmor,
            _ => null
        };
    }

    /// <summary>
    /// Получить цвет эффекта.
    /// </summary>
    private static Color? GetEffectTypeColor(UnitAttackType unitEffectAttackType)
    {
        if (unitEffectAttackType is UnitAttackType.Paralyze
            or UnitAttackType.Poison
            or UnitAttackType.Frostbite
            or UnitAttackType.Blister)
        {
            return GetUnitAttackTypeColor(unitEffectAttackType, false);
        }

        return null;
    }

    /// <summary>
    /// Получить наименования эффекта, что воздействует на юнита.
    /// </summary>
    private static string GetAttackTypeTextId(UnitAttackType attackType, bool isEffectCompleted, int? power = null)
    {
        return attackType switch
        {
            UnitAttackType.Paralyze => isEffectCompleted
                ? "X008TA0024"
                : "X008TA0008",
            UnitAttackType.Fear => "X008TA0007",
            UnitAttackType.IncreaseDamage => "X008TA0003",
            UnitAttackType.Petrify => isEffectCompleted
                ? "X008TA0025"
                : "X008TA0009",
            UnitAttackType.ReduceDamage => "X008TA0012",
            UnitAttackType.ReduceInitiative => "X008TA0013",
            UnitAttackType.Poison => power == null
                ? "X008TA0026"
                : "X008TA0014",
            UnitAttackType.Frostbite => power == null
                ? "X008TA0027"
                : "X008TA0015",
            UnitAttackType.Revive => "X008TA0016",
            UnitAttackType.Cure => "X008TA0017",
            UnitAttackType.ReduceLevel => isEffectCompleted
                ? "X008TA0029"
                : "X008TA0018",
            UnitAttackType.GiveAdditionalAttack => "X008TA0019",
            UnitAttackType.Doppelganger => "X008TA0022",
            UnitAttackType.TransformSelf => "X008TA0022",
            UnitAttackType.TransformEnemy => isEffectCompleted
                ? "X008TA0023"
                : "X008TA0022",
            UnitAttackType.Blister => power == null
                ? "X160TA0011"
                : "X160TA0022",
            UnitAttackType.GiveProtection => "X160TA0013",
            UnitAttackType.ReduceArmor => "X160TA0019",
            _ => throw new ArgumentOutOfRangeException(nameof(attackType), attackType, null)
        };
    }

    /// <summary>
    /// Добавить на портрет изображение указанного цвета.
    /// </summary>
    private IImageSceneObject? AddColorImage(UnitActionType unitActionType, UnitAttackType? unitAttackType, EffectDuration? effectDuration)
    {
        switch (unitActionType)
        {
            case UnitActionType.Miss:
                return AddColorImage(BattleColors.Miss);

            case UnitActionType.Experience:
                return AddColorImage(BattleColors.Experience);

            case UnitActionType.Defend:
            case UnitActionType.Waiting:
            case UnitActionType.Dying:
            case UnitActionType.Ward:
            case UnitActionType.Immunity:
                return null;
        }

        if (unitAttackType == null || _battleEffectsForegrounds.ContainsKey(unitAttackType.Value))
            return null;

        var color = GetUnitAttackTypeColor(unitAttackType, effectDuration?.IsCompleted == true);
        if (color != null)
        {
            return AddColorImage(color.Value, color == BattleColors.Damage
                                              || color == BattleColors.Heal
                                              || color == BattleColors.Boost
                                              || color == BattleColors.ReduceArmor
                                              || color == BattleColors.ReduceLevel);
        }

        return null;
    }

    /// <summary>
    /// Добавить на портрет изображение указанного цвета.
    /// </summary>
    private IImageSceneObject AddColorImage(Color color, bool shouldRemoveDamageImage = true)
    {
        // Если мы добавляем изображение поверх портрета, то в некоторых случаях должны на время очистить изображение с % здоровья.
        if (shouldRemoveDamageImage)
        {
            RemoveSceneObject(ref _unitDamageForeground);
            RemoveBattleEffectsForegrounds();
        }

        return _sceneObjectContainer.AddColorImage(color, PortraitBounds, BattleLayers.INTERFACE_LAYER + FOREGROUND_LAYER_SHIFT);
    }

    /// <summary>
    /// Добавить на портрет указанный текст.
    /// </summary>
    private ITextSceneObject AddDamageText(string textId, UnitAttackType attackType, int? power)
    {
        var text = _textProvider.GetText(textId);

        switch (attackType)
        {
            case UnitAttackType.Damage:
            case UnitAttackType.DrainLife:
            case UnitAttackType.Poison:
            case UnitAttackType.Frostbite:
            case UnitAttackType.DrainLifeOverflow:
            case UnitAttackType.Blister:
                text = text.ReplacePlaceholders(new []{ ("%DAMAGE%", new TextContainer($"-{power}")) });
                break;

            case UnitAttackType.Heal:
                text = text.ReplacePlaceholders(new []{ ("%DAMAGE%", new TextContainer($"+{power}")) });
                break;
        }

        return AddText(text);
    }

    /// <summary>
    /// Добавить на портрет повреждение с критическим уроном.
    /// </summary>
    private ITextSceneObject AddCriticalDamageText(int power, int criticalDamage)
    {
        var text = _textProvider
            .GetText(CRITICAL_DAMAGE_TEXT_ID)
            .ReplacePlaceholders(new []{ ("%DAMAGE%", new TextContainer(power.ToString())) })
            .ReplacePlaceholders(new []{ ("%CRITICAL%", new TextContainer(criticalDamage.ToString())) });
        return AddText(text);
    }

    /// <summary>
    /// Добавить текст с начислением опыта.
    /// </summary>
    private ITextSceneObject? AddExperienceText(int experience)
    {
        if (experience == 0)
            return null;

        var text = _textProvider
            .GetText(EXPERIENCE_TEXT_ID)
            .ReplacePlaceholders(new []{ ("%GAIN%", new TextContainer(experience.ToString())) });
        return AddText(text);
    }

    /// <summary>
    /// Добавить на портрет указанный текст.
    /// </summary>
    private ITextSceneObject? AddText(BattleUnitPortraitEventData eventData)
    {
        switch (eventData.UnitActionType)
        {
            case UnitActionType.Attacked:
            {
                switch (eventData.AttackType)
                {
                    case UnitAttackType.Damage:
                    case UnitAttackType.DrainLife:
                    case UnitAttackType.DrainLifeOverflow:
                    case UnitAttackType.Heal:
                        return eventData.CriticalDamage > 0
                            ? AddCriticalDamageText(eventData.Power!.Value, eventData.CriticalDamage.Value)
                            : AddDamageText(DAMAGE_TEXT_ID, eventData.AttackType!.Value, eventData.Power);

                    case UnitAttackType.Paralyze:
                    case UnitAttackType.Fear:
                    case UnitAttackType.IncreaseDamage:
                    case UnitAttackType.Petrify:
                    case UnitAttackType.ReduceDamage:
                    case UnitAttackType.ReduceInitiative:
                    case UnitAttackType.Revive:
                    case UnitAttackType.Cure:
                    case UnitAttackType.ReduceLevel:
                    case UnitAttackType.GiveAdditionalAttack:
                    case UnitAttackType.Doppelganger:
                    case UnitAttackType.TransformSelf:
                    case UnitAttackType.TransformEnemy:
                    case UnitAttackType.GiveProtection:
                    case UnitAttackType.ReduceArmor:
                        return AddText(GetAttackTypeTextId(eventData.AttackType!.Value, eventData.EffectDuration?.IsCompleted == true));

                    case UnitAttackType.Poison:
                    case UnitAttackType.Frostbite:
                    case UnitAttackType.Blister:
                    {
                        // Если идёт наложение эффекта, то выводим просто его название.
                        // При срабатывании эффекта и нанесения урона, выводим еще и его.
                        return eventData.IsEffectTriggered
                            ? AddDamageText(
                                GetAttackTypeTextId(eventData.AttackType!.Value, eventData.EffectDuration!.IsCompleted, eventData.Power),
                                eventData.AttackType!.Value,
                                eventData.Power)
                            : AddText(GetAttackTypeTextId(eventData.AttackType!.Value, eventData.EffectDuration?.IsCompleted == true));
                    }

                    default:
                        return null;
                }
            }

            case UnitActionType.Miss:
                return AddText(MISS_TEXT_ID);

            case UnitActionType.Defend:
                return AddText(DEFEND_TEXT_ID);

            case UnitActionType.Waiting:
                return AddText(WAIT_TEXT_ID);

            case UnitActionType.Dying:
                return null;

            case UnitActionType.Ward:
                return AddText(WARD_TEXT_ID);

            case UnitActionType.Immunity:
                return AddText(IMMUNITY_TEXT_ID);

            case UnitActionType.Experience:
                return AddExperienceText(Unit.BattleExperience);
        }

        return null;
    }

    /// <summary>
    /// Добавить на портрет указанный текст.
    /// </summary>
    private ITextSceneObject AddText(string textId)
    {
        return AddText(_textProvider.GetText(textId));
    }

    /// <summary>
    /// Добавить на портрет указанный текст.
    /// </summary>
    private ITextSceneObject AddText(TextContainer text)
    {
        return _sceneObjectContainer.AddText(
            text,
            new TextStyle { ForegroundColor = GameColors.White, FontSize = 12, FontWeight = FontWeight.Bold },
            PortraitBounds,
            BattleLayers.INTERFACE_LAYER + TEXT_LAYER_SHIFT);
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
            PortraitBounds.X + (PortraitBounds.Width - icon.OriginalSize.Width) / 2,
            PortraitBounds.Y,
            BattleLayers.INTERFACE_LAYER + EFFECTS_LAYER_SHIFT);
    }

    /// <summary>
    /// Получить информацию о состоянии здоровья юнита.
    /// </summary>
    private TextContainer GetUnitHitPoints()
    {
        return _textProvider
            .GetText(UNIT_HIT_POINTS_TEXT_ID)
            .ReplacePlaceholders(new []
            {
                ("%HP%", new TextContainer(Unit.HitPoints.ToString()) ),
                ("%HPMAX%", new TextContainer(Unit.MaxHitPoints.ToString()) ),
            });
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
        sceneObject = default;
    }
}