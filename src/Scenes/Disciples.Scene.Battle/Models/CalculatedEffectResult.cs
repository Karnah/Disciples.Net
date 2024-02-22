using Disciples.Engine.Common.Models;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Вычисленный результат срабатывания эффекта.
/// </summary>
internal class CalculatedEffectResult
{
    /// <summary>
    /// Создать объект типа <see cref="CalculatedEffectResult" />.
    /// </summary>
    public CalculatedEffectResult(AttackProcessorContext context, UnitBattleEffect effect, int? power, bool isForceCompleting, EffectDuration newDuration)
    {
        Context = context;
        Effect = effect;
        Power = power;
        IsForceCompleting = isForceCompleting;
        NewDuration = newDuration;
    }

    /// <summary>
    /// Контекст срабатывания эффекта.
    /// </summary>
    public AttackProcessorContext Context { get; }

    /// <summary>
    /// Эффект, который сработал.
    /// </summary>
    public UnitBattleEffect Effect { get; }

    /// <summary>
    /// Сила эффекта.
    /// </summary>
    public int? Power { get; }

    /// <summary>
    /// Признак, что эффект завершается принудительно.
    /// </summary>
    public bool IsForceCompleting { get; }

    /// <summary>
    /// Длительность эффекта после обработки.
    /// </summary>
    public EffectDuration NewDuration { get; }
}