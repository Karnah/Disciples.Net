using Engine.Battle.Enums;

namespace Engine.Battle.Models
{
    public class BattleUnitEffect
    {
        public BattleUnitEffect(BattleUnitEffectType effectType, int roundDuration)
        {
            EffectType = effectType;
            RoundDuration = roundDuration;
        }


        /// <summary>
        /// Тип эффекта, оказываемого на юнита
        /// </summary>
        public BattleUnitEffectType EffectType { get; }

        /// <summary>
        /// Длительность эффекта в раундах
        /// </summary>
        public int RoundDuration { get; set; }
    }
}
