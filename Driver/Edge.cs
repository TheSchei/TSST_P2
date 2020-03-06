using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;


namespace Driver
{
    public static class Constants
    {
        public const int MAX_NUM_OF_SLOTS = 200;
        public const double SLOT_WIDTH = 12.5;
        public const int EFFICIENCY = 2;
        public const double LIFTING = 15;
        public const double DISTANCE = 2.5;
    }

    class Edge
    {
        private int id;
        private IPAddress node1;
        private IPAddress node2;
        private int length;
        private List<int> occupiedSlots = new List<int>();

        public int ID
        {
            get => id;
        }

        public IPAddress Node1
        {
            get => node1;
        }

        public IPAddress Node2
        {
            get => node2;
        }

        public List<int> OccupiedSlots
        {
            get => occupiedSlots;
        }

        public int Length
        {
            get => length;
        }

        public Edge(int id, IPAddress node1, IPAddress node2, int length)
        {
            this.id = id;
            this.node1 = node1;
            this.node2 = node2;
            this.length = length;
        }

        public bool CheckSlotsAvailability(int firstSlot, int numOfSlots)
        {
            //Zabezpieczenie
            if (firstSlot < 0 || firstSlot >= Constants.MAX_NUM_OF_SLOTS || numOfSlots < 0 || firstSlot + numOfSlots >= Constants.MAX_NUM_OF_SLOTS) return false;
            for (int i = firstSlot; i < firstSlot + numOfSlots; i++)
            {
                if (occupiedSlots.Contains(i)) return false;
            }
            return true;
        }

        public bool OccupySlots(int firstSlot, int numOfSlots)
        {
            //Zabezpieczenie
            if (firstSlot < 0 || firstSlot >= Constants.MAX_NUM_OF_SLOTS || numOfSlots < 0 || firstSlot + numOfSlots >= Constants.MAX_NUM_OF_SLOTS) return false;

            //Sprawdzmy jeszcze raz dla bezpieczenstwa
            for (int i=firstSlot; i<firstSlot+numOfSlots; i++)
            {
                if (occupiedSlots.Contains(i)) return false;
            }

            for(int i=firstSlot; i<firstSlot+numOfSlots; i++)
            {
                occupiedSlots.Add(i);
            }
            return true;
        }

        public bool ReleaseSlots(int firstSlot, int numOfSlots)
        {
            //Zabezpieczenie przed złem
            if (firstSlot < 0 || firstSlot >= Constants.MAX_NUM_OF_SLOTS || numOfSlots < 0 || firstSlot + numOfSlots >= Constants.MAX_NUM_OF_SLOTS) return false;

            for (int i=firstSlot; i<firstSlot+numOfSlots; i++)
            {
                if(occupiedSlots.Contains(i))
                    occupiedSlots.Remove(i);
            }
            return true;
        }
    }
}
