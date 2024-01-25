using System;
using Disciples.Engine.Common.Enums.Units;

namespace Disciples.Engine.Common.Models;

/// <summary>
/// Защита юнита от источника атак.
/// </summary>
public class UnitAttackSourceProtection
{
    /// <summary>
    /// Создать объект типа <see cref="UnitAttackSourceProtection" />.
    /// </summary>
    public UnitAttackSourceProtection()
    {
    }

    /// <summary>
    /// Создать объект типа <see cref="UnitAttackSourceProtection" />.
    /// </summary>
    public UnitAttackSourceProtection(UnitAttackSource unitAttackSource, ProtectionCategory protectionCategory)
    {
        UnitAttackSource = unitAttackSource;
        ProtectionCategory = protectionCategory;
    }

    /// <summary>
    /// Источник атаки.
    /// </summary>
    public UnitAttackSource UnitAttackSource { get; init; }

    /// <summary>
    /// Категория защиты.
    /// </summary>
    public ProtectionCategory ProtectionCategory { get; init; }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((UnitAttackSourceProtection)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine((int)UnitAttackSource, (int)ProtectionCategory);
    }

    /// <summary>
    /// Сравнить две защиты.
    /// </summary>
    public static bool operator ==(UnitAttackSourceProtection? left, UnitAttackSourceProtection? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Сравнить две защиты.
    /// </summary>
    public static bool operator !=(UnitAttackSourceProtection? left, UnitAttackSourceProtection? right)
    {
        return !Equals(left, right);
    }

    /// <summary>
    /// Сравнить две защиты.
    /// </summary>
    private bool Equals(UnitAttackSourceProtection other)
    {
        return UnitAttackSource == other.UnitAttackSource && ProtectionCategory == other.ProtectionCategory;
    }
}