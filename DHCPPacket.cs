using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer
{
    public struct DHCPPacket
    {
        public byte Opcode;
        public byte HType;
        public byte HLen;
        public byte HOps;
        public byte[] TransactionId;
        public byte[] ElapsedBootTime;
        public byte[] Flags;
        public byte[] ClientIP;
        public byte[] SelfClientIP;
        public byte[] ServerIP;
        public byte[] RelayIP;
        public byte[] ClientHWAddress;
        public byte[] ServerHostname;
        public byte[] BootFileName;
        public byte[] Cookie;
        public byte[] Options;

        public IEnumerable<DHCPOption> GetDHCPOptions()
        {
            if (Options.Length == 0)
                yield break;

            DHCPOption result;
            byte index = 0;
            while (Options[index] != (byte)DHCPOptions.END_Option)
            {
                result = new DHCPOption
                {
                    OptionType = (DHCPOptions)Options[index++],
                    Length = Options[index],
                    Data = new Span<byte>(Options, index + 1, Options[index]).ToArray()
                };

                index += Options[index];
                index++;

                yield return result;
            }

            yield break;
        }

        public DHCPPacket(byte[] data)
        {
            using MemoryStream memoryStream = new(data);
            using BinaryReader reader = new(memoryStream);

            Opcode = reader.ReadByte();
            HType = reader.ReadByte();
            HLen = reader.ReadByte();
            HOps = reader.ReadByte();
            TransactionId = reader.ReadBytes(4);
            ElapsedBootTime = reader.ReadBytes(2);
            Flags = reader.ReadBytes(2);
            ClientIP = reader.ReadBytes(4);
            SelfClientIP = reader.ReadBytes(4);
            ServerIP = reader.ReadBytes(4);
            RelayIP = reader.ReadBytes(4);
            ClientHWAddress = reader.ReadBytes(16);
            ServerHostname = reader.ReadBytes(64);
            BootFileName = reader.ReadBytes(128);
            Cookie = reader.ReadBytes(4);

            Options = data.Length > 240 ? 
                reader.ReadBytes(data.Length - 240) :
                Array.Empty<byte>();
        }
    }

    public struct DHCPOption
    {
        public DHCPOptions OptionType;
        public byte Length;
        public byte[] Data;

        public override string ToString()
        {
            return $"{OptionType} " + OptionType switch
            {
                DHCPOptions.HostName => $"[{Encoding.ASCII.GetString(Data)}]",
                DHCPOptions.RequestedIPAddress => $"[{string.Join('.', Data.Select(x => x.ToString()))}]",
                _ => $"[Length: {Length}]",
            };
        }
    }
}
