using Disciples.Resources.Database.Sqlite.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Disciples.Resources.Database.Sqlite.Configurations;

/// <summary>
/// Конфигурация EF для <see cref="UnitAttackSummonTransform" />.
/// </summary>
internal class UnitAttackSummonTransformConfiguration : IEntityTypeConfiguration<UnitAttackSummonTransform>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UnitAttackSummonTransform> builder)
    {
        builder.HasOne(uast => uast.UnitAttack)
            .WithMany(ua => ua.AttackSummonTransforms)
            .HasForeignKey(uast => uast.Id);

        builder
            .HasOne(r => r.UnitType)
            .WithMany()
            .HasForeignKey(uast => uast.UnitTypeId);
    }
}