using Disciples.Resources.Database.Sqlite.Migrator.Extensions;
using FluentMigrator;
using static Disciples.Resources.Database.Sqlite.Migrator.Constants.StringColumnConstants;

namespace Disciples.Resources.Database.Sqlite.Migrator.Migrations;

/// <summary>
/// Начальная миграция БД.
/// </summary>
[Migration(202303111302)]
public class InitialMigration : Migration
{
    /// <inheritdoc />
    public override void Up()
    {
        Create.Table("GlobalTextResource").WithDescription("Общая текстовая информация")
            .WithColumn("Id").AsString(ID_LENGTH).NotNullable().PrimaryKey().WithColumnDescription("Идентификатор записи")
            .WithColumn("Text").AsString(TEXT_LENGTH).NotNullable().WithColumnDescription("Текст")
            .WithColumn("IsVerified").AsBoolean().NotNullable().WithColumnDescription("Верифицирована ли запись")
            .WithColumn("Context").AsString(TEXT_LENGTH).Nullable().WithColumnDescription("Контекст записи")
            ;

        Create.Table("InterfaceTextResource").WithDescription("Текстовая информация для интерфейса")
            .WithColumn("Id").AsString(ID_LENGTH).NotNullable().PrimaryKey().WithColumnDescription("Идентификатор записи")
            .WithColumn("Text").AsString(TEXT_LENGTH).NotNullable().WithColumnDescription("Текст")
            .WithColumn("IsVerified").AsBoolean().NotNullable().WithColumnDescription("Верифицирована ли запись")
            .WithColumn("Context").AsString(TEXT_LENGTH).Nullable().WithColumnDescription("Контекст записи")
            ;

        Create.Table("Race").WithDescription("Раса")
            .WithColumn("Id").AsString(ID_LENGTH).NotNullable().PrimaryKey().WithColumnDescription("Идентификатор расы")
            .WithColumn("GuardingUnitTypeId").AsString(ID_LENGTH).Nullable().WithColumnDescription("Идентификатор юнита-стража столицы").ForeignKey("UnitType", "Id")
            .WithColumn("LeaderThiefUnitTypeId").AsString(ID_LENGTH).Nullable().WithColumnDescription("Идентификатор лидера-вора расы").ForeignKey("UnitType", "Id")
            .WithColumn("Leader1UnitTypeId").AsString(ID_LENGTH).Nullable().WithColumnDescription("Идентификатор первого героя расы").ForeignKey("UnitType", "Id")
            .WithColumn("Leader2UnitTypeId").AsString(ID_LENGTH).Nullable().WithColumnDescription("Идентификатор второго героя расы").ForeignKey("UnitType", "Id")
            .WithColumn("Leader3UnitTypeId").AsString(ID_LENGTH).Nullable().WithColumnDescription("Идентификатор третьего героя расы").ForeignKey("UnitType", "Id")
            .WithColumn("Leader4UnitTypeId").AsString(ID_LENGTH).Nullable().WithColumnDescription("Идентификатор четвертого героя расы").ForeignKey("UnitType", "Id")
            .WithColumn("Soldier1UnitTypeId").AsString(ID_LENGTH).Nullable().WithColumnDescription("Идентификатор первого солдата расы").ForeignKey("UnitType", "Id")
            .WithColumn("Soldier2UnitTypeId").AsString(ID_LENGTH).Nullable().WithColumnDescription("Идентификатор второго солдата расы").ForeignKey("UnitType", "Id")
            .WithColumn("Soldier3UnitTypeId").AsString(ID_LENGTH).Nullable().WithColumnDescription("Идентификатор третьего солдата расы").ForeignKey("UnitType", "Id")
            .WithColumn("Soldier4UnitTypeId").AsString(ID_LENGTH).Nullable().WithColumnDescription("Идентификатор четвертого солдата расы").ForeignKey("UnitType", "Id")
            .WithColumn("Soldier5UnitTypeId").AsString(ID_LENGTH).Nullable().WithColumnDescription("Идентификатор пятого солдата расы").ForeignKey("UnitType", "Id")
            .WithColumn("CapitalScoutPoints").AsInt32().NotNullable().WithColumnDescription("Дальность обзора столицы вокруг себя")
            .WithColumn("CapitalHitPointsRegeneration").AsInt32().NotNullable().WithColumnDescription("Восстановление % жизней за ход в столице")
            .WithResourceSet("Income", "Доход в ресурсах каждый ход")
            .WithColumn("CapitalProtectionModifierId").AsString(ID_LENGTH).Nullable().WithColumnDescription("Идентификатор модификатора, который устанавливает бонус к защите юнитов в столице")
            .WithColumn("NameTextId").AsString(ID_LENGTH).NotNullable().WithColumnDescription("Наименование расы").ForeignKey("GlobalTextResource", "Id")
            .WithColumn("RaceType").AsInt32().NotNullable().WithColumnDescription("Тип расы")
            .WithColumn("IsPlayable").AsBoolean().NotNullable().WithColumnDescription("Можно ли играть данной расой")
            ;

        Create.Table("UnitAttack").WithDescription("Атака юнита")
            .WithColumn("Id").AsString(ID_LENGTH).NotNullable().PrimaryKey().WithColumnDescription("Идентификатор атаки")
            .WithColumn("NameTextId").AsString(ID_LENGTH).NotNullable().WithColumnDescription("Название атаки").ForeignKey("GlobalTextResource", "Id")
            .WithColumn("DescriptionTextId").AsString(ID_LENGTH).NotNullable().WithColumnDescription("Описание атаки").ForeignKey("GlobalTextResource", "Id")
            .WithColumn("Initiative").AsInt32().NotNullable().WithColumnDescription("Инициатива")
            .WithColumn("AttackSource").AsInt32().NotNullable().WithColumnDescription("Источник атаки")
            .WithColumn("AttackType").AsInt32().NotNullable().WithColumnDescription("Тип атаки")
            .WithColumn("Accuracy").AsInt32().NotNullable().WithColumnDescription("Точность")
            .WithColumn("Reach").AsInt32().NotNullable().WithColumnDescription("Достижимость целей для атаки")
            .WithColumn("DamagePower").AsInt32().NotNullable().WithColumnDescription("Наносимый урон")
            .WithColumn("HealPower").AsInt32().NotNullable().WithColumnDescription("Сила исцеления")
            .WithColumn("AttackPowerLevel").AsInt32().NotNullable().WithColumnDescription("Уровень атаки")
            .WithColumn("AlternativeUnitAttackId").AsString(ID_LENGTH).Nullable().WithColumnDescription("Идентификатор альтернативной атаки").ForeignKey("UnitAttack", "Id")
            .WithColumn("IsInfinitive").AsBoolean().NotNullable().WithColumnDescription("Признак, что эффект, накладываемый атакой, длится до конца боя")
            .WithColumn("WardsCount").AsInt32().NotNullable().WithColumnDescription("Количество защит, которые накладывается на цель при атаке")
            .WithColumn("Ward1Id").AsString(ID_LENGTH).Nullable().WithColumnDescription("Тип первой защиты, которая накладывается при атаке")
            .WithColumn("Ward2Id").AsString(ID_LENGTH).Nullable().WithColumnDescription("Тип второй защиты, которая накладывается при атаке")
            .WithColumn("Ward3Id").AsString(ID_LENGTH).Nullable().WithColumnDescription("Тип третьей защиты, которая накладывается при атаке")
            .WithColumn("Ward4Id").AsString(ID_LENGTH).Nullable().WithColumnDescription("Тип четвертой защиты, которая накладывается при атаке")
            .WithColumn("IsCritical").AsBoolean().NotNullable().WithColumnDescription("Признак, что при ударе наносится критический урон")
            ;

        Create.Table("UnitAttackSourceProtection").WithDescription("Защита юнита от источника атак")
            .WithColumn("Id").AsString(ID_LENGTH).NotNullable().Indexed().WithColumnDescription("Идентификатор юнита, который имеет защиту").ForeignKey("UnitType", "Id")
            .WithColumn("UnitAttackSource").AsInt32().NotNullable().WithColumnDescription("Источник атаки")
            .WithColumn("ProtectionCategory").AsInt32().NotNullable().WithColumnDescription("Категория защиты")
            ;

        Create.Table("UnitAttackTypeProtection").WithDescription("Защита юнита от типа атаки")
            .WithColumn("Id").AsString(ID_LENGTH).NotNullable().Indexed().WithColumnDescription("Идентификатор юнита, который имеет защиту").ForeignKey("UnitType", "Id")
            .WithColumn("UnitAttackType").AsInt32().NotNullable().WithColumnDescription("Тип атаки")
            .WithColumn("ProtectionCategory").AsInt32().NotNullable().WithColumnDescription("Категория защиты")
            ;

        Create.Table("UnitLevelUpgrade").WithDescription("Данные о том, как растут характеристики юнита с повышением уровня")
            .WithColumn("Id").AsString(ID_LENGTH).NotNullable().PrimaryKey().WithColumnDescription("Идентификатор")
            .WithResourceSet("RecruitCost", "Рост стоимости юнита")
            .WithColumn("HitPoints").AsInt32().NotNullable().WithColumnDescription("Рост количества жизней")
            .WithColumn("Armor").AsInt32().NotNullable().WithColumnDescription("Рост базовой защиты юнита")
            .WithColumn("HitPointsRegeneration").AsInt32().NotNullable().WithColumnDescription("Рост базового восстановления % жизней за ход")
            .WithResourceSet("ReviveCost", "Рост стоимости воскрешения юнита")
            .WithResourceSet("HealCost", "Рост стоимости восстановления 1 единицы здоровья")
            .WithResourceSet("TrainingCost", "Рост стоимости обучения 1 очка опыта юнита у инструктора")
            .WithColumn("XpKilled").AsInt32().NotNullable().WithColumnDescription("Рост количества опыта за убийство юнита")
            .WithColumn("XpNext").AsInt32().NotNullable().WithColumnDescription("Рост количества опыта, необходимо для получения следующего уровня")
            .WithColumn("LeaderMovePoints").AsInt32().Nullable().WithColumnDescription("Рост количества очков движения для юнита-героя")
            .WithColumn("LeaderThiefProtection").AsInt32().Nullable().WithColumnDescription("Умение вести переговоры? Что-то для воров?")
            .WithColumn("DamagePower").AsInt32().NotNullable().WithColumnDescription("Рост наносимого урона")
            .WithColumn("HealPower").AsInt32().NotNullable().WithColumnDescription("Рост силы исцеления")
            .WithColumn("Initiative").AsInt32().NotNullable().WithColumnDescription("Рост инициативы")
            .WithColumn("Accuracy").AsInt32().NotNullable().WithColumnDescription("Рост точности")
            ;

        Create.Table("UnitType").WithDescription("Информация о типе юнита")
            .WithColumn("Id").AsString(ID_LENGTH).NotNullable().PrimaryKey().WithColumnDescription("Идентификатор типа юнита")
            .WithColumn("UnitCategory").AsInt32().NotNullable().WithColumnDescription("Категория юнита")
            .WithColumn("Level").AsInt32().NotNullable().WithColumnDescription("Базовый уровень юнита")
            .WithColumn("PreviousUnitTypeId").AsString(ID_LENGTH).Nullable().WithColumnDescription("Предыдущий тип юнита в иерархии").ForeignKey("UnitType", "Id")
            .WithColumn("RaceId").AsString(ID_LENGTH).Nullable().WithColumnDescription("Раса юнита").ForeignKey("Race", "Id")
            .WithColumn("Subrace").AsInt32().NotNullable().WithColumnDescription("Подраса юнита")
            .WithColumn("Branch").AsInt32().NotNullable().WithColumnDescription("Ветвь юнита")
            .WithColumn("IsSmall").AsBoolean().NotNullable().WithColumnDescription("Занимает ли юнит одну клетку. Занимает две клетки, если 1")
            .WithColumn("IsMale").AsBoolean().NotNullable().WithColumnDescription("Является ли юнит мужчиной")
            .WithResourceSet("RecruitCost", "Стоимость найма юнита")
            .WithColumn("RecruitBuildingId").AsString(ID_LENGTH).Nullable().WithColumnDescription("Здание, которое нужно построить в столице, чтобы нанимать юнита")
            .WithColumn("NameTextId").AsString(ID_LENGTH).NotNullable().WithColumnDescription("Имя типа юнита").ForeignKey("GlobalTextResource", "Id")
            .WithColumn("DescriptionTextId").AsString(ID_LENGTH).NotNullable().WithColumnDescription("Описание типа юнита").ForeignKey("GlobalTextResource", "Id")
            .WithColumn("AbilityTextId").AsString(ID_LENGTH).NotNullable().WithColumnDescription("Описание основной способности героя").ForeignKey("GlobalTextResource", "Id")
            .WithColumn("MainUserAttackId").AsString(ID_LENGTH).NotNullable().WithColumnDescription("Основная атака").ForeignKey("UnitAttack", "Id")
            .WithColumn("SecondaryUserAttackId").AsString(ID_LENGTH).Nullable().WithColumnDescription("Дополнительная атака").ForeignKey("UnitAttack", "Id")
            .WithColumn("IsAttackTwice").AsBoolean().NotNullable().WithColumnDescription("Атакует ли юнит дважды")
            .WithColumn("HitPoints").AsInt32().NotNullable().WithColumnDescription("Количество жизней")
            .WithColumn("LeaderBaseUnitId").AsString(ID_LENGTH).Nullable().WithColumnDescription("Базовый тип юнита для юнита-героя").ForeignKey("UnitType", "Id")
            .WithColumn("Armor").AsInt32().NotNullable().WithColumnDescription("Базовая защита юнита")
            .WithColumn("HitPointsRegeneration").AsInt32().NotNullable().WithColumnDescription("Базовое восстановления % жизней за ход")
            .WithResourceSet("ReviveCost", "Стоимость воскрешения юнита")
            .WithResourceSet("HealCost", "Стоимость восстановления 1 единицы здоровья")
            .WithResourceSet("TrainingCost", "Стоимость обучения 1 очка опыта юнита у инструктора")
            .WithColumn("XpKilled").AsInt32().NotNullable().WithColumnDescription("Количество опыта за убийство юнита")
            .WithColumn("UpgradeBuildingId").AsString(ID_LENGTH).Nullable().WithColumnDescription("Здание, которое позволяет тренировать данный тип юнита")
            .WithColumn("XpNext").AsInt32().NotNullable().WithColumnDescription("Количество опыта, необходимо для получения следующего уровня")
            .WithColumn("LeaderMovePoints").AsInt32().Nullable().WithColumnDescription("Количество очков движения для юнита-героя")
            .WithColumn("LeaderScoutPoints").AsInt32().Nullable().WithColumnDescription("Дальность обзора для юнита-героя")
            .WithColumn("LeaderLifeTime").AsInt32().Nullable().WithColumnDescription("Количество ходов, которое живёт юнит-герой")
            .WithColumn("Leadership").AsInt32().Nullable().WithColumnDescription("Лидерство героя")
            .WithColumn("LeaderThiefProtection").AsInt32().Nullable().WithColumnDescription("Рост снижения шанса успеха, когда вор применяет навык на отряд этого героя")
            .WithColumn("LeaderCategory").AsInt32().Nullable().WithColumnDescription("Категория лидера")
            .WithColumn("LowLevelUpgradeId").AsString(ID_LENGTH).NotNullable().WithColumnDescription("Идентификатор записи, которая указывает рост характеристик юнита при повышении уровня до UpgradeChangeLevel").ForeignKey("UnitLevelUpgrade", "Id")
            .WithColumn("UpgradeChangeLevel").AsInt32().NotNullable().WithColumnDescription("оследний уровень, когда рост характеристик рассчитывается по формуле LowLevelUpgradeId")
            .WithColumn("HighLevelUpgradeId").AsString(ID_LENGTH).NotNullable().WithColumnDescription("Идентификатор записи, которая указывает рост характеристик юнита при повышении уровня сверх UpgradeChangeLevel").ForeignKey("UnitLevelUpgrade", "Id")
            .WithColumn("IsLeaderWaterOnly").AsBoolean().NotNullable().WithColumnDescription("Признак, что юнит перемещается только по воде")
            .WithColumn("DeathAnimationType").AsInt32().NotNullable().WithColumnDescription("Анимация, которая отображается при смерти юнита")
            ;
    }

    /// <inheritdoc />
    public override void Down()
    {
        throw new NotSupportedException();
    }
}