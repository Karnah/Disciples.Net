using System.Text;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Enums;
using Disciples.Engine.Models;
using Disciples.Scene.Battle.Providers;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Extensions;
using Disciples.Scene.Battle.Constants;
using Disciples.Scene.Battle.Controllers;
using Disciples.Engine.Implementation.Dialogs.Base;

namespace Disciples.Scene.Battle.Dialogs;

/// <summary>
/// Диалог отображения информации о юните.
/// </summary>
internal class UnitDetailInfoDialog : BaseReleaseButtonCloseDialog
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

    private readonly IBattleUnitResourceProvider _battleUnitResourceProvider;
    private readonly ITextProvider _textProvider;

    private readonly Unit _unit;

    /// <summary>
    /// Создать объект типа <see cref="UnitDetailInfoDialog" />.
    /// </summary>
    public UnitDetailInfoDialog(
        IBattleGameObjectContainer gameObjectContainer,
        ISceneInterfaceController sceneInterfaceController,
        IInterfaceProvider interfaceProvider,
        IBattleUnitResourceProvider battleUnitResourceProvider,
        ITextProvider textProvider,
        Unit unit
        ) : base(gameObjectContainer, sceneInterfaceController, interfaceProvider)
    {
        _unit = unit;
        _battleUnitResourceProvider = battleUnitResourceProvider;
        _textProvider = textProvider;
    }

    /// <inheritdoc />
    protected override string DialogName => "DLG_R_C_UNIT";

    protected override void OpenInternal(IReadOnlyList<GameObject> dialogGameObjects)
    {
        base.OpenInternal(dialogGameObjects);

        var unitPortrait = dialogGameObjects.Get<ImageObject>(UnitDetailInfoElementNames.PORTRAIT_IMAGE);
        unitPortrait.Bitmap = _battleUnitResourceProvider.GetUnitPortrait(_unit.UnitType);
        unitPortrait.HorizontalAlignment = HorizontalAlignment.Center;
        unitPortrait.VerticalAlignment = VerticalAlignment.Center;

        var unitName = dialogGameObjects.Get<TextBlockObject>(UnitDetailInfoElementNames.NAME_TEXT_BLOCK);
        unitName.Text = new TextContainer(new[] { new TextPiece(new TextStyle { FontWeight = FontWeight.Bold }, _unit.UnitType.Name) });

        var unitDescription = dialogGameObjects.Get<TextBlockObject>(UnitDetailInfoElementNames.DESCRIPTION_TEXT_BLOCK);
        unitDescription.Text = new TextContainer(_unit.UnitType.Description);

        var unitStats = dialogGameObjects.Get<TextBlockObject>(UnitDetailInfoElementNames.STATS_TEXT_BLOCK);
        unitStats.Text = ReplacePlaceholders(_textProvider.GetText(UNIT_BASE_INFO_ID), _unit);

        var unitAttack = dialogGameObjects.Get<TextBlockObject>(UnitDetailInfoElementNames.ATTACK_INFO_TEXT_BLOCK);
        var firstPartAttack = ReplacePlaceholders(_textProvider.GetText(UNIT_ATTACK_INFO_FIRST_PART_ID), _unit);
        var secondPartAttack = ReplacePlaceholders(_textProvider.GetText(UNIT_ATTACK_INFO_SECOND_PART_ID), _unit);
        unitAttack.Text = new TextContainer(
            firstPartAttack
                .TextPieces
                .Concat(secondPartAttack.TextPieces)
                .ToArray());
    }

    /// <summary>
    /// Заменить плейсхолдеры значениями характеристик юнита.
    /// </summary>
    private TextContainer ReplacePlaceholders(TextContainer value, Unit unit)
    {
        var mainAttack = unit.MainAttack;
        var alternativeAttack = unit.AlternativeAttack;
        var defaultMainAttack = alternativeAttack ?? mainAttack;
        var secondaryAttack = unit.SecondaryAttack;

        return value
            .ReplacePlaceholders(new[]
            {
                ("%LEVEL%", new TextContainer(unit.Level.ToString())),
                ("%XP%", new TextContainer($"{unit.Experience} / {unit.UnitType.XpNext}")),
                ("%HP1%", new TextContainer(unit.HitPoints.ToString())),
                ("%HP2%", new TextContainer(unit.MaxHitPoints.ToString())),
                ("%ARMOR%", GetValueWithModifier(unit.BaseArmor.ToString(), unit.ArmorModifier)),
                ("%IMMU%", GetProtectionInfo(unit, ProtectionCategory.Immunity)),
                ("%WARD%", GetProtectionInfo(unit, ProtectionCategory.Ward)),

                ("%TWICE%", new TextContainer(unit.UnitType.IsAttackTwice ? "(2x) " : string.Empty)),
                ("%ALTATTACK%", new TextContainer(string.Empty)), // Альтернативная атака заполняется в GetMainAttack. Непонятно, зачем нужен этот плейсхолдер.
                ("%ATTACK%", GetMainAttack(mainAttack, alternativeAttack)),
                ("%SECOND%", new TextContainer(secondaryAttack == null ? string.Empty : $" / {secondaryAttack.Description}")),
                ("%HIT%", new TextContainer($"{GetValueWithModifier(defaultMainAttack.BaseAccuracy.ToString(), defaultMainAttack.AccuracyBonus)}%")),
                ("%HIT2%", new TextContainer(secondaryAttack == null ? string.Empty : $" / {secondaryAttack.TotalAccuracy}%")),

                ("%EFFECT%", GetUnitEffectTitle(defaultMainAttack)),
                ("%DAMAGE%", GetDamage(defaultMainAttack, secondaryAttack)),
                ("%SOURCE%", GetAttackSourceTitle(defaultMainAttack.AttackSource)),
                ("%SOURCE2%", new TextContainer(secondaryAttack == null ? string.Empty : $" / {GetAttackSourceTitle(secondaryAttack.AttackSource)}")),
                ("%INIT%", GetValueWithModifier(unit.BaseInitiative.ToString(), unit.InitiativeModifier)),
                ("%REACH%", GetReachTitle(defaultMainAttack)),
                ("%TARGETS%", GetReachCount(defaultMainAttack))
            })
            ;
    }

    /// <summary>
    /// Получить описание основной атаки.
    /// </summary>
    private TextContainer GetMainAttack(CalculatedUnitAttack mainAttack, CalculatedUnitAttack? alternativeAttack)
    {
        if (alternativeAttack == null)
            return new TextContainer(mainAttack.Description);

        return new TextContainer(
            new[] { new TextPiece(mainAttack.Description) }
                .Concat(_textProvider.GetText("X005TA0459").TextPieces) // Строка вида " or " / " или ".
                .Append(new TextPiece(alternativeAttack.Description))
                .ToArray());
    }

    /// <summary>
    /// Получить урон юнита.
    /// </summary>
    private static TextContainer GetDamage(CalculatedUnitAttack mainAttack, CalculatedUnitAttack? secondaryAttack)
    {
        var mainAttackPower = GetValueWithModifier(
            GetAttackPower(mainAttack.BasePower, mainAttack.AttackType),
            mainAttack.PowerBonus);

        if (secondaryAttack != null &&
            secondaryAttack.AttackType is not UnitAttackType.ReduceDamage
                and not UnitAttackType.ReduceInitiative
                and not UnitAttackType.ReduceArmor)
        {
            var secondaryAttackPower = GetValueWithModifier(
                GetAttackPower(secondaryAttack.BasePower, secondaryAttack.AttackType),
                0);
            return new TextContainer(mainAttackPower
                .TextPieces
                .Append(new TextPiece($" / {secondaryAttackPower}"))
                .ToArray());
        }

        return mainAttackPower;
    }

    /// <summary>
    /// Получить строковое значение силы атаки.
    /// </summary>
    private static string GetAttackPower(int power, UnitAttackType unitAttackType)
    {
        switch (unitAttackType)
        {
            case UnitAttackType.IncreaseDamage:
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
                new TextPiece(new TextStyle { ForegroundColor = BattleColors.Boost }, $" + {modifier}")
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
    private TextContainer GetUnitEffectTitle(CalculatedUnitAttack unitAttack)
    {
        if (unitAttack.AttackType == UnitAttackType.Heal)
            return _textProvider.GetText("X005TA0504");

        if (unitAttack.AttackType == UnitAttackType.IncreaseDamage)
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
            UnitAttackType.DrainLife => "X005TA0792",
            UnitAttackType.Paralyze => "X005TA0789",
            UnitAttackType.Heal => "X005TA0802",
            UnitAttackType.Fear => "X005TA0794",
            UnitAttackType.IncreaseDamage => "X005TA0795",
            UnitAttackType.Petrify => "X005TA0790",
            UnitAttackType.ReduceDamage => "X005TA0796",
            UnitAttackType.ReduceInitiative => "X005TA0797",
            UnitAttackType.Poison => "X005TA0798",
            UnitAttackType.Frostbite => "X005TA0799",
            UnitAttackType.Revive => "X005TA0800",
            UnitAttackType.DrainLifeOverflow => "X005TA0801",
            UnitAttackType.Cure => "X005TA0793",
            UnitAttackType.Summon => "X005TA0803",
            UnitAttackType.ReduceLevel => "X005TA0804",
            UnitAttackType.GiveAdditionalAttack => "X005TA0805",
            UnitAttackType.Doppelganger => "X005TA0806",
            UnitAttackType.TransformSelf => "X005TA0807",
            UnitAttackType.TransformEnemy => "X005TA0808",
            UnitAttackType.Blister => "X160TA0012",
            UnitAttackType.GiveProtection => "X160TA0014",
            UnitAttackType.ReduceArmor => "X160TA0020",
            _ => throw new ArgumentOutOfRangeException(nameof(attackType), attackType, null)
        };

        return _textProvider.GetText(attackTypeTextId);
    }

    /// <summary>
    /// Получить наименование, каких юнитов относительного своего расположения можно атаковать.
    /// </summary>
    private TextContainer GetReachTitle(CalculatedUnitAttack unitAttack)
    {
        if (unitAttack.Reach == UnitAttackReach.Adjacent)
            return _textProvider.GetText("X005TA0201");

        return _textProvider.GetText("X005TA0200");
    }

    /// <summary>
    /// Получить количество целей для атаки.
    /// </summary>
    private static TextContainer GetReachCount(CalculatedUnitAttack unitAttack)
    {
        if (unitAttack.Reach == UnitAttackReach.All)
            return new TextContainer("6");

        return new TextContainer("1");
    }
}