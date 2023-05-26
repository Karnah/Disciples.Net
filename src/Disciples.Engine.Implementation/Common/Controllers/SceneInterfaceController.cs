using System;
using System.Collections.Generic;
using Disciples.Common.Models;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Constants;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Implementation.Common.Controllers;

/// <inheritdoc />
internal class SceneInterfaceController : ISceneInterfaceController
{
    private readonly IGameObjectContainer _gameObjectContainer;

    /// <summary>
    /// Создать объект типа <see cref="SceneInterfaceController" />.
    /// </summary>
    public SceneInterfaceController(IGameObjectContainer gameObjectContainer)
    {
        _gameObjectContainer = gameObjectContainer;
    }

    /// <summary>
    /// Добавить объекты на сцену.
    /// </summary>
    public IReadOnlyList<GameObject> AddSceneGameObjects(SceneInterface sceneInterface, Layers layers)
    {
        var gameObjects = new List<GameObject>();

        var background = sceneInterface.Background;
        if (background != null)
        {
            var offsetX = (GameInfo.OriginalWidth - background.Width) / 2;
            var offsetY = (GameInfo.OriginalHeight - background.Height) / 2;

            gameObjects.Add(_gameObjectContainer.AddImage(new ImageSceneElement
            {
                Name = "BACKGROUND",
                Position = new Bounds((int)(offsetY + background.Height), (int)offsetY, (int)offsetX, (int)(offsetX + background.Width)),
                ImageBitmap = background
            }, layers.BackgroundLayer));
        }

        foreach (var sceneElement in sceneInterface.Elements.Values)
        {
            switch (sceneElement.Type)
            {
                case SceneElementType.Image:
                    var image = (ImageSceneElement)sceneElement;
                    gameObjects.Add(_gameObjectContainer.AddImage(image, layers.InterfaceLayer));
                    break;

                case SceneElementType.Animation:
                    var animation = (AnimationSceneElement)sceneElement;
                    gameObjects.Add(_gameObjectContainer.AddAnimation(animation, layers.InterfaceLayer));
                    break;

                case SceneElementType.TextBlock:
                    var textBlock = (TextBlockSceneElement)sceneElement;
                    gameObjects.Add(_gameObjectContainer.AddTextBlock(textBlock, layers.InterfaceLayer));
                    break;

                case SceneElementType.Button:
                case SceneElementType.ToggleButton:
                case SceneElementType.RadioButton:
                case SceneElementType.ListBox:
                case SceneElementType.EditTextBox:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return gameObjects;
    }
}