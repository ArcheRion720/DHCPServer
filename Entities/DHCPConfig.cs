#nullable disable
using Newtonsoft.Json;
using System.Net;

namespace DHCPServer
{
    public struct DHCPConfig
    {
        public IPEndPoint Endpoint { get; private set; }
        public IPEndPoint Broadcast { get; private set; }
        public uint Network { get; private set; }
        public uint StartPool { get; private set; }
        public uint EndPool { get; private set; }
        public byte[] Mask { get; private set; }
        public byte[] DNS { get; private set; }
        public byte[] Gateway { get; private set; }
        public byte[] LeaseTime { get; private set; }
        public uint LeaseTimeLong { get; private set; }
        public uint Length { get; private set; }
        public DHCPConfigStub Data { get; private set; }

        public static bool CreateConfig(DHCPConfigStub stub, out DHCPConfig config)
        {
            DHCPConfig result = new();

            if (IPAddress.TryParse(stub.StartPool,  out IPAddress startAddr)    &&
                IPAddress.TryParse(stub.EndPool,    out IPAddress endAddr)      &&
                IPAddress.TryParse(stub.Mask,       out IPAddress maskAddr)     &&
                IPAddress.TryParse(stub.Endpoint,   out IPAddress bindAddr))
            {
                uint startpool = BitConverter.ToUInt32(startAddr.GetAddressBytes().Reverse().ToArray());
                uint endpool = BitConverter.ToUInt32(endAddr.GetAddressBytes().Reverse().ToArray());
                byte[] maskBytes = maskAddr.GetAddressBytes();
                uint maskBits = BitConverter.ToUInt32(maskBytes.Reverse().ToArray());
                uint bind = BitConverter.ToUInt32(bindAddr.GetAddressBytes().Reverse().ToArray());

                uint network = bind & maskBits;
                uint broadcast = network | (~maskBits);

                if (((startpool & maskBits) == network) && ((endpool & maskBits) == network))
                {
                    result.Endpoint = new IPEndPoint(bindAddr, 67);
                    result.Broadcast = new IPEndPoint(new IPAddress(BitConverter.GetBytes(broadcast).Reverse().ToArray()), 68);
                    result.StartPool = startpool;
                    result.EndPool = endpool;
                    result.Mask = maskBytes;
                    result.Length = endpool - startpool + 1;
                }
                else
                {
                    config = default;
                    return false;
                }

                if(stub.DNSServer != null && IPAddress.TryParse(stub.DNSServer, out IPAddress dnsAddr))
                {
                    result.DNS = dnsAddr.GetAddressBytes();
                }

                if (stub.Gateway != null && IPAddress.TryParse(stub.Gateway, out IPAddress gateway))
                {
                    result.Gateway = gateway.GetAddressBytes();
                }

                if(stub.LeaseTime != null)
                {
                    result.LeaseTime = BitConverter.GetBytes(stub.LeaseTime.Value).Reverse().ToArray();
                    result.LeaseTimeLong = stub.LeaseTime.Value;
                }
                else
                {
                    config = default;
                    return false;
                }

                result.Data = stub;

                config = result;
                return true;
            }

            config = default;
            return false;
        }
    }

    public struct IPReservation
    {
        [JsonProperty(Required = Required.Always)] public string Hardware { get; set; }
        [JsonProperty(Required = Required.Always)] public string IPAddress { get; set; }
    }

    public struct DHCPConfigStub
    {
        [JsonProperty(Required = Required.Always)] public string Endpoint { get; set; }
        [JsonProperty(Required = Required.Always)] public string StartPool { get; set; }
        [JsonProperty(Required = Required.Always)] public string EndPool { get; set; }
        [JsonProperty(Required = Required.Always)] public string Mask { get; set; }
        public string Name { get; set; }
        public string Gateway { get; set; }
        public string DNSServer { get; set; }
        public uint? LeaseTime { get; set; }
        public List<IPReservation> Reservations { get; set; }
    }
}
