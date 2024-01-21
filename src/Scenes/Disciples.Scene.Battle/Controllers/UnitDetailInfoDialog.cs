using System.Text;
using Disciples.Engine.Common.Constants;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Enums;
using Disciples.Engine.Implementation.Common.Controllers;
using Disciples.Engine.Models;
using Disciples.Scene.Battle.Providers;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Extensions;
using Disciples.Scene.Battle.Constants;

namespace Disciples.Scene.Battle.Controllers;

/// <summary>
/// Диалог отображения информации о юните.
/// </summary>
internal class UnitDetailInfoDialog : BaseDialog
{
    /// <summary>
    /// Идентификатор в ресурсах с текстом основной информации о юните.
    /// </summary>
    private const string UNIT_BASE_INFO_ID = "X005TA0423";
    /// <summary>
    /// Идентификатор в ресурсах с первой частью текста информации об атаке юнита.
    /// </summary>
    private const string UNIT_ATTACK_INFO_FIRST_PART_ID = "X005TA0787";
    /// <summary>
    /// Идентификатор в ресурсах со второй частью текста информации об атаке юнита.
    /// </summary>
    private const string UNIT_ATTACK_INFO_SECOND_PART_ID = "X005TA0788";

    private readonly IBattleGameObjectContainer _gameObjectContainer;
    private readonly IBattleInterfaceProvider _battleInterfaceProvider;
    private readonly IBattleUnitResourceProvider _battleUnitResourceProvider;
    private readonly ITextProvider _textProvider;
    private readonly ISceneInterfaceController _sceneInterfaceController;

    private readonly Unit _unit;

    private IReadOnlyList<GameObject> _gameObjects = null!;
    private GameObject? _beforeOpenSelectedGameObject;
    private GameObject? _lastSelectedGameObject;

    /// <summary>
    /// Создать объект типа <see cref="UnitDetailInfoDialog" />.
    /// </summary>
    public UnitDetailInfoDialog(IBattleGameObjectContainer gameObjectContainer,
        IBattleInterfaceProvider battleInterfaceProvider,
        IBattleUnitResourceProvider battleUnitResourceProvider,
        ITextProvider textProvider,
        ISceneInterfaceController sceneInterfaceController,
        Unit unit)
    {
        _gameObjectContainer = gameObjectContainer;
        _unit = unit;
        _battleInterfaceProvider = battleInterfaceProvider;
        _battleUnitResourceProvider = battleUnitResourceProvider;
        _textProvider = textProvider;
        _sceneInterfaceController = sceneInterfaceController;
    }

    /// <inheritdoc />
    public override void Open()
    {
        _beforeOpenSelectedGameObject = _gameObjectContainer
            .GameObjects
            .FirstOrDefault(go => go.SelectionComponent?.IsHover == true);
        _lastSelectedGameObject = _beforeOpenSelectedGameObject;
        _gameObjects = _sceneInterfaceController.AddSceneGameObjects(_battleInterfaceProvider.UnitDetailInfoInterface, Layers.DialogLayers);

        var unitPortrait = _gameObjects.Get<ImageObject>(UnitDetailInfoElementNames.PORTRAIT_IMAGE);
        unitPortrait.Bitmap = _battleUnitResourceProvider.GetUnitPortrait(_unit.UnitType);
        unitPortrait.HorizontalAlignment = HorizontalAlignment.Center;
        unitPortrait.VerticalAlignment = VerticalAlignment.Center;

        var unitName = _gameObjects.Get<TextBlockObject>(UnitDetailInfoElementNames.NAME_TEXT_BLOCK);
        unitName.Text = new TextContainer(new []{ new TextPiece(new TextStyle { FontWeight = FontWeight.Bold }, _unit.UnitType.Name) });

        var unitDescription = _gameObjects.Get<TextBlockObject>(UnitDetailInfoElementNames.DESCRIPTION_TEXT_BLOCK);
        unitDescription.Text = new TextContainer(_unit.UnitType.Description);

        var unitStats = _gameObjects.Get<TextBlockObject>(UnitDetailInfoElementNames.STATS_TEXT_BLOCK);
        unitStats.Text = ReplacePlaceholders(_textProvider.GetText(UNIT_BASE_INFO_ID), _unit);

        var unitAttack = _gameObjects.Get<TextBlockObject>(UnitDetailInfoElementNames.ATTACK_INFO_TEXT_BLOCK);
        var firstPartAttack = ReplacePlaceholders(_textProvider.GetText(UNIT_ATTACK_INFO_FIRST_PART_ID), _unit);
        var secondPartAttack = ReplacePlaceholders(_textProvider.GetText(UNIT_ATTACK_INFO_SECOND_PART_ID), _unit);
        unitAttack.Text = new TextContainer(
            firstPartAttack
                .TextPieces
                .Concat(secondPartAttack.TextPieces)
                .ToArray());
    }

