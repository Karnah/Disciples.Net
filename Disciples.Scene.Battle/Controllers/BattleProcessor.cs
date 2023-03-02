using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Models;
using Disciples.Engine;
using Disciples.Engine.Extensions;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Models;

namespace Disciples.Scene.Battle.Controllers;

/// <summary>
/// Обработчик битвы.
/// </summary>
internal class BattleProcessor
{
    /// <summary>
    /// Разброс инициативы при вычислении очередности.
    /// </summary>
    private const int INITIATIVE_RANGE = 5;

    /// <summary>
    /// Разброс атаки при ударе.
    /// </summary>
    private const int ATTACK_RANGE = 5;

    #region Очерёдность ходов

    /// <summary>
    /// Получить очередность ходов юнитов.
    /// </summary>
    /// <param name="attackingSquad">Атакующий отряд.</param>
    /// <param name="defendingSquad">Защищающийся отряд.</param>
    public Queue<Unit> GetTurnOrder(Squad attackingSquad, Squad defendingSquad)
    {
        return new Queue<Unit>(
            attackingSquad.Units
                .Concat(defendingSquad.Units)
                .Where(u => !u.IsDead)
                .OrderByDescending(u => u.Initiative + RandomGenerator.Get(0, INITIATIVE_RANGE)));
    }

    #endregion

    #region Проверка возможности атаковать

