using Disciples.Engine.Base;

namespace Disciples.Engine.Implementation.Base;

/// <inheritdoc />
public abstract class BaseSupportLoading : ISupportLoading
{
    /// <inheritdoc />
    public bool IsLoaded { get; private set; }


    /// <inheritdoc />
    public void Load()
    {
        if (IsLoaded)
            return;

        LoadInternal();
        IsLoaded = true;
    }

    /// <inheritdoc />
    public void Unload()
    {
        if (!IsLoaded)
            return;

        UnloadInternal();
        IsLoaded = false;
    }

    /// <summary>
    /// Инициализировать объект.
    /// </summary>
    protected abstract void LoadInternal();

    /// <summary>
    /// Очистить занимаемые объектом ресурсы.
    /// </summary>
    protected abstract void UnloadInternal();
}