    /// <inheritdoc />
    public override void ProcessInputDeviceEvents(IReadOnlyList<InputDeviceEvent> inputDeviceEvents)
    {
        // Запоминаем последний выбранный объект.
        var selectionEvent = inputDeviceEvents
            .LastOrDefault(e => e.ActionType == InputDeviceActionType.Hover);
        if (selectionEvent != null)
        {
            _lastSelectedGameObject = selectionEvent.ActionState == InputDeviceActionState.Activated
                ? selectionEvent.GameObject
                : null;
        }

        // Диалог закрывается по отжатой ПКМ.
        var releasedRightMouseButtonEvent = inputDeviceEvents
            .FirstOrDefault(e => e.ActionType == InputDeviceActionType.MouseRight && e.ActionState == InputDeviceActionState.Deactivated);
        if (releasedRightMouseButtonEvent == null)
            return;

        IsClosed = true;

        foreach (var gameObject in _gameObjects)
        {
            gameObject.Destroy();
        }

        // Обрабатываем событие изменения выбранного объекта.
        if (_beforeOpenSelectedGameObject != _lastSelectedGameObject)
        {
            _beforeOpenSelectedGameObject?.SelectionComponent!.Unhovered();
            _lastSelectedGameObject?.SelectionComponent!.Hovered();
        }

        // Прокидываем событие отжатой кнопки до объекта, на котором она была нажата.
        var pressedGameObject = _gameObjectContainer
            .GameObjects
            .FirstOrDefault(go => go.MouseRightButtonClickComponent?.IsPressed == true);
        pressedGameObject?.MouseRightButtonClickComponent!.Released();
    }

    /// <summary>
    /// Заменить плейсхолдеры значениями характеристик юнита.
    /// </summary>
    private TextContainer ReplacePlaceholders(TextContainer value, Unit unit)
    {
        return value
            .ReplacePlaceholders(new []
            {
                ("%LEVEL%", new TextContainer(unit.Level.ToString())),
                ("%XP%", new TextContainer($"{unit.Experience} / {unit.UnitType.XpNext}")),
                ("%HP1%", new TextContainer(unit.HitPoints.ToString())),
                ("%HP2%", new TextContainer(unit.MaxHitPoints.ToString())),
                ("%ARMOR%", GetValueWithModifier(unit.BaseArmor.ToString(), unit.ArmorModifier)),
                ("%IMMU%", GetProtectionInfo(unit, ProtectionCategory.Immunity)),
                ("%WARD%", GetProtectionInfo(unit, ProtectionCategory.Ward)),

                ("%TWICE%", new TextContainer(unit.UnitType.IsAttackTwice ? "(2x) " : string.Empty)),
                ("%ALTATTACK%", new TextContainer(string.Empty)), // todo Что это?
                ("%ATTACK%", new TextContainer(unit.UnitType.MainAttack.Description)),
                ("%SECOND%", new TextContainer(unit.UnitType.SecondaryAttack == null ? string.Empty : $" / {unit.UnitType.SecondaryAttack.Description}")),
                ("%HIT%", new TextContainer($"{GetValueWithModifier(unit.MainAttackBaseAccuracy.ToString(), unit.MainAttackAccuracyModifier)}%")),
                ("%HIT2%", new TextContainer(unit.SecondaryAttackAccuracy == null ? string.Empty : $" / {unit.SecondaryAttackAccuracy}%")),

                ("%EFFECT%", GetUnitEffectTitle()),
                ("%DAMAGE%", GetDamage(unit)),
                ("%SOURCE%", GetAttackSourceTitle(unit.UnitType.MainAttack.AttackSource)),
                ("%SOURCE2%", new TextContainer(unit.UnitType.SecondaryAttack == null ? string.Empty : $" / {GetAttackSourceTitle(unit.UnitType.MainAttack.AttackSource)}")),
                ("%INIT%", GetValueWithModifier(unit.BaseInitiative.ToString(), unit.InitiativeModifier)),
                ("%REACH%", GetReachTitle(unit)),
                ("%TARGETS%", GetReachCount(unit))
            })
            ;
    }

