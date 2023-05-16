using System.Collections.Generic;
using System.Drawing;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Common.Providers;

/// <summary>
/// Базовый поставщик ресурсов.
/// </summary>
public interface IInterfaceProvider : ISupportLoading
{
    /// <summary>
    /// Получить данные интерфейса по него имени.
    /// </summary>
    SceneInterface GetSceneInterface(string name);

    /// <summary>
    /// Получить изображение по имени.
    /// </summary>
    IBitmap GetImage(string imageName);

    /// <summary>
    /// Получить части из которых состоит изображение.
    /// </summary>
    IReadOnlyDictionary<string, IBitmap> GetImageParts(string imageName);

    /// <summary>
    /// Получить анимацию.
    /// </summary>
    /// <param name="animationName">Имя анимации.</param>
    IReadOnlyList<Frame> GetAnimation(string animationName);

    /// <summary>
    /// Получить изображение указанного цвета.
    /// </summary>
    IBitmap GetColorBitmap(Color color);
}