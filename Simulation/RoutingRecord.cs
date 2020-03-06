using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation
{
    public class RoutingRecord
    {
        private int sessionID;
        private int inFiberID;
        private int indexOfChannel;
        private int outFiberID;

        public int SessionID
        {
            get => sessionID;
        }

        public int InFiberID
        {
            get => inFiberID;
        }

        public int IndexOfChannel
        {
            get => indexOfChannel;
        }

        public int OutFiberID
        {
            get => outFiberID;
        }

        public RoutingRecord(int sessionID, int inFiberID, int indexOfChannel, int outFiberID)
        {
            this.sessionID = sessionID;
            this.inFiberID = inFiberID;
            this.indexOfChannel = indexOfChannel;
            this.outFiberID = outFiberID;
        }

        public RoutingRecord(byte[] bytes)
        {
            this.sessionID = BitConverter.ToInt32(bytes, 0);
            this.inFiberID = BitConverter.ToInt32(bytes, 4);
            this.indexOfChannel = BitConverter.ToInt32(bytes, 8);
            this.outFiberID = BitConverter.ToInt32(bytes, 12);
        }
        public byte[] toBytes()
        {
            List<byte> bytes = new List<byte>();;
            bytes.AddRange(BitConverter.GetBytes(sessionID));
            bytes.AddRange(BitConverter.GetBytes(inFiberID));
            bytes.AddRange(BitConverter.GetBytes(indexOfChannel));
            bytes.AddRange(BitConverter.GetBytes(outFiberID));
            return bytes.ToArray();
        }
    }
}
