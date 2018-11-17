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


        /// <summary>
        /// Тип моментального эффекта, который произошел с юнитом.
        /// </summary>
        public UnitMomentalEffectType EffectType { get; }
    }
}
