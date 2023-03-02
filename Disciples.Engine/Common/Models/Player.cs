namespace Disciples.Engine.Common.Models;

/// <summary>
/// Информация об игроке.
/// </summary>
public class Player
{
    /// <summary>
    /// Создать объект типа <see cref="Player" />.
    /// </summary>
    public Player(int id, bool isComputer)
    {
        Id = id;
        IsComputer = isComputer;
    }

    /// <summary>
    /// Уникальный идентификатор игрока.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Управляется ли игрок компьютером (ИИ).
    /// </summary>
    public bool IsComputer { get; }
}