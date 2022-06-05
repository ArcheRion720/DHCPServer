using System.Net;

namespace DHCPServer
{
    public enum LeaseState
    {
        Offer = 1,
        Assigned = 2,
        Release = 3,
        Static = 4,
    }

    public class DHCPClient
    {
        public byte[]? HardwareAddress { get; set; }
        public byte[]? IPAddress { get; set; }
        public uint IPAddressLong { get; set; }
        public LeaseState State { get; set; }
        public DateTime OfferTime { get; set; }
        public DateTime LeaseStartTime { get; set; }
        public DateTime LeaseEndTime { get; set; }

        public override string ToString()
        {
            #pragma warning disable CS8604
            return $"IP: {new IPAddress(IPAddress)} | HW: {string.Join('.', HardwareAddress)}";
            #pragma warning restore CS8604
        }
    }
}
