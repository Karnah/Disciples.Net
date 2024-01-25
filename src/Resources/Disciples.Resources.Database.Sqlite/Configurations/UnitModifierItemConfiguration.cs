using Disciples.Resources.Database.Sqlite.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Disciples.Resources.Database.Sqlite.Configurations;

/// <summary>
/// Конфигурация EF для <see cref="UnitModifierItem" />.
/// </summary>
internal class UnitModifierItemConfiguration : IEntityTypeConfiguration<UnitModifierItem>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UnitModifierItem> builder)
    {
        builder.HasOne(umi => umi.UnitModifier)
            .WithMany(um => um.ModifierItems)
            .HasForeignKey(umi => umi.Id);

        builder
            .HasOne(r => r.Description)
            .WithOne()
            .HasForeignKey<UnitModifierItem>("DescriptionTextId");
    }
}