using DHCPServer.Logging;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace DHCPServer
{
    public class DHCPResponseBuilder
    {
        public static byte[] BuildResponseBytes(DHCPPacket packet, IEnumerable<DHCPOption> options)
        {
            int optionsBytes = options.Sum(x => x.Length) + (options.Count() * 2) + 1;
            byte[] result = new byte[optionsBytes + 240];

            using MemoryStream memStream = new(result);
            using BinaryWriter writer = new(memStream);

            writer.Write(packet.Opcode);
            writer.Write(packet.HType);
            writer.Write(packet.HLen);
            writer.Write(packet.HOps);
            writer.Write(packet.TransactionId);
            writer.Write(packet.ElapsedBootTime);
            writer.Write(packet.Flags);
            writer.Write(packet.ClientIP);
            writer.Write(packet.SelfClientIP);
            writer.Write(packet.ServerIP);
            writer.Write(packet.RelayIP);
            writer.Write(packet.ClientHWAddress);
            writer.Write(packet.ServerHostname);
            writer.Write(packet.BootFileName);
            writer.Write(packet.Cookie);

            foreach(DHCPOption opt in options)
            {
                writer.Write((byte)opt.OptionType);
                writer.Write(opt.Length);
                writer.Write(opt.Data);
            }

            writer.Write((byte)255);
            return result;
        }

        public static byte[]? BuildDHCPOfferResponse(DHCPPacket dhcpDiscoverPacket, DHCPServer server)
        {
            byte[] address = server.GetNextAddress(dhcpDiscoverPacket.ClientHWAddress, out bool clientExists);
            DHCPPacket packet = new DHCPPacket
            {
                Opcode = (byte)BOOTOpcode.BOOTREPLY,
                HType = 1,
                HLen = 6,
                HOps = 0,
                TransactionId = dhcpDiscoverPacket.TransactionId,
                ElapsedBootTime = new byte[2],
                ClientIP = new byte[4],
                SelfClientIP = address,
                ServerIP = new byte[4],
                Flags = dhcpDiscoverPacket.Flags,
                RelayIP = dhcpDiscoverPacket.RelayIP,
                ClientHWAddress = dhcpDiscoverPacket.ClientHWAddress,
                ServerHostname = new byte[64],
                BootFileName = new byte[128],
                Cookie = new byte[4] { 99, 130, 83, 99 }
            };

            AddServerIdentifier(ref packet);

            List<DHCPOption> options = new List<DHCPOption>
            {
                DHCPGeometry.OfferTypeOption,
                new DHCPOption()
                {
                    OptionType = DHCPOptions.SubnetMask,
                    Length = 4,
                    Data = server.Config.Mask
                },
                new DHCPOption()
                {
                    OptionType = DHCPOptions.Router,
                    Length = 4,
                    Data = server.Config.Gateway
                },
                new DHCPOption()
                {
                    OptionType = DHCPOptions.ServerIdentifier,
                    Length = 4,
                    Data = server.Config.Endpoint.Address.GetAddressBytes()
                },
                new DHCPOption()
                {
                    OptionType = DHCPOptions.IPAddressLeaseTime,
                    Length = 4,
                    Data = server.Config.LeaseTime
                },
                new DHCPOption()
                {
                    OptionType = DHCPOptions.PerformRouterDiscovery,
                    Length = 1,
                    Data = new byte[1] { 0 }
                },
            };

            if(server.Config.DNS is not null)
            {
                options.Add(new DHCPOption()
                {
                    OptionType = DHCPOptions.DomainNameServer,
                    Length = 4,
                    Data = server.Config.DNS
                });
            }

            if (!clientExists)
            {
                server.Clients.Add(new DHCPClient()
                {
                    IPAddress = address,
                    IPAddressLong = BitConverter.ToUInt32(address.Reverse().ToArray()),
                    OfferTime = DateTime.Now,
                    HardwareAddress = packet.ClientHWAddress,
                    State = LeaseState.Offer
                });
            }

            if (server.Logger is not null)
            {
                var message = $"Sending OFFER packet to MAC {new PhysicalAddress(packet.ClientHWAddress.Take(6).ToArray())}. Offering {new IPAddress(address)}";
                server.Logger.Log(LogLevel.INFO, message, server.Name);
            }

            return BuildResponseBytes(packet, options);
        }

        public static byte[] BuildDHCPNakResponse(DHCPPacket dhcpPacket, DHCPServer server)
        {
            DHCPPacket packet = new DHCPPacket
            {
                Opcode = (byte)BOOTOpcode.BOOTREPLY,
                HType = 1,
                HLen = 6,
                HOps = 0,
                TransactionId = dhcpPacket.TransactionId,
                ElapsedBootTime = new byte[2],
                ClientIP = new byte[4],
                SelfClientIP = new byte[4],
                ServerIP = new byte[4],
                Flags = dhcpPacket.Flags,
                RelayIP = dhcpPacket.RelayIP,
                ClientHWAddress = dhcpPacket.ClientHWAddress,
                ServerHostname = new byte[64],
                BootFileName = new byte[128],
                Cookie = new byte[4] { 99, 130, 83, 99 }
            };

            AddServerIdentifier(ref packet);

            List<DHCPOption> options = new List<DHCPOption>()
            {
                DHCPGeometry.NAKTypeOption,
                new DHCPOption()
                {
                    OptionType = DHCPOptions.ServerIdentifier,
                    Length = 4,
                    Data = server.Config.Endpoint.Address.GetAddressBytes()
                },
            };

            return BuildResponseBytes(packet, options);
        }

        public static byte[]? BuildDHCPAckResponse(DHCPPacket dhcpPacket, DHCPServer server)
        {
            DHCPClient? client = server.Clients.FirstOrDefault(x => MACAddressComparer.Instance.Equals(x.HardwareAddress, dhcpPacket.ClientHWAddress));
            if(client is null)
                return null;

            if (client.IPAddress is null)
                return null;

            DHCPPacket packet = new DHCPPacket
            {
                Opcode = (byte)BOOTOpcode.BOOTREPLY,
                HType = 1,
                HLen = 6,
                HOps = 0,
                TransactionId = dhcpPacket.TransactionId,
                ElapsedBootTime = new byte[2],
                ClientIP = new byte[4],
                SelfClientIP = client.IPAddress,
                ServerIP = new byte[4],
                Flags = dhcpPacket.Flags,
                RelayIP = dhcpPacket.RelayIP,
                ClientHWAddress = dhcpPacket.ClientHWAddress,
                ServerHostname = new byte[64],
                BootFileName = new byte[128],
                Cookie = new byte[4] { 99, 130, 83, 99 }
            };

            AddServerIdentifier(ref packet);

            List<DHCPOption> options = new List<DHCPOption>()
            {
                DHCPGeometry.AcceptTypeOption,
                new DHCPOption()
                {
                    OptionType = DHCPOptions.ServerIdentifier,
                    Length = 4,
                    Data = server.Config.Endpoint.Address.GetAddressBytes()
                },
                new DHCPOption()
                {
                    OptionType = DHCPOptions.IPAddressLeaseTime,
                    Length = 4,
                    Data = server.Config.LeaseTime
                },
                new DHCPOption()
                {
                    OptionType = DHCPOptions.SubnetMask,
                    Length = 4,
                    Data = server.Config.Mask
                },
                new DHCPOption()
                {
                    OptionType = DHCPOptions.Router,
                    Length = 4,
                    Data = server.Config.Endpoint.Address.GetAddressBytes()
                },
                new DHCPOption()
                {
                    OptionType = DHCPOptions.PerformRouterDiscovery,
                    Length = 1,
                    Data = new byte[1] { 0 }
                },
            };

            if(server.Config.DNS is not null)
            {
                options.Add(new DHCPOption()
                {
                    OptionType = DHCPOptions.DomainNameServer,
                    Length = 4,
                    Data = server.Config.DNS
                });
            }

            if (client.State == LeaseState.Offer)
            {
                client.State = LeaseState.Assigned;
                client.LeaseStartTime = DateTime.Now;
            }

            client.LeaseEndTime = DateTime.Now.AddSeconds(server.Config.LeaseTimeLong);

            return BuildResponseBytes(packet, options);
        }

        private static readonly char[] SID = Environment.MachineName.ToCharArray();
        private static void AddServerIdentifier(ref DHCPPacket packet)
        {
            Encoding.ASCII.GetBytes(SID, packet.ServerHostname);
        }
    }
}
