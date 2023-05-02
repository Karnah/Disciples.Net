using Disciples.Resources.Database.Sqlite.Extensions;
using Disciples.Resources.Database.Sqlite.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Disciples.Resources.Database.Sqlite.Mappings;

/// <summary>
/// Конфигурация EF для <see cref="UnitLevelUpgrade" />.
/// </summary>
internal class UnitLevelUpgradeConfiguration : IEntityTypeConfiguration<UnitLevelUpgrade>
{
    public void Configure(EntityTypeBuilder<UnitLevelUpgrade> builder)
    {
        builder.HasKey(ulu => ulu.Id);

        builder.OwnsOne(
            ut => ut.RecruitCost,
            navigationBuilder => navigationBuilder.BuildResourceSet(nameof(UnitType.RecruitCost)));

        builder.OwnsOne(
            ut => ut.ReviveCost,
            navigationBuilder => navigationBuilder.BuildResourceSet(nameof(UnitType.ReviveCost)));

        builder.OwnsOne(
            ut => ut.HealCost,
            navigationBuilder => navigationBuilder.BuildResourceSet(nameof(UnitType.HealCost)));

        builder.OwnsOne(
            ut => ut.TrainingCost,
            navigationBuilder => navigationBuilder.BuildResourceSet(nameof(UnitType.TrainingCost)));
    }
}