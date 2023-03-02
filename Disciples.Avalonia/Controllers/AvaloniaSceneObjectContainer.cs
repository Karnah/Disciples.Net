using System.Collections.Generic;
using System.Linq;
using Avalonia.Collections;
using Disciples.Avalonia.SceneObjects;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.SceneObjects;

namespace Disciples.Avalonia.Controllers;

/// <inheritdoc />
public class AvaloniaSceneObjectContainer : IPlatformSceneObjectContainer
{
    private readonly AvaloniaList<ISceneObject> _visuals;
    private readonly IList<ISceneObject> _addVisualBuffer;
    private readonly IList<ISceneObject> _removeVisualBuffer;

    /// <summary>
    /// Создать объект типа <see cref="AvaloniaSceneObjectContainer" />.
    /// </summary>
    public AvaloniaSceneObjectContainer()
    {
        _visuals = new AvaloniaList<ISceneObject>();
        _addVisualBuffer = new List<ISceneObject>();
        _removeVisualBuffer = new List<ISceneObject>();
    }

    /// <inheritdoc />
    public IReadOnlyList<ISceneObject> SceneObjects => _visuals;

    /// <inheritdoc />
    public IImageSceneObject AddImageSceneObject(int layer)
    {
        return AddSceneObject(new ImageSceneObject(layer));
    }

    /// <inheritdoc />
    public ITextSceneObject AddTextSceneObject(string text, double fontSize, int layer, bool isBold = false)
    {
        return AddSceneObject(new TextSceneObject(text, fontSize, layer, isBold));
    }

    /// <inheritdoc />
    public ITextSceneObject AddTextSceneObject(string text,
        double fontSize,
        int layer,
        double width,
        TextAlignment textAlignment = TextAlignment.Center,
        bool isBold = false,
        GameColor? foregroundColor = null)
    {
        return AddSceneObject(new TextSceneObject(text, fontSize, layer, width, textAlignment, isBold, foregroundColor));
    }

    /// <inheritdoc />
    public void RemoveSceneObject(ISceneObject sceneObject)
    {
        // Обрабатываем ситуацию, когда объект был добавлен и тут же удалён.
        if (_addVisualBuffer.Contains(sceneObject))
        {
            _addVisualBuffer.Remove(sceneObject);
            return;
        }

        _removeVisualBuffer.Add(sceneObject);
    }

    /// <inheritdoc />
    public void UpdateContainer()
    {
        if (_removeVisualBuffer.Any())
        {
            _visuals.RemoveAll(_removeVisualBuffer);

            foreach (var visualObject in _removeVisualBuffer)
                visualObject.Destroy();

            _removeVisualBuffer.Clear();
        }

        if (_addVisualBuffer.Any())
        {
            _visuals.AddRange(_addVisualBuffer);
            _addVisualBuffer.Clear();
        }
    }

    /// <summary>
    /// Добавить объект на сцену.
    /// </summary>
    private TSceneObject AddSceneObject<TSceneObject>(TSceneObject sceneObject)
        where TSceneObject : ISceneObject
    {
        _addVisualBuffer.Add(sceneObject);
        return sceneObject;
    }
}