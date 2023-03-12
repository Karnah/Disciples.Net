using System.Diagnostics;
using Disciples.Resources.Database.Sqlite.Mappings;
using Disciples.Resources.Database.Sqlite.Models;
using Microsoft.EntityFrameworkCore;

namespace Disciples.Resources.Database.Sqlite;

/// <summary>
/// Контекст подключения к БД SQLite.
/// </summary>
public class GameDataContext : DbContext
{
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
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=Resources/gamedata.db");
        optionsBuilder.LogTo(s => Debug.Print(s));
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new GlobalTextResourceConfiguration());
        modelBuilder.ApplyConfiguration(new InterfaceTextResourceConfiguration());
        modelBuilder.ApplyConfiguration(new RaceConfiguration());
        modelBuilder.ApplyConfiguration(new UnitAttackConfiguration());
        modelBuilder.ApplyConfiguration(new UnitAttackSourceProtectionConfiguration());
        modelBuilder.ApplyConfiguration(new UnitAttackTypeProtectionConfiguration());
        modelBuilder.ApplyConfiguration(new UnitLevelUpgradeConfiguration());
        modelBuilder.ApplyConfiguration(new UnitTypeConfiguration());
    }
}