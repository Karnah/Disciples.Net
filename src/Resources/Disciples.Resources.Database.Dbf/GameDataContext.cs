using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;
using Disciples.Resources.Database.Dbf.Components;
using Disciples.Resources.Database.Dbf.Models;
using NDbfReader;

namespace Disciples.Resources.Database.Dbf;

/// <summary>
/// База данных Disciples.
/// </summary>
public class GameDataContext
{
    /// <summary>
    /// Кодировка базы данных.
    /// </summary>
    private const int DATABASE_ENCODING = 866;

    /// <summary>
    /// Путь, где лежат большинство таблиц.
    /// </summary>
    private const string DEFAULT_RESOURCE_FOLDER = "Globals";

    /// <summary>
    /// Путь, где лежит таблица с текстами интерфейса.
    /// </summary>
    private const string INTERFACE_RESOURCE_FOLDER = "interf";

    /// <summary>
    /// Признак пустой ссылки на объект.
    /// </summary>
    private const string NULL_REFERENCE = "G000000000";

    private readonly string _databasePath;
    private readonly Encoding _encoding;

    /// <summary>
    /// Создать объект типа <see cref="GameDataContext" />.
    /// </summary>
    /// <param name="databasePath">Путь до папки с таблицами.</param>
    public GameDataContext(string databasePath)
    {
        _databasePath = databasePath;
        _encoding = CodePagesEncodingProvider.Instance.GetEncoding(DATABASE_ENCODING)!;

        InterfaceTextResources = GetTable<InterfaceTextResource>(INTERFACE_RESOURCE_FOLDER);
        GlobalTextResources = GetTable<GlobalTextResource>();
        UnitAttacks = GetTable<UnitAttack>();
        UnitTypes = GetTable<UnitType>();
        UnitLevelUpgrades = GetTable<UnitLevelUpgrade>();
        UnitAttackSourceProtections = GetUnionTable<UnitAttackSourceProtection>();
        UnitAttackTypeProtections = GetUnionTable<UnitAttackTypeProtection>();
        UnitModifiers = GetTable<UnitModifier>();
        UnitModifierItems = GetUnionTable<UnitModifierItem>();
        UnitAttackSummonTransforms = GetUnionTable<UnitAttackSummonTransform>();
        Races = GetTable<Race>();
    }

    /// <summary>
    /// Тексты для интерфейсы.
    /// </summary>
    public IReadOnlyDictionary<string, InterfaceTextResource> InterfaceTextResources { get; }

    /// <summary>
    /// Общие тексты.
    /// </summary>
    public IReadOnlyDictionary<string, GlobalTextResource> GlobalTextResources { get; }

    /// <summary>
    /// Список атак юнитов.
    /// </summary>
    public IReadOnlyDictionary<string, UnitAttack> UnitAttacks { get; }

    /// <summary>
    /// Тип юнитов.
    /// </summary>
    public IReadOnlyDictionary<string, UnitType> UnitTypes { get; }

    /// <summary>
    /// Данные о том, как растут характеристики юнита с повышением уровня.
    /// </summary>
    public IReadOnlyDictionary<string, UnitLevelUpgrade> UnitLevelUpgrades { get; }

    /// <summary>
    /// Защита юнита от источников атак.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<UnitAttackSourceProtection>> UnitAttackSourceProtections { get; }

    /// <summary>
    /// Защита юнита от типов атак.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<UnitAttackTypeProtection>> UnitAttackTypeProtections { get; }

    /// <summary>
    /// Модификатор характеристик или способностей юнитов.
    /// </summary>
    public IReadOnlyDictionary<string, UnitModifier> UnitModifiers { get; }

    /// <summary>
    /// Модификатор отдельных характеристик или способностей юнитов.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<UnitModifierItem>> UnitModifierItems { get; }

    /// <summary>
    /// Идентификаторы вызываемых/превращаемых юнитов при атаке.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<UnitAttackSummonTransform>> UnitAttackSummonTransforms { get; }

    /// <summary>
    /// Список рас.
    /// </summary>
    public IReadOnlyDictionary<string, Race> Races { get; }

    /// <summary>
    /// Получить данные таблицы.
    /// </summary>
    /// <typeparam name="TEntity">Сущность.</typeparam>
    private IReadOnlyDictionary<string, TEntity> GetTable<TEntity>(string folder = DEFAULT_RESOURCE_FOLDER)
        where TEntity : IEntity, new()
    {
        var properties = typeof(TEntity).GetProperties();
        var tableName = GetTableName<TEntity>();
        var dictionary = new Dictionary<string, TEntity>();
        var connection = Path.Combine(_databasePath, folder, $"{tableName}.dbf");
        using (var table = Table.Open(connection))
        {
            var reader = table.OpenReader(_encoding);
            while (reader.Read())
            {
                var entity = GetEntity<TEntity>(reader, properties);
                dictionary.Add(entity.Id, entity);
            }
        }

        return dictionary;
    }