    /// <summary>
    /// Получить урон юнита.
    /// </summary>
    private static TextContainer GetDamage(Unit unit)
    {
        var mainAttack = GetValueWithModifier(
            GetAttackPower(unit.MainAttackBasePower, unit.UnitType.MainAttack.AttackType),
            unit.MainAttackPowerModifier);

        if (unit.SecondaryAttackPower > 0 &&
            unit.UnitType.SecondaryAttack!.AttackType is not UnitAttackType.ReduceDamage and not UnitAttackType.ReduceInitiative)
        {
            var secondaryAttackPower = GetValueWithModifier(
                GetAttackPower(unit.SecondaryAttackPower.Value, unit.UnitType.SecondaryAttack!.AttackType),
                unit.MainAttackPowerModifier);
            return new TextContainer(mainAttack
                .TextPieces
                .Append(new TextPiece($" / {secondaryAttackPower}"))
                .ToArray());
        }

        return mainAttack;
    }

    /// <summary>
    /// Получить строковое значение силы атаки.
    /// </summary>
    private static string GetAttackPower(int power, UnitAttackType unitAttackType)
    {
        switch (unitAttackType)
        {
            case UnitAttackType.BoostDamage:
                return $"+{power}%";
            case UnitAttackType.ReduceDamage:
            case UnitAttackType.ReduceInitiative:
                return $"-{power}%";
            default:
                return power.ToString();
        }
    }

    /// <summary>
    /// Получить строковое значение характеристики и её модификатора.
    /// </summary>
    /// <param name="value">Значение характеристики.</param>
    /// <param name="modifier">Модификатор характеристики.</param>
    private static TextContainer GetValueWithModifier(string value, int modifier)
    {
        if (modifier == 0)
            return new TextContainer(value);

        if (modifier > 0)
        {
            return new TextContainer(new[]
            {
                new TextPiece(value),
                new TextPiece(new TextStyle { ForegroundColor = BattleColors.BoostDamage }, $" + {modifier}")
            });
        }

        return new TextContainer(new[]
        {
            new TextPiece(value),
            new TextPiece(new TextStyle { ForegroundColor = BattleColors.Damage }, $" - {-modifier}")
        });
    }

    /// <summary>
    /// Получить наименование класса атаки юнита.
    /// </summary>
    private TextContainer GetUnitEffectTitle()
    {
        if (_unit.UnitType.MainAttack.AttackType == UnitAttackType.Heal)
            return _textProvider.GetText("X005TA0504");

        if (_unit.UnitType.MainAttack.AttackType == UnitAttackType.BoostDamage)
            return _textProvider.GetText("X005TA0534");

        return _textProvider.GetText("X005TA0503");
    }

