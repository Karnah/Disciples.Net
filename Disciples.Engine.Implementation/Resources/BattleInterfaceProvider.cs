using System;
using System.Collections.Generic;
using System.IO;

using Disciples.Engine.Battle.Enums;
using Disciples.Engine.Battle.Providers;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Implementation.Helpers;
using Disciples.Engine.Platform.Factories;
using Disciples.ResourceProvider;
using Disciples.ResourceProvider.Models;

namespace Disciples.Engine.Implementation.Resources
{
    /// <inheritdoc />
    public class BattleInterfaceProvider : IBattleInterfaceProvider
    {
        private readonly IBattleResourceProvider _battleResourceProvider;
        private readonly IBitmapFactory _bitmapFactory;
        private readonly ImagesExtractor _extractor;
        private readonly IDictionary<string, RawBitmap> _battleIcons;

        private Dictionary<GameColor, IBitmap> _gameColors;

        /// <inheritdoc />
        public BattleInterfaceProvider(IBattleResourceProvider battleResourceProvider, IBitmapFactory bitmapFactory)
        {
            _battleResourceProvider = battleResourceProvider;
            _bitmapFactory = bitmapFactory;
            _extractor = new ImagesExtractor($"{Directory.GetCurrentDirectory()}\\Resources\\interf\\Interf.ff");
            _battleIcons = _extractor.GetImageParts("DLG_BATTLE_A.PNG");

            LoadBitmaps();
            InitGameColors();
        }

        /// <summary>
        /// Инициализировать картинки.
        /// </summary>
        private void LoadBitmaps()
        {
            Battleground = _battleResourceProvider.GetRandomBattleground();
            RightPanel = _bitmapFactory.FromRawToBitmap(_battleIcons["DLG_BATTLE_A_RUNITGROUP"]);
            BottomPanel = _bitmapFactory.FromRawToBitmap(_battleIcons["DLG_BATTLE_A_MAINCOMBATBG"]);
            PanelSeparator = _bitmapFactory.FromRawToBitmap(_battleIcons["DLG_BATTLE_A_SPLITLRG"]);
            // todo Не смог найти эту картинку в ресурсах игры. Скачал другую где-то на просторах интернета.
            DeathSkull = _bitmapFactory.FromFile(Path.Combine(Directory.GetCurrentDirectory(), "Resources\\Common\\Skull.png"));
            UnitInfoBackground = _bitmapFactory.FromRawToBitmap(_extractor.GetImage("_PG0500IX"));

            UnitBattleEffectsIcon = new Dictionary<UnitBattleEffectType, IBitmap> {
                { UnitBattleEffectType.Defend, _battleResourceProvider.GetBattleFrame("FIDEFENDING").Bitmap }
            };

            ToggleRightButton = GetButtonBitmaps("TOGGLERIGHT");
            DefendButton = GetButtonBitmaps("DEFEND");
            RetreatButton = GetButtonBitmaps("RETREAT");
            WaitButton = GetButtonBitmaps("WAIT");
            InstantResolveButton = GetButtonBitmaps("INSTANTRESOLVE");
            AutoBattleButton = GetButtonBitmaps("AUTOB");
        }


        /// <inheritdoc />
        public IReadOnlyList<IBitmap> Battleground { get; private set; }

        /// <inheritdoc />
        public IBitmap RightPanel { get; private set; }

        /// <inheritdoc />
        public IBitmap BottomPanel { get; private set; }

        /// <inheritdoc />
        public IBitmap PanelSeparator { get; private set; }

        /// <inheritdoc />
        public IBitmap DeathSkull { get; private set; }

        /// <inheritdoc />
        public IBitmap UnitInfoBackground { get; private set; }


        /// <inheritdoc />
        public IDictionary<UnitBattleEffectType, IBitmap> UnitBattleEffectsIcon { get; private set; }


        #region Buttons

