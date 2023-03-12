namespace Disciples.Resources.Database.Sqlite;

/// <summary>
/// Фабрика для создания подключения к БД.
/// </summary>
public class GameDataContextFactory
{
    /// <summary>
    /// Создать подключение к БД.
    /// </summary>
    public GameDataContext Create()
    {
        return new GameDataContext();
    }
}