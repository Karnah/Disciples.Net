namespace Disciples.Resources.Common;

/// <summary>
/// Базовый класс для извлечения ресурсов, которые запакованы в MQDB.
/// </summary>
public abstract class BaseResourceExtractor
{
    private bool _isLoaded;

    /// <summary>
    /// Создать объект типа <see cref="BaseResourceExtractor" />.
    /// </summary>
    protected BaseResourceExtractor(string resourceFilePath)
    {
        ResourceFilePath = resourceFilePath;
    }

    /// <summary>
    /// Путь до файла ресурса.
    /// </summary>
    protected string ResourceFilePath { get; }

    /// <summary>
    /// Загрузить ресурс.
    /// </summary>
    public void Load()
    {
        if (_isLoaded)
            return;

        LoadInternal();

        _isLoaded = true;
    }

    /// <summary>
    /// Загрузить ресурс.
    /// </summary>
    protected abstract void LoadInternal();
}