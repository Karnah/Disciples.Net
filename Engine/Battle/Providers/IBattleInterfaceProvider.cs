using System.Collections.Generic;

using Avalonia.Media.Imaging;

using Engine.Common.Enums;
using Engine.Common.Models;

namespace Engine.Battle.Providers
{
    public interface IBattleInterfaceProvider
    {
        Bitmap Battleground { get; }

        Bitmap RightPanel { get; }

        Bitmap BottomPanel { get; }

        Bitmap PanelSeparator { get; }

        Bitmap DeathSkull { get; }


        IDictionary<ButtonState, Bitmap> ToggleRightButton { get; }

        IDictionary<ButtonState, Bitmap> DefendButton { get; }

        IDictionary<ButtonState, Bitmap> RetreatButton { get; }

        IDictionary<ButtonState, Bitmap> WaitButton { get; }

        IDictionary<ButtonState, Bitmap> InstantResolveButton { get; }

        IDictionary<ButtonState, Bitmap> AutoBattleButton { get; }


        IReadOnlyList<Frame> GetUnitAttackBorder(bool sizeSmall);

        IReadOnlyList<Frame> GetFieldAttackBorder();


        IReadOnlyList<Frame> GetUnitSelectionBorder(bool sizeSmall);


        IReadOnlyList<Frame> GetUnitHealBorder(bool sizeSmall);

        IReadOnlyList<Frame> GetFieldHealBorder();
    }
}
