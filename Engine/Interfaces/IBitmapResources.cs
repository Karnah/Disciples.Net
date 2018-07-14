using System.Collections.Generic;

using Avalonia.Media.Imaging;

using Engine.Enums;

namespace Engine.Interfaces
{
    public interface IBitmapResources
    {
        IReadOnlyList<Bitmap> GetBitmapResources(string name, string code, Action action, Direction direction);
    }
}
