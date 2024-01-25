using Disciples.Resources.Database.Sqlite.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Disciples.Resources.Database.Sqlite.Configurations;

/// <summary>
/// Конфигурация EF для <see cref="InterfaceTextResource" />.
/// </summary>
internal class InterfaceTextResourceConfiguration : IEntityTypeConfiguration<InterfaceTextResource>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<InterfaceTextResource> builder)
    {
        builder
            .ToTable(nameof(InterfaceTextResource))
            .HasKey(itr => itr.Id);
    }
}