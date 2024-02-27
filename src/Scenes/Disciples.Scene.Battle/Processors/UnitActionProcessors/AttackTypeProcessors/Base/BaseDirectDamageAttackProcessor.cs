using Disciples.Engine;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Models;
using static Disciples.Scene.Battle.Extensions.UnitAttackProcessorExtensions;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;

/// <summary>
/// Базовый обработчик для атак, которые наносят прямой урон.
/// </summary>
internal abstract class BaseDirectDamageAttackProcessor : IAttackTypeProcessor
{
    /// <summary>
    /// Разброс атаки при ударе.
    /// </summary>
    private const int ATTACK_RANGE = 5;

    /// <summary>
    /// Размер критического урона.
    /// </summary>
    private const decimal CRITICAL_DAMAGE_MODIFIER = 0.05M;

    /// <inheritdoc />
    public abstract UnitAttackType AttackType { get; }

    /// <inheritdoc />
    public bool CanMainAttackBeSkipped => false;

    /// <inheritdoc />
    public bool CanAttack(AttackProcessorContext context, CalculatedUnitAttack unitAttack)
    {
        return CanAttackEnemy(context, unitAttack);
    }

    /// <inheritdoc />
    public CalculatedAttackResult CalculateAttackResult(AttackProcessorContext context, CalculatedUnitAttack unitAttack)
    {
        // todo Максимальное значение атаки - 250/300/400.
        var targetUnit = context.TargetUnit;
        var attackRandomBonus = RandomGenerator.Get(ATTACK_RANGE);
        var attackPower = unitAttack.TotalPower + attackRandomBonus;

        // Уменьшаем входящий урон в зависимости от защиты.
        attackPower = (int)(attackPower * (1 - targetUnit.Armor / 100.0));

        // Если юнит защитился, то урон уменьшается в два раза.
        // BUG Механизм хитрее и зависит от брони юнита. Кроме того есть параметр в GVar.
        if (targetUnit.Effects.IsDefended)
            attackPower /= 2;

        // Критический урон вычисляется от базового, без учёта усилений,
        // Но с учётом случайного разброса.
        int? criticalDamage = null;
        if (unitAttack.IsCritical)
            criticalDamage = Math.Min((int)((unitAttack.BasePower + attackRandomBonus) * CRITICAL_DAMAGE_MODIFIER), targetUnit.HitPoints);

        // Мы не можем нанести урон больше, чем осталось очков здоровья.
        attackPower = Math.Min(attackPower, targetUnit.HitPoints - (criticalDamage ?? 0));

        return new CalculatedAttackResult(
            context,
            unitAttack.AttackType,
            unitAttack.AttackSource,
            attackPower,
            criticalDamage);
    }

    /// <inheritdoc />
    public void ProcessAttack(CalculatedAttackResult attackResult)
    {
        var targetUnit = attackResult.Context.TargetUnit;
        var totalDamage = attackResult.Power!.Value + (attackResult.CriticalDamage ?? 0);
        targetUnit.HitPoints -= totalDamage;
    }
}