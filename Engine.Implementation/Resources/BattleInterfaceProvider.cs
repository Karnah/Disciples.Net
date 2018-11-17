using System;
using System.Collections.Generic;
using System.IO;

using Avalonia.Media.Imaging;

using Engine.Battle.Enums;
using Engine.Battle.Providers;
using Engine.Common.Enums;
using Engine.Common.Models;
using Engine.Implementation.Helpers;
using ResourceProvider;

namespace Engine.Implementation.Resources
{
    /// <inheritdoc />
    public class BattleInterfaceProvider : IBattleInterfaceProvider
    {
        private readonly IBattleResourceProvider _battleResourceProvider;
        private readonly ImagesExtractor _extractor;

        private Dictionary<GameColor, Bitmap> _gameColors;

        /// <inheritdoc />
        public BattleInterfaceProvider(IBattleResourceProvider battleResourceProvider)
        {
            _battleResourceProvider = battleResourceProvider;
            _extractor = new ImagesExtractor($"{Directory.GetCurrentDirectory()}\\Resources\\interf\\Interf.ff");

            LoadBitmaps();
            InitGameColors();
        }

        /// <summary>
        /// Инициализировать картинки.
        /// </summary>
        private void LoadBitmaps()
        {
            Battleground = _battleResourceProvider.GetRandomBattleground();

            RightPanel = _extractor.GetImage("DLG_BATTLE_A_RUNITGROUP").ToBitmap();

            BottomPanel = _extractor.GetImage("DLG_BATTLE_A_MAINCOMBATBG").ToBitmap();

            PanelSeparator = _extractor.GetImage("SPLITLRG_BATTLE").ToBitmap();

            // todo Хоть убейте, не могу найти эту картинку в ресурсах игры. Скачал другую где-то на просторах интернета.
            DeathSkull = new Bitmap($"{Directory.GetCurrentDirectory()}\\Resources\\Common\\Skull.png");


            UnitButtleEffectsIcon = new Dictionary<UnitBattleEffectType, Bitmap> {
                { UnitBattleEffectType.Defend, _battleResourceProvider.GetBattleFrame("FIDEFENDING").Bitmap }
            };


            ToggleRightButton = GetBattleBitmaps("TOGGLERIGHT");
            DefendButton = GetBattleBitmaps("DEFEND");
            RetreatButton = GetBattleBitmaps("RETREAT");
            WaitButton = GetBattleBitmaps("WAIT");
            InstantResolveButton = GetBattleBitmaps("INSTANTRESOLVE");
            AutoBattleButton = GetBattleBitmaps("AUTOB");
        }


        /// <inheritdoc />
        public Bitmap Battleground { get; private set; }

        /// <inheritdoc />
        public Bitmap RightPanel { get; private set; }

        /// <inheritdoc />
        public Bitmap BottomPanel { get; private set; }

        /// <inheritdoc />
        public Bitmap PanelSeparator { get; private set; }

        /// <inheritdoc />
        public Bitmap DeathSkull { get; private set; }


        /// <inheritdoc />
        public IDictionary<UnitBattleEffectType, Bitmap> UnitButtleEffectsIcon { get; private set; }


        #region Buttons

        /// <inheritdoc />
        public IDictionary<ButtonState, Bitmap> ToggleRightButton { get; private set; }

        /// <inheritdoc />
        public IDictionary<ButtonState, Bitmap> DefendButton { get; private set; }

        /// <inheritdoc />
        public IDictionary<ButtonState, Bitmap> RetreatButton { get; private set; }

        /// <inheritdoc />
        public IDictionary<ButtonState, Bitmap> WaitButton { get; private set; }

        /// <inheritdoc />
        public IDictionary<ButtonState, Bitmap> InstantResolveButton { get; private set; }

        /// <inheritdoc />
        public IDictionary<ButtonState, Bitmap> AutoBattleButton { get; private set; }


        private IDictionary<ButtonState, Bitmap> GetBattleBitmaps(string buttonName)
        {
            return new Dictionary<ButtonState, Bitmap> {
                { ButtonState.Disabled, _extractor.GetImage($"DLG_BATTLE_A_{buttonName}_D").ToBitmap() },
                { ButtonState.Active, _extractor.GetImage($"DLG_BATTLE_A_{buttonName}_N").ToBitmap() },
                { ButtonState.Selected, _extractor.GetImage($"DLG_BATTLE_A_{buttonName}_H").ToBitmap() },
                { ButtonState.Pressed, _extractor.GetImage($"DLG_BATTLE_A_{buttonName}_C").ToBitmap() }
            };
        }

        #endregion

        #region GameColors

        /// <inheritdoc />
        public Bitmap GetColorBitmap(GameColor color)
        {
            return _gameColors[color];
        }

        /// <summary>
        /// Инициализировать цвета приложения.
        /// </summary>
        private void InitGameColors()
        {
            var gameColors = new Dictionary<GameColor, Bitmap>();

            foreach (GameColor color in Enum.GetValues(typeof(GameColor))) {
                var colorFilePath = $"Resources/Colors/{color}.png";
                if (!File.Exists(colorFilePath))
                    gameColors.Add(color, null);

                var bitmap = new Bitmap(colorFilePath);
                gameColors.Add(color, bitmap);
            }

            _gameColors = gameColors;
        }

        // todo Так как наблюдаются проблемы со Skia, то генерировать во время выполнения так цвета - не вариант.
        // Используем вариант с загрузкой.
        //private void InitGameColors()
        //{
        //    var gameColors = new Dictionary<GameColor, Bitmap>();

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
        //            default:
        //                throw new ArgumentOutOfRangeException();
        //        }

        //        var bitmap = GetColorBitmap(colorBytes);
        //        bitmap.Save($"Resources/Colors/{color}.png");
        //        gameColors.Add(color, bitmap);
        //    }

        //    _gameColors = gameColors;
        //}

        //private static Bitmap GetColorBitmap(byte[] colorBytes)
        //{
        //    var rowImage = new ResourceProvider.Models.RowImage(0, 1, 0, 1, 1, 1, colorBytes);
        //    var bitmap = rowImage.ToBitmap();

        //    return bitmap;
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
