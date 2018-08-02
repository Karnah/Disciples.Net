namespace Engine.Enums
{
    public enum AttackClass
    {
        /// <summary>
        /// Повреждение
        /// </summary>
        Damage = 1,

        /// <summary>
        /// Истощение
        /// </summary>
        Drain,

        /// <summary>
        /// Паралич
        /// </summary>
        Paralyze,

        /// <summary>
        /// Исцеление
        /// </summary>
        Heal,

        /// <summary>
        /// Страх
        /// </summary>
        Fear,

        /// <summary>
        /// Увеличение урона
        /// </summary>
        BoostDamage,

        /// <summary>
        /// Окаменение
        /// </summary>
        Petrify,

        /// <summary>
        /// Снижение повреждения
        /// </summary>
        LowerDamage,

        /// <summary>
        /// Снижение инициативы
        /// </summary>
        LowerInitiative,

        /// <summary>
        /// Отравление
        /// </summary>
        Poison,

        /// <summary>
        /// Обморожение
        /// </summary>
        Frostbite,

        /// <summary>
        /// Воскрешение
        /// </summary>
        Revive,

        /// <summary>
        /// Выпить жизненную силу
        /// </summary>
        DrainOverflow,

        /// <summary>
        /// todo Очищение?
        /// </summary>
        Cure,

        /// <summary>
        /// Призыв
        /// </summary>
        Summon,

        /// <summary>
        /// Позинить уровень
        /// </summary>
        DrainLevel,

        /// <summary>
        /// todo Передать атаку?
        /// </summary>
        GiveAttack,

        /// <summary>
        /// Передать жизненную силу
        /// </summary>
        Doppelganger,

        /// <summary>
        /// Превратить себя
        /// </summary>
        TransformSelf,

        /// <summary>
        /// Превратить другого
        /// </summary>
        TransformOther,

        /// <summary>
        /// Ожог
        /// </summary>
        Blister,

        /// <summary>
        /// Даровать защиту от стихий
        /// </summary>
        BestowWards,

        /// <summary>
        /// Разбить броню
        /// </summary>
        Shatter
    }
}
