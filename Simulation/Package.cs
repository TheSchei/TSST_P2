using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Simulation
{
    public class Package
    {
        public int SessionID;
        private readonly IPAddress source;
        public IPAddress LastNode;
        public int IndexOfChanel; // to nie powinno byc w Session ?
        public int FiberID;
        private int length;
        private string payload;

        public string Payload { get => payload; set => payload = value; }
        public IPAddress Source { get => source; }

        public Package(byte[] data)//konstruktor do odczytywania pakietu z bajtów, czyli po odebraniu z socketu
        {
            source = new IPAddress(new byte[] { data[0], data[1], data[2], data[3] });//konwertuje kolejne 4 bajty na source
            LastNode = new IPAddress(new byte[] { data[4], data[5], data[6], data[7] });//konwertuje kolejne 4 bajty na destination
            IndexOfChanel = BitConverter.ToInt32(data, 8);// długość
            FiberID = BitConverter.ToInt32(data, 12);// długość
            length = BitConverter.ToInt32(data, 16);// długość
            SessionID = BitConverter.ToInt32(data, 20);// długość
            List<byte> myPayload = new List<byte>();
            myPayload.AddRange(data.ToList().GetRange(24, length - 24));
            payload = Encoding.UTF8.GetString(myPayload.ToArray());//konwertujemy na stringa
        }
        public Package(IPAddress source, IPAddress LastNode, int IndexOfChanel, int FiberID, string payload)// konstruktor do tworzenia pakietu (np. w hoście lub managerze?)
        {
            this.source = source;
            this.LastNode = LastNode;
            this.IndexOfChanel = IndexOfChanel;
            this.FiberID = FiberID;
            this.payload = payload;
            length = 4 + 4 + 4 + 4 + 4 + 4 + Encoding.UTF8.GetBytes(payload).Length;//po prostu długość
        }
        public Package(IPAddress source, int FiberID, int SessionID, string payload)// konstruktor do tworzenia pakietu (np. w hoście lub managerze?)
        {
            this.source = source;
            this.LastNode = source;
            this.IndexOfChanel = -1;
            this.SessionID = SessionID;
            this.FiberID = FiberID;
            this.payload = payload;
            length = 4 + 4 + 4 + 4 + 4 + 4 + Encoding.UTF8.GetBytes(payload).Length;//po prostu długość
        }
        public byte[] toBytes()
        {
            List<byte> output = new List<byte>();
            output.AddRange(Source.GetAddressBytes());
            output.AddRange(LastNode.GetAddressBytes());
            output.AddRange(BitConverter.GetBytes(IndexOfChanel));
            output.AddRange(BitConverter.GetBytes(FiberID));
            output.AddRange(BitConverter.GetBytes(length));
            output.AddRange(BitConverter.GetBytes(SessionID));
            output.AddRange(Encoding.UTF8.GetBytes(payload));
            return output.ToArray();
        }
    }
}