    /// <summary>
    /// Получить информацию о защитах юнита указанной категории.
    /// </summary>
    private TextContainer GetProtectionInfo(Unit unit, ProtectionCategory protectionCategory)
    {
        var attackSourceProtections = unit
            .AttackSourceProtections
            .Where(p => p.ProtectionCategory == protectionCategory)
            .OrderBy(p => p.UnitAttackSource)
            .ToList();
        var attackTypeProtections = unit
            .AttackTypeProtections
            .Where(p => p.ProtectionCategory == protectionCategory)
            .OrderBy(p => p.UnitAttackType)
            .ToList();

        if (attackSourceProtections.Count == 0 && attackTypeProtections.Count == 0)
            return _textProvider.GetText("X005TA0469");

        var stringBuilder = new StringBuilder();
        stringBuilder.Append(
            string.Join(", ", attackSourceProtections.Select(sp => GetAttackSourceTitle(sp.UnitAttackSource))));
        stringBuilder.Append(
            string.Join(", ", attackTypeProtections.Select(atp => GetAttackTypeTitle(atp.UnitAttackType))));
        return new TextContainer(stringBuilder.ToString());
    }

    /// <summary>
    /// Получить наименование источники атаки.
    /// </summary>
    private TextContainer GetAttackSourceTitle(UnitAttackSource attackSource)
    {
        var attackSourceTextId = attackSource switch
        {
            UnitAttackSource.Weapon => "X005TA0145",
            UnitAttackSource.Mind => "X005TA0146",
            UnitAttackSource.Life => "X005TA0147",
            UnitAttackSource.Death => "X005TA0148",
            UnitAttackSource.Fire => "X005TA0149",
            UnitAttackSource.Water => "X005TA0150",
            UnitAttackSource.Earth => "X005TA0152",
            UnitAttackSource.Air => "X005TA0151",
            _ => throw new ArgumentOutOfRangeException(nameof(attackSource), attackSource, null)
        };

        return _textProvider.GetText(attackSourceTextId);
    }

    /// <summary>
    /// Получить наименование класса атаки юнита.
    /// </summary>
    private TextContainer GetAttackTypeTitle(UnitAttackType attackType)
    {
        var attackTypeTextId = attackType switch
        {
            UnitAttackType.Damage => "X005TA0791",
            UnitAttackType.Drain => "X005TA0792",
            UnitAttackType.Paralyze => "X005TA0789",
            UnitAttackType.Heal => "X005TA0802",
            UnitAttackType.Fear => "X005TA0794",
            UnitAttackType.BoostDamage => "X005TA0795",
            UnitAttackType.Petrify => "X005TA0790",
            UnitAttackType.ReduceDamage => "X005TA0796",
            UnitAttackType.ReduceInitiative => "X005TA0797",
            UnitAttackType.Poison => "X005TA0798",
            UnitAttackType.Frostbite => "X005TA0799",
            UnitAttackType.Revive => "X005TA0800",
            UnitAttackType.DrainOverflow => "X005TA0801", // TODO перепроверить.
            UnitAttackType.Cure => "X005TA0793",
            UnitAttackType.Summon => "X005TA0803",
            UnitAttackType.DrainLevel => "X005TA0804",
            UnitAttackType.GiveAdditionalAttack => "X005TA0805",
            UnitAttackType.Doppelganger => "X005TA0806",
            UnitAttackType.TransformSelf => "X005TA0807",
            UnitAttackType.TransformOther => "X005TA0808",
            UnitAttackType.Blister => "X160TA0012",
            UnitAttackType.BestowWards => "X160TA0014",
            UnitAttackType.Shatter => "X160TA0020",
            _ => throw new ArgumentOutOfRangeException(nameof(attackType), attackType, null)
        };

        return _textProvider.GetText(attackTypeTextId);
    }

    /// <summary>
    /// Получить наименование, каких юнитов относительного своего расположения можно атаковать.
    /// </summary>
    private TextContainer GetReachTitle(Unit unit)
    {
        if (unit.UnitType.MainAttack.Reach == UnitAttackReach.Adjacent)
            return _textProvider.GetText("X005TA0201");

        return _textProvider.GetText("X005TA0200");
    }

    /// <summary>
    /// Получить количество целей для атаки.
    /// </summary>
    private TextContainer GetReachCount(Unit unit)
    {
        if (unit.UnitType.MainAttack.Reach == UnitAttackReach.All)
            return new TextContainer("6");

        return new TextContainer("1");
    }
}