using Disciples.Resources.Database.Sqlite.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Disciples.Resources.Database.Sqlite.Configurations;

/// <summary>
/// Конфигурация EF для <see cref="UnitAttackSourceProtection" />.
/// </summary>
internal class UnitAttackSourceProtectionConfiguration : IEntityTypeConfiguration<UnitAttackSourceProtection>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UnitAttackSourceProtection> builder)
    {
        builder.HasOne(asp => asp.UnitType)
            .WithMany()
            .HasForeignKey(asp => asp.Id);
    }
}