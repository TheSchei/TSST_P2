using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Simulation
{
    public class Fibers
    {
        public List<Fiber> forwarding = new List<Fiber>();

        public void AddFiber(int FiberID, IPAddress interface1, IPAddress interface2, int port1, int port2, double length, string enable)
        {
            if (!exists(interface1, interface2))
                forwarding.Add(new Fiber(FiberID, interface1, interface2, port1, port2, length, enable));
        }
        public int forward(IPAddress interface1, IPAddress interface2)
        {
            //podajesz interfejsy 1 i 2, i zwraca ci odpowiedni port
            //jeśli interfejsy to 1 i 2, port 2
            //jeśli interfejsy to 2 i 1, port 1
            //jeśli nie ma połączenia albo jest disabled to np. 0 albo -1, albo wyjątek, jak kto woli
            foreach (Fiber f in forwarding)
            {
                if ((f.interface1.Equals(interface1)) && f.interface2.Equals(interface2) && f.enable) return f.port2;
                if ((f.interface1.Equals(interface2)) && f.interface2.Equals(interface1) && f.enable) return f.port1;
            }
            throw new Exception("Inactive or not existing connection, package dropped");
        }
        private bool exists(IPAddress interface1, IPAddress interface2)
        {
            foreach (Fiber f in forwarding)
            {
                if ((f.interface1.Equals(interface1)) && f.interface2.Equals(interface2)) return true;
                if ((f.interface1.Equals(interface2)) && f.interface2.Equals(interface1)) return true;
            }
            return false;
        }
        public bool deleteFiber(int i)
        {
            if (forwarding.Count < i) return false;
            forwarding.RemoveAt(i);
            return true;
        }
        public string[] FiberStrings()
        {
            string[] output = new string[forwarding.Count];
            for (int i = 0; i < forwarding.Count; i++)
                output[i] = forwarding[i].getString();
            return output;
        }
    }
    public class Fiber
    {
        public int fiberID;
        public IPAddress interface1;
        public IPAddress interface2;
        private double length;
        public int port1;
        public int port2;
       // public bool inUsed; // nwm czy sie przyda
        public bool enable;

        public Fiber(string filePath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            fiberID = Convert.ToInt32(doc.SelectSingleNode("/Fiber/id").InnerText);
            interface1 = IPAddress.Parse(doc.SelectSingleNode("/Fiber/interface1").InnerText);
            interface2 = IPAddress.Parse(doc.SelectSingleNode("/Fiber/interface2").InnerText);
            port1 = Convert.ToInt32(doc.SelectSingleNode("/Fiber/port1").InnerText);
            port2 = Convert.ToInt32(doc.SelectSingleNode("/Fiber/port2").InnerText);
            length = Convert.ToDouble(doc.SelectSingleNode("/Fiber/length").InnerText);
            //inUsed = Convert.ToBoolean(doc.SelectSingleNode("/Fiber/inUsed").InnerText);
            enable = Convert.ToBoolean(doc.SelectSingleNode("/Fiber/enable").InnerText);
        }

        public Fiber(int fiberID, IPAddress interface1, IPAddress interface2, int port1, int port2, double length, string enable)//możemy zrobić tak i w XMLu w enable będzie ON albo OFF
        {
            this.fiberID = fiberID;
            this.interface1 = interface1;
            this.interface2 = interface2;
            this.port1 = port1;
            this.port2 = port2;
            this.length = length;

            /*
            if (inUsed == "TRUE") this.inUsed = true;
            else if (inUsed == "FALSE") this.inUsed = false;
            else throw new FormatException("Wrong format of \"inUsed\" Fiber.");
            */
            if (enable == "ON") this.enable = true;
            else if (enable == "OFF") this.enable = false;
            else throw new FormatException("Wrong format of \"enable\" Fiber.");
        }
        public string getString()
        {
            return interface1.ToString() + " <-> " + interface2.ToString();
        }
        public int getID()
        {
            return this.fiberID;
        }

        public void reverseStatus()
        {
            enable = !enable;
        }

        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(fiberID); //chyba tak ?
        } 
    }
}
