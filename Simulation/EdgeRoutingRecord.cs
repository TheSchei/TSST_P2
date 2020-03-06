using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation
{
    public class EdgeRoutingRecord
    {
        private int sessionID;
        private int indexOfChannel;
        private int outFiberID;

        public int SessionID
        {
            get => sessionID;
        }

        public int IndexOfChannel
        {
            get => indexOfChannel;
        }

        public int OutFiberID
        {
            get => outFiberID;
        }

        public EdgeRoutingRecord(int sessionID, int indexOfChannel, int outFiberID)
        {
            this.sessionID = sessionID;
            this.indexOfChannel = indexOfChannel;
            this.outFiberID = outFiberID;
        }

        public EdgeRoutingRecord(byte[] bytes)
        {
            this.sessionID = BitConverter.ToInt32(bytes, 0);
            this.indexOfChannel = BitConverter.ToInt32(bytes, 4);
            this.outFiberID = BitConverter.ToInt32(bytes, 8);
        }
        public byte[] toBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(sessionID));
            bytes.AddRange(BitConverter.GetBytes(indexOfChannel));
            bytes.AddRange(BitConverter.GetBytes(outFiberID));
            return bytes.ToArray();
        }
    }
}