        /// <inheritdoc />
        public IDictionary<ButtonState, IBitmap> ToggleRightButton { get; private set; }

        /// <inheritdoc />
        public IDictionary<ButtonState, IBitmap> DefendButton { get; private set; }

        /// <inheritdoc />
        public IDictionary<ButtonState, IBitmap> RetreatButton { get; private set; }

        /// <inheritdoc />
        public IDictionary<ButtonState, IBitmap> WaitButton { get; private set; }

        /// <inheritdoc />
        public IDictionary<ButtonState, IBitmap> InstantResolveButton { get; private set; }

        /// <inheritdoc />
        public IDictionary<ButtonState, IBitmap> AutoBattleButton { get; private set; }


        /// <summary>
        /// Получить словарь с изображениями кнопки для каждого её состояния.
        /// </summary>
        /// <param name="buttonName">Имя кнопки.</param>
        private IDictionary<ButtonState, IBitmap> GetButtonBitmaps(string buttonName)
        {
            return new Dictionary<ButtonState, IBitmap> {
                { ButtonState.Disabled, _bitmapFactory.FromRawToBitmap(_battleIcons[$"DLG_BATTLE_A_{buttonName}_D"]) },
                { ButtonState.Active, _bitmapFactory.FromRawToBitmap(_battleIcons[$"DLG_BATTLE_A_{buttonName}_N"]) },
                { ButtonState.Selected,_bitmapFactory.FromRawToBitmap(_battleIcons[$"DLG_BATTLE_A_{buttonName}_H"]) },
                { ButtonState.Pressed, _bitmapFactory.FromRawToBitmap(_battleIcons[$"DLG_BATTLE_A_{buttonName}_C"]) }
            };
        }

        #endregion

        #region GameColors

        /// <inheritdoc />
        public IBitmap GetColorBitmap(GameColor color)
        {
            return _gameColors[color];
        }

        /// <summary>
        /// Инициализировать цвета приложения.
        /// </summary>
        private void InitGameColors()
        {
            var gameColors = new Dictionary<GameColor, IBitmap>();

            foreach (GameColor color in Enum.GetValues(typeof(GameColor))) {
                var colorFilePath = $"Resources/Colors/{color}.png";
                if (!File.Exists(colorFilePath))
                    gameColors.Add(color, null);

                var bitmap = _bitmapFactory.FromFile(colorFilePath);
                gameColors.Add(color, bitmap);
            }

            _gameColors = gameColors;
        }

        // todo Так как наблюдаются проблемы со Skia, то генерировать во время выполнения так цвета - не вариант.
        // Используем вариант с загрузкой.
        //private void InitGameColors()
        //{
        //    var gameColors = new Dictionary<GameColor, IBitmap>();

        //    foreach (GameColor color in Enum.GetValues(typeof(GameColor))) {
        //        byte[] colorBytes = new byte[4];

        //        switch (color) {
        //            case GameColor.Red:
        //                colorBytes = new byte[] { 255, 0, 0, 128 };
        //                break;
        //            case GameColor.Gray:
        //                break;
        //            case GameColor.Green:
        //                break;
        //            case GameColor.Yellow:
        //                colorBytes = new byte[] { 255, 255, 0, 128 };
        //                break;
        //            case GameColor.Blue:
        //                colorBytes = new byte[] { 0, 0, 255, 128 };
        //                break;
        //            case GameColor.Black:
        //                colorBytes = new byte[] { 0, 0, 0, 255 };
        //                break;
        //            case GameColor.White:
        //                colorBytes = new byte[] { 255, 255, 255, 255 };
        //                break;
        //            default:
        //                throw new ArgumentOutOfRangeException();
        //        }

        //        var rawBitmap = new RawBitmap(0, 1, 0, 1, 1, 1, colorBytes);
        //        var bitmap = _bitmapFactory.FromRawToBitmap(rawBitmap);
        //        gameColors.Add(color, bitmap);

