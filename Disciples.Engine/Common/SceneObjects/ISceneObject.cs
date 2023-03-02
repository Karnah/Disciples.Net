namespace Disciples.Engine.Common.SceneObjects;

/// <summary>
/// Объект, который отрисовывается на сцене.
/// </summary>
public interface ISceneObject
{
    /// <summary>
    /// Координата X объекта (справа - налево).
    /// </summary>
    double X { get; set; }

    /// <summary>
    /// Координата Y объекта (сверху - вниз).
    /// </summary>
    double Y { get; set; }

    /// <summary>
    /// Ширина объекта.
    /// </summary>
    double Width { get; set; }

    /// <summary>
    /// Высота объекта.
    /// </summary>
    double Height { get; set; }

    /// <summary>
    /// Слой на котором располагается объект на экране.
    /// </summary>
    int Layer { get; }


    /// <summary>
    /// Очистить занимаемые объектом ресурсы.
    /// </summary>
    void Destroy();
}