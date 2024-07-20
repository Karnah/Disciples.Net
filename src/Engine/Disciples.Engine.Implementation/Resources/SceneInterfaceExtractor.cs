using System.IO;

namespace Disciples.Engine.Implementation.Resources;

/// <summary>
/// Класс для извлечения интерфейса сцен и диалогов.
/// </summary>
public class SceneInterfaceExtractor : Disciples.Resources.Images.SceneInterfaceExtractor
{
    /// <summary>
    /// Создать объект типа <see cref="SceneInterfaceExtractor" />.
    /// </summary>
    public SceneInterfaceExtractor() : base($"{Directory.GetCurrentDirectory()}/Resources/interf/Interf.dlg")
    {
    }
}