    /// <summary>
    /// Получить данные таблицы, сущности которых не имеют уникального ключа.
    /// В итоге все записи с одинаковым ключом объединяться в список.
    /// </summary>
    /// <typeparam name="TEntity">Сущность.</typeparam>
    private IReadOnlyDictionary<string, IReadOnlyList<TEntity>> GetUnionTable<TEntity>(string folder = DEFAULT_RESOURCE_FOLDER)
        where TEntity : IEntity, new()
    {
        var properties = typeof(TEntity).GetProperties();
        var tableName = GetTableName<TEntity>();
        var dictionary = new Dictionary<string, List<TEntity>>();
        var connection = Path.Combine(_databasePath, folder, $"{tableName}.dbf");
        using (var table = Table.Open(connection))
        {
            var reader = table.OpenReader(_encoding);
            while (reader.Read())
            {
                var entity = GetEntity<TEntity>(reader, properties);

                if (!dictionary.ContainsKey(entity.Id))
                    dictionary.Add(entity.Id, new List<TEntity>());

                dictionary[entity.Id].Add(entity);
            }
        }

        return dictionary.ToDictionary(d => d.Key, d => (IReadOnlyList<TEntity>)d.Value);
    }

    /// <summary>
    /// Получить название таблицы сущности.
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности..</typeparam>
    private static string GetTableName<TEntity>()
        where TEntity : IEntity
    {
        var type = typeof(TEntity);
        var tableName = (TableAttribute?)type.GetCustomAttribute(typeof(TableAttribute));
        if (string.IsNullOrWhiteSpace(tableName?.Name))
            throw new ArgumentException($"Для типа {type.Name} не указан атрибут {nameof(TableAttribute)}.{nameof(TableAttribute.Name)}");

        return tableName.Name;
    }

    /// <summary>
    /// Получить данные сущности.
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности.</typeparam>
    /// <param name="reader">Ридер данных.</param>
    /// <param name="properties">Свойства сущности.</param>
    /// <returns>Объект сущности.</returns>
    private static TEntity GetEntity<TEntity>(Reader reader, IReadOnlyList<PropertyInfo> properties)
        where TEntity : new()
    {
        var entity = new TEntity();

        foreach (var property in properties)
        {
            var columnName = property.GetCustomAttribute<ColumnAttribute>();
            if (string.IsNullOrWhiteSpace(columnName?.Name))
                throw new ArgumentException($"Для свойства {typeof(TEntity).Name}.{property.Name} не указан атрибут {nameof(ColumnAttribute)}.{nameof(ColumnAttribute.Name)}");

            // TODO Проверить на nullable reference.
            var value = GetValue(reader, columnName.Name, property.Name, property.PropertyType);
            property.SetValue(entity, value);
        }

        return entity;
    }

    /// <summary>
    /// Получить значение свойства.
    /// </summary>
    /// <param name="reader">Ридер данных.</param>
    /// <param name="columnName">Имя колонки.</param>
    /// <param name="propertyName">Имя свойства.</param>
    /// <param name="propertyType">Тип свойства.</param>
    /// <returns>Значение свойства.</returns>
    private static object? GetValue(Reader reader, string columnName, string propertyName, Type propertyType)
    {
        // Отдельный тип для ссылок?
        if (propertyName.EndsWith("Id"))
        {
            var value = reader.GetString(columnName)?.ToUpperInvariant();
            if (string.Equals(value, NULL_REFERENCE))
                return null;

            return value;
        }

        if (propertyType == typeof(string))
        {
            // В ресурсах почему-то часть строк null. Конвертируем их в пустые строки.
            if (propertyName is nameof(GlobalTextResource.Text) or nameof(InterfaceTextResource.Text))
                return reader.GetString(columnName) ?? string.Empty;

            return reader.GetString(columnName);
        }

        if (propertyType == typeof(int?))
            return (int?)reader.GetDecimal(columnName);

        if (IsNullableEnum(propertyType))
            return ConvertNullableEnum(propertyType, (int?)reader.GetDecimal(columnName));

        if (propertyType.IsEnum || propertyType == typeof(int))
            return (int)(reader.GetDecimal(columnName) ?? 0);

        if (propertyType == typeof(bool))
            return reader.GetBoolean(columnName) ?? false;

        if (propertyType == typeof(ResourceSet))
            return ResourceSet.Parse(reader.GetString(columnName));

        throw new ArgumentException($"Некорректный тип для разбора {propertyType.Name}");
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

    /// <summary>
    /// Сконвертировать значение в Enum.
    /// </summary>
    private static object? ConvertNullableEnum(Type t, int? value)
    {
        if (value == null)
            return null;

        return Enum.ToObject(t.GetGenericArguments()[0], value.Value);
    }
}