        //        //_bitmapFactory.SaveToFile(bitmap, $"Resources/Colors/{color}.png");
        //    }

        //    _gameColors = gameColors;
        //}

        #endregion




        #region UnitPanelBorders

        /// <inheritdoc />
        public IReadOnlyList<Frame> GetUnitAttackBorder(bool sizeSmall)
        {
            return GetAttackBorder(sizeSmall ? BattleBorderSize.SmallUnit : BattleBorderSize.LargeUnit);
        }

        /// <inheritdoc />
        public IReadOnlyList<Frame> GetFieldAttackBorder()
        {
            return GetAttackBorder(BattleBorderSize.Field);
        }

        /// <summary>
        /// Извлечь из ресурсов рамку атаки.
        /// </summary>
        /// <param name="battleBorderSize">Размер рамки.</param>
        private IReadOnlyList<Frame> GetAttackBorder(BattleBorderSize battleBorderSize)
        {
            var suffix = GetBattleBorderTypeSuffix(battleBorderSize);

            // todo Выяснить в чем отличие от SELALLA
            return _battleResourceProvider.GetBattleAnimation($"ISEL{suffix}A");
        }


        /// <inheritdoc />
        public IReadOnlyList<Frame> GetUnitSelectionBorder(bool sizeSmall)
        {
            return GetSelectionBorder(sizeSmall ? BattleBorderSize.SmallUnit : BattleBorderSize.LargeUnit);
        }

        /// <summary>
        /// Извлечь из ресурсов рамку выделения текущего юнита.
        /// </summary>
        /// <param name="battleBorderSize">Размер рамки.</param>
        private IReadOnlyList<Frame> GetSelectionBorder(BattleBorderSize battleBorderSize)
        {
            if (battleBorderSize == BattleBorderSize.Field)
                throw new ArgumentException("Selection border can not use for field", nameof(battleBorderSize));

            var suffix = GetBattleBorderTypeSuffix(battleBorderSize);
            return _battleResourceProvider.GetBattleAnimation($"PLY{suffix}A");
        }


        /// <inheritdoc />
        public IReadOnlyList<Frame> GetUnitHealBorder(bool sizeSmall)
        {
            return GetHealBorder(sizeSmall ? BattleBorderSize.SmallUnit : BattleBorderSize.LargeUnit);
        }

        /// <inheritdoc />
        public IReadOnlyList<Frame> GetFieldHealBorder()
        {
            return GetHealBorder(BattleBorderSize.Field);
        }

        /// <summary>
        /// Извлечь из ресурсов рамку исцеления.
        /// </summary>
        /// <param name="battleBorderSize">Размер рамки.</param>
        private IReadOnlyList<Frame> GetHealBorder(BattleBorderSize battleBorderSize)
        {
            var suffix = GetBattleBorderTypeSuffix(battleBorderSize);
            return _battleResourceProvider.GetBattleAnimation($"HEA{suffix}A");
        }


        /// <summary>
        /// Получить часть названия файла по размеру рамки.
        /// </summary>
        private static string GetBattleBorderTypeSuffix(BattleBorderSize battleBorderSize)
        {
            switch (battleBorderSize) {
                case BattleBorderSize.SmallUnit:
                    return "SMALL";
                case BattleBorderSize.LargeUnit:
                    return "LARGE";
                case BattleBorderSize.Field:
                    return "ALL";
                default:
                    throw new ArgumentOutOfRangeException(nameof(battleBorderSize), battleBorderSize, null);
            }
        }

        /// <summary>
        /// Размер рамки на панели с юнитами.
        /// </summary>
        private enum BattleBorderSize
        {
            /// <summary>
            /// Рамка занимает одну клетку.
            /// </summary>
            SmallUnit,

            /// <summary>
            /// Рамка занимает две клетки.
            /// </summary>
            LargeUnit,

            /// <summary>
            /// Рамка занимает всю панель целиком.
            /// </summary>
            Field
        }

        #endregion
    }
}