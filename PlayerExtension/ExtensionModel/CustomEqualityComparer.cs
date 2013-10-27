using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerExtension.ExtensionModel
{
    public class CustomEqualityComparer<T> : IEqualityComparer<T>
    {
        private Func<T, T, bool> mComparer;

        public CustomEqualityComparer(Func<T, T, bool> comparer)
        {
            mComparer = comparer;
        }

        public bool Equals(T x, T y)
        {
            return mComparer(x, y);
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }
    }
}
