using Disciples.Resources.Database.Sqlite.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Disciples.Resources.Database.Sqlite.Mappings;

/// <summary>
/// Конфигурация EF для <see cref="GlobalTextResource" />.
/// </summary>
internal class GlobalTextResourceConfiguration : IEntityTypeConfiguration<GlobalTextResource>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<GlobalTextResource> builder)
    {
        builder
            .ToTable(nameof(GlobalTextResource))
            .HasKey(gtr => gtr.Id);
    }
}