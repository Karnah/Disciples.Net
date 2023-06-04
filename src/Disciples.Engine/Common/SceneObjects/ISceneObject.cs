using Disciples.Common.Models;

namespace Disciples.Engine.Common.SceneObjects;

/// <summary>
/// Объект, который отрисовывается на сцене.
/// </summary>
public interface ISceneObject
{
    /// <summary>
    /// Границы объекта.
    /// </summary>
    RectangleD Bounds { get; set; }

    /// <summary>
    /// Слой на котором располагается объект на экране.
    /// </summary>
    int Layer { get; }

    /// <summary>
    /// Признак, что объект скрыт со сцены.
    /// </summary>
    public bool IsHidden { get; set; }


    /// <summary>
    /// Очистить занимаемые объектом ресурсы.
    /// </summary>
    void Destroy();
}