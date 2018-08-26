using Avalonia.Media.Imaging;

using Engine.Common.Enums.Units;

namespace Engine.Common.Models
{
    public class UnitType
    {
        // todo перепилить под рефлектор
        public UnitType(
            string unitTypeId,
            UnitCategory unitCategory,
            int level,
            UnitType prevUnitType,
            string race,
            Subrace subrace,
            UnitBranch branch,
            bool sizeSmall,
            bool isMale,
            string enrollCost,
            string enrollBnding,
            string name,
            string description,
            string abil,
            Attack firstAttack,
            Attack secondAttack,
            bool attackTwice,
            int hitPoints,
            UnitType baseUnit,
            int armor,
            int regen,
            string reviveCost,
            string healCost,
            string trainingCost,
            int xpKilled,
            string upgradeBuilding,
            int xpNext,
            int deathAnimationId,
            Bitmap face,
            Bitmap battleFace)
        {
            UnitTypeId = unitTypeId;
            UnitCategory = unitCategory;
            Level = level;
            PrevUnitType = prevUnitType;
            Race = race;
            Subrace = subrace;
            Branch = branch;
            SizeSmall = sizeSmall;
            IsMale = isMale;
            EnrollCost = enrollCost;
            EnrollBnding = enrollBnding;
            Name = name;
            Description = description;
            Abil = abil;
            FirstAttack = firstAttack;
            SecondAttack = secondAttack;
            AttackTwice = attackTwice;
            HitPoints = hitPoints;
            BaseUnit = baseUnit;
            Armor = armor;
            Regen = regen;
            ReviveCost = reviveCost;
            HealCost = healCost;
            TrainingCost = trainingCost;
            XpKilled = xpKilled;
            UpgradeBuilding = upgradeBuilding;
            XpNext = xpNext;
            DeathAnimationId = deathAnimationId;
            
            Face = face;
            BattleFace = battleFace;
        }

        /// <summary>
        /// Идентификатор типа юнита
        /// </summary>
        public string UnitTypeId { get; }

        /// <summary>
        /// Kатегория юнита
        /// </summary>
        public UnitCategory UnitCategory { get; }

        /// <summary>
        /// Базовый уровень юнита
        /// </summary>
        public int Level { get; }

        /// <summary>
        /// Предыдущий тип юнита в иерархии
        /// </summary>
        public UnitType PrevUnitType { get; }

        /// <summary>
        /// todo Раса юнита
        /// </summary>
        public string Race { get; }

        /// <summary>
        /// Подраса юнита
        /// </summary>
        public Subrace Subrace { get; }

        /// <summary>
        /// Ветвь юнита
        /// </summary>
        public UnitBranch Branch { get; }

        /// <summary>
        /// Занимает ли юнит одну клетку. Занимает две клетки, если false
        /// </summary>
        public bool SizeSmall { get; }

        /// <summary>
        /// Является ли юнит мужчиной
        /// </summary>
        public bool IsMale { get; }

        /// <summary>
        /// todo Определяет стоимость найма юнита в столице
        /// </summary>
        public string EnrollCost { get; }

        /// <summary>
        /// todo Определяет здание, позволяющее нанимать юнита
        /// </summary>
        public string EnrollBnding { get; }

        /// <summary>
        /// Имя типа юнита
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Описание тип юнита
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// todo
        /// </summary>
        public string Abil { get; }

        /// <summary>
        /// Основная атака
        /// </summary>
        public Attack FirstAttack { get; }

        /// <summary>
        /// Дополнительная атака
        /// </summary>
        public Attack SecondAttack { get; }

        /// <summary>
        /// Атакует ли юнит дважды
        /// </summary>
        public bool AttackTwice { get; }

        /// <summary>
        /// Количество жизней
        /// </summary>
        public int HitPoints { get; }

        /// <summary>
        /// Базовый юнит для юнита-героя
        /// </summary>
        public UnitType BaseUnit { get; }

        /// <summary>
        /// Базовая защита юнита
        /// </summary>
        public int Armor { get; }

        /// <summary>
        /// Базовое восстановления % жизней за ход
        /// </summary>
        public int Regen { get; }

        /// <summary>
        /// todo Стоимость возрождения юнита
        /// </summary>
        public string ReviveCost { get; }

        /// <summary>
        /// todo Стоимость восстановления здоровья
        /// </summary>
        public string HealCost { get; }

        /// <summary>
        /// todo Стоимость обучения юнита
        /// </summary>
        public string TrainingCost { get; }

        /// <summary>
        /// Количество опыта за убийство юнита
        /// </summary>
        public int XpKilled { get; }

        /// <summary>
        /// Здание, которое позволяет тренировать данный тип юнита
        /// </summary>
        public string UpgradeBuilding { get; }

        /// <summary>
        /// Количество опыта, необходимо для получения следующего уровня
        /// </summary>
        public int XpNext { get; }

        /// <summary>
        /// Анимация, которая отображается при смерти юнита
        /// </summary>
        public int DeathAnimationId { get; }


        /// <summary>
        /// Картинка лица юнита
        /// </summary>
        public Bitmap Face { get; }

        /// <summary>
        /// Скруглённая картинка лица юнита, используется в битвах
        /// </summary>
        public Bitmap BattleFace { get; }

        /// <summary>
        /// todo Большой портрет в информации о юните. Извлекать из Events.ff
        /// </summary>
        public Bitmap Portret { get; }
    }
}
