using System.Collections.Generic;
using System.Linq;

using Engine.Battle.Enums;
using Engine.Battle.Models;

namespace Engine.Common.Models
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
        /// <summary>
        /// Мгновенные эффекты, типа лечения, промаха и т.д.
        /// </summary>
        private readonly LinkedList<UnitMomentalEffect> _momentalEffects;

        public UnitEffects()
        {
            _globalEffects = new HashSet<object>();
            _battleEffects = new Dictionary<UnitBattleEffectType, UnitBattleEffect>();
            _momentalEffects = new LinkedList<UnitMomentalEffect>();
        }


        /// <summary>
        /// Добавить эффект в поединке.
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
        /// Получить эффект указанного типа, воздействующий на юнита.
        /// </summary>
        public UnitBattleEffect GetBattleEffect(UnitBattleEffectType effectType)
        {
            return _battleEffects[effectType];
        }


        /// <summary>
        /// Добавить моментальный эффект.
        /// </summary>
        public void AddMomentalEffect(UnitMomentalEffect momentalEffect)
        {
            _momentalEffects.AddLast(momentalEffect);
        }

        /// <summary>
        /// Получить моментальный эффект и удалить его из списка.
        /// </summary>
        public UnitMomentalEffect GetMomentalEffect()
        {
            // todo Можно привязать на эту же логику звуки. Тогда удалять будет нельзя.

            var first = _momentalEffects.First;
            if (first == null)
                return null;

            _momentalEffects.RemoveFirst();

            return first.Value;
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