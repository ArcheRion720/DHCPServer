using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer
{
    public class ByteArrayCompare : EqualityComparer<byte[]>
    {
        public override bool Equals(byte[]? x, byte[]? y)
        {
            if (x == null || y == null)
                return x == y;

            if (ReferenceEquals(x, y))
                return true;

            if (x.Length != y.Length)
                return false;

            return x.SequenceEqual(y);
        }

        public override int GetHashCode(byte[] arg)
        {
            ArgumentNullException.ThrowIfNull(arg, nameof(arg));

            return arg.Length;
        }
    }
}
