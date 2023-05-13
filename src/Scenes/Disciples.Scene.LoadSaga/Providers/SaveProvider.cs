using Disciples.Engine.Implementation;
using Disciples.Engine.Settings;
using Disciples.Scene.LoadSaga.Models;

namespace Disciples.Scene.LoadSaga.Providers;

/// <summary>
/// Класс для работы с сохранёнными играми пользователя.
/// </summary>
internal class SaveProvider
{
    /// <summary>
    /// Фильтр для поиска файлов сейвов.
    /// </summary>
    private const string SAVE_EXTENSION_FILTER = "*.json";

    private readonly GameController _gameController;
    private readonly string _savesPath;

    /// <summary>
    /// Создать объект типа <see cref="SaveProvider" />.
    /// </summary>
    public SaveProvider(GameController gameController, GameSettings settings)
    {
        _gameController = gameController;
        _savesPath = Path.Combine(Directory.GetCurrentDirectory(), settings.SavesFolder);
    }

    /// <summary>
    /// Получить список сейвов.
    /// </summary>
    public IReadOnlyList<Save> GetSaves()
    {
        return Directory
            .GetFiles(_savesPath, SAVE_EXTENSION_FILTER)
            .Where(f => !string.IsNullOrEmpty(f))
            .Select(f => new Save
            {
                Name = Path.GetFileNameWithoutExtension(f),
                Path = Path.Combine(_savesPath, f),
                GameContext = _gameController.LoadGame(Path.Combine(_savesPath, f))
            })
            .ToArray();
    }
}