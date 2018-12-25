using System;
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
        /// Длительность моментального эффекта.
        /// </summary>
        private const int MOMENTAL_EFFECT_DURATION = 1000;

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

        /// <summary>
        /// Сколько осталось до конца моментального эффекта.
        /// </summary>
        private long? _momentalEffectRemainDuration;

        public UnitEffects()
        {
            _globalEffects = new HashSet<object>();
            _battleEffects = new Dictionary<UnitBattleEffectType, UnitBattleEffect>();
            _momentalEffects = new LinkedList<UnitMomentalEffect>();
        }


        /// <summary>
        /// Текущий мгновенный эффект, который воздействует на юнита.
        /// </summary>
        public UnitMomentalEffect CurrentMomentalEffect => _momentalEffects.First?.Value;

        /// <summary>
        /// Указатель того, что моментальный эффект только начал действовать.
        /// </summary>
        public bool MomentalEffectBegin => _momentalEffectRemainDuration == MOMENTAL_EFFECT_DURATION;

        /// <summary>
        /// Указатель того, что моментальный эффект завершается.
        /// </summary>
        public bool MomentalEffectEnded => _momentalEffectRemainDuration == 0;


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
        /// Получить эффекты, воздействующие на юнита.
        /// </summary>
        public IReadOnlyList<UnitBattleEffect> GetBattleEffects()
        {
            return _battleEffects.Select(be => be.Value).ToList();
        }


        /// <summary>
        /// Добавить моментальный эффект.
        /// </summary>
        public void AddMomentalEffect(UnitMomentalEffect momentalEffect)
        {
            _momentalEffects.AddLast(momentalEffect);
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

        /// <summary>
        /// Уменьшить длительность моментального эффекта.
        /// </summary>
        public void OnTick(long ticksCount)
        {
            if (!_momentalEffects.Any())
                return;

            // Если эффект только появился, запускаем таймер его действия.
            if (_momentalEffectRemainDuration == null) {
                _momentalEffectRemainDuration = MOMENTAL_EFFECT_DURATION;
                return;
            }

            // Обрабатываем завершение моментального эффекта.
            if (_momentalEffectRemainDuration == 0) {
                _momentalEffects.RemoveFirst();

                // В очереди лежит еще один моментальный эффект, перезапускаем таймер.
                if (CurrentMomentalEffect != null)
                    _momentalEffectRemainDuration = MOMENTAL_EFFECT_DURATION;
                else
                    _momentalEffectRemainDuration = null;

                return;
            }

            // Уменьшаем оставшееся время действия эффекта.
            _momentalEffectRemainDuration -= ticksCount;
            _momentalEffectRemainDuration = Math.Max(_momentalEffectRemainDuration.Value, 0);
        }
    }
}