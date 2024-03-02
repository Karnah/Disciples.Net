using System;
using System.Collections.Generic;
using System.Linq;
using Disciples.Common.Models;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Constants;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Implementation.Common.Controllers;

/// <inheritdoc />
public class SceneInterfaceController : ISceneInterfaceController
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

        gameObjects.AddRange(GetBackground(sceneInterface, layers));

        var commonObjects = sceneInterface
            .Elements
            .Values
            .Where(e => e.Type is not SceneElementType.ListBox and not SceneElementType.TextListBox)
            .Select(e => GetElement(e, layers))
            .Where(go => go != null);
        gameObjects.AddRange(commonObjects!);

        // Эти элементы обрабатываем отдельно в конце, так как они ссылаются на другие элементы сцены.
        var additionalObjects = sceneInterface
            .Elements
            .Values
            .Where(e => e.Type is SceneElementType.ListBox or SceneElementType.TextListBox)
            .Select(e => GetElement(e, layers))
            .Where(go => go != null);
        gameObjects.AddRange(additionalObjects!);

        return gameObjects;
    }

    /// <summary>
    /// Получить фон сцены.
    /// </summary>
    private IReadOnlyList<GameObject> GetBackground(SceneInterface sceneInterface, Layers layers)
    {
        var background = sceneInterface.Background;
        if (background == null)
            return Array.Empty<GameObject>();

        var backgroundObject = _gameObjectContainer.AddImage(new ImageSceneElement
        {
            Name = "BACKGROUND",
            Position = new RectangleD(
                sceneInterface.Bounds.X, sceneInterface.Bounds.Y,
                background.OriginalSize.Width, background.OriginalSize.Height),
            ImageBitmap = background
        }, layers.BackgroundLayer);

        return new[] { backgroundObject };
    }

    /// <summary>
    /// Получить объект сцены.
    /// </summary>
    protected virtual GameObject? GetElement(SceneElement sceneElement, Layers layers)
    {
        switch (sceneElement.Type)
        {
            case SceneElementType.Image:
                var image = (ImageSceneElement)sceneElement;
                return _gameObjectContainer.AddImage(image, layers.InterfaceLayer);

            case SceneElementType.Animation:
                var animation = (AnimationSceneElement)sceneElement;
                return _gameObjectContainer.AddAnimation(animation, layers.InterfaceLayer);

            case SceneElementType.TextBlock:
                var textBlock = (TextBlockSceneElement)sceneElement;
                return _gameObjectContainer.AddTextBlock(textBlock, layers.InterfaceLayer);

            case SceneElementType.Button:
                var button = (ButtonSceneElement)sceneElement;
                return  _gameObjectContainer.AddButton(button, layers.InterfaceLayer + 1);

            case SceneElementType.ToggleButton:
                var toggleButton = (ToggleButtonSceneElement)sceneElement;
                return  _gameObjectContainer.AddToggleButton(toggleButton, layers.InterfaceLayer + 1);

            case SceneElementType.RadioButton:
            case SceneElementType.ListBox:
                break;

            case SceneElementType.TextListBox:
                var textListBox = (TextListBoxSceneElement)sceneElement;
                return _gameObjectContainer.AddTextListBox(textListBox, layers.InterfaceLayer);

            case SceneElementType.EditTextBox:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return null;
    }
}