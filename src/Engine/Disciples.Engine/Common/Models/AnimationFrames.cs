using System;
using System.Collections;
using System.Collections.Generic;

namespace Disciples.Engine.Common.Models;

/// <summary>
/// Кадры анимации.
/// </summary>
public class AnimationFrames : IReadOnlyList<IBitmap>
{
    private readonly Lazy<IReadOnlyList<IBitmap>> _frames;

    /// <summary>
    /// Создать объект типа <see cref="AnimationFrames" />.
    /// </summary>
    public AnimationFrames() : this(Array.Empty<IBitmap>())
    {
    }

    /// <summary>
    /// Создать объект типа <see cref="AnimationFrames" />.
    /// </summary>
    public AnimationFrames(IReadOnlyList<IBitmap> frames)
    {
        _frames = new Lazy<IReadOnlyList<IBitmap>>(frames);
    }

    /// <summary>
    /// Создать объект типа <see cref="AnimationFrames" />.
    /// </summary>
    public AnimationFrames(Lazy<IReadOnlyList<IBitmap>> frames)
    {
        _frames = frames;
    }

    /// <inheritdoc />
    public IBitmap this[int index] => _frames.Value[index];

    /// <inheritdoc />
    public int Count => _frames.Value.Count;

    /// <inheritdoc />
    public IEnumerator<IBitmap> GetEnumerator()
    {
        return _frames.Value.GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}