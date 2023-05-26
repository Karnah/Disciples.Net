using System.Collections.Generic;
using Disciples.Engine.Common.Constants;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Common.Controllers;

/// <summary>
/// Контроллер для размещения объектов на сцене.
/// </summary>
public interface ISceneInterfaceController
{
    /// <summary>
    /// Добавить объекты на сцену.
    /// </summary>
    IReadOnlyList<GameObject> AddSceneGameObjects(SceneInterface sceneInterface, Layers layers);
}