using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
                Position = new Rectangle((int)offsetX, (int)offsetY, (int)(offsetX + background.Width), (int)(offsetY + background.Height)),
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
                    var button = (ButtonSceneElement)sceneElement;
                    gameObjects.Add(_gameObjectContainer.AddButton(button, layers.InterfaceLayer));
                    break;

                case SceneElementType.ToggleButton:
                case SceneElementType.RadioButton:
                case SceneElementType.ListBox:
                case SceneElementType.TextListBox:
                case SceneElementType.EditTextBox:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // Эти элементы обрабатываем отдельно в конце, так как они ссылаются на другие элементы сцены.
        var additionalElements = sceneInterface
            .Elements
            .Values
            .Where(se => se.Type is SceneElementType.ListBox or SceneElementType.TextListBox);
        foreach (var sceneElement in additionalElements)
        {
            switch (sceneElement.Type)
            {
                case SceneElementType.ListBox:
                    break;

                case SceneElementType.TextListBox:
                    var textListBox = (TextListBoxSceneElement)sceneElement;
                    gameObjects.Add(_gameObjectContainer.AddTextListBox(textListBox, layers.InterfaceLayer));
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return gameObjects;
    }
}