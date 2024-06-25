using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Extensions;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors;

/// <summary>
/// Процессор для атаки типа <see cref="UnitAttackType.Summon" />.
/// </summary>
internal class SummonAttackProcessor : BaseEffectAttackProcessor
{
    /// <inheritdoc />
    public override UnitAttackType AttackType => UnitAttackType.Summon;

    /// <inheritdoc />
    protected override bool CanAttackFriends => true;

    /// <inheritdoc />
    public override bool CanAttack(AttackProcessorContext context, CalculatedUnitAttack unitAttack)
    {
        return base.CanAttack(context, unitAttack) &&
               context.TargetUnit is SummonedUnit;
    }

    /// <inheritdoc />
    public override void ProcessAttack(CalculatedAttackResult attackResult)
    {
        // Добавляем призванного юнита в отряд.
        // И удаляем мертвых/отступивших юнитов на место которых вызван юнит.
        var squad = attackResult.Context.TargetUnitSquad;
        var summonedUnit = (SummonedUnit)attackResult.Context.TargetUnit;
        squad.Units.Add(summonedUnit);

        foreach (var hiddenUnit in summonedUnit.HiddenUnits)
            squad.Units.Remove(hiddenUnit);

        base.ProcessAttack(attackResult);
    }

    /// <inheritdoc />
    protected override void ProcessEffectCompleted(AttackProcessorContext context, UnitBattleEffect battleEffect)
    {
        base.ProcessEffectCompleted(context, battleEffect);

        // Удаляем призванного юнита из отряда и возвращаем тех юнитов, которые были перекрыты.
        var squad = context.TargetUnitSquad;
        var summonedUnit = (SummonedUnit)context.TargetUnit;
        summonedUnit.IsUnsummoned = true;
        squad.Units.Remove(summonedUnit);

        foreach (var hiddenUnit in summonedUnit.HiddenUnits)
        {
            // Если вызванный юнит маленький и перекрывает большого юнита, то нужно проверить вторую клетку в линии.
            // Если там тоже призванный юнит, то пока возвращать исходного юнита на место нельзя.
            if (summonedUnit.UnitType.IsSmall && !hiddenUnit.UnitType.IsSmall)
            {
                var otherLinePosition = summonedUnit.Position.GetOtherLine();
                var hasOtherSummonedUnit = squad.IsPositionBusy(otherLinePosition, u => u is SummonedUnit);
                if (hasOtherSummonedUnit)
                    continue;
            }

            squad.Units.Add(hiddenUnit);
        }
    }

    /// <inheritdoc />
    protected override EffectDuration GetEffectDuration(CalculatedUnitAttack unitAttack, bool isMaximum)
    {
        return unitAttack.IsInfinitive
            ? EffectDuration.CreateInfinitive()
            : EffectDuration.Create(2);
    }

    /// <summary>
    /// Получить юниты, которые будут вызваны.
    /// </summary>
    public IReadOnlyList<Unit> GetSummonedUnits(BattleProcessorContext context, UnitSquadPosition summonPosition)
    {
        var currentUnit = context.CurrentUnit;
        var summonPositions = GetSummonPositions(context);
        var summonUnitTypes = context.CurrentUnit.UnitType.MainAttack.SummonTransformUnitTypes;
        var bigSummonUnitTypes = summonUnitTypes.Where(ut => !ut.IsSmall).ToList();
        var smallSummonUnitTypes = summonUnitTypes.Where(ut => ut.IsSmall).ToList();
        var summonUnits = new List<Unit>();
        foreach (var position in summonPositions.Where(p => currentUnit.MainAttack.Reach == UnitAttackReach.All || p == summonPosition))
        {
            // Позиция могла быть уже занята ранее большим юнитом.
            if (bigSummonUnitTypes.Count > 0 && summonUnits.Any(su=> su.Position.IsIntersect(position)))
                continue;

            // Если юнит имеет вызывать больших существ и есть место для него.
            var canSummonBigUnit = bigSummonUnitTypes.Count > 0 &&
                                   summonPositions.Any(sp => sp == position.GetOtherLine());
            var summonUnitType = canSummonBigUnit
                ? bigSummonUnitTypes.GetRandomElement()
                : smallSummonUnitTypes.GetRandomElement();
            summonUnits.Add(GetSummonedUnit(summonUnitType, position, context.CurrentUnitSquad));
        }

        return summonUnits;
    }

    /// <summary>
    /// Получить возможные расположения для вызова юнитов.
    /// </summary>
    public IReadOnlyList<UnitSquadPosition> GetSummonPositions(BattleProcessorContext context)
    {
        var currentUnitSquad = context.CurrentUnitSquad;
        return UnitSquadPositionExtensions
            .Positions
            .Where(p => currentUnitSquad.IsPositionEmpty(p, u => !u.IsInactive))
            .ToList();
    }

    /// <summary>
    /// Получить вызванного юнита.
    /// </summary>
    private static SummonedUnit GetSummonedUnit(UnitType unitType, UnitSquadPosition position, Squad squad)
    {
        position = unitType.IsSmall
            ? position
            : new UnitSquadPosition(UnitSquadLinePosition.Both, position.Flank);

        // Вычисляем какие юниты перекрываются вызываемым юнитом.
        // Также отдельно обрабатываем следующий случай: в отряде был большой юнит, он умер/сбежал.
        // Одна его клетка уже занята вызванным юнитом, сейчас занимаем вторую. Для нового юнита также будет хранить ссылку на исходного юнита.
        var hiddenUnits = squad.GetUnits(position).ToList();
        if (unitType.IsSmall)
        {
            var otherLinePosition = position.GetOtherLine();
            var otherSummonedUnit = squad.GetUnits(otherLinePosition).OfType<SummonedUnit>().FirstOrDefault();
            var bigHiddenUnit = otherSummonedUnit?.HiddenUnits.FirstOrDefault(hu => !hu.UnitType.IsSmall);
            if (bigHiddenUnit != null)
                hiddenUnits.Add(bigHiddenUnit);
        }

        return new SummonedUnit(unitType, squad.Player, squad, position, hiddenUnits);
    }
}