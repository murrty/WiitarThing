namespace WiitarThing {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;

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
        public int CompareTo(object obj)
        {
            if (obj is PointD other) {
                return CompareTo(this, other);
            }

            return 0;
        }

        // IComparable<PointD>
        /// <inheritdoc/>
        public int CompareTo(PointD other)
        {
            return CompareTo(this, other);
        }

        // IComparer
        /// <inheritdoc/>
        public int Compare(object x, object y)
        {
            if (x is PointD a) {
                if (y is PointD b) {
                    return Compare(a, b);
                }

                return -1;
            }

            if (y is PointD) {
                return 1;
            }

            return 0;
        }

        /// <inheritdoc/>
        int IComparer<PointD>.Compare(PointD x, PointD y)
        {
            return Compare(x, y);
        }

        /// <inheritdoc/>
        bool IEqualityComparer.Equals(object x, object y)
        {
            if (x is PointD a && y is PointD b) {
                return Equals(a, b);
            }

            return false;
        }
        /// <inheritdoc/>
        int IEqualityComparer.GetHashCode(object obj)
        {
            if (obj is PointD version) {
                return GetHashCode(version);
            }

            return 0;
        }

        /// <inheritdoc/>
        bool IEqualityComparer<PointD>.Equals(PointD x, PointD y)
        {
            return Equals(x, y);
        }
        /// <inheritdoc/>
        int IEqualityComparer<PointD>.GetHashCode(PointD obj)
        {
            return obj.GetHashCode();
        }

        // IEquatable<PointD>
        /// <inheritdoc/>
        public bool Equals(PointD other) {
            return Equals(this, other);
        }
        #endregion interfaces

        #region overrides
        public override bool Equals(object obj)
        {
            if (obj is PointD other)
            {
                return Equals(this, other);
            }

            return false;
        }

        public override int GetHashCode()
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
}
