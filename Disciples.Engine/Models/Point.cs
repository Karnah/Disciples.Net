namespace Disciples.Engine.Models;

/// <summary>
/// Координата точки.
/// </summary>
public struct Point
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
    public double X { get; set; }

    /// <summary>
    /// Y-координата.
    /// </summary>
    public double Y { get; set; }
}