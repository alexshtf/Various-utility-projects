using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StraightGenCylinder
{
    static class DelegateComparer
    {
        public static DelegateComparer<T> Create<T>(Comparison<T> comparison)
        {
            return new DelegateComparer<T>(comparison);
        }
    }

    class DelegateComparer<T> : IComparer<T>
    {
        private readonly Comparison<T> comparison;

        public DelegateComparer(Comparison<T> comparison)
        {
            this.comparison = comparison;
        }

        public int Compare(T x, T y)
        {
            return comparison(x, y);
        }
    }
}
