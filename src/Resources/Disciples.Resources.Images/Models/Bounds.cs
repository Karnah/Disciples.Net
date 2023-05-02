namespace Disciples.Resources.Images.Models;

/// <summary>
/// Границы изображения.
/// </summary>
public readonly struct Bounds
{
    /// <summary>
    /// Создать границы с размером изображения.
    /// </summary>
    public Bounds(int width, int height) : this()
    {
        MaxRow = height;
        MaxColumn = width;
    }

    /// <summary>
    /// Создать границы изображения.
    /// </summary>
    public Bounds(int minRow, int maxRow, int minColumn, int maxColumn)
    {
        MinRow = minRow;
        MaxRow = maxRow;
        MinColumn = minColumn;
        MaxColumn = maxColumn;
    }

    /// <summary>
    /// Минимальная строка.
    /// </summary>
    public int MinRow { get; init; }

    /// <summary>
    /// Максимальная строка.
    /// </summary>
    public int MaxRow { get; init; }

    /// <summary>
    /// Минимальная колонка.
    /// </summary>
    public int MinColumn { get; init; }

    /// <summary>
    /// Максимальная колонка.
    /// </summary>
    public int MaxColumn { get; init; }

    /// <summary>
    /// Ширина изображения.
    /// </summary>
    public int Width => MaxColumn - MinColumn;

    /// <summary>
    /// Высота изображения.
    /// </summary>
    public int Height => MaxRow - MinRow;

    /// <summary>
    /// Сравнить два объекта границ.
    /// </summary>
    public bool Equals(Bounds other)
    {
        return MinRow == other.MinRow && MaxRow == other.MaxRow && MinColumn == other.MinColumn && MaxColumn == other.MaxColumn;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Bounds other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(MinRow, MaxRow, MinColumn, MaxColumn);
    }

    public static bool operator ==(Bounds left, Bounds right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Bounds left, Bounds right)
    {
        return !left.Equals(right);
    }
}