using System.Collections.Generic;
using System.Linq;
using Disciples.Engine.Common.Enums.Units;

namespace Disciples.Engine.Common.Models;

/// <summary>
/// Эффекты, которые действуют на юнита.
/// </summary>
public class UnitEffects
{
    /// <summary>
    /// Эффекты, которые наложены и действуют во время схватки.
    /// </summary>
    private readonly Dictionary<UnitAttackType, UnitBattleEffect> _battleEffects;

    /// <summary>
    /// Создать объект типа <see cref="UnitEffects" />.
    /// </summary>
    public UnitEffects()
    {
        _battleEffects = new Dictionary<UnitAttackType, UnitBattleEffect>();
    }

    /// <summary>
    /// Есть ли на юните эффекты, которые наложены во время битвы.
    /// </summary>
    public bool HasBattleEffects => IsDefended || IsRetreating || _battleEffects.Count > 0;

    /// <summary>
    /// Признак, что юнит защитился.
    /// </summary>
    public bool IsDefended { get; set; }

    /// <summary>
    /// Признак, что юнит собирается сбежать.
    /// </summary>
    public bool IsRetreating { get; set; }

    /// <summary>
    /// Признак, что юнит парализован и не может выполнить ход.
    /// </summary>
    public bool IsParalyzed =>
        ExistsBattleEffect(UnitAttackType.Paralyze) || ExistsBattleEffect(UnitAttackType.Petrify);

    /// <summary>
    /// Добавить эффект в поединке.
    /// </summary>
    public void AddBattleEffect(UnitBattleEffect battleEffect)
    {
        _battleEffects[battleEffect.AttackType] = battleEffect;
    }

    /// <summary>
    /// Проверить, что на юнита наложен эффект указанного типа.
    /// </summary>
    public bool ExistsBattleEffect(UnitAttackType effectAttackType)
    {
        return _battleEffects.ContainsKey(effectAttackType);
    }

    /// <summary>
    /// Удалить эффект указанного типа.
    /// </summary>
    public void Remove(UnitAttackType effectAttackType)
    {
        _battleEffects.Remove(effectAttackType);
    }

    /// <summary>
    /// Получить эффекты, воздействующие на юнита.
    /// </summary>
    public IReadOnlyList<UnitBattleEffect> GetBattleEffects()
    {
        return _battleEffects.Values.ToArray();
    }

    /// <summary>
    /// Удалить все действующие эффекты.
    /// </summary>
    public void Clear()
    {
        IsDefended = false;
        IsRetreating = false;
        _battleEffects.Clear();
    }
}