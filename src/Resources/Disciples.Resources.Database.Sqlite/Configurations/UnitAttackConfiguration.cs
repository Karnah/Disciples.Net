using Disciples.Resources.Database.Sqlite.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Disciples.Resources.Database.Sqlite.Configurations;

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

        builder
            .HasOne(ua => ua.Ward1)
            .WithOne()
            .HasForeignKey<UnitAttack>("Ward1Id");

        builder
            .HasOne(ua => ua.Ward2)
            .WithOne()
            .HasForeignKey<UnitAttack>("Ward2Id");

        builder
            .HasOne(ua => ua.Ward3)
            .WithOne()
            .HasForeignKey<UnitAttack>("Ward3Id");

        builder
            .HasOne(ua => ua.Ward4)
            .WithOne()
            .HasForeignKey<UnitAttack>("Ward4Id");
    }
}