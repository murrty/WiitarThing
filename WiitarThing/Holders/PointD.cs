#nullable enable
namespace WiitarThing;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

[DebuggerDisplay("X = {X}, Y = {Y}")]
public struct PointD : IComparable, IComparable<PointD>, IComparer, IComparer<PointD>, IEqualityComparer, IEqualityComparer<PointD>, IEquatable<PointD> {
    [System.Xml.Serialization.XmlAttribute]
    public double X;

    [System.Xml.Serialization.XmlAttribute]
    public double Y;

    public PointD(double x, double y)
    {
        X = x;
        Y = y;
    }

    #region interfaces
    // IComparable
    /// <inheritdoc/>
    public readonly int CompareTo(object? obj)
    {
        if (obj is not PointD other) {
            return 1;
        }

        return CompareTo(this, other);
    }

    // IComparable<PointD>
    /// <inheritdoc/>
    public readonly int CompareTo(PointD other)
    {
        return CompareTo(this, other);
    }

    // IComparer
    /// <inheritdoc/>
    public readonly int Compare(object? x, object? y)
    {
        if (x is not PointD a) {
            if (y is not PointD) {
                return 0;
            }

            return 1;
        }

        if (y is not PointD b) {
            return -1;
        }

        return Compare(a, b);
    }

    /// <inheritdoc/>
    readonly int IComparer<PointD>.Compare(PointD x, PointD y)
    {
        return Compare(x, y);
    }

    /// <inheritdoc/>
    readonly bool IEqualityComparer.Equals(object? x, object? y)
    {
        if (x is not PointD a || y is not PointD b) {
            return false;
        }

        return Equals(a, b);
    }
    /// <inheritdoc/>
    readonly int IEqualityComparer.GetHashCode([DisallowNull] object obj)
    {
        if (obj is not PointD version) {
            return 0;
        }

        return GetHashCode(version);
    }

    /// <inheritdoc/>
    readonly bool IEqualityComparer<PointD>.Equals(PointD x, PointD y)
    {
        return Equals(x, y);
    }
    /// <inheritdoc/>
    readonly int IEqualityComparer<PointD>.GetHashCode(PointD obj)
    {
        return obj.GetHashCode();
    }

    // IEquatable<PointD>
    /// <inheritdoc/>
    public readonly bool Equals(PointD other) {
        return Equals(this, other);
    }
    #endregion interfaces

    #region overrides
    public override readonly bool Equals(object obj)
    {
        if (obj is not PointD other)
            return false;

        return Equals(this, other);
    }

    public override readonly int GetHashCode()
    {
        return GetHashCode(this);
    }
    #endregion overrides

    #region methods
    private static bool Equals(PointD x, PointD y)
    {
        return x.X == y.X &&
               x.Y == y.Y;
    }

    private static int GetHashCode(PointD point)
    {
        int hashCode = 1861411795;
        hashCode = (hashCode * -1521134295) + point.X.GetHashCode();
        hashCode = (hashCode * -1521134295) + point.Y.GetHashCode();
        return hashCode;
    }

    private static int Compare(PointD x, PointD y)
    {
        if (x.X > y.X) {
            return 1;
        }

        if (x.X < y.X) {
            return -1;
        }

        if (x.Y > y.Y) {
            return 1;
        }

        if (x.Y < y.Y) {
            return -1;
        }

        return 0;
    }

    private static int CompareTo(PointD x, PointD y)
    {
        return Compare(x, y);
    }
    #endregion methods

    #region operators
    public static bool operator ==(PointD left, PointD right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(PointD left, PointD right)
    {
        return !Equals(left, right);
    }
    #endregion operators
}
