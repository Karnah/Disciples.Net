using System.Reflection;
using Disciples.Resources.Database.Dbf;
using Disciples.Resources.Database.Dbf.Components;
using FluentMigrator;

namespace Disciples.Resources.Database.Sqlite.Migrator.Migrations;

/// <summary>
/// Миграция данных из dbf.
/// </summary>
/// <remarks>
/// Используем TransactionBehavior.None, так как "PRAGMA foreign_keys" не работает в транзакции.
/// </remarks>
[Migration(202303111412, TransactionBehavior.None)]
public class DataMigration : Migration
{
    /// <inheritdoc />
    public override void Up()
    {
        var database = new GameDataContext("Resources");

        Execute.Sql("PRAGMA foreign_keys = OFF;");

        AddTableData("GlobalTextResource", database.GlobalTextResources.Values.ToArray());
        AddTableData("InterfaceTextResource", database.InterfaceTextResources.Values.ToArray());
        AddTableData("Race", database.Races.Values.ToArray());
        AddTableData("UnitModifier", database.UnitModifiers.Values.ToArray());
        AddTableData("UnitModifierItem", database.UnitModifierItems.Values.SelectMany(um => um).ToArray());
        AddTableData("UnitAttack", database.UnitAttacks.Values.ToArray());
        AddTableData("UnitAttackSourceProtection", database.UnitAttackSourceProtections.Values.SelectMany(p => p).ToArray());
        AddTableData("UnitAttackTypeProtection", database.UnitAttackTypeProtections.Values.SelectMany(p => p).ToArray());
        AddTableData("UnitLevelUpgrade", database.UnitLevelUpgrades.Values.ToArray());
        AddTableData("UnitType", database.UnitTypes.Values.ToArray());
        AddTableData("UnitAttackSummonTransform", database.UnitAttackSummonTransforms.Values.SelectMany(p => p).ToArray());

        Execute.Sql("PRAGMA foreign_keys = ON;");
    }

    /// <inheritdoc />
    public override void Down()
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// Добавить данные в таблицу.
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности.</typeparam>
    /// <param name="tableName">Имя таблицы.</param>
    /// <param name="entities">Данные.</param>
    private void AddTableData<TEntity>(string tableName, IReadOnlyList<TEntity> entities)
    {
        var insertSyntax = Insert.IntoTable(tableName);
        var properties = typeof(TEntity).GetProperties();
        foreach (var entity in entities)
        {
            insertSyntax.Row(ConvertEntityPropertiesToDictionary(entity!, properties));
        }
    }

    /// <summary>
    /// Получить свойства сущности в виде словаря.
    /// </summary>
    private static Dictionary<string, object?> ConvertEntityPropertiesToDictionary(object entity, IReadOnlyList<PropertyInfo> entityProperties)
    {
        var dictionary = new Dictionary<string, object?>();

        foreach (var entityProperty in entityProperties)
        {
            var entityValue = entityProperty.GetValue(entity);
            if (entityValue is ResourceSet resourceSet)
            {
                dictionary.Add($"{entityProperty.Name}{nameof(resourceSet.Gold)}", resourceSet.Gold);
                dictionary.Add($"{entityProperty.Name}{nameof(resourceSet.DeathMana)}", resourceSet.DeathMana);
                dictionary.Add($"{entityProperty.Name}{nameof(resourceSet.RuneMana)}", resourceSet.RuneMana);
                dictionary.Add($"{entityProperty.Name}{nameof(resourceSet.LifeMana)}", resourceSet.LifeMana);
                dictionary.Add($"{entityProperty.Name}{nameof(resourceSet.InfernalMana)}", resourceSet.InfernalMana);
                dictionary.Add($"{entityProperty.Name}{nameof(resourceSet.GroveMana)}", resourceSet.GroveMana);
            }
            else if(entityProperty.PropertyType.IsEnum)
            {
                dictionary.Add(entityProperty.Name, (int)entityValue!);
            }
            else if (IsNullableEnum(entityProperty.PropertyType) && entityValue != null)
            {
                dictionary.Add(entityProperty.Name, Convert.ToInt32(entityValue));
            }
            else
            {
                dictionary.Add(entityProperty.Name, entityValue);
            }
        }

        return dictionary;
    }

    /// <summary>
    /// Проверить, что тип nullable от перечисления.
    /// </summary>
    private static bool IsNullableEnum(Type t)
    {
        return t.IsGenericType &&
               t.GetGenericTypeDefinition() == typeof(Nullable<>) &&
               t.GetGenericArguments()[0].IsEnum;
    }
}