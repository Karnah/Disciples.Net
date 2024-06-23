using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Models;
using static Disciples.Scene.Battle.Extensions.UnitAttackProcessorExtensions;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;

/// <summary>
/// Базовый класс для накладывания эффектов.
/// </summary>
internal abstract class BaseEffectAttackProcessor : IEffectAttackProcessor
{
    /// <inheritdoc />
    public abstract UnitAttackType AttackType { get; }

    /// <inheritdoc />
    /// <remarks>
    /// Можно пропускать основные атаки, если они воздействуют только на союзников.
    /// </remarks>
    public bool CanMainAttackBeSkipped => CanAttackFriends && !CanAttackEnemies;

    /// <inheritdoc />
    public bool CanAttackAfterBattle => false;

    /// <summary>
    /// Можно ли атаковать данным типом атаки врагов.
    /// </summary>
    protected virtual bool CanAttackEnemies => false;

    /// <summary>
    /// Можно ли атаковать данным типом атаки друзей.
    /// </summary>
    protected virtual bool CanAttackFriends => false;

    /// <summary>
    /// Признак, что на юнита может быть наложен только один эффект данного типа.
    /// </summary>
    /// <remarks>
    /// Единственный эффект, который может быть наложен сразу несколько раз - <see cref="UnitAttackType.GiveProtection" />.
    /// </remarks>
    protected virtual bool IsSingleEffectOnly => true;

    /// <inheritdoc />
    public virtual bool CanAttack(AttackProcessorContext context, CalculatedUnitAttack unitAttack)
    {
        // Если атака распространяется на врагов, то проверяем, что цель враг и до неё можно достать.
        // Если атаки распространяется на союзников, то проверяем что союзник.
        var canAttackEnemy = CanAttackEnemies && CanAttackEnemy(context, unitAttack);
        var canAttackFriend = CanAttackFriends && CanAttackFriend(context);
        if (!canAttackEnemy && !canAttackFriend)
            return false;

        // Если такой эффект на юните уже есть, то проверяем, что его можно заменить.
        if (IsSingleEffectOnly)
        {
            var existingEffect = context
                .TargetUnit
                .Effects
                .GetBattleEffects(unitAttack.AttackType)
                .SingleOrDefault();
            if (existingEffect != null)
                return CanReplaceEffect(existingEffect, GetPower(context, unitAttack), GetEffectDuration(unitAttack, true));
        }

        return true;
    }

    /// <inheritdoc />
    public virtual CalculatedAttackResult CalculateAttackResult(AttackProcessorContext context, CalculatedUnitAttack unitAttack)
    {
        var attackingUnit = context.CurrentUnit;
        var targetUnit = context.TargetUnit;
        return new CalculatedAttackResult(
            context,
            unitAttack.AttackType,
            unitAttack.AttackSource,
            GetPower(context, unitAttack),
            GetEffectDuration(unitAttack, false),
            GetEffectDurationControlUnit(attackingUnit, targetUnit),
            GetAttackTypeProtections(unitAttack, targetUnit),
            GetAttackSourceProtections(unitAttack, targetUnit),
            GetTransformedUnit(attackingUnit, targetUnit, unitAttack));
    }

    /// <inheritdoc />
    public bool CanCure(UnitBattleEffect battleEffect)
    {
        if (battleEffect.AttackType != AttackType)
            throw new ArgumentException($"Обработчик атаки типа {AttackType} не может обработать эффект типа {battleEffect.AttackType}", nameof(battleEffect));

        if (battleEffect.Duration.IsInfinitive)
            return false;

        // Можно вылечить только те эффекты, которые действуют только на врагов.
        return CanAttackEnemies && !CanAttackFriends;
    }

    /// <inheritdoc />
    public virtual void ProcessAttack(CalculatedAttackResult attackResult)
    {
        var targetUnitEffects = attackResult.Context.TargetUnit.Effects;

        if (IsSingleEffectOnly)
        {
            var existingEffect = targetUnitEffects
                .GetBattleEffects(attackResult.AttackType)
                .SingleOrDefault();
            if (existingEffect != null)
            {
                if (!CanReplaceEffect(existingEffect, attackResult.Power, attackResult.EffectDuration!))
                    return;

                targetUnitEffects.Remove(existingEffect);
            }
        }

        var newBattleEffect = new UnitBattleEffect(
            attackResult.AttackType,
            attackResult.AttackSource,
            attackResult.EffectDuration!,
            attackResult.EffectDurationControlUnit!,
            attackResult.Power,
            attackResult.AttackTypeProtections,
            attackResult.AttackSourceProtections);
        targetUnitEffects.AddBattleEffect(newBattleEffect);
    }

