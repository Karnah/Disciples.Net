using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Disciples.Resources.Database.Sqlite;

/// <summary>
/// Фабрика для создания подключения к БД.
/// </summary>
public class GameDataContextFactory
{
    private readonly string _connectionString;

    /// <summary>
    /// Создать объект типа <see cref="GameDataContextFactory" />.
    /// </summary>
    /// <param name="connectionString">Строка подключения к бд.</param>
    public GameDataContextFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Создать подключение к БД.
    /// </summary>
    public GameDataContext Create()
    {
        var optionsBuilder = new DbContextOptionsBuilder<GameDataContext>()
            .UseSqlite(_connectionString)
            .LogTo(s => Debug.Print(s));

        return new GameDataContext(optionsBuilder.Options);
    }
}