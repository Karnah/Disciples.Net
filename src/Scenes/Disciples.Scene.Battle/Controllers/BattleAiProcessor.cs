using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Extensions;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Models;

namespace Disciples.Scene.Battle.Controllers;

/// <summary>
/// Управление компьютером для битвы.
/// </summary>
internal class BattleAiProcessor
{
    private readonly BattleProcessor _battleProcessor;

    /// <summary>
    /// Создать объект типа <see cref="BattleAiProcessor" />.
    /// </summary>
    public BattleAiProcessor(BattleProcessor battleProcessor)
    {
        _battleProcessor = battleProcessor;
    }

    /// <summary>
    /// Получить команду следующего хода от компьютера.
    /// </summary>
    /// <param name="attackingUnit">Атакующий юнит.</param>
    /// <param name="attackingSquad">Отряд атакующего юнита.</param>
    /// <param name="defendingSquad">Защищающийся отряд.</param>
    /// <returns>Команда для юнита.</returns>
    public BattleAiCommand GetAiCommand(Unit attackingUnit, Squad attackingSquad, Squad defendingSquad)
    {
        if (attackingUnit.HasAllyAbility())
            return GetOwnSquadCommand(attackingUnit, attackingSquad, defendingSquad);

        return GetEnemySquadCommand(attackingUnit, attackingSquad, defendingSquad);
    }

    /// <summary>
    /// Быстрое завершение битвы
    /// </summary>
    /// <remarks>
    /// TODO Просто оставляем всех атакующих юнитов с 1 ХП.
    /// Нужно использовать последовательные выводы GetAiCommand и обработки атак/результатов.
    /// Плюс получать очередность ходов в качестве параметра.
    ///
    /// TODO Также не очень хорошо, что меняется Unit напрямую. Хотя, может и норм.
    /// </remarks>
    /// <returns>Победивший отряд в битве.</returns>
    public Squad ProcessInstantBattle(Squad attackingSquad, Squad defendingSquad)
    {
        if (attackingSquad.Units.All(u => u.IsDead))
            return defendingSquad;

        if (defendingSquad.Units.All(u => u.IsDead))
            return attackingSquad;

        foreach (var attackingSquadUnit in attackingSquad.Units)
        {
            if (attackingSquadUnit.IsDead)
                continue;

            attackingSquadUnit.HitPoints = 1;
        }

        foreach (var defendingSquadUnit in defendingSquad.Units)
        {
            if (defendingSquadUnit.IsDead)
                continue;

            defendingSquadUnit.HitPoints = 0;
            defendingSquadUnit.IsDead = true;
        }

        return attackingSquad;
    }

    /// <summary>
    /// Получить команду для использования на союзниках.
    /// </summary>
    /// <param name="attackingUnit">Атакующий юнит.</param>
    /// <param name="attackingSquad">Отряд атакующего юнита.</param>
    /// <param name="defendingSquad">Защищающийся отряд.</param>
    /// <returns>Команда для юнита.</returns>
    private BattleAiCommand GetOwnSquadCommand(Unit attackingUnit, Squad attackingSquad, Squad defendingSquad)
    {
        if (attackingUnit.UnitType.MainAttack.AttackType == UnitAttackType.Heal)
        {
            var target = attackingSquad
                .Units
                .Where(u => u.HitPoints < u.MaxHitPoints)
                .MinBy(u => u.HitPoints);
            if (target != null)
                return new BattleAiCommand { CommandType = BattleCommandType.Attack, Target = target };
        }

        return new BattleAiCommand { CommandType = BattleCommandType.Defend };
    }

    /// <summary>
    /// Получить команду для использования на врагах.
    /// </summary>
    /// <param name="attackingUnit">Атакующий юнит.</param>
    /// <param name="attackingSquad">Отряд атакующего юнита.</param>
    /// <param name="defendingSquad">Защищающийся отряд.</param>
    /// <returns>Команда для юнита.</returns>
    private BattleAiCommand GetEnemySquadCommand(Unit attackingUnit, Squad attackingSquad, Squad defendingSquad)
    {
        var availableForAttackUnits =
            defendingSquad
                .Units
                .Where(u => _battleProcessor.CanAttack(attackingUnit, attackingSquad, u, defendingSquad))
                .ToList();

        if (availableForAttackUnits.Count == 0)
            return new BattleAiCommand { CommandType = BattleCommandType.Defend };

        var target = availableForAttackUnits
            .OrderBy(a => a.HitPoints)
            .First();
        return new BattleAiCommand
        {
            CommandType = BattleCommandType.Attack,
            Target = target
        };
    }
}