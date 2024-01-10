using System;
using System.Collections.Generic;
using Disciples.Common.Models;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Constants;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.SceneObjects;

namespace Disciples.Engine.Common.Components;

/// <summary>
/// Базовый компонент для анимаций.
/// </summary>
public abstract class BaseAnimationComponent : BaseComponent
{
    /// <summary>
    /// Промежуток времени в мс через которое происходит смена кадра в анимации.
    /// </summary>
    private const int FRAME_CHANGE_SPEED = 75;

    private long _ticksCount;
    private bool _isEnabled;

    /// <inheritdoc />
    protected BaseAnimationComponent(GameObject gameObject, ISceneObjectContainer sceneObjectContainer, int layer, PointD? animationOffset = null) :
        base(gameObject)
    {
        SceneObjectContainer = sceneObjectContainer;

        Layer = layer;
        AnimationOffset = animationOffset ?? new PointD();
        IsEnabled = true;
    }

    /// <summary>
    /// Текущий кадр анимации.
    /// </summary>
    public int FrameIndex { get; protected set; }

    /// <summary>
    /// Количество кадров в анимации.
    /// </summary>
    public int FramesCount { get; protected set; }

    /// <summary>
    /// Слой, на котором располагается анимация.
    /// </summary>
    public int Layer { get; }

    /// <summary>
    /// Смещение для анимаций.
    /// </summary>
    protected virtual PointD AnimationOffset { get;}

    /// <summary>
    /// Признак, что анимация выполняется.
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled == value)
                return;

            _isEnabled = value;

            foreach (var animationHost in GetAnimationHosts())
            {
                if (animationHost != null)
                    animationHost.IsHidden = !value;
            }
        }
    }

    /// <summary>
    /// Контроллер сцены.
    /// </summary>
    private ISceneObjectContainer SceneObjectContainer { get; }

    /// <inheritdoc />
    public override void Update(long tickCount)
    {
        if (!IsEnabled || FramesCount == 0)
            return;

        _ticksCount += tickCount;
        if (_ticksCount < FRAME_CHANGE_SPEED)
            return;

        ++FrameIndex;
        FrameIndex %= FramesCount;
        _ticksCount %= FRAME_CHANGE_SPEED;

        OnAnimationFrameChange();
    }

    /// <inheritdoc />
    public override void Destroy()
    {
        base.Destroy();

        // Уничтожаем все объекты для анимаций, которые были размещены на сцене.
        var animationHosts = GetAnimationHosts();
        foreach (var animationHost in animationHosts)
        {
            SceneObjectContainer.RemoveSceneObject(animationHost);
        }
    }

    /// <summary>
    /// Обработать событие того, что изменился кадр анимации.
    /// </summary>
    protected abstract void OnAnimationFrameChange();

    /// <summary>
    /// Получить список всех изображений для уничтожения.
    /// </summary>
    protected abstract IReadOnlyList<IImageSceneObject?> GetAnimationHosts();

    /// <summary>
    /// Обновить кадр анимации.
    /// </summary>
    /// <param name="animationHost">Изображение, которое которое отрисовывает кадры анимации.</param>
    /// <param name="frames">Кадры анимации.</param>
    /// <remarks>
    /// В отличие от <see cref="UpdatePosition" /> не проверяет изменение положения и размеров.
    /// Предполагается, что внутри одной анимации кадры имеют одинаковые размеры и смещение.
    /// </remarks>
    protected void UpdateFrame(IImageSceneObject? animationHost, AnimationFrames? frames)
    {
        if (animationHost == null)
            return;

        if (frames == null || frames.Count == 0)
            return;

        // Если одна анимация состоит из нескольких частей (например, анимацию юнита состоит из анимации тела юнита, его тени и ауры),
        // То чаще всего они имеют одинаковое количество фреймов. Но иногда анимация ауры состоит из меньшего количества фреймов, поэтому берём остаток от деления.
        animationHost.Bitmap = frames[FrameIndex % frames.Count];
    }

    /// <summary>
    /// Обновить позицию изображения на сцене.
    /// </summary>
    /// <param name="animationHost">Изображение, которое которое отрисовывает кадры анимации.</param>
    /// <param name="frames">Кадры анимации.</param>
    /// <param name="layer">Слой, на котором необходимо располагать анимацию.</param>
    /// <remarks>
    /// Необходимо использовать, когда изменилась анимация.
    /// </remarks>
    protected void UpdatePosition(ref IImageSceneObject? animationHost, AnimationFrames? frames, int? layer = null)
    {
        // Если фреймов нет, то удаляем анимацию со сцены.
        if (frames == null || frames.Count == 0)
        {
            if (animationHost != null)
                animationHost.IsHidden = true;

            return;
        }

        var posX = GameObject.X + AnimationOffset.X;
        var posY = GameObject.Y + AnimationOffset.Y;
        var frame = frames[FrameIndex % frames.Count];

        // Добавляем изображение, если его раньше не было.
        if (animationHost == null)
        {
            animationHost = SceneObjectContainer.AddImage(frame, posX, posY, layer ?? Layer);
            return;
        }

        if (animationHost.IsHidden)
            animationHost.IsHidden = false;

        animationHost.Bitmap = frame;

        if (Math.Abs(animationHost.Bounds.X - posX) > EngineConstants.DOUBLE_TOLERANCE ||
            Math.Abs(animationHost.Bounds.Y - posY) > EngineConstants.DOUBLE_TOLERANCE ||
            Math.Abs(animationHost.Bounds.Width - frame.OriginalSize.Width) > EngineConstants.DOUBLE_TOLERANCE ||
            Math.Abs(animationHost.Bounds.Height - frame.OriginalSize.Height) > EngineConstants.DOUBLE_TOLERANCE)
        {
            animationHost.Bounds = new RectangleD(posX, posY, frame.OriginalSize.Width, frame.OriginalSize.Height);
        }
    }
}