using System;
using PacketDotNet;

namespace PCapture
{
    public class PacketInfo
    {
        public string? srcIP { get; set; }
        public string? dstIP { get; set; }
        public int? srcPort { get; set; }
        public int? dstPort { get; set; }
        public string? Protocol { get; set; }

        public PacketInfo() { }

        public PacketInfo(string srcIP, string dstIP, int srcPort, int dstPort, string protocol)
        {
            this.srcIP = srcIP;
            this.dstIP = dstIP;
            this.srcPort = srcPort;
            this.dstPort = dstPort;
            this.Protocol = protocol;
        }

        // packet 데이터 받아오는 함수..
        public void GetPacketInfo(Packet packet, IPPacket ipPacket)
        {
            if (ipPacket != null)
            {
                srcIP = ipPacket.SourceAddress.ToString();
                dstIP = ipPacket.DestinationAddress.ToString();

                if (packet is TcpPacket tcpPacket)
                {
                    srcPort = tcpPacket.SourcePort;
                    dstPort = tcpPacket.DestinationPort;
                    Protocol = "TCP";
                }
                else if (packet is UdpPacket udpPacket)
                {
                    srcPort = udpPacket.SourcePort;
                    dstPort = udpPacket.DestinationPort;
                    Protocol = "UDP";
                }
            }
        }
    }
}