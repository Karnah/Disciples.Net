using Engine.Enums;

namespace Engine.Models
{
    public class Attack
    {
        public Attack(
            string attackId,
            string name,
            string description,
            int initiative,
            AttackSource attackSource,
            AttackClass attackClass,
            int accuracy,
            Reach reach,
            int healPower,
            int damagePower)
        {
            AttackId = attackId;
            Name = name;
            Description = description;
            Initiative = initiative;
            AttackSource = attackSource;
            AttackClass = attackClass;
            Accuracy = accuracy;
            Reach = reach;
            HealPower = healPower;
            DamagePower = damagePower;
        }


        /// <summary>
        /// Идентификатор атаки
        /// </summary>
        public string AttackId { get; }

        /// <summary>
        /// Имя
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Инициатива
        /// </summary>
        public int Initiative { get; }

        /// <summary>
        /// Тип атаки
        /// </summary>
        public AttackSource AttackSource { get; }

        /// <summary>
        /// Класс атаки
        /// </summary>
        public AttackClass AttackClass { get; }

        /// <summary>
        /// Точность
        /// </summary>
        public int Accuracy { get; }

        /// <summary>
        /// Достижимость целей для атаки
        /// </summary>
        public Reach Reach { get; }

        /// <summary>
        /// Сила исцеления
        /// </summary>
        public int HealPower { get; }

        /// <summary>
        /// Наносимый урон
        /// </summary>
        public int DamagePower { get; }

        //todo остальные поля
    }
}
