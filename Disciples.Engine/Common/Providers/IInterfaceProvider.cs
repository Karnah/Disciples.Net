using System.Collections.Generic;

using Disciples.Engine.Base;
using Disciples.Engine.Common.Enums;

namespace Disciples.Engine.Common.Providers
{
    /// <summary>
    /// Базовый поставщик ресурсов.
    /// </summary>
    public interface IInterfaceProvider : ISupportLoading
    {
        /// <summary>
        /// Получить изображение по имени.
        /// </summary>
        IBitmap GetImage(string imageName);

        /// <summary>
        /// Получить части из которых состоит изображение.
        /// </summary>
        IReadOnlyDictionary<string, IBitmap> GetImageParts(string imageName);

        /// <summary>
        /// Получить изображение указанного цвета.
        /// </summary>
        IBitmap GetColorBitmap(GameColor color);
    }
}