using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Simulation
{
    public class CloudField
    {
        private readonly int fiberID;
        private IPAddress node1;
        private IPAddress node2;
        private readonly int port1;
        private readonly int port2;
        private bool isActive;

        public CloudField(int fiberID, IPAddress node1, IPAddress node2, int port1, int port2, bool isActive)
        {
            this.fiberID = fiberID;
            this.node1 = node1;
            this.port1 = port1;
            this.node2 = node2;
            this.port2 = port2;
            this.isActive = isActive;
        }

        public int Port2 { get => port2; }
        public int Port1 { get => port1; }
        public IPAddress Node1 { get => node1; }
        public IPAddress Node2 { get => node2; }
        public bool IsActive { get => isActive;  }
        public int FiberID { get => fiberID; }
        public string GetString()
        {
            return FiberID.ToString() + ". " + Node1.ToString() + " <-> " + Node2.ToString();
        }
        public bool reverseStatus()
        {
            return isActive = !isActive;
        }
    }

    public class CloudFields
    {
        private List<CloudField> Fields = new List<CloudField>();

        public void addFiber(int fiberID, IPAddress node1, IPAddress node2, int port1, int port2, bool isActive)
        {
            Fields.Add(new CloudField(fiberID, node1, node2, port1, port2, isActive));
        }
        public void addFiber(string fiberID, string node1, string node2, string port1, string port2, string isActive)
        {
            Fields.Add(new CloudField(Convert.ToInt32(fiberID), IPAddress.Parse(node1), IPAddress.Parse(node2), Convert.ToInt32(port1), Convert.ToInt32(port2), ConvertBool(isActive)));
        }
        public int getDest(IPAddress lastNodeID, int fiberID)
        {
            CloudField temp = Fields.Find(item => item.FiberID == fiberID);
            if (temp != null)
            {
                if (!temp.IsActive) throw new Exception("Packet lost");
                else if (lastNodeID.Equals(temp.Node1)) return temp.Port2;
                else if (lastNodeID.Equals(temp.Node2)) return temp.Port1;
                else throw new Exception("Unknown node");
            }
            else throw new Exception("Fiber not found");
        }

        public IPAddress getNextAdres(IPAddress lastNodeID, int fiberID)
        {
            CloudField temp = Fields.Find(item => item.FiberID == fiberID);
            if (temp != null)
            {
                if (!temp.IsActive) throw new Exception("Packet lost");
                else if (lastNodeID == temp.Node1) return temp.Node2;
                else if (lastNodeID == temp.Node2) return temp.Node1;
                else throw new Exception("Unknown node");
            }
            else throw new Exception("Fiber not found");
        }

        public string[] GetStrings()
        {
            List<string> output = new List<string>();
            foreach (CloudField field in Fields)
                output.Add(field.GetString());
            return output.ToArray();
        }
        public bool reverseStatus(int i)
        {
            return Fields[i].reverseStatus();
        }
        public bool reverseStatus(int i, out int FiberID)
        {
            FiberID = Fields[i].FiberID;
            return Fields[i].reverseStatus();
        }
        public int Count() { return Fields.Count; }
        public bool isActive(int i) { return Fields[i].IsActive; }
        public int getID(int i) { return Fields[i].FiberID; }
        private bool ConvertBool(string isActive)
        {
            if (isActive == "ON") return true;//jeśli ON to true, jeśli cokolwiek to false :p
            else return false;
        }

        public CloudField FindCloud(int id)
        {
            foreach(CloudField cf in Fields)
            {
                if(cf.FiberID == id)
                {
                    return cf;
                }
            }
            return null;
        }
    }
}
