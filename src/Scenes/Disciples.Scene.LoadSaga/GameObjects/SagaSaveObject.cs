using Disciples.Engine.Base;
using Disciples.Engine.Common.Components;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.SceneObjects;
using Disciples.Engine.Models;
using Disciples.Scene.LoadSaga.Models;

namespace Disciples.Scene.LoadSaga.GameObjects;

/// <summary>
/// Объект сейва.
/// </summary>
internal class SagaSaveObject : GameObject
{
    private const int WIDTH = 300;
    private const int HEIGHT = 20;

    private readonly ISceneObjectContainer _sceneObjectContainer;

    private ITextSceneObject _saveText = null!;

    /// <summary>
    /// Создать объект типа <see cref="SagaSaveObject" />.
    /// </summary>
    public SagaSaveObject(
        ISceneObjectContainer sceneObjectContainer,
        double x,
        double y,
        Action<SagaSaveObject> onSavePressed,
        Action<SagaSaveObject> onSaveMouseLeftButtonDoubleClicked,
        Save save) : base(x, y)
    {
        _sceneObjectContainer = sceneObjectContainer;
        Save = save;

        Width = WIDTH;
        Height = HEIGHT;

        Components = new IComponent[]
        {
            new SelectionComponent(this),
            new MouseLeftButtonClickComponent(this, Array.Empty<KeyboardButton>(),
                () => onSavePressed.Invoke(this),
                onDoubleClickedAction: () => onSaveMouseLeftButtonDoubleClicked.Invoke(this))
        };
    }

    /// <summary>
    /// Сейв-файл.
    /// </summary>
    public Save Save { get; }

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        _saveText = _sceneObjectContainer.AddText(
            new TextContainer(Save.Name),
            WIDTH,
            HEIGHT,
            X,
            Y,
            2);
    }

    /// <inheritdoc />
    public override void Destroy()
    {
        base.Destroy();

        _sceneObjectContainer.RemoveSceneObject(_saveText);
    }
}