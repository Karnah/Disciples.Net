using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Numerics;

namespace Disciples.Common.Models;

/// <summary>
/// Represents an ordered pair of x and y coordinates that define a point in a two-dimensional plane.
/// </summary>
/// <remarks>
/// Realization <see cref="PointF" /> with <see cref="double" />.
/// </remarks>
[Serializable]
public struct PointD : IEquatable<PointD>
{
    /// <summary>
    /// Creates a new instance of the <see cref='PointD'/> class with member data left uninitialized.
    /// </summary>
    public static readonly PointD Empty;
    private double _x; // Do not rename (binary serialization)
    private double _y; // Do not rename (binary serialization)

    /// <summary>
    /// Initializes a new instance of the <see cref='PointD'/> class with the specified coordinates.
    /// </summary>
    public PointD(double x, double y)
    {
        _x = x;
        _y = y;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref='System.Drawing.PointF'/> struct from the specified
    /// <see cref="System.Numerics.Vector2"/>.
    /// </summary>
    public PointD(Vector2 vector)
    {
        _x = vector.X;
        _y = vector.Y;
    }

    /// <summary>
    /// Gets a value indicating whether this <see cref='System.Drawing.PointF'/> is empty.
    /// </summary>
    [Browsable(false)]
    public readonly bool IsEmpty => _x == 0d && _y == 0d;

    /// <summary>
    /// Gets the x-coordinate of this <see cref='System.Drawing.PointF'/>.
    /// </summary>
    public double X
    {
        readonly get => _x;
        set => _x = value;
    }

    /// <summary>
    /// Gets the y-coordinate of this <see cref='System.Drawing.PointF'/>.
    /// </summary>
    public double Y
    {
        readonly get => _y;
        set => _y = value;
    }

    /// <summary>
    /// Translates a <see cref='System.Drawing.PointF'/> by a given <see cref='System.Drawing.Size'/> .
    /// </summary>
    public static PointD operator +(PointD pt, Size sz) => Add(pt, sz);

    /// <summary>
    /// Translates a <see cref='System.Drawing.PointF'/> by the negative of a given <see cref='System.Drawing.Size'/> .
    /// </summary>
    public static PointD operator -(PointD pt, Size sz) => Subtract(pt, sz);

    /// <summary>
    /// Translates a <see cref='System.Drawing.PointF'/> by a given <see cref='System.Drawing.SizeF'/> .
    /// </summary>
    public static PointD operator +(PointD pt, SizeF sz) => Add(pt, sz);

    /// <summary>
    /// Translates a <see cref='System.Drawing.PointF'/> by the negative of a given <see cref='System.Drawing.SizeF'/> .
    /// </summary>
    public static PointD operator -(PointD pt, SizeF sz) => Subtract(pt, sz);

    /// <summary>
    /// Compares two <see cref='System.Drawing.PointF'/> objects. The result specifies whether the values of the
    /// <see cref='System.Drawing.PointF.X'/> and <see cref='System.Drawing.PointF.Y'/> properties of the two
    /// <see cref='System.Drawing.PointF'/> objects are equal.
    /// </summary>
    public static bool operator ==(PointD left, PointD right) => left.X == right.X && left.Y == right.Y;

    /// <summary>
    /// Compares two <see cref='System.Drawing.PointF'/> objects. The result specifies whether the values of the
    /// <see cref='System.Drawing.PointF.X'/> or <see cref='System.Drawing.PointF.Y'/> properties of the two
    /// <see cref='System.Drawing.PointF'/> objects are unequal.
    /// </summary>
    public static bool operator !=(PointD left, PointD right) => !(left == right);

    /// <summary>
    /// Translates a <see cref='System.Drawing.PointF'/> by a given <see cref='System.Drawing.Size'/> .
    /// </summary>
    public static PointD Add(PointD pt, Size sz) => new PointD(pt.X + sz.Width, pt.Y + sz.Height);

    /// <summary>
    /// Translates a <see cref='System.Drawing.PointF'/> by the negative of a given <see cref='System.Drawing.Size'/> .
    /// </summary>
    public static PointD Subtract(PointD pt, Size sz) => new PointD(pt.X - sz.Width, pt.Y - sz.Height);

    /// <summary>
    /// Translates a <see cref='System.Drawing.PointF'/> by a given <see cref='System.Drawing.SizeF'/> .
    /// </summary>
    public static PointD Add(PointD pt, SizeF sz) => new PointD(pt.X + sz.Width, pt.Y + sz.Height);

    /// <summary>
    /// Translates a <see cref='System.Drawing.PointF'/> by the negative of a given <see cref='System.Drawing.SizeF'/> .
    /// </summary>
    public static PointD Subtract(PointD pt, SizeF sz) => new PointD(pt.X - sz.Width, pt.Y - sz.Height);

    /// <inheritdoc />
    public readonly override bool Equals([NotNullWhen(true)] object? obj) => obj is PointD && Equals((PointD)obj);

    /// <inheritdoc />
    public readonly bool Equals(PointD other) => this == other;

    /// <inheritdoc />
    public readonly override int GetHashCode() => HashCode.Combine(X.GetHashCode(), Y.GetHashCode());

    /// <inheritdoc />
    public readonly override string ToString() => $"{{X={_x}, Y={_y}}}";
}