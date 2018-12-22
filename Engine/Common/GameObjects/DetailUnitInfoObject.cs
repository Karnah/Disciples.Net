using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Avalonia.Media;

using Engine.Battle.Providers;
using Engine.Common.Controllers;
using Engine.Common.Enums.Units;
using Engine.Common.Models;
using Engine.Common.Providers;
using Engine.Common.VisualObjects;

namespace Engine.Common.GameObjects
{
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

        private readonly IVisualSceneController _visualSceneController;
        private readonly IBattleInterfaceProvider _battleInterfaceProvider;
        private readonly ITextProvider _textProvider;
        private readonly List<TextVisualObject> _unitInfo;

        private ImageVisualObject _unitInfoBackground;
        private ImageVisualObject _unitPortret;
        private TextVisualObject _unitName;
        private TextVisualObject _unitDescription;

        /// <inheritdoc />
        public DetailUnitInfoObject(
            IVisualSceneController visualSceneController,
            IBattleInterfaceProvider battleInterfaceProvider,
            ITextProvider textProvider,
            Unit unit)
        {
            _visualSceneController = visualSceneController;
            _battleInterfaceProvider = battleInterfaceProvider;
            _textProvider = textProvider;

            _unitInfo = new List<TextVisualObject>();

            X = ((double) GameInfo.OriginalWidth - _battleInterfaceProvider.UnitInfoBackground.PixelSize.Width) / 2;
            Y = ((double) GameInfo.OriginalHeight - _battleInterfaceProvider.UnitInfoBackground.PixelSize.Height) / 2;

            Unit = unit;
        }


        /// <summary>
        /// Юнит, о котором выводится информация.
        /// </summary>
        public Unit Unit { get; }

        /// <inheritdoc />
        public override bool IsInteractive => false;


        /// <inheritdoc />
        public override void OnInitialize()
        {
            base.OnInitialize();

            _unitInfoBackground = _visualSceneController.AddImageVisual(
                _battleInterfaceProvider.UnitInfoBackground, X, Y, INTERFACE_LAYER);

            _unitPortret = _visualSceneController.AddImageVisual(
                Unit.UnitType.Portrait,
                X + 70,
                Y + 10,
                INTERFACE_LAYER + 1
            );
            _unitName = _visualSceneController.AddTextVisual(
                Unit.UnitType.Name, 11, X + 110, Y + 440, INTERFACE_LAYER + 1, 260, TextAlignment.Center, true);
            _unitDescription = _visualSceneController.AddTextVisual(
                Unit.UnitType.Description, 11, X + 110, Y + 440 + ROW_HEIGHT, INTERFACE_LAYER + 1, 260, TextAlignment.Left);

            _unitInfo.AddRange(GetUnitBaseInfo(UNIT_BASE_INFO_ID, 60, out var _));
            _unitInfo.AddRange(GetUnitBaseInfo(UNIT_ATTACK_INFO_FIRST_PART_ID, 200, out var endVerticalOffset));
            _unitInfo.AddRange(GetUnitBaseInfo(UNIT_ATTACK_INFO_SECOND_PART_ID, endVerticalOffset, out var _));
        }

        /// <summary>
        /// Получить информацию о юните в виде объектов на сцене.
        /// </summary>
        /// <param name="textId">Идентификатор текста-паттерна в ресурсах.</param>
        /// <param name="verticalOffset">Расстояние по вертикали, с которого необходимо размещать текст.</param>
        /// <param name="endVerticalOffset">Расстояние до строки, где можно размещать текст ниже.</param>
        private IReadOnlyList<TextVisualObject> GetUnitBaseInfo(string textId, int verticalOffset, out int endVerticalOffset)
        {
            var result = new List<TextVisualObject>();
            var text = _textProvider.GetText(textId);

            endVerticalOffset = verticalOffset;
            var rows = Regex.Matches(text, @"(?<Title>[%\w ]+:)\\t(?:(?<Value1>[\w\W]+?)(?:\\n)|(?<Value2>%[\w]+%))");
            foreach (Match row in rows) {
                var titlePattern = row.Groups["Title"].Value;
                var title = ReplacePlaceholders(titlePattern.Trim());
                var titleObject = _visualSceneController.AddTextVisual(
                    title, 11, X + 400, Y + endVerticalOffset, INTERFACE_LAYER + 1, true);
                result.Add(titleObject);

                var valuePattern1 = row.Groups["Value1"].Value;
                var valuePattern2 = row.Groups["Value2"].Value;
                var valuePattern = string.IsNullOrEmpty(valuePattern1)
                    ? valuePattern2
                    : valuePattern1;
                var value = ReplacePlaceholders(valuePattern);
                var valueObject = _visualSceneController.AddTextVisual(
                    value, 11, X + 510, Y + endVerticalOffset, INTERFACE_LAYER + 1, 120, TextAlignment.Left);
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

                .Replace("%TWICE%", Unit.UnitType.AttackTwice ? "(2x) " : string.Empty)
                .Replace("%ALTATTACK%", string.Empty) // todo Что это?
                .Replace("%ATTACK%", Unit.UnitType.FirstAttack.Description)
                .Replace("%SECOND%", Unit.UnitType.SecondAttack == null ? string.Empty : $" / {Unit.UnitType.SecondAttack.Description}")
                .Replace("%HIT%", $"{GetValueWithModifier(Unit.BaseFirstAttackAccuracy, Unit.FirstAttackAccuracyModifier)}%")
                .Replace("%HIT2%", Unit.SecondAttackAccuracy == null ? string.Empty : $" / {Unit.SecondAttackAccuracy}%")

                .Replace("%EFFECT%", GetUnitEffectTitle())
                .Replace("%DAMAGE%", GetValueWithModifier(Unit.BaseFirstAttackPower, Unit.FirstAttackPowerModifier)
                                     + (Unit.SecondAttackPower > 0 ? $" / {Unit.SecondAttackPower}" : string.Empty))
                .Replace("%SOURCE%", GetAttackSourceTitle(Unit.UnitType.FirstAttack.AttackSource))
                .Replace("%SOURCE2%", Unit.UnitType.SecondAttack == null ? string.Empty : $" / {GetAttackSourceTitle(Unit.UnitType.FirstAttack.AttackSource)}")
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
            if (Unit.UnitType.FirstAttack.AttackClass == AttackClass.Heal)
                return _textProvider.GetText("X005TA0504");

            if (Unit.UnitType.FirstAttack.AttackClass == AttackClass.BoostDamage)
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
            if (Unit.UnitType.FirstAttack.Reach == Reach.Adjacent)
                return _textProvider.GetText("X005TA0201");

            return _textProvider.GetText("X005TA0200");
        }

        /// <summary>
        /// Получить количество целей для атаки.
        /// </summary>
        private string GetReachCount()
        {
            if (Unit.UnitType.FirstAttack.Reach == Reach.All)
                return "6";

            return "1";
        }

        /// <inheritdoc />
        public override void Destroy()
        {
            base.Destroy();

            _visualSceneController.RemoveVisualObject(_unitInfoBackground);
            _visualSceneController.RemoveVisualObject(_unitPortret);
            _visualSceneController.RemoveVisualObject(_unitName);
            _visualSceneController.RemoveVisualObject(_unitDescription);
            foreach (var unitInfo in _unitInfo) {
                _visualSceneController.RemoveVisualObject(unitInfo);
            }
        }
    }
}