using Disciples.Engine.Battle.Enums;

namespace Disciples.Engine.Battle.Models
{
    /// <summary>
    /// Мгновенный эффект, произошедший с юнитом.
    /// </summary>
    public class UnitInstantaneousEffect
    {
        /// <inheritdoc />
        public UnitInstantaneousEffect(UnitInstantaneousEffectType effectType)
        {
            EffectType = effectType;
        }

        /// <inheritdoc />
        public UnitInstantaneousEffect(UnitInstantaneousEffectType effectType, int power)
        {
            EffectType = effectType;
            Power = power;
        }

        /// <inheritdoc />
        public UnitInstantaneousEffect(UnitInstantaneousEffectType effectType, int power, int duration)
        {
            EffectType = effectType;
            Power = power;
            Duration = duration;
        }


        /// <summary>
        /// Тип моментального эффекта, который произошел с юнитом.
        /// </summary>
        public UnitInstantaneousEffectType EffectType { get; }

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