    /// <inheritdoc />
    public virtual CalculatedEffectResult? CalculateEffect(AttackProcessorContext context,
        UnitBattleEffect battleEffect, bool isForceCompleting)
    {
        if (battleEffect.AttackType != AttackType)
            throw new ArgumentException($"Обработчик атаки типа {AttackType} не может обработать эффект типа {battleEffect.AttackType}", nameof(battleEffect));

        if (isForceCompleting)
            return new CalculatedEffectResult(context, battleEffect, null, true, EffectDuration.Completed);

        var targetUnit = context.TargetUnit;

        // Эффект срабатывает на ходу другого юнита.
        if (battleEffect.DurationControlUnit.Id != context.CurrentUnit.Id)
            return null;

        // Эффект уже срабатывал в этом раунде.
        if (battleEffect.RoundTriggered == context.RoundNumber)
            return null;

        var newEffectDuration = battleEffect.Duration.Clone();
        newEffectDuration.DecreaseTurn();

        return new CalculatedEffectResult(context, battleEffect, GetEffectPower(targetUnit, battleEffect), isForceCompleting, newEffectDuration);
    }

    /// <inheritdoc />
    public virtual void ProcessEffect(CalculatedEffectResult effectResult)
    {
        var battleEffect = effectResult.Effect;

        if (effectResult.IsForceCompleting)
            battleEffect.Duration.Complete();
        else
            battleEffect.Duration.DecreaseTurn();

        if (battleEffect.Duration.IsCompleted)
            ProcessEffectCompleted(effectResult.Context, battleEffect);
    }

    /// <summary>
    /// Получить силу атаки.
    /// </summary>
    protected virtual int GetPower(AttackProcessorContext context, CalculatedUnitAttack unitAttack)
    {
        return unitAttack.TotalPower;
    }

    /// <summary>
    /// Получить длительность эффекта.
    /// </summary>
    /// <param name="unitAttack">Данные атаки.</param>
    /// <param name="isMaximum">Признак, что нужно отдать максимальную длительность эффекта для данного типа атаки.</param>
    protected abstract EffectDuration GetEffectDuration(CalculatedUnitAttack unitAttack, bool isMaximum);

    /// <summary>
    /// Получить силу срабатывания эффекта.
    /// </summary>
    protected virtual int? GetEffectPower(Unit targetUnit, UnitBattleEffect effect)
    {
        return null;
    }

    /// <summary>
    /// Получить список защит от источников атак.
    /// </summary>
    protected virtual IReadOnlyList<UnitAttackTypeProtection> GetAttackTypeProtections(CalculatedUnitAttack unitAttack,
        Unit targetUnit)
    {
        return Array.Empty<UnitAttackTypeProtection>();
    }

    /// <summary>
    /// Получить список защит от источников атак.
    /// </summary>
    protected virtual IReadOnlyList<UnitAttackSourceProtection> GetAttackSourceProtections(
        CalculatedUnitAttack unitAttack, Unit targetUnit)
    {
        return Array.Empty<UnitAttackSourceProtection>();
    }

    /// <summary>
    /// Получить юнита после трансформации.
    /// </summary>
    protected virtual ITransformedUnit? GetTransformedUnit(Unit attackingUnit,
        Unit targetUnit, CalculatedUnitAttack unitAttack)
    {
        return null;
    }

    /// <summary>
    /// Можно ли заменить эффект, который уже действует на юните.
    /// </summary>
    protected virtual bool CanReplaceEffect(UnitBattleEffect existingBattleEffect, int? newEffectPower, EffectDuration newEffectDuration)
    {
        var isNewEffectLonger = !existingBattleEffect.Duration.IsInfinitive &&
                                existingBattleEffect.Duration.Turns < newEffectDuration!.Turns;

        if (existingBattleEffect.Power != null && newEffectPower != null)
        {
            // Новый эффект сильнее, нужно заменить.
            if (existingBattleEffect.Power < newEffectPower)
                return true;

            // Если сила эффектов равна, то заменять нужно только в том случае, если новый длится дольше.
            if (existingBattleEffect.Power == newEffectPower)
                return isNewEffectLonger;

            // Не заменяем сильный эффект на слабый, даже если он длится дольше.
            return false;
        }

        return isNewEffectLonger;
    }

    /// <summary>
    /// Обработать завершение действия эффекта.
    /// </summary>
    protected virtual void ProcessEffectCompleted(AttackProcessorContext context, UnitBattleEffect battleEffect)
    {
        context.TargetUnit.Effects.Remove(battleEffect);
    }

    /// <summary>
    /// Получить юнита, к которому привязан срок действия эффекта.
    /// </summary>
    private Unit GetEffectDurationControlUnit(Unit attackingUnit, Unit targetUnit)
    {
        return CanAttackEnemies
            ? targetUnit
            : attackingUnit;
    }
}