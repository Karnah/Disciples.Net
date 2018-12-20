using Engine.Battle.Enums;

namespace Engine.Battle.Models
{
    /// <summary>
    /// Мгновенный эффект, произошедший с юнитом.
    /// </summary>
    public class UnitMomentalEffect
    {
        /// <inheritdoc />
        public UnitMomentalEffect(UnitMomentalEffectType effectType)
        {
            EffectType = effectType;
        }

        /// <inheritdoc />
        public UnitMomentalEffect(UnitMomentalEffectType effectType, int power)
        {
            EffectType = effectType;
            Power = power;
        }

        /// <inheritdoc />
        public UnitMomentalEffect(UnitMomentalEffectType effectType, int power, int duration)
        {
            EffectType = effectType;
            Power = power;
            Duration = duration;
        }


        /// <summary>
        /// Тип моментального эффекта, который произошел с юнитом.
        /// </summary>
        public UnitMomentalEffectType EffectType { get; }

        /// <summary>
        /// Сила эффекта.
        /// </summary>
        public int? Power { get; }

        /// <summary>
        /// Продолжительность эффекта.
        /// </summary>
        public int? Duration { get; }
    }
}