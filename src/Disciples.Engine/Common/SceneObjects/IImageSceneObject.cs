using Disciples.Engine.Common.Enums;

namespace Disciples.Engine.Common.SceneObjects;

/// <summary>
/// Изображение, которое отображается на сцене.
/// </summary>
public interface IImageSceneObject : ISceneObject
{
    /// <summary>
    /// Изображение, отображаемое на сцене.
    /// </summary>
    IBitmap? Bitmap { get; set; }

    /// <summary>
    /// Необходимо ли развернуть изображение.
    /// </summary>
    bool IsReflected { get; set; }

    /// <summary>
    /// Выравнивание по ширине.
    /// </summary>
    HorizontalAlignment HorizontalAlignment { set; get; }

    /// <summary>
    /// Выравнивание по высоте.
    /// </summary>
    VerticalAlignment VerticalAlignment { get; set; }
}