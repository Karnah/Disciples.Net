using Disciples.Resources.Database.Sqlite.Extensions;
using Disciples.Resources.Database.Sqlite.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Disciples.Resources.Database.Sqlite.Configurations;

/// <summary>
/// Конфигурация EF для <see cref="Race" />.
/// </summary>
internal class RaceConfiguration : IEntityTypeConfiguration<Race>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Race> builder)
    {
        builder
            .ToTable(nameof(Race))
            .HasKey(r => r.Id);

        builder.HasOne(r => r.GuardingUnitType);
        builder.HasOne(r => r.LeaderThiefUnitType);
        builder.HasOne(r => r.Leader1UnitType);
        builder.HasOne(r => r.Leader2UnitType);
        builder.HasOne(r => r.Leader3UnitType);
        builder.HasOne(r => r.Leader4UnitType);
        builder.HasOne(r => r.Soldier1UnitType);
        builder.HasOne(r => r.Soldier2UnitType);
        builder.HasOne(r => r.Soldier3UnitType);
        builder.HasOne(r => r.Soldier4UnitType);
        builder.HasOne(r => r.Soldier5UnitType);

        builder.OwnsOne(
            r => r.Income,
            navigationBuilder => navigationBuilder.BuildResourceSet(nameof(Race.Income)));

        builder
            .HasOne(r => r.Name)
            .WithOne()
            .HasForeignKey<Race>("NameTextId");
    }
}