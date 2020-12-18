using Disciples.Engine.Battle.Enums;

namespace Disciples.Engine.Battle.Models
{
    /// <summary>
    /// Эффект, наложенный на юнита во время схватки.
    /// </summary>
    public class UnitBattleEffect
    {
        /// <inheritdoc />
        public UnitBattleEffect(UnitBattleEffectType effectType, int roundDuration)
        {
            EffectType = effectType;
            RoundDuration = roundDuration;
        }

        /// <inheritdoc />
        public UnitBattleEffect(UnitBattleEffectType effectType, int roundDuration, int? power)
        {
            EffectType = effectType;
            RoundDuration = roundDuration;
            Power = power;
        }


        /// <summary>
        /// Тип эффекта, оказываемого на юнита.
        /// </summary>
        public UnitBattleEffectType EffectType { get; }

        /// <summary>
        /// Длительность эффекта в раундах.
        /// </summary>
        public int RoundDuration { get; set; }

        /// <summary>
        /// Сила эффекта.
        /// </summary>
        public int? Power { get; }
    }
}