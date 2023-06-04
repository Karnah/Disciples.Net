using Disciples.Engine.Common.Enums;
using System.Collections;
using System.Collections.Generic;

namespace Disciples.Engine.Common.Models;

/// <summary>
/// Состояния кнопки.
/// </summary>
public class ButtonStates : IReadOnlyDictionary<SceneButtonState, IBitmap>
{
    private readonly IReadOnlyDictionary<SceneButtonState, IBitmap> _buttonStates;

    /// <summary>
    /// Создать объект типа <see cref="ButtonStates" />.
    /// </summary>
    public ButtonStates(IReadOnlyDictionary<SceneButtonState, IBitmap> buttonStates)
    {
        _buttonStates = buttonStates;
    }

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<SceneButtonState, IBitmap>> GetEnumerator()
    {
        return _buttonStates.GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc />
    public int Count => _buttonStates.Count;

    /// <inheritdoc />
    public bool ContainsKey(SceneButtonState key)
    {
        return _buttonStates.ContainsKey(key);
    }

    /// <inheritdoc />
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
    public bool TryGetValue(SceneButtonState key, out IBitmap? value)
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
    {
        return _buttonStates.TryGetValue(key, out value);
    }

    /// <inheritdoc />
    public IBitmap this[SceneButtonState key] => _buttonStates[key];

    /// <inheritdoc />
    public IEnumerable<SceneButtonState> Keys => _buttonStates.Keys;

    /// <inheritdoc />
    public IEnumerable<IBitmap> Values => _buttonStates.Values;
}