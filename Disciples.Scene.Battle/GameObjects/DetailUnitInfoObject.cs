using System.Text.RegularExpressions;
using Disciples.Engine;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Common.SceneObjects;

namespace Disciples.Scene.Battle.GameObjects;

/// <summary>
/// Объект, который выводит на сцену детальную информацию о юните.
/// </summary>
public class DetailUnitInfoObject : GameObject
{
    /// <summary>
    /// Слой, на котором располагается информация о юните.
    /// </summary>
    private const int INTERFACE_LAYER = 2000;
    /// <summary>
    /// Высота строк.
    /// </summary>
    private const int ROW_HEIGHT = 17;
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

    private readonly ISceneController _sceneController;
    private readonly IBattleInterfaceProvider _battleInterfaceProvider;
    private readonly ITextProvider _textProvider;
    private readonly List<ITextSceneObject> _unitInfo;

    private IImageSceneObject _unitInfoBackground;
    private IImageSceneObject _unitPortrait;
    private ITextSceneObject _unitName;
    private ITextSceneObject _unitDescription;

    /// <inheritdoc />
    public DetailUnitInfoObject(
        ISceneController sceneController,
        IBattleInterfaceProvider battleInterfaceProvider,
        ITextProvider textProvider,
        Unit unit)
    {
        _sceneController = sceneController;
        _battleInterfaceProvider = battleInterfaceProvider;
        _textProvider = textProvider;

        _unitInfo = new List<ITextSceneObject>();

        X = (GameInfo.OriginalWidth - _battleInterfaceProvider.UnitInfoBackground.Width) / 2;
        Y = (GameInfo.OriginalHeight - _battleInterfaceProvider.UnitInfoBackground.Height) / 2;

        Unit = unit;
    }


    /// <summary>
    /// Юнит, о котором выводится информация.
    /// </summary>
    public Unit Unit { get; }

    /// <inheritdoc />
    public override bool IsInteractive => false;


    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        _unitInfoBackground = _sceneController.AddImage(
            _battleInterfaceProvider.UnitInfoBackground, X, Y, INTERFACE_LAYER);

        _unitPortrait = _sceneController.AddImage(
            Unit.UnitType.Portrait,
            X + 70,
            Y + 10,
            INTERFACE_LAYER + 1
        );
        _unitName = _sceneController.AddText(
            Unit.UnitType.Name, 11, X + 110, Y + 440, INTERFACE_LAYER + 1, 260, TextAlignment.Center, true);
        _unitDescription = _sceneController.AddText(
            Unit.UnitType.Description, 11, X + 110, Y + 440 + ROW_HEIGHT, INTERFACE_LAYER + 1, 260, TextAlignment.Left);

