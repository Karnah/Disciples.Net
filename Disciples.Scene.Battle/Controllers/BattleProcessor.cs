using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Models;
using Disciples.Engine;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Models;

namespace Disciples.Scene.Battle.Controllers
{
    /// <summary>
    /// Обработчик битвы.
    /// </summary>
    public class BattleProcessor
    {
        /// <summary>
        /// Разброс атаки при ударе.
        /// </summary>
        private const int ATTACK_RANGE = 5;

        /// <summary>
        /// Выполнить одну атаку юнита на другого с помощью основной атаки.
        /// </summary>
        public BattleProcessorAttackResult? ProcessMainAttack(Unit attackingUnit, Unit targetUnit)
        {
            var power = attackingUnit.FirstAttackPower;
            var attack = attackingUnit.UnitType.MainAttack;
            var accuracy = attackingUnit.MainAttackAccuracy;
            return ProcessAttack(targetUnit, attack, power, accuracy);
        }

        /// <summary>
        /// Выполнить одну атаку юнита на другого с помощью второстепенной атаки.
        /// </summary>
        public BattleProcessorAttackResult? ProcessSecondaryAttack(Unit attackingUnit, Unit targetUnit, int? externalPower)
        {
            var power = externalPower ?? attackingUnit.SecondAttackPower;
            var attack = attackingUnit.UnitType.SecondaryAttack!;
            var accuracy = attackingUnit.SecondaryAttackAccuracy!.Value;
            return ProcessAttack(targetUnit, attack, power, accuracy);
        }

        /// <summary>
        /// Обработать действие одного юнита на другого.
        /// </summary>
        /// <param name="targetUnit">Юнит, на которого воздействует.</param>
        /// <param name="attack">Тип атаки.</param>
        /// <param name="power">Сила атаки.</param>
        /// <param name="accuracy">Точность атаки.</param>
        private static BattleProcessorAttackResult? ProcessAttack(Unit targetUnit, Attack attack, int? power, int accuracy)
        {
            // Проверяем меткость юнита.
            var chanceOfFirstAttack = RandomGenerator.Next(0, 100);
            if (chanceOfFirstAttack > accuracy)
                return new BattleProcessorAttackResult(AttackResult.Miss);

            // todo Сразу обработать иммунитет + сопротивления. Также вернуть результат.
            // Вторая атака не будет действовать, если первая упёрлась в иммунитет.

            switch (attack.AttackClass)
            {
                case AttackClass.Damage:
                    // todo Максимальное значение атаки - 250/300/400.
                    var attackPower = power!.Value + RandomGenerator.Next(ATTACK_RANGE);

                    // Уменьшаем входящий урон в зависимости от защиты.
                    attackPower = (int)(attackPower * (1 - targetUnit.Armor / 100.0));

                    // Если юнит защитился, то урон уменьшается в два раза.
                    if (targetUnit.Effects.ExistsBattleEffect(UnitBattleEffectType.Defend))
                    {
                        attackPower /= 2;
                    }

                    // Мы не можем нанести урон больше, чем осталось очков здоровья.
                    attackPower = Math.Min(attackPower, targetUnit.HitPoints);
                    return new BattleProcessorAttackResult(
                        AttackResult.Attack,
                        attackPower,
                        attack.AttackClass);

                case AttackClass.Drain:
                case AttackClass.Paralyze:
                    break;

                case AttackClass.Heal:
                    var healPower = Math.Min(power!.Value, targetUnit.MaxHitPoints - targetUnit.HitPoints);
                    if (healPower != 0)
                    {
                        return new BattleProcessorAttackResult(
                            AttackResult.Heal,
                            healPower,
                            attack.AttackClass);
                    }

                    break;

                case AttackClass.Fear:
                case AttackClass.BoostDamage:
                case AttackClass.Petrify:
                case AttackClass.LowerDamage:
                case AttackClass.LowerInitiative:
                    break;

                case AttackClass.Poison:
                case AttackClass.Frostbite:
                case AttackClass.Blister:
                    return new BattleProcessorAttackResult(
                        AttackResult.Effect,
                        power,
                        2,
                        attack.AttackClass);

                case AttackClass.Revive:
                case AttackClass.DrainOverflow:
                case AttackClass.Cure:
                case AttackClass.Summon:
                case AttackClass.DrainLevel:
                case AttackClass.GiveAttack:
                case AttackClass.Doppelganger:
                case AttackClass.TransformSelf:
                case AttackClass.TransformOther:
                case AttackClass.BestowWards:
                case AttackClass.Shatter:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }
    }
}
