﻿#nullable disable
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace DHCPServer
{
    public class DHCPServer
    {
        public bool Running { get; set; }
        public string Name { get; set; }
        public DHCPConfig Config { get; private set; }
        public List<DHCPClient> Clients { get; private set; }

        private readonly UdpClient udp;
        public readonly CancellationTokenSource cancellationToken;

        public DHCPServer(DHCPConfig config)
        {
            udp = new UdpClient(config.Endpoint);
            cancellationToken = new CancellationTokenSource();

            Config = config;
            Clients = new List<DHCPClient>();
            Running = false;
            Name = config.Data.Name;
        }
        
        public void Start()
        {
            Running = true;
            Thread serverThread = new(() =>
            {
                UdpState state = new()
                {
                    endpoint = Config.Endpoint,
                    udpClient = udp
                };

                var areceive = state.udpClient.BeginReceive(new AsyncCallback(HandleIncomingPacket), state);
                while (!cancellationToken.IsCancellationRequested)
                {
                    Thread.Sleep(100);
                }

                udp.EndReceive(areceive, ref state.endpoint!);
            });

            serverThread.Start();
        }

        public void Stop()
        {
            Running = false;
            cancellationToken.Cancel();
        }

        public void ReserveAddress(byte[] clientHW, IPAddress address)
        {
            byte[] bytes = address.GetAddressBytes();
            DHCPClient client = new()
            {
                State = LeaseState.Static,
                IPAddress = bytes,
                IPAddressLong = BitConverter.ToUInt32(bytes.Reverse().ToArray()),
                HardwareAddress = new byte[16]
            };

            Array.Copy(clientHW, client.HardwareAddress, 6);

            Clients.Add(client);
        }

        private void HandleIncomingPacket(IAsyncResult result)
        {
            if (!(result.AsyncState as UdpState?).HasValue)
                return;

            UdpState state = (UdpState)result.AsyncState;

            byte[] bytes = state.udpClient.EndReceive(result, ref state.endpoint);
            DHCPPacket packet = new(bytes);
            IEnumerable<DHCPOption> options = packet.GetDHCPOptions();

            DHCPOption request = options.FirstOrDefault(x => x.OptionType == DHCPOptions.DHCPMessageTYPE);
            if(request.Length == 1)
            {
                byte[] response = null;
                switch ((DHCPMessageType)request.Data[0])
                {
                    case DHCPMessageType.Discover:
                        response = DHCPResponseBuilder.BuildDHCPOfferResponse(packet, this);
                        break;
                    case DHCPMessageType.Request:
                        if(Clients.Any(x => MACAddressComparer.Instance.Equals(packet.ClientHWAddress, x.HardwareAddress)))
                        {
                            response = DHCPResponseBuilder.BuildDHCPAckResponse(packet, this);
                        }
                        else
                        {
                            response = DHCPResponseBuilder.BuildDHCPNakResponse(packet, this);
                        }
                        break;
                    case DHCPMessageType.Release:
                        DHCPClient client = Clients.FirstOrDefault(x => MACAddressComparer.Instance.Equals(packet.ClientHWAddress, x.HardwareAddress));
                        response = null;

                        if(client is not null)
                        {
                            client.State = LeaseState.Release;
                        }

                        break;
                    default:
                        Console.WriteLine("Unknown packet");
                        break;
                }
                
                if(response != null)
                    state.udpClient.Send(response, response.Length, Config.Broadcast);
            }
            else
            {
                Console.WriteLine("Malformed or invalid packet");
            }

            if(!cancellationToken.IsCancellationRequested)
                state.udpClient.BeginReceive(HandleIncomingPacket, state);
        }

        private uint ipaccess = 0;
        public byte[] GetNextAddress(byte[] clienthw, out bool exists)
        {
            exists = false;
            DHCPClient client = Clients.FirstOrDefault(x => MACAddressComparer.Instance.Equals(x.HardwareAddress, clienthw));
            if (client is not null)
            {
                exists = true;
                return client.IPAddress;
            }

            if (++ipaccess == Config.Length)
                ipaccess = 0;

            var address = Config.StartPool + ipaccess;
            if(Clients.Any(x => x.IPAddressLong == address))
            {
                DateTime now = DateTime.Now;
                Clients.RemoveAll(x => {
                    return x.State switch
                    {
                        LeaseState.Release => true,
                        LeaseState.Offer => x.OfferTime.AddMinutes(1) < now,
                        LeaseState.Assigned => x.LeaseEndTime < now,
                        _ => true,
                    };
                });

                for(uint i = 0; i < Config.Length; i++)
                {
                    address = Config.StartPool + i;
                    if (!Clients.Any(x => x.IPAddressLong == address))
                        return BitConverter.GetBytes(address).Reverse().ToArray();
                }

                return null;
            }

            return BitConverter.GetBytes(address).Reverse().ToArray();
        }
    }
}
