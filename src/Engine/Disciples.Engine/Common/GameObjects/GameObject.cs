using System;
using System.Collections.Generic;
using System.Linq;
using Disciples.Common.Models;
using Disciples.Engine.Common.Components;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Exceptions;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Models;

namespace Disciples.Engine.Common.GameObjects;

/// <summary>
/// Объект, который располагается на сцене.
/// </summary>
public abstract class GameObject
{
    private bool _isHidden;

    /// <summary>
    /// Создать объект типа <see cref="GameObject" />.
    /// </summary>
    protected GameObject()
    {
        Components = Array.Empty<IComponent>();
    }

    /// <summary>
    /// Создать объект типа <see cref="GameObject" />.
    /// </summary>
    protected GameObject(double x, double y) : this()
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Создать объект типа <see cref="GameObject" />.
    /// </summary>
    protected GameObject((double X, double Y) position) : this(position.X, position.Y)
    {
    }

    /// <summary>
    /// Создать объект типа <see cref="GameObject" />.
    /// </summary>
    protected GameObject(SceneElement sceneElement) : this(sceneElement.Position)
    {
        Name = sceneElement.Name;
        ToolTip = GetToolTip(sceneElement);
    }

    /// <summary>
    /// Создать объект типа <see cref="GameObject" />.
    /// </summary>
    protected GameObject(RectangleD position) : this()
    {
        X = position.X;
        Y = position.Y;
        Width = position.Width;
        Height = position.Height;
    }

    /// <summary>
    /// Имя объекта.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Подсказка для объекта.
    /// </summary>
    public TextContainer? ToolTip { get; }

    /// <summary>
    /// Положение объекта на сцене, координата X.
    /// </summary>
    /// <remarks>Отсчёт идёт справа налево.</remarks>
    public double X { get; protected set; }

    /// <summary>
    /// Положение объекта на сцене, координата Y.
    /// </summary>
    /// <remarks>Отсчёт идет сверху вниз.</remarks>
    public double Y { get; protected set; }

    /// <summary>
    /// Ширина объекта на сцене.
    /// </summary>
    public double Width { get; protected set; }

    /// <summary>
    /// Высота объекта на сцене.
    /// </summary>
    public double Height { get; protected set; }

    /// <summary>
    /// Границы элемента.
    /// </summary>
    public RectangleD Bounds => new(X, Y, Width, Height);

    /// <summary>
    /// Компоненты из которых состоит объект.
    /// </summary>
    public IReadOnlyCollection<IComponent> Components { get; protected set; }

    /// <summary>
    /// Был ли объект инициализирован.
    /// </summary>
    public bool IsInitialized { get; private set; }

    /// <summary>
    /// Признак, что объект скрыт.
    /// </summary>
    public bool IsHidden
    {
        get => _isHidden;
        set
        {
            _isHidden = value;
            OnHiddenChanged(value);
        }
    }

    /// <summary>
    /// Был ли объект удалён со сцены.
    /// </summary>
    public bool IsDestroyed { get; private set; }

    /// <summary>
    /// Признак, что объект временно не реагирует ни на какие события мыши/клавиатуры.
    /// </summary>
    /// <remarks>
    /// Необходимо для работы с диалогами.
    /// При открытии диалога, все объекты основной сцены деактивируются до тех пор, пока диалог не будет закрыт.
    /// </remarks>
    public bool IsDeactivated { get; set; }

    #region Components

    /// <summary>
    /// Компонент для игровых объектов, который могут быть выбраны в качестве цели.
    /// </summary>
    public SelectionComponent? SelectionComponent { get; private set; }

    /// <summary>
    /// Компонент для объектов, которые могут быть нажаты ЛКМ.
    /// </summary>
    public MouseLeftButtonClickComponent? MouseLeftButtonClickComponent { get; private set; }

