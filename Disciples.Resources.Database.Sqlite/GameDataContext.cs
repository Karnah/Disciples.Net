using Disciples.Resources.Database.Sqlite.Models;
using Microsoft.EntityFrameworkCore;

namespace Disciples.Resources.Database.Sqlite;

/// <summary>
/// Контекст подключения к БД SQLite.
/// </summary>
public class GameDataContext : DbContext
{
    /// <summary>
    /// Создать объект типа <see cref="GameDataContext" />.
    /// </summary>
    public GameDataContext(DbContextOptions options) : base(options)
    {
    }

    /// <summary>
    /// Общие строки.
    /// </summary>
    public DbSet<GlobalTextResource> GlobalTextResources => Set<GlobalTextResource>();

    /// <summary>
    /// Строки для интерфейса.
    /// </summary>
    public DbSet<InterfaceTextResource> InterfaceTextResources => Set<InterfaceTextResource>();

    /// <summary>
    /// Список рас.
    /// </summary>
    public DbSet<Race> Races => Set<Race>();

    /// <summary>
    /// Список типов юнитов.
    /// </summary>
    public DbSet<UnitType> UnitTypes => Set<UnitType>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GameDataContext).Assembly);
    }
}