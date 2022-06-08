namespace DHCPServer
{
    public class MACAddressComparer : EqualityComparer<byte[]>
    {
        public override bool Equals(byte[]? x, byte[]? y)
        {
            if (x == null || y == null)
                return x == y;

            if (x.Length < 6 || y.Length < 6)
                return false;

            if (ReferenceEquals(x, y))
                return true;

            if (x.Length != y.Length)
                return false;

            for (int i = 0; i < 6; i++)
                if (x[i] != y[i])
                    return false;

            return true;
        }

        public override int GetHashCode(byte[] arg)
        {
            ArgumentNullException.ThrowIfNull(arg, nameof(arg));

            return arg.Length;
        }

        private static MACAddressComparer? instance;
        public static MACAddressComparer Instance
        {
            get => instance ??= new MACAddressComparer();
        }
    }
}
