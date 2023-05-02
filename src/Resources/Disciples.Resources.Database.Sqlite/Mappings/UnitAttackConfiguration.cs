using Disciples.Resources.Database.Sqlite.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Disciples.Resources.Database.Sqlite.Mappings;

/// <summary>
/// Конфигурация EF для <see cref="UnitType" />.
/// </summary>
internal class UnitAttackConfiguration : IEntityTypeConfiguration<UnitAttack>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UnitAttack> builder)
    {
        builder.HasKey(ua => ua.Id);

        builder
            .HasOne(ua => ua.Name)
            .WithOne()
            .HasForeignKey<UnitAttack>("NameTextId");
        builder
            .HasOne(ua => ua.Description)
            .WithOne()
            .HasForeignKey<UnitAttack>("DescriptionTextId");

        builder
            .HasOne(ua => ua.AlternativeAttack)
            .WithOne()
            .HasForeignKey<UnitAttack>("AlternativeUnitAttackId");
    }
}