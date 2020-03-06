using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

public enum Modulation { BPSK = 1, QAM4, QAM8, QAM16, QAM32, QAM64 };

namespace Simulation
{
    public class Channel
    {
        private int startID;
        private int numOfSlots;
        private Modulation modulation;
        private readonly int spectralEfficency = 2;
        private float length;
        private int capacity;
        public bool enable; // nwm czy sie przyda

        public int StartID
        {
            get => startID;
        }

        public int NumOfSlots
        {
            get => numOfSlots;
        }

        public Modulation Modulation
        {
            get => modulation;
        }

        public int Capacity
        {
            get => capacity;
        }

        public float Length
        {
            get => length;            
        }

        public Channel(string filePath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            startID = Convert.ToInt32(doc.SelectSingleNode("/Channel/starID").InnerText);
            numOfSlots = Convert.ToInt32(doc.SelectSingleNode("/Channel/numOfSlots").InnerText);
            Modulation modulation = (Modulation)Enum.Parse(typeof(Modulation), doc.SelectSingleNode("/Channel/modulation").InnerText);
            capacity = Convert.ToInt32(doc.SelectSingleNode("/Channel/capacity").InnerText);
            length = Convert.ToInt32(doc.SelectSingleNode("/Channel/length").InnerText);
        }

        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(startID); //chyba tak ?
        }

        public bool CheckChannel(int id)
        {
            if (startID == id)
                return true;
            else
                return false;
        }
    }
}
