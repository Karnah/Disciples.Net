using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors;

namespace Disciples.Scene.Battle.Processors;

/// <summary>
/// Обработчик мгновенной битвы.
/// </summary>
internal class BattleInstantProcessor
{
    private readonly BattleProcessor _battleProcessor;
    private readonly BattleAiProcessor _battleAiProcessor;

    /// <summary>
    /// Создать объект типа <see cref="BattleInstantProcessor" />.
    /// </summary>
    public BattleInstantProcessor(BattleProcessor battleProcessor, BattleAiProcessor battleAiProcessor)
    {
        _battleProcessor = battleProcessor;
        _battleAiProcessor = battleAiProcessor;
    }

    /// <summary>
    /// Быстрое завершение битвы
    /// </summary>
    /// <returns>Победивший отряд в битве.</returns>
    public Squad Process(Unit currentUnit, Squad attackingSquad, Squad defendingSquad,
        UnitTurnQueue unitTurnQueue, int roundNumber)
    {
        var instantBattleBeginRoundNumber = roundNumber;

        while (true)
        {
            var winnerSquad = _battleProcessor.GetBattleWinnerSquad(attackingSquad, defendingSquad);
            if (winnerSquad != null)
                return winnerSquad;

            // Обрабатываем ход юнита.
            var isCurrentUnitAttacker = currentUnit.Player.Id == attackingSquad.Player.Id;
            var unitSquad = isCurrentUnitAttacker
                ? attackingSquad
                : defendingSquad;
            var unitEnemySquad = isCurrentUnitAttacker
                ? defendingSquad
                : attackingSquad;
            var command = GetCompleteBattleAiCommand(isCurrentUnitAttacker, roundNumber, instantBattleBeginRoundNumber) ??
                          _battleAiProcessor.GetAiCommand(currentUnit, unitSquad, unitEnemySquad, unitTurnQueue, roundNumber);
            ProcessAiCommand(command, currentUnit, unitSquad, unitEnemySquad, unitTurnQueue, roundNumber);

            winnerSquad = _battleProcessor.GetBattleWinnerSquad(attackingSquad, defendingSquad);
            if (winnerSquad != null)
                return winnerSquad;

            // Обрабатываем вторую атакую юнита, если он бьёт дважды.
            if (currentUnit.UnitType.IsAttackTwice && command.CommandType == BattleCommandType.Attack)
            {
                var secondAttackCommand = _battleAiProcessor.GetAiCommand(currentUnit, unitSquad, unitEnemySquad, unitTurnQueue, roundNumber);
                ProcessAiCommand(secondAttackCommand, currentUnit, unitSquad, unitEnemySquad, unitTurnQueue, roundNumber);

                winnerSquad = _battleProcessor.GetBattleWinnerSquad(attackingSquad, defendingSquad);
                if (winnerSquad != null)
                    return winnerSquad;
            }

            currentUnit = GetNextUnit(attackingSquad, defendingSquad, unitTurnQueue, ref roundNumber);
        }
    }

    /// <summary>
    /// Получить команду, которая должна привести к быстрому завершению битвы.
    /// Она используется в том случае, если победителя определить невозможно, поэтому атакующий отряд должен начать отступать.
    /// </summary>
    private static BattleAiCommand? GetCompleteBattleAiCommand(bool isCurrentUnitAttacker, int roundNumber, int instantBattleBeginRoundNumber)
    {
        const int retreatRoundDiff = 10;
        const int noAttackRoundDiff = 20;

        // Прошло меньше 10 раундов, пока отряды сражаются в прежнем режиме.
        if (roundNumber < instantBattleBeginRoundNumber + retreatRoundDiff)
            return null;

        // Прошло 10 или более раундов, атакующие юниты должны отсутпать.
        if (isCurrentUnitAttacker)
            return new BattleAiCommand(BattleCommandType.Retreat);

        // Прошло менее 20 раундов, защищающиеся юнита атакуют отступающих.
        if (roundNumber < instantBattleBeginRoundNumber + noAttackRoundDiff)
            return null;

        // Прошло 20 или более ходов.
        // Возможно в защищающемся отряде слишком много парализаторов.
        // Уводим их в защиту, чтобы атакующие юниты смогли отступить.
        return new BattleAiCommand(BattleCommandType.Defend);
    }

