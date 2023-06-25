using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Disciples.Common.Models;
using Disciples.Engine;
using DynamicData;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.SceneObjects;
using Disciples.Engine.Models;
using Disciples.WPF.SceneObjects;

namespace Disciples.WPF.Controllers;

/// <inheritdoc />
public class WpfSceneObjectContainer : IPlatformSceneObjectContainer
{
    private readonly ObservableCollection<ISceneObject> _visuals;
    private readonly IList<ISceneObject> _addVisualBuffer;
    private readonly IList<ISceneObject> _removeVisualBuffer;

    /// <summary>
    /// Создать объект типа <see cref="WpfSceneObjectContainer" />.
    /// </summary>
    public WpfSceneObjectContainer()
    {
        _visuals = new ObservableCollection<ISceneObject>();
        _addVisualBuffer = new List<ISceneObject>();
        _removeVisualBuffer = new List<ISceneObject>();
    }

    /// <inheritdoc />
    public IReadOnlyList<ISceneObject> SceneObjects => _visuals;

    /// <inheritdoc />
    public IImageSceneObject AddImageSceneObject(IBitmap? bitmap, RectangleD bounds, int layer)
    {
        return AddSceneObject(new ImageSceneObject(bitmap, bounds, layer));
    }

    /// <inheritdoc />
    public ITextSceneObject AddTextSceneObject(TextContainer? text, TextStyle textStyle, RectangleD bounds, int layer)
    {
        return AddSceneObject(new TextSceneObject(text, textStyle, bounds, layer));
    }

    /// <inheritdoc />
    public IVideoSceneObject AddVideoSceneObject(Stream stream, RectangleD bounds, int layer)
    {
        return AddSceneObject(new VideoSceneObject(stream, bounds, layer));
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
            _visuals.RemoveMany(_removeVisualBuffer);

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