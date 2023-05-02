using Disciples.Resources.Database.Sqlite.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Disciples.Resources.Database.Sqlite.Mappings;

/// <summary>
/// Конфигурация EF для <see cref="UnitAttackTypeProtection" />.
/// </summary>
internal class UnitAttackTypeProtectionConfiguration : IEntityTypeConfiguration<UnitAttackTypeProtection>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UnitAttackTypeProtection> builder)
    {
        builder.HasOne(atp => atp.UnitType)
            .WithMany()
            .HasForeignKey(atp => atp.Id);
    }
}