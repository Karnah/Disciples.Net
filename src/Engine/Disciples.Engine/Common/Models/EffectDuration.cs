using System;

namespace Disciples.Engine.Common.Models;

/// <summary>
/// Длительность эффекта.
/// </summary>
public class EffectDuration
{
    /// <summary>
    /// Эффект, который закончил действие.
    /// </summary>
    public static EffectDuration Completed { get; } = new() { IsCompleted = true };

    /// <summary>
    /// Создать объект типа <see cref="EffectDuration" /> с указанным количеством ходов.
    /// </summary>
    public static EffectDuration Create(int turns)
    {
        if (turns <= 0)
            throw new ArgumentException("Количество ходов до завершения эффекта должно быть больше 0", nameof(turns));

        return new EffectDuration { Turns = turns };
    }

    /// <summary>
    /// Создать объект типа <see cref="EffectDuration" /> с случайным количеством ходов из диапазона.
    /// </summary>
    public static EffectDuration CreateRandom(int minTurns, int maxTurns)
    {
        if (minTurns <= 0)
            throw new ArgumentException("Минимальное количество ходов до завершения эффекта должно быть больше 0", nameof(minTurns));
        if (maxTurns < minTurns)
            throw new ArgumentException("Максимальное количество ходов до завершения эффекта должно быть больше минимального", nameof(maxTurns));

        return new EffectDuration { Turns = RandomGenerator.Get(minTurns, maxTurns + 1) };
    }

    /// <summary>
    /// Создать объект типа <see cref="EffectDuration" /> с бесконечным эффектом.
    /// </summary>
    public static EffectDuration CreateInfinitive()
    {
        return new EffectDuration { IsInfinitive = true, Turns = int.MaxValue };
    }

    /// <summary>
    /// Создать объект типа <see cref="EffectDuration" />
    /// </summary>
    private EffectDuration()
    {
    }

    /// <summary>
    /// Признак, что эффект действует бесконечно.
    /// </summary>
    public bool IsInfinitive { get; private set; }

    /// <summary>
    /// Признак, что эффект завершился.
    /// </summary>
    public bool IsCompleted { get; private set; }

    /// <summary>
    /// Количество ходов, которое действует эффект.
    /// </summary>
    public int Turns { get; private set; }

    /// <summary>
    /// Уменьшить количество оставшихся кодов на 1.
    /// </summary>
    public void DecreaseTurn()
    {
        if (IsInfinitive)
            return;

        Turns--;

        if (Turns <= 0)
            IsCompleted = true;
    }

    /// <summary>
    /// Принудительно завершить действие эффекта.
    /// </summary>
    public void Complete()
    {
        IsInfinitive = false;
        IsCompleted = true;
        Turns = 0;
    }

    /// <summary>
    /// Создать копию.
    /// </summary>
    public EffectDuration Clone()
    {
        return (EffectDuration)MemberwiseClone();
    }
}