using Disciples.Resources.Database.Sqlite.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Disciples.Resources.Database.Sqlite.Extensions;

/// <summary>
/// Методы для настройки строителя EF.
/// </summary>
internal static class EntityBuilderExtensions
{
    /// <summary>
    /// Настроить компонент <see cref="ResourceSet" />.
    /// </summary>
    /// <param name="ownedNavigationBuilder">Строитель.</param>
    /// <param name="propertyName">Имя колонки в основном классе.</param>
    public static void BuildResourceSet<T>(this OwnedNavigationBuilder<T, ResourceSet> ownedNavigationBuilder, string propertyName)
        where T : class
    {
        ownedNavigationBuilder.Property(rs => rs.Gold).HasColumnName($"{propertyName}{nameof(ResourceSet.Gold)}");
        ownedNavigationBuilder.Property(rs => rs.DeathMana).HasColumnName($"{propertyName}{nameof(ResourceSet.DeathMana)}");
        ownedNavigationBuilder.Property(rs => rs.RuneMana).HasColumnName($"{propertyName}{nameof(ResourceSet.RuneMana)}");
        ownedNavigationBuilder.Property(rs => rs.LifeMana).HasColumnName($"{propertyName}{nameof(ResourceSet.LifeMana)}");
        ownedNavigationBuilder.Property(rs => rs.InfernalMana).HasColumnName($"{propertyName}{nameof(ResourceSet.InfernalMana)}");
        ownedNavigationBuilder.Property(rs => rs.GroveMana).HasColumnName($"{propertyName}{nameof(ResourceSet.GroveMana)}");
    }
}