    /// <summary>
    /// Проверить, может ли атаковать один юнит другого.
    /// </summary>
    /// <param name="attackingUnit">Атакующий юнит.</param>
    /// <param name="attackingSquad">Отряд атакующего юнита.</param>
    /// <param name="targetUnit">Цель.</param>
    /// <param name="targetSquad">Отряд цели.</param>
    public bool CanAttack(Unit attackingUnit, Squad attackingSquad, Unit targetUnit, Squad targetSquad)
    {
        if (attackingUnit.IsDead)
            return false;

        // Лекарь не может атаковать врага, а воин не может атаковать союзника.
        if (attackingUnit.Player == targetUnit.Player && attackingUnit.HasAllyAbility() == false ||
            attackingUnit.Player != targetUnit.Player && attackingUnit.HasEnemyAbility() == false)
        {
            return false;
        }

        // BUG Патриарх может воскресить юнита, так что эта проверка не совсем корректна.
        // Если юнит бьёт по площади, то разрешаем кликнуть на мертвого юнита.
        if (targetUnit.IsDead && attackingUnit.UnitType.MainAttack.Reach != Reach.All)
            return false;

        // Лекарь по одиночной цели без второй атаки может лечить только тех,
        // у кого меньше максимального значения здоровья.
        if (attackingUnit.Player == targetUnit.Player &&
            attackingUnit.UnitType.MainAttack.AttackClass == AttackClass.Heal &&
            attackingUnit.UnitType.MainAttack.Reach == Reach.Any &&
            attackingUnit.UnitType.SecondaryAttack == null &&
            targetUnit.HitPoints == targetUnit.UnitType.HitPoints)
        {
            return false;
        }

        // Если юнит может атаковать только ближайшего, то проверяем препятствия.
        if (attackingUnit.UnitType.MainAttack.Reach == Reach.Adjacent)
        {
            // Если атакующий юнит находится сзади и есть линия союзников впереди.
            if (attackingUnit.SquadLinePosition == 0 && IsFirstLineEmpty(attackingSquad) == false)
                return false;

            // Если враг находится сзади, то проверяем, что нет первой вражеской линии.
            if (targetUnit.SquadLinePosition == 0 && IsFirstLineEmpty(targetSquad) == false)
                return false;

            // Проверка, может ли юнит дотянуться до врага.
            if (CanAttackOnFlank(
                    attackingUnit.SquadFlankPosition,
                    targetUnit.SquadFlankPosition,
                    targetUnit.SquadLinePosition,
                    targetSquad) == false)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Проверить, свободна ли первая линия в отряде.
    /// </summary>
    private static bool IsFirstLineEmpty(Squad squad)
    {
        return !squad.Units.Any(u => u.SquadLinePosition == 1 && !u.IsDead);
    }

    /// <summary>
    /// Проверить, можно ли атаковать цель в зависимости от расположения на фланге.
    /// </summary>
    private static bool CanAttackOnFlank(
        int currentUnitFlankPosition,
        int targetUnitFlankPosition,
        int targetUnitLinePosition,
        Squad targetSquad)
    {
        // Если юниты находятся по разные стороны флагов и занят вражеский центр или соседняя с атакующим клетка, то атаковать нельзя.
        if (Math.Abs(currentUnitFlankPosition - targetUnitFlankPosition) > 1 &&
            (IsPlaceEmpty(targetSquad, targetUnitLinePosition, 1) == false || IsPlaceEmpty(targetSquad, targetUnitLinePosition, currentUnitFlankPosition) == false))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Проверить, свободна ли клетка на арене.
    /// </summary>
    private static bool IsPlaceEmpty(Squad squad, int line, int flank)
    {
        return squad.Units.Any(u => u.SquadLinePosition == line &&
                                    u.SquadFlankPosition == flank &&
                                    u.IsDead == false) == false;
    }

    #endregion

    #region Рассчет атаки

    /// <summary>
    /// Выполнить одну атаку юнита на другого с помощью основной атаки.
    /// </summary>
    public BattleProcessorAttackResult? ProcessMainAttack(Unit attackingUnit, Unit targetUnit)
    {
        var power = attackingUnit.FirstAttackPower;
        var attack = attackingUnit.UnitType.MainAttack;
        var accuracy = attackingUnit.MainAttackAccuracy;
        return ProcessAttack(targetUnit, attack, power, accuracy);
    }

    /// <summary>
    /// Выполнить одну атаку юнита на другого с помощью второстепенной атаки.
    /// </summary>
    public BattleProcessorAttackResult? ProcessSecondaryAttack(Unit attackingUnit, Unit targetUnit, int? externalPower)
    {
        var power = externalPower ?? attackingUnit.SecondAttackPower;
        var attack = attackingUnit.UnitType.SecondaryAttack!;
        var accuracy = attackingUnit.SecondaryAttackAccuracy!.Value;
        return ProcessAttack(targetUnit, attack, power, accuracy);
    }

    /// <summary>
    /// Обработать действие одного юнита на другого.
    /// </summary>
    /// <param name="targetUnit">Юнит, на которого воздействует.</param>
    /// <param name="attack">Тип атаки.</param>
    /// <param name="power">Сила атаки.</param>
    /// <param name="accuracy">Точность атаки.</param>
    private static BattleProcessorAttackResult? ProcessAttack(Unit targetUnit, Attack attack, int? power, int accuracy)
    {
        // Единственная атаки, которая действует на мёртвых юнитов - воскрешение и призыв.
        if (targetUnit.IsDead
            && attack.AttackClass is not AttackClass.Revive or AttackClass.Summon)
        {
            return null;
        }

        // Проверяем меткость юнита.
        var chanceOfFirstAttack = RandomGenerator.Get(0, 100);
        if (chanceOfFirstAttack > accuracy)
            return new BattleProcessorAttackResult(AttackResult.Miss);

        // todo Сразу обработать иммунитет + сопротивления. Также вернуть результат.
        // Вторая атака не будет действовать, если первая упёрлась в иммунитет.

        switch (attack.AttackClass)
        {
            case AttackClass.Damage:
                // todo Максимальное значение атаки - 250/300/400.
                var attackPower = power!.Value + RandomGenerator.Get(ATTACK_RANGE);

                // Уменьшаем входящий урон в зависимости от защиты.
                attackPower = (int)(attackPower * (1 - targetUnit.Armor / 100.0));

                // Если юнит защитился, то урон уменьшается в два раза.
                if (targetUnit.Effects.ExistsBattleEffect(UnitBattleEffectType.Defend))
                {
                    attackPower /= 2;
                }

                // Мы не можем нанести урон больше, чем осталось очков здоровья.
                attackPower = Math.Min(attackPower, targetUnit.HitPoints);
                return new BattleProcessorAttackResult(
                    AttackResult.Attack,
                    attackPower,
                    attack.AttackClass);

            case AttackClass.Drain:
            case AttackClass.Paralyze:
                break;

            case AttackClass.Heal:
                var healPower = Math.Min(power!.Value, targetUnit.MaxHitPoints - targetUnit.HitPoints);
                if (healPower != 0)
                {
                    return new BattleProcessorAttackResult(
                        AttackResult.Heal,
                        healPower,
                        attack.AttackClass);
                }

                break;

            case AttackClass.Fear:
            case AttackClass.BoostDamage:
            case AttackClass.Petrify:
            case AttackClass.LowerDamage:
            case AttackClass.LowerInitiative:
                break;

            case AttackClass.Poison:
            case AttackClass.Frostbite:
            case AttackClass.Blister:
                return new BattleProcessorAttackResult(
                    AttackResult.Effect,
                    power,
                    2,
                    attack.AttackClass);

            case AttackClass.Revive:
            case AttackClass.DrainOverflow:
            case AttackClass.Cure:
            case AttackClass.Summon:
            case AttackClass.DrainLevel:
            case AttackClass.GiveAttack:
            case AttackClass.Doppelganger:
            case AttackClass.TransformSelf:
            case AttackClass.TransformOther:
            case AttackClass.BestowWards:
            case AttackClass.Shatter:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return null;
    }

    #endregion

    #region Проверка окончания битвы

    // TODO Если в отряде есть целитель, то перед завершением битвы ему даётся ход.

    /// <summary>
    /// Проверить битва завершилась победой одной из сторон.
    /// </summary>
    /// <param name="attackingSquad">Атакующий отряд.</param>
    /// <param name="defendingSquad">Защищающийся отряд.</param>
    public bool IsBattleCompleted(Squad attackingSquad, Squad defendingSquad)
    {
        return
            attackingSquad.Units.All(u => u.IsDead)
            || defendingSquad.Units.All(u => u.IsDead);
    }

    #endregion
}