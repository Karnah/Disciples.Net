using Engine.Battle.Enums;

namespace Engine.Battle.Models
{
    /// <summary>
    /// Эффект, наложенный на юнита во время схватки.
    /// </summary>
    public class UnitBattleEffect
    {
        public UnitBattleEffect(UnitBattleEffectType effectType, int roundDuration)
        {
            EffectType = effectType;
            RoundDuration = roundDuration;
        }


        /// <summary>
        /// Тип эффекта, оказываемого на юнита.
        /// </summary>
        public UnitBattleEffectType EffectType { get; }

        /// <summary>
        /// Длительность эффекта в раундах.
        /// </summary>
        public int RoundDuration { get; set; }
    }
}