        _unitInfo.AddRange(GetUnitBaseInfo(UNIT_BASE_INFO_ID, 60, out _));
        _unitInfo.AddRange(GetUnitBaseInfo(UNIT_ATTACK_INFO_FIRST_PART_ID, 200, out var endVerticalOffset));
        _unitInfo.AddRange(GetUnitBaseInfo(UNIT_ATTACK_INFO_SECOND_PART_ID, endVerticalOffset, out _));
    }

    /// <summary>
    /// Получить информацию о юните в виде объектов на сцене.
    /// </summary>
    /// <param name="textId">Идентификатор текста-паттерна в ресурсах.</param>
    /// <param name="verticalOffset">Расстояние по вертикали, с которого необходимо размещать текст.</param>
    /// <param name="endVerticalOffset">Расстояние до строки, где можно размещать текст ниже.</param>
    private IReadOnlyList<ITextSceneObject> GetUnitBaseInfo(string textId, int verticalOffset, out int endVerticalOffset)
    {
        var result = new List<ITextSceneObject>();
        var text = _textProvider.GetText(textId);

        endVerticalOffset = verticalOffset;
        var rows = Regex.Matches(text, @"(?<Title>[%\w ]+:)\\t(?:(?<Value1>[\w\W]+?)(?:\\n)|(?<Value2>%[\w]+%))");
        foreach (Match row in rows) {
            var titlePattern = row.Groups["Title"].Value;
            var title = ReplacePlaceholders(titlePattern.Trim());
            var titleObject = _sceneController.AddText(
                title, 11, X + 395, Y + endVerticalOffset, INTERFACE_LAYER + 1, true);
            result.Add(titleObject);

            var valuePattern1 = row.Groups["Value1"].Value;
            var valuePattern2 = row.Groups["Value2"].Value;
            var valuePattern = string.IsNullOrEmpty(valuePattern1)
                ? valuePattern2
                : valuePattern1;
            var value = ReplacePlaceholders(valuePattern);
            var valueObject = _sceneController.AddText(
                value, 11, X + 500, Y + endVerticalOffset, INTERFACE_LAYER + 1, 130, TextAlignment.Left);
            result.Add(valueObject);

            endVerticalOffset += ROW_HEIGHT;

            // Если слишком длинная строка, то следующую располагаем ниже.
            if (value.Length > 20)
                endVerticalOffset += ROW_HEIGHT;
        }

        return result;
    }

    private string ReplacePlaceholders(string value)
    {
        // todo Модификаторы добавляются обычным цветом, а не зелёным/красном.
        // Пробовал сделать контрол, который будет принимать текст, содержащий теги разметки/размеры шрифтов/цвет шрифта,
        // Но он слишком медленно работал (StackPanel). Возможно, стоит дождаться TextBlock с поддержкой InlineUIContainer
        // https://github.com/AvaloniaUI/Avalonia/pull/1689.

        return value
            .Replace("%LEVEL%", Unit.Level.ToString())
            .Replace("%XP%", $"{Unit.Experience}/{Unit.UnitType.XpNext}")
            .Replace("%HP1%", Unit.HitPoints.ToString())
            .Replace("%HP2%", Unit.MaxHitPoints.ToString())
            .Replace("%ARMOR%", GetValueWithModifier(Unit.BaseArmor, Unit.ArmorModifier))
            .Replace("%IMMU%", "Нет") // todo Заполнить.
            .Replace("%WARD%", "Нет") // todo Заполнить.

            .Replace("%TWICE%", Unit.UnitType.IsAttackTwice ? "(2x) " : string.Empty)
            .Replace("%ALTATTACK%", string.Empty) // todo Что это?
            .Replace("%ATTACK%", Unit.UnitType.MainAttack.Description)
            .Replace("%SECOND%", Unit.UnitType.SecondaryAttack == null ? string.Empty : $" / {Unit.UnitType.SecondaryAttack.Description}")
            .Replace("%HIT%", $"{GetValueWithModifier(Unit.BaseFirstAttackAccuracy, Unit.FirstAttackAccuracyModifier)}%")
            .Replace("%HIT2%", Unit.SecondaryAttackAccuracy == null ? string.Empty : $" / {Unit.SecondaryAttackAccuracy}%")

            .Replace("%EFFECT%", GetUnitEffectTitle())
            .Replace("%DAMAGE%", GetValueWithModifier(Unit.BaseFirstAttackPower, Unit.FirstAttackPowerModifier)
                                 + (Unit.SecondAttackPower > 0 ? $" / {Unit.SecondAttackPower}" : string.Empty))
            .Replace("%SOURCE%", GetAttackSourceTitle(Unit.UnitType.MainAttack.AttackSource))
            .Replace("%SOURCE2%", Unit.UnitType.SecondaryAttack == null ? string.Empty : $" / {GetAttackSourceTitle(Unit.UnitType.MainAttack.AttackSource)}")
            .Replace("%INIT%", GetValueWithModifier(Unit.BaseInitiative, Unit.InitiativeModifier))
            .Replace("%REACH%", GetReachTitle())
            .Replace("%TARGETS%", GetReachCount());
    }

    /// <summary>
    /// Получить строковое значение характеристики и её модификатора.
    /// </summary>
    /// <param name="value">Значение характеристики.</param>
    /// <param name="modifier">Модификатор характеристики.</param>
    private static string GetValueWithModifier(int value, int modifier)
    {
        if (modifier == 0)
            return value.ToString();

        if (modifier > 0)
            return $"{value} + {modifier}";

        return $"{value} - {-modifier}";
    }

    /// <summary>
    /// Получить наименование класса атаки юнита.
    /// </summary>
    private string GetUnitEffectTitle()
    {
        if (Unit.UnitType.MainAttack.AttackClass == AttackClass.Heal)
            return _textProvider.GetText("X005TA0504");

        if (Unit.UnitType.MainAttack.AttackClass == AttackClass.BoostDamage)
            return _textProvider.GetText("X005TA0534");

        return _textProvider.GetText("X005TA0503");
    }

    /// <summary>
    /// Получить наименование источники атаки.
    /// </summary>
    private string GetAttackSourceTitle(AttackSource source)
    {
        string attackSourceId;

        switch (source) {
            case AttackSource.Weapon:
                attackSourceId = "X005TA0145";
                break;
            case AttackSource.Mind:
                attackSourceId = "X005TA0146";
                break;
            case AttackSource.Life:
                attackSourceId = "X005TA0147";
                break;
            case AttackSource.Death:
                attackSourceId = "X005TA0148";
                break;
            case AttackSource.Fire:
                attackSourceId = "X005TA0149";
                break;
            case AttackSource.Water:
                attackSourceId = "X005TA0150";
                break;
            case AttackSource.Earth:
                attackSourceId = "X005TA0152";
                break;
            case AttackSource.Air:
                attackSourceId = "X005TA0151";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(source), source, null);
        }

        return _textProvider.GetText(attackSourceId);
    }

    /// <summary>
    /// Получить наименование, каких юнитов относительного своего расположения можно атаковать.
    /// </summary>
    private string GetReachTitle()
    {
        if (Unit.UnitType.MainAttack.Reach == Reach.Adjacent)
            return _textProvider.GetText("X005TA0201");

        return _textProvider.GetText("X005TA0200");
    }

    /// <summary>
    /// Получить количество целей для атаки.
    /// </summary>
    private string GetReachCount()
    {
        if (Unit.UnitType.MainAttack.Reach == Reach.All)
            return "6";

        return "1";
    }

    /// <inheritdoc />
    public override void Destroy()
    {
        base.Destroy();

        _sceneController.RemoveSceneObject(_unitInfoBackground);
        _sceneController.RemoveSceneObject(_unitPortrait);
        _sceneController.RemoveSceneObject(_unitName);
        _sceneController.RemoveSceneObject(_unitDescription);
        foreach (var unitInfo in _unitInfo) {
            _sceneController.RemoveSceneObject(unitInfo);
        }
    }
}