using System.Collections.Generic;
using System.Linq;

using Disciples.Engine.Battle.Enums;
using Disciples.Engine.Battle.Models;

namespace Disciples.Engine.Common.Models
{
    /// <summary>
    /// Эффекты, которые действуют на юнита.
    /// </summary>
    public class UnitEffects
    {
        /// <summary>
        /// Эффекты, которые наложены и действуют на глобальной карте.
        /// </summary>
        private readonly HashSet<object> _globalEffects;
        /// <summary>
        /// Эффекты, которые наложены и действуют во время схватки.
        /// </summary>
        private readonly Dictionary<UnitBattleEffectType, UnitBattleEffect> _battleEffects;

        /// <inheritdoc />
        public UnitEffects()
        {
            _globalEffects = new HashSet<object>();
            _battleEffects = new Dictionary<UnitBattleEffectType, UnitBattleEffect>();
        }


        /// <summary>
        /// Добавить эффект в схватке.
        /// </summary>
        public void AddBattleEffect(UnitBattleEffect battleEffect)
        {
            _battleEffects[battleEffect.EffectType] = battleEffect;
        }

        /// <summary>
        /// Проверить, что на юнита наложен эффект указанного типа.
        /// </summary>
        public bool ExistsBattleEffect(UnitBattleEffectType effectType)
        {
            return _battleEffects.ContainsKey(effectType);
        }

        /// <summary>
        /// Получить эффекты, воздействующие на юнита.
        /// </summary>
        public IReadOnlyList<UnitBattleEffect> GetBattleEffects()
        {
            return _battleEffects.Select(be => be.Value).ToList();
        }


        /// <summary>
        /// Уменьшить длительность всех эффектов в схватке на единицу.
        /// </summary>
        public void OnUnitTurn()
        {
            foreach (var battleEffect in _battleEffects) {
                --battleEffect.Value.RoundDuration;
            }

            var expiredEffects = _battleEffects
                .Where(be => be.Value.RoundDuration <= 0)
                .ToList();
            foreach (var expiredEffect in expiredEffects) {
                _battleEffects.Remove(expiredEffect.Key);
            }
        }
    }
}