using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer
{
    public static class DHCPGeometry
    {
        public static readonly DHCPOption OfferTypeOption = new()
        {
            OptionType = DHCPOptions.DHCPMessageTYPE,
            Length = 1,
            Data = new byte[] { (byte)DHCPMessageType.Offer }
        };

        public static readonly DHCPOption AcceptTypeOption = new()
        {
            OptionType = DHCPOptions.DHCPMessageTYPE,
            Length = 1,
            Data = new byte[] { (byte)DHCPMessageType.Ack }
        };

        public static readonly DHCPOption NAKTypeOption = new()
        {
            OptionType = DHCPOptions.DHCPMessageTYPE,
            Length = 1,
            Data = new byte[] { (byte)DHCPMessageType.Nak }
        };
    }
}
