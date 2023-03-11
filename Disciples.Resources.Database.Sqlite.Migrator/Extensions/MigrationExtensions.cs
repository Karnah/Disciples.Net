using FluentMigrator.Builders.Create.Table;

namespace Disciples.Resources.Database.Sqlite.Migrator.Extensions;

/// <summary>
/// Методы-расширения для миграций.
/// </summary>
internal static class MigrationExtensions
{
    /// <summary>
    /// Добавить колонки, относящиеся к ресурсам.
    /// </summary>
    /// <param name="expression">Выражение для создания таблицы.</param>
    /// <param name="columnName">Имя колонки.</param>
    /// <param name="columnDescription">Описание колонки.</param>
    public static ICreateTableWithColumnSyntax WithResourceSet(this ICreateTableWithColumnSyntax expression, string columnName, string columnDescription)
    {
        return expression
            .WithColumn($"{columnName}Gold").AsInt32().NotNullable().WithColumnDescription($"{columnDescription} - золото")
            .WithColumn($"{columnName}DeathMana").AsInt32().NotNullable().WithColumnDescription($"{columnDescription} - мана смерти")
            .WithColumn($"{columnName}RuneMana").AsInt32().NotNullable().WithColumnDescription($"{columnDescription} - мана рун")
            .WithColumn($"{columnName}LifeMana").AsInt32().NotNullable().WithColumnDescription($"{columnDescription} - мана жизни")
            .WithColumn($"{columnName}InfernalMana").AsInt32().NotNullable().WithColumnDescription($"{columnDescription} - мана ада")
            .WithColumn($"{columnName}GroveMana").AsInt32().NotNullable().WithColumnDescription($"{columnDescription} - мана рощи")
            ;
    }
}