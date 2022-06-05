//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Text;
//using System.Threading.Tasks;

//namespace DHCPServer
//{

//    public class IPPool
//    {
//        public string Name { get; set; }

//        private readonly uint Network;
//        public byte[] Mask;


//        private readonly uint StartAddr;
//        private readonly uint Length;


//        private const int accessRandom = 989309449;
//        private uint access;

//        private object poolLock = new object();

//        private IPPool(uint network, uint startAddr, uint length, byte[] mask)
//        {
//            Network = network;
//            StartAddr = startAddr;
//            Length = length;
//            Mask = mask;

//            leases = new Dictionary<byte[], IPState>(new ByteArrayCompare());
//            access = accessRandom % length;
//            Name = "Pool0";
//        }

//        public static IPPool? CreateIPPool(IPAddress start, IPAddress end, IPAddress mask)
//        {
//            uint startAddr = BitConverter.ToUInt32(start.GetAddressBytes().Reverse().ToArray());
//            uint endAddr = BitConverter.ToUInt32(end.GetAddressBytes().Reverse().ToArray());

//            byte[] maskBytes = mask.GetAddressBytes();
//            uint maskAddr = BitConverter.ToUInt32(maskBytes.Reverse().ToArray());

//            if ((startAddr & maskAddr) != (endAddr & maskAddr))
//                return null;

//            if (startAddr > endAddr)
//                return null;

//            return new IPPool((startAddr & maskAddr), startAddr, endAddr - startAddr + 1, maskBytes);
//        }

//        public void AddLeaseOffer(byte[] bytes, byte[] clientHW)
//        {
//            lock (poolLock)
//            {
//                leases.Add(clientHW, new IPState()
//                {
//                    Lease = LeaseState.Offer,
//                    IPAddress = bytes
//                });
//            }
//        }

//        public byte[]? GetNextAddress(byte[] clientHW)
//        {
//            lock (poolLock)
//            {
//                var addr = StartAddr + access;
//                access = (access + accessRandom) % Length;

//                if (leases.ContainsKey(clientHW))
//                {
//                    return leases[clientHW].IPAddress;
//                }

//                if (leases.Count < Length)
//                {
//                    for (uint i = 0; i < Length; i++)
//                    {
//                        addr = StartAddr + i;
//                        foreach (KeyValuePair<byte[], IPState> kv in leases)
//                        {
//                            if(BitConverter.ToUInt32(kv.Value.IPAddress) == addr)
//                            {

//                            }
//                        }
//                    }
//                }

//                //if (leases.Count < Length)
//                //{
//                //    for (uint i = 0; i < Length; i++)
//                //    {
//                //        if (!leases.ContainsKey(StartAddr + i))
//                //            return BitConverter.GetBytes(StartAddr + i);
//                //    }

//                //    return null;
//                //}
//                //else
//                //{
//                //    return null;
//                //}

//                return BitConverter.GetBytes(addr).Reverse().ToArray();
//            }
//    }
//    }
//}
