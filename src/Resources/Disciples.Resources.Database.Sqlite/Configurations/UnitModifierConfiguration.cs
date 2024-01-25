using Disciples.Resources.Database.Sqlite.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Disciples.Resources.Database.Sqlite.Configurations;

/// <summary>
/// Конфигурация EF для <see cref="UnitModifier" />.
/// </summary>
internal class UnitModifierConfiguration : IEntityTypeConfiguration<UnitModifier>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UnitModifier> builder)
    {
        builder
            .ToTable(nameof(UnitModifier))
            .HasKey(ut => ut.Id);

        builder.HasMany(um => um.ModifierItems)
            .WithOne(umi => umi.UnitModifier)
            .HasForeignKey(umi => umi.Id);
    }
}