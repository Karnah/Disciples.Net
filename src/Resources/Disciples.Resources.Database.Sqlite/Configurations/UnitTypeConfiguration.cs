using Disciples.Resources.Database.Sqlite.Extensions;
using Disciples.Resources.Database.Sqlite.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Disciples.Resources.Database.Sqlite.Configurations;

/// <summary>
/// Конфигурация EF для <see cref="UnitType" />.
/// </summary>
internal class UnitTypeConfiguration : IEntityTypeConfiguration<UnitType>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UnitType> builder)
    {
        builder
            .ToTable(nameof(UnitType))
            .HasKey(ut => ut.Id);

        builder.OwnsOne(
            ut => ut.RecruitCost,
            navigationBuilder => navigationBuilder.BuildResourceSet(nameof(UnitType.RecruitCost)));

        builder.HasOne(ut => ut.PreviousUnitType)
            .WithOne()
            .HasForeignKey<UnitType>(ut => ut.PreviousUnitTypeId);
        builder.HasOne(ut => ut.Race);
        builder.HasOne(ut => ut.LeaderBaseUnitType)
            .WithOne()
            .HasForeignKey<UnitType>(ut => ut.LeaderBaseUnitTypeId);

        builder
            .HasOne(r => r.Name)
            .WithOne()
            .HasForeignKey<UnitType>("NameTextId");
        builder
            .HasOne(r => r.Description)
            .WithOne()
            .HasForeignKey<UnitType>("DescriptionTextId");
        builder
            .HasOne(r => r.AbilityDescription)
            .WithOne()
            .HasForeignKey<UnitType>("AbilityTextId");

        builder.HasOne(ut => ut.MainAttack)
            .WithOne()
            .HasForeignKey<UnitType>("MainUnitAttackId");
        builder.HasOne(ut => ut.SecondaryAttack)
            .WithOne()
            .HasForeignKey<UnitType>("SecondaryUnitAttackId");

        builder.OwnsOne(
            ut => ut.ReviveCost,
            navigationBuilder => navigationBuilder.BuildResourceSet(nameof(UnitType.ReviveCost)));

        builder.OwnsOne(
            ut => ut.HealCost,
            navigationBuilder => navigationBuilder.BuildResourceSet(nameof(UnitType.HealCost)));

        builder.OwnsOne(
            ut => ut.TrainingCost,
            navigationBuilder => navigationBuilder.BuildResourceSet(nameof(UnitType.TrainingCost)));

        builder.HasOne(ut => ut.LowLevelUpgrade);
        builder.HasOne(ut => ut.HighLevelUpgrade);

        builder.HasMany(ut => ut.AttackSourceProtections)
            .WithOne(asp => asp.UnitType)
            .HasForeignKey(asp => asp.Id);

        builder.HasMany(ut => ut.AttackTypeProtections)
            .WithOne(asp => asp.UnitType)
            .HasForeignKey(atp => atp.Id);
    }
}