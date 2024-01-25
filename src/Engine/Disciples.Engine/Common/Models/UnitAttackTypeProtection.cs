using System;
using Disciples.Engine.Common.Enums.Units;

namespace Disciples.Engine.Common.Models;

/// <summary>
/// Защита юнита от типа атаки.
/// </summary>
public class UnitAttackTypeProtection
{
    /// <summary>
    /// Создать объект типа <see cref="UnitAttackTypeProtection" />
    /// </summary>
    public UnitAttackTypeProtection()
    {
    }

    /// <summary>
    /// Создать объект типа <see cref="UnitAttackTypeProtection" />
    /// </summary>
    public UnitAttackTypeProtection(UnitAttackType unitAttackType, ProtectionCategory protectionCategory)
    {
        UnitAttackType = unitAttackType;
        ProtectionCategory = protectionCategory;
    }

    /// <summary>
    /// Источник атаки.
    /// </summary>
    public UnitAttackType UnitAttackType { get; init; }

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
        return Equals((UnitAttackTypeProtection)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine((int)UnitAttackType, (int)ProtectionCategory);
    }

    /// <summary>
    /// Сравнить две защиты.
    /// </summary>
    public static bool operator ==(UnitAttackTypeProtection? left, UnitAttackTypeProtection? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Сравнить две защиты.
    /// </summary>
    public static bool operator !=(UnitAttackTypeProtection? left, UnitAttackTypeProtection? right)
    {
        return !Equals(left, right);
    }

    /// <summary>
    /// Сравнить две защиты.
    /// </summary>
    private bool Equals(UnitAttackTypeProtection other)
    {
        return UnitAttackType == other.UnitAttackType && ProtectionCategory == other.ProtectionCategory;
    }
}