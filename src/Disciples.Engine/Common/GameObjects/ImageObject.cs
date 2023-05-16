using Disciples.Engine.Base;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.SceneObjects;

namespace Disciples.Engine.Common.GameObjects;

/// <summary>
/// Игровой объект изображения.
/// </summary>
public class ImageObject : GameObject
{
    private readonly ISceneObjectContainer _sceneObjectContainer;
    private readonly ImageSceneElement _image;
    private readonly int _layer;

    private IImageSceneObject _imageSceneObject = null!;

    /// <summary>
    /// Создать объект типа <see cref="ImageObject" />.
    /// </summary>
    public ImageObject(
        ISceneObjectContainer sceneObjectContainer,
        ImageSceneElement image,
        int layer
        ) : base(image)
    {
        _sceneObjectContainer = sceneObjectContainer;
        _image = image;
        _layer = layer;
    }

    /// <summary>
    /// Изображение.
    /// </summary>
    public IBitmap? Bitmap
    {
        get => _imageSceneObject.Bitmap;
        set => _imageSceneObject.Bitmap = value;
    }

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        _imageSceneObject = _sceneObjectContainer.AddImage(_image, _layer);
    }

    /// <inheritdoc />
    public override void Destroy()
    {
        base.Destroy();

        _sceneObjectContainer.RemoveSceneObject(_imageSceneObject);
    }
}