    /// <summary>
    /// Компонент для объектов, которые могут быть нажаты ПКМ.
    /// </summary>
    public MouseRightButtonClickComponent? MouseRightButtonClickComponent { get; private set; }

    #endregion

    /// <summary>
    /// Инициализировать игровой объект.
    /// </summary>
    public virtual void Initialize()
    {
        if (IsInitialized)
            throw new InvalidOperationException("Game object already initialized");

        foreach (var component in Components)
        {
            component.Initialize();

            if (component is SelectionComponent selectionComponent)
                SelectionComponent = selectionComponent;
            else if (component is MouseLeftButtonClickComponent mouseLeftButtonClickComponent)
                MouseLeftButtonClickComponent = mouseLeftButtonClickComponent;
            else if (component is MouseRightButtonClickComponent mouseRightButtonClickComponent)
                MouseRightButtonClickComponent = mouseRightButtonClickComponent;
        }

        IsInitialized = true;
    }

    /// <summary>
    /// Обработать событие обновление объекта.
    /// </summary>
    /// <param name="ticksCount">Количество тиков, которое прошло со времени предыдущего обновления.</param>
    public virtual void Update(long ticksCount)
    {
        if (IsDestroyed)
            return;

        foreach (var component in Components)
            component.Update(ticksCount);
    }

    /// <summary>
    /// Уничтожить объект.
    /// </summary>
    public virtual void Destroy()
    {
        if (IsDestroyed)
            throw new ObjectDisposedException("Game object already destroyed");

        foreach (var component in Components)
            component.Destroy();

        IsDestroyed = true;
    }

    /// <summary>
    /// Получить компонент указанного типа.
    /// </summary>
    /// <typeparam name="TComponent">Тип искомого компонента.</typeparam>
    public TComponent GetComponent<TComponent>()
        where TComponent : IComponent
    {
        return (TComponent)GetComponent(typeof(TComponent));
    }

    /// <summary>
    /// Получить компонент указанного типа.
    /// </summary>
    /// <typeparam name="TComponent">Тип искомого компонента.</typeparam>
    public TComponent? TryGetComponent<TComponent>()
        where TComponent : IComponent
    {
        return (TComponent?)TryGetComponent(typeof(TComponent));
    }

    /// <summary>
    /// Получить компонент указанного типа.
    /// </summary>
    /// <param name="componentType">Тип искомого компонента.</param>
    public object GetComponent(Type componentType)
    {
        var component = TryGetComponent(componentType);
        if (component == null)
            throw new ComponentNotFoundException(componentType);

        return component;
    }

    /// <summary>
    /// Получить компонент указанного типа.
    /// </summary>
    /// <param name="componentType">Тип искомого компонента.</param>
    public object? TryGetComponent(Type componentType)
    {
        foreach (var component in Components)
        {
            if (componentType.IsInstanceOfType(component))
                return component;
        }

        return null;
    }

    /// <summary>
    /// Обработать изменение параметра <see cref="IsHidden" />.
    /// </summary>
    protected virtual void OnHiddenChanged(bool isHidden)
    {

    }

    /// <summary>
    /// Получить подсказку при наведении.
    /// </summary>
    private static TextContainer? GetToolTip(SceneElement sceneElement)
    {
        var toolTip = sceneElement.ToolTip;
        if (toolTip == null)
            return null;

        var hotkeys = sceneElement switch
        {
            ButtonSceneElement bse => bse.HotKeys,
            ToggleButtonSceneElement tbse => tbse.HotKeys,
            _ => null
        };
        var displayHotkeys = hotkeys
            ?.Where(h => h != KeyboardButton.Escape && h != KeyboardButton.Enter)
            .ToList();
        if (displayHotkeys == null || displayHotkeys.Count == 0)
            return toolTip;

        return new TextContainer(
            toolTip
                .TextPieces
                .Append(new TextPiece($" ({string.Join(", ", displayHotkeys)})"))
                .ToList());
    }
}