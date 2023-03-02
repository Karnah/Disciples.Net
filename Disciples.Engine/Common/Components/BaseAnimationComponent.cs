using System;
using System.Collections.Generic;
using Disciples.Engine.Base;
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

    /// <inheritdoc />
    protected BaseAnimationComponent(GameObject gameObject, ISceneController sceneController, int layer) :
        base(gameObject)
    {
        SceneController = sceneController;

        Layer = layer;
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
    /// Контроллер сцены.
    /// </summary>
    private ISceneController SceneController { get; }

    /// <inheritdoc />
    public override void Update(long tickCount)
    {
        if (FramesCount == 0)
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
            SceneController.RemoveSceneObject(animationHost);
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
    protected void UpdateFrame(IImageSceneObject? animationHost, IReadOnlyList<Frame>? frames)
    {
        if (animationHost == null)
            return;

        if (frames == null || frames.Count == 0)
            return;

        animationHost.Bitmap = frames[FrameIndex].Bitmap;
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
    protected void UpdatePosition(ref IImageSceneObject? animationHost, IReadOnlyList<Frame>? frames,
        int? layer = null)
    {
        // Если фреймов нет, то удаляем анимацию со сцены.
        if (frames == null || frames.Count == 0)
        {
            if (animationHost != null)
            {
                // todo Здесь происходит удаление.
                // Возможно, просто достаточно перенести в невидимую область
                SceneController.RemoveSceneObject(animationHost);
                animationHost = null;
            }

            return;
        }

        // Добавляем изображение, если его раньше не было.
        if (animationHost == null)
            animationHost = SceneController.AddImage(layer ?? Layer);

        // Обновляем кадр анимации.
        var frame = frames[FrameIndex];
        animationHost.Bitmap = frame.Bitmap;

        // Пересчитываем новую позицию изображения.
        var posX = GameObject.X + frame.OffsetX;
        if (Math.Abs(animationHost.X - posX) > float.Epsilon)
            animationHost.X = posX;

        var posY = GameObject.Y + frame.OffsetY;
        if (Math.Abs(animationHost.Y - posY) > float.Epsilon)
            animationHost.Y = posY;

        // Изменяем, если необходимо, размеры изображения.
        if (Math.Abs(animationHost.Width - frame.Width) > float.Epsilon)
            animationHost.Width = frame.Width;

        if (Math.Abs(animationHost.Height - frame.Height) > float.Epsilon)
            animationHost.Height = frame.Height;
    }
}