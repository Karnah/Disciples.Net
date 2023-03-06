namespace Disciples.Resources.Database.Components;

/// <summary>
/// Стоимость найма юнита/постройки здания и так далее.
/// </summary>
public class ResourceCost
{
    /// <summary>
    /// Стоимость в золоте.
    /// </summary>
    public int Gold { get; init; }

    /// <summary>
    /// Стоимость в мане смерти.
    /// </summary>
    public int DeathMana { get; init; }

    /// <summary>
    /// Стоимость в мане рун.
    /// </summary>
    public int RuneMana { get; init; }

    /// <summary>
    /// Стоимость в мане жизни.
    /// </summary>
    public int LifeMana { get; init; }

    /// <summary>
    /// Стоимость в мане ада.
    /// </summary>
    public int InfernalMana { get; init; }

    /// <summary>
    /// Стоимость в мане рощи.
    /// </summary>
    public int GroveMana { get; init; }

    /// <summary>
    /// Получить данные о стоимости.
    /// </summary>
    public static ResourceCost Parse(string? costString)
    {
        if (string.IsNullOrWhiteSpace(costString))
            throw new ArgumentException($"Стоимость не может быть пустой строкой", nameof(costString));

        int gold = 0,
            deathMana = 0,
            runeMana = 0,
            lifeMana = 0,
            infernalMana = 0,
            groveMana = 0;
        var resourceCosts = costString.Split(":", StringSplitOptions.RemoveEmptyEntries);
        foreach (var resourceCost in resourceCosts)
        {
            var resourceType = resourceCost[0];
            var price = int.Parse(resourceCost[1..]);
            switch (resourceType)
            {
                case 'g':
                    gold = price;
                    break;
                case 'e':
                    deathMana = price;
                    break;
                case 'w':
                    runeMana = price;
                    break;
                case 'y':
                    lifeMana = price;
                    break;
                case 'r':
                    infernalMana = price;
                    break;
                case 'b':
                    groveMana = price;
                    break;
                default:
                    throw new ArgumentException($"Некорректная стоимость в ресурсах: {costString}. Неизвестный тип ресурса {resourceType}", nameof(costString));
            }
        }

        return new ResourceCost
        {
            Gold = gold,
            DeathMana = deathMana,
            RuneMana = runeMana,
            LifeMana = lifeMana,
            InfernalMana = infernalMana,
            GroveMana = groveMana
        };
    }
}