    /// <summary>
    /// Обработать команду компьютера.
    /// </summary>
    private void ProcessAiCommand(BattleAiCommand command, Unit currentUnit,
        Squad currentUnitSquad, Squad enemySquad,
        UnitTurnQueue unitTurnQueue, int roundNumber)
    {
        switch (command.CommandType)
        {
            case BattleCommandType.Attack:
                var targetUnitSquad = command.Target!.Player.Id == currentUnitSquad.Player.Id
                    ? currentUnitSquad
                    : enemySquad;
                var attackProcessorContext = new AttackProcessorContext(currentUnit, command.Target!,
                    currentUnitSquad, targetUnitSquad,
                    unitTurnQueue, roundNumber);
                ProcessAttack(attackProcessorContext);
                break;

            case BattleCommandType.Defend:
                var defendProcessor = new DefendProcessor(currentUnit);
                defendProcessor.ProcessBeginAction();
                defendProcessor.ProcessCompletedAction();
                break;

            case BattleCommandType.Wait:
                var waitProcessor = new UnitWaitingProcessor(currentUnit, unitTurnQueue);
                waitProcessor.ProcessBeginAction();
                waitProcessor.ProcessCompletedAction();
                break;

            case BattleCommandType.Retreat:
                var retreatProcessor = new UnitRetreatingProcessor(currentUnit);
                retreatProcessor.ProcessBeginAction();
                retreatProcessor.ProcessCompletedAction();
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Обработать атаку.
    /// </summary>
    private void ProcessAttack(AttackProcessorContext attackProcessorContext)
    {
        // Обрабатываем основную атаку.
        var attackResult = _battleProcessor.ProcessMainAttack(attackProcessorContext);
        foreach (var attackProcessor in attackResult.AttackProcessors)
        {
            attackProcessor.ProcessBeginAction();
            attackProcessor.ProcessCompletedAction();
        }

        ProcessDeaths(attackProcessorContext.TargetUnitSquad, attackProcessorContext.UnitTurnQueue, attackProcessorContext.RoundNumber);

        // Обрабатываем вторую атаку.
        var secondAttackProcessors = attackResult
            .SecondaryAttackUnits
            .Select(sau =>
            {
                var secondaryAttackContext = new AttackProcessorContext(attackProcessorContext.CurrentUnit, sau,
                    attackProcessorContext.CurrentUnitSquad, attackProcessorContext.TargetUnitSquad,
                    attackProcessorContext.UnitTurnQueue, attackProcessorContext.RoundNumber);
                return _battleProcessor.ProcessSecondaryAttack(secondaryAttackContext);
            })
            .Where(p => p != null)
            .Select(p => p!)
            .ToArray();
        foreach (var attackProcessor in secondAttackProcessors)
        {
            attackProcessor.ProcessBeginAction();
            attackProcessor.ProcessCompletedAction();
        }

        ProcessDeaths(attackProcessorContext.TargetUnitSquad, attackProcessorContext.UnitTurnQueue, attackProcessorContext.RoundNumber);
    }

    /// <summary>
    /// Получить юнита, который будет ходить следующим.
    /// </summary>
    private Unit GetNextUnit(Squad attackingSquad, Squad defendingSquad,
        UnitTurnQueue unitTurnQueue, ref int roundNumber)
    {
        while (true)
        {
            var currentUnit = unitTurnQueue.GetNextUnit();
            if (currentUnit == null)
            {
                ++roundNumber;
                currentUnit = unitTurnQueue.NextRound(_battleProcessor.GetTurnOrder(attackingSquad, defendingSquad, roundNumber));
            }

            // Этот флаг запоминаем до обработки эффектов, так как после они могут быть сняты.
            // Если паралич снимается на ходу юнита, то он всё равно не совершает действия.
            var isCurrentUnitDisabled = currentUnit.Effects.IsDisabled;

            var currentUnitSquad = currentUnit.Player.Id == attackingSquad.Player.Id
                ? attackingSquad
                : defendingSquad;
            var otherUnitSquad = currentUnit.Player.Id == attackingSquad.Player.Id
                ? defendingSquad
                : attackingSquad;
            ProcessUnitTurn(currentUnit, currentUnitSquad, otherUnitSquad, unitTurnQueue, roundNumber);

            // Если ход текущего юнита завершает битву (он умер или сбежал), то сразу выходим.
            // Не передаём ход следующему юниту, чтобы на нём не сработали эффекты.
            var winnerSquad = _battleProcessor.GetBattleWinnerSquad(attackingSquad, defendingSquad);
            if (winnerSquad != null)
                return currentUnit;

            if (isCurrentUnitDisabled || currentUnit.IsDead)
                continue;

            return currentUnit;
        }
    }

    /// <summary>
    /// Обработать наступление хода юнита.
    /// </summary>
    private void ProcessUnitTurn(Unit currentUnit, Squad currentUnitSquad, Squad otherUnitSquad, UnitTurnQueue unitTurnQueue, int roundNumber)
    {
        var effectProcessors = _battleProcessor.GetEffectProcessors(currentUnit, currentUnitSquad, otherUnitSquad, unitTurnQueue, roundNumber);
        foreach (var effectProcessor in effectProcessors)
        {
            effectProcessor.ProcessBeginAction();
            effectProcessor.ProcessCompletedAction();

            var targetUnit = effectProcessor.TargetUnit;
            var targetUnitSquad = targetUnit.Player.Id == currentUnitSquad.Player.Id
                ? currentUnitSquad
                : otherUnitSquad;
            if (targetUnit.HitPoints == 0 && !targetUnit.IsDead)
                ProcessUnitDeath(targetUnit, targetUnitSquad, unitTurnQueue, roundNumber);
        }
    }

    /// <summary>
    /// Обработать смерти в отряде.
    /// </summary>
    private void ProcessDeaths(Squad squad, UnitTurnQueue unitTurnQueue, int roundNumber)
    {
        // Создаём защитную копию, так как обработка смерти может менять состав юнитов в отряде.
        // Например, это происходит когда юнит был превращён.
        foreach (var unit in squad.Units.ToArray())
        {
            if (unit.HitPoints == 0 && !unit.IsDead)
                ProcessUnitDeath(unit, squad, unitTurnQueue, roundNumber);
        }
    }

    /// <summary>
    /// Обработать смерть юнита.
    /// </summary>
    private void ProcessUnitDeath(Unit unit, Squad unitSquad, UnitTurnQueue unitTurnQueue, int roundNumber)
    {
        var context = new AttackProcessorContext(unit, unit, unitSquad, unitSquad, unitTurnQueue, roundNumber);
        var effectProcessors = _battleProcessor.GetForceCompleteEffectProcessors(context);
        var deathProcessor = new UnitDeathProcessor(unit, effectProcessors);
        deathProcessor.ProcessBeginAction();
        deathProcessor.ProcessCompletedAction();
    }
}