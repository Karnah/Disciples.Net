using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Numerics;

namespace Disciples.Common.Models;

/// <summary>
/// Represents the size of a rectangular region with an ordered pair of width and height.
/// </summary>
/// <remarks>
/// Realization <see cref="SizeF" /> with <see cref="double" />.
/// </remarks>
[Serializable]
public struct SizeD : IEquatable<SizeD>
{
    /// <summary>
    /// Initializes a new instance of the <see cref='SizeD'/> class.
    /// </summary>
    public static readonly SizeD Empty;
    private double width; // Do not rename (binary serialization)
    private double height; // Do not rename (binary serialization)

    /// <summary>
    /// Initializes a new instance of the <see cref='SizeD'/> class from the specified
    /// existing <see cref='SizeD'/>.
    /// </summary>
    public SizeD(SizeD size)
    {
        width = size.width;
        height = size.height;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref='SizeD'/> class from the specified
    /// <see cref='System.Drawing.PointF'/>.
    /// </summary>
    public SizeD(PointF pt)
    {
        width = pt.X;
        height = pt.Y;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref='SizeD'/> struct from the specified
    /// <see cref="System.Numerics.Vector2"/>.
    /// </summary>
    public SizeD(Vector2 vector)
    {
        width = vector.X;
        height = vector.Y;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref='SizeD'/> class from the specified dimensions.
    /// </summary>
    public SizeD(double width, double height)
    {
        this.width = width;
        this.height = height;
    }


    /// <summary>
    /// Converts the specified <see cref="System.Numerics.Vector2"/> to a <see cref="SizeD"/>.
    /// </summary>
    public static explicit operator SizeD(Vector2 vector) => new SizeD(vector);

    /// <summary>
    /// Performs vector addition of two <see cref='SizeD'/> objects.
    /// </summary>
    public static SizeD operator +(SizeD sz1, SizeD sz2) => Add(sz1, sz2);

    /// <summary>
    /// Contracts a <see cref='SizeD'/> by another <see cref='SizeD'/>
    /// </summary>
    public static SizeD operator -(SizeD sz1, SizeD sz2) => Subtract(sz1, sz2);

    /// <summary>
    /// Multiplies <see cref="SizeD"/> by a <see cref="double"/> producing <see cref="SizeD"/>.
    /// </summary>
    /// <param name="left">Multiplier of type <see cref="double"/>.</param>
    /// <param name="right">Multiplicand of type <see cref="SizeD"/>.</param>
    /// <returns>Product of type <see cref="SizeD"/>.</returns>
    public static SizeD operator *(double left, SizeD right) => Multiply(right, left);

    /// <summary>
    /// Multiplies <see cref="SizeD"/> by a <see cref="double"/> producing <see cref="SizeD"/>.
    /// </summary>
    /// <param name="left">Multiplicand of type <see cref="SizeD"/>.</param>
    /// <param name="right">Multiplier of type <see cref="double"/>.</param>
    /// <returns>Product of type <see cref="SizeD"/>.</returns>
    public static SizeD operator *(SizeD left, double right) => Multiply(left, right);

    /// <summary>
    /// Divides <see cref="SizeD"/> by a <see cref="double"/> producing <see cref="SizeD"/>.
    /// </summary>
    /// <param name="left">Dividend of type <see cref="SizeD"/>.</param>
    /// <param name="right">Divisor of type <see cref="int"/>.</param>
    /// <returns>Result of type <see cref="SizeD"/>.</returns>
    public static SizeD operator /(SizeD left, double right)
        => new SizeD(left.width / right, left.height / right);

    /// <summary>
    /// Tests whether two <see cref='SizeD'/> objects are identical.
    /// </summary>
    public static bool operator ==(SizeD sz1, SizeD sz2) => sz1.Width == sz2.Width && sz1.Height == sz2.Height;

    /// <summary>
    /// Tests whether two <see cref='SizeD'/> objects are different.
    /// </summary>
    public static bool operator !=(SizeD sz1, SizeD sz2) => !(sz1 == sz2);

    /// <summary>
    /// Converts the specified <see cref='SizeD'/> to a <see cref='System.Drawing.PointF'/>.
    /// </summary>
    public static explicit operator PointD(SizeD size) => new PointD(size.Width, size.Height);

    /// <summary>
    /// Tests whether this <see cref='SizeD'/> has zero width and height.
    /// </summary>
    [Browsable(false)]
    public readonly bool IsEmpty => width == 0 && height == 0;

    /// <summary>
    /// Represents the horizontal component of this <see cref='SizeD'/>.
    /// </summary>
    public double Width
    {
        readonly get => width;
        set => width = value;
    }

    /// <summary>
    /// Represents the vertical component of this <see cref='SizeD'/>.
    /// </summary>
    public double Height
    {
        readonly get => height;
        set => height = value;
    }

    /// <summary>
    /// Performs vector addition of two <see cref='SizeD'/> objects.
    /// </summary>
    public static SizeD Add(SizeD sz1, SizeD sz2) => new SizeD(sz1.Width + sz2.Width, sz1.Height + sz2.Height);

    /// <summary>
    /// Contracts a <see cref='SizeD'/> by another <see cref='SizeD'/>.
    /// </summary>
    public static SizeD Subtract(SizeD sz1, SizeD sz2) => new SizeD(sz1.Width - sz2.Width, sz1.Height - sz2.Height);

    /// <summary>
    /// Tests to see whether the specified object is a <see cref='SizeD'/>  with the same dimensions
    /// as this <see cref='SizeD'/>.
    /// </summary>
    public readonly override bool Equals([NotNullWhen(true)] object? obj) => obj is SizeD && Equals((SizeD)obj);

    /// <inheritdoc />
    public readonly bool Equals(SizeD other) => this == other;

    /// <inheritdoc />
    public readonly override int GetHashCode() => HashCode.Combine(Width, Height);

    /// <summary>
    /// Convert to <see cref="PointD" />.
    /// </summary>
    public readonly PointD ToPointD() => (PointD)this;

    /// <summary>
    /// Convert to <see cref="Size" />.
    /// </summary>
    public readonly Size ToSize() => new Size(unchecked((int)Width), unchecked((int)Height));

    /// <summary>
    /// Creates a human-readable string that represents this <see cref='SizeD'/>.
    /// </summary>
    public readonly override string ToString() => $"{{Width={width}, Height={height}}}";

    /// <summary>
    /// Multiplies <see cref="SizeD"/> by a <see cref="double"/> producing <see cref="SizeD"/>.
    /// </summary>
    /// <param name="size">Multiplicand of type <see cref="SizeD"/>.</param>
    /// <param name="multiplier">Multiplier of type <see cref="double"/>.</param>
    /// <returns>Product of type SizeD.</returns>
    private static SizeD Multiply(SizeD size, double multiplier) =>
        new SizeD(size.width * multiplier, size.height * multiplier);
}