using System.Collections.Generic;

using Avalonia.Media.Imaging;

using Inftastructure.Enums;

namespace Inftastructure.Interfaces
{
    public interface IBitmapResources
    {
        IReadOnlyList<Bitmap> GetBitmapResources(string name, string code, Action action, Direction direction);
    }
}
