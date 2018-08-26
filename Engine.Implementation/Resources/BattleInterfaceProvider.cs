using System;
using System.Collections.Generic;
using System.IO;

using Avalonia.Media.Imaging;

using Engine.Battle.Providers;
using Engine.Common.Enums;
using Engine.Common.Models;
using Engine.Implementation.Helpers;
using ResourceProvider;

namespace Engine.Implementation.Resources
{
    public class BattleInterfaceProvider : IBattleInterfaceProvider
    {
        private readonly IBattleResourceProvider _battleResourceProvider;
        private readonly ImagesExtractor _extractor;

        public BattleInterfaceProvider(IBattleResourceProvider battleResourceProvider)
        {
            _battleResourceProvider = battleResourceProvider;
            _extractor = new ImagesExtractor($"{Directory.GetCurrentDirectory()}\\Resources\\interf\\Interf.ff");

            LoadBitmaps();
        }

        private void LoadBitmaps()
        {
            Battleground = _battleResourceProvider.GetRandomBattleground();

            RightPanel = _extractor.GetImage("DLG_BATTLE_A_RUNITGROUP").ToBitmap();

            BottomPanel = _extractor.GetImage("DLG_BATTLE_A_MAINCOMBATBG").ToBitmap();

            PanelSeparator = _extractor.GetImage("SPLITLRG_BATTLE").ToBitmap();

            // todo Хоть убейте, не могу найти эту картинку в ресурсах игры. Скачал другую где-то на просторах интернета
            DeathSkull = new Bitmap($"{Directory.GetCurrentDirectory()}\\Resources\\Common\\Skull.png");


            ToggleRightButton = GetBattleBitmaps("TOGGLERIGHT");
            DefendButton = GetBattleBitmaps("DEFEND");
            RetreatButton = GetBattleBitmaps("RETREAT");
            WaitButton = GetBattleBitmaps("WAIT");
            InstantResolveButton = GetBattleBitmaps("INSTANTRESOLVE");
            AutoBattleButton = GetBattleBitmaps("AUTOB");
        }


        public Bitmap Battleground { get; private set; }

        public Bitmap RightPanel { get; private set; }

        public Bitmap BottomPanel { get; private set; }

        public Bitmap PanelSeparator { get; private set; }

        public Bitmap DeathSkull { get; private set; }


        #region Buttons

        public IDictionary<ButtonState, Bitmap> ToggleRightButton { get; private set; }

        public IDictionary<ButtonState, Bitmap> DefendButton { get; private set; }

        public IDictionary<ButtonState, Bitmap> RetreatButton { get; private set; }

        public IDictionary<ButtonState, Bitmap> WaitButton { get; private set; }

        public IDictionary<ButtonState, Bitmap> InstantResolveButton { get; private set; }

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





        #region UnitPanelBorders

        public IReadOnlyList<Frame> GetUnitAttackBorder(bool sizeSmall)
        {
            return GetAttackBorder(sizeSmall ? BattleBorderType.SmallUnit : BattleBorderType.LargeUnit);
        }

        public IReadOnlyList<Frame> GetFieldAttackBorder()
        {
            return GetAttackBorder(BattleBorderType.Field);
        }

        private IReadOnlyList<Frame> GetAttackBorder(BattleBorderType battleBorderType)
        {
            var suffix = GetBattleBorderTypeSuffix(battleBorderType);

            // todo Выяснить в чем отличие от SELALLA
            return _battleResourceProvider.GetBattleAnimation($"ISEL{suffix}A");
        }


        public IReadOnlyList<Frame> GetUnitSelectionBorder(bool sizeSmall)
        {
            return GetSelectionBorder(sizeSmall ? BattleBorderType.SmallUnit : BattleBorderType.LargeUnit);
        }

        private IReadOnlyList<Frame> GetSelectionBorder(BattleBorderType battleBorderType)
        {
            if (battleBorderType == BattleBorderType.Field)
                throw new ArgumentException("Selection border can not use for field", nameof(battleBorderType));

            var suffix = GetBattleBorderTypeSuffix(battleBorderType);
            return _battleResourceProvider.GetBattleAnimation($"PLY{suffix}A");
        }


        public IReadOnlyList<Frame> GetUnitHealBorder(bool sizeSmall)
        {
            return GetHealBorder(sizeSmall ? BattleBorderType.SmallUnit : BattleBorderType.LargeUnit);
        }

        public IReadOnlyList<Frame> GetFieldHealBorder()
        {
            return GetHealBorder(BattleBorderType.Field);
        }

        private IReadOnlyList<Frame> GetHealBorder(BattleBorderType battleBorderType)
        {
            var suffix = GetBattleBorderTypeSuffix(battleBorderType);
            return _battleResourceProvider.GetBattleAnimation($"HEA{suffix}A");
        }


        /// <summary>
        /// Получить часть названия файла по типу рамки
        /// </summary>
        private static string GetBattleBorderTypeSuffix(BattleBorderType battleBorderType)
        {
            switch (battleBorderType) {
                case BattleBorderType.SmallUnit:
                    return "SMALL";
                case BattleBorderType.LargeUnit:
                    return "LARGE";
                case BattleBorderType.Field:
                    return "ALL";
                default:
                    throw new ArgumentOutOfRangeException(nameof(battleBorderType), battleBorderType, null);
            }
        }

        private enum BattleBorderType
        {
            /// <summary>
            /// Тип рамки для юнита, занимающего одну клетку
            /// </summary>
            SmallUnit,

            /// <summary>
            /// Тип рамки для юнита, занимающего две клетки
            /// </summary>
            LargeUnit,

            /// <summary>
            /// Тип рамки, когда юнит атакует всё поле
            /// </summary>
            Field
        }

        #endregion
    }
}
