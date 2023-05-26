namespace Disciples.Common.Models;

/// <summary>
/// Координата точки.
/// </summary>
public readonly struct Point
{
    /// <summary>
    /// Создать точку с указанными координатами.
    /// </summary>
    public Point(double x, double y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// X-координата.
    /// </summary>
    public double X { get; }

    /// <summary>
    /// Y-координата.
    /// </summary>
    public double Y { get; }
}