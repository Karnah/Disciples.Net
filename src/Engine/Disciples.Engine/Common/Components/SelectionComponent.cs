using System;
using System.Diagnostics;
using Disciples.Common.Models;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Constants;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.SceneObjects;
using Disciples.Engine.Models;

namespace Disciples.Engine.Common.Components;

/// <summary>
/// Компонент для игровых объектов, который могут быть выбраны в качестве цели.
/// </summary>
public class SelectionComponent : BaseComponent
{
    private const int TOOL_TIP_DELAY = 500;

    private readonly Action? _onHoveredAction;
    private readonly Action? _onUnhoveredAction;
    private readonly ISceneObjectContainer _sceneObjectContainer;

    private readonly Stopwatch _toolTipStopwatch = new();
    private ITextSceneObject? _toolTip;

    /// <summary>
    /// Создать объект типа <see cref="SelectionComponent" />.
    /// </summary>
    public SelectionComponent(GameObject gameObject, ISceneObjectContainer sceneObjectContainer, Action? onHoveredAction = null, Action? onUnhoveredAction = null) : base(gameObject)
    {
        _sceneObjectContainer = sceneObjectContainer;
        _onHoveredAction = onHoveredAction;
        _onUnhoveredAction = onUnhoveredAction;
    }

    /// <summary>
    /// Признак, что указатель находится над объектом.
    /// </summary>
    public bool IsHover { get; private set; }

    /// <summary>
    /// Признак, что объект доступен для выделения.
    /// </summary>
    public bool IsSelectionEnabled { get; set; } = true;

    /// <summary>
    /// Обработать наведение указателя на объект.
    /// </summary>
    public void Hovered()
    {
        IsHover = true;
        _onHoveredAction?.Invoke();
        _toolTipStopwatch.Restart();
    }

    /// <summary>
    /// Обработать снятие указателя с объекта.
    /// </summary>
    public void Unhovered()
    {
        IsHover = false;

        if (_toolTip != null)
        {
            _sceneObjectContainer.RemoveSceneObject(_toolTip);
            _toolTip = null;
        }

        // Если убрали выделение с объекта, сбрасываем нажатие.
        GameObject.MouseLeftButtonClickComponent?.Unpressed();

        _onUnhoveredAction?.Invoke();
    }

    /// <inheritdoc />
    public override void Update(long tickCount)
    {
        base.Update(tickCount);

        // Добавляем подсказку спустя некоторое время, после того как объект был выбран.
        if (IsHover &&
            _toolTip == null &&
            GameObject.ToolTip != null &&
            _toolTipStopwatch.ElapsedMilliseconds > TOOL_TIP_DELAY)
        {
            var gameObjectBounds = GameObject.Bounds;
            var x = Math.Min(gameObjectBounds.Right, GameInfo.OriginalWidth - 70);
            var y = gameObjectBounds.Top;

            _toolTip = _sceneObjectContainer.AddText(
                GameObject.ToolTip,
                new TextStyle { BackgroundColor = GameColors.White },
                new RectangleD(x, y, double.NaN, double.NaN),
                Layers.SceneLayers.AboveAllLayer);
        }
    }

    /// <inheritdoc />
    public override void Destroy()
    {
        base.Destroy();

        _sceneObjectContainer.RemoveSceneObject(_toolTip);
    }
}