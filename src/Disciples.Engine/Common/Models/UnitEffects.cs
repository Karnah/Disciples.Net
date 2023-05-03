using System;
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
    public bool HasBattleEffects => _battleEffects.Count > 0;

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
        _battleEffects.Clear();
    }

    /// <summary>
    /// Получить эффект, который срабатывает при ходе юнита.
    /// </summary>
    public IReadOnlyList<UnitBattleEffect> GetTurnUnitBattleEffect(int currentRound)
    {
        if (_battleEffects.Count == 0)
            return Array.Empty<UnitBattleEffect>();

        ProcessCommonBattleEffects(currentRound);
        return GetProcessingBattleEffect(currentRound);
    }

    /// <summary>
    /// Обработать все эффекты, которые не требуют отдельной обработки.
    /// </summary>
    private void ProcessCommonBattleEffects(int currentRound)
    {
        // TODO Избавиться от ToList().
        foreach (var battleEffect in _battleEffects.Values.ToList())
        {
            if (ShouldProcessEffectType(battleEffect.AttackType))
                continue;

            battleEffect.RoundTriggered = currentRound;
            battleEffect.RoundDuration -= 1;

            if (battleEffect.RoundDuration <= 0)
                _battleEffects.Remove(battleEffect.AttackType);
        }
    }

    /// <summary>
    /// Добавить обрабатываемые эффект юнита.
    /// </summary>
    private IReadOnlyList<UnitBattleEffect> GetProcessingBattleEffect(int currentRound)
    {
        var processingBattleEffects = _battleEffects
            .Values
            .Where(be => be.RoundDuration > 0 && be.RoundTriggered < currentRound && ShouldProcessEffectType(be.AttackType))
            .ToArray();
        foreach (var battleEffect in processingBattleEffects)
        {
            battleEffect.RoundTriggered = currentRound;
            battleEffect.RoundDuration -= 1;

            if (battleEffect.RoundDuration <= 0)
                _battleEffects.Remove(battleEffect.AttackType);
        }

        return processingBattleEffects;
    }

    /// <summary>
    /// Проверить, необходимо ли отдельно обрабатывать эффект битвы.
    /// </summary>
    private static bool ShouldProcessEffectType(UnitAttackType effectAttackType)
    {
        return effectAttackType is UnitAttackType.Poison
            or UnitAttackType.Frostbite
            or UnitAttackType.Blister;
    }
}