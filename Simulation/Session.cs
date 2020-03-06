using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Simulation
{
    public class Session
    {
        public int sessionID;
        public IPAddress Source;
        public IPAddress Destinantion;
        private int indexOfChanel;
        private int numberOfChanels;
        private List<int> FiberIDs;//
        public double Band;//
        public bool isTerminated;

        private DateTime Timestamp;

        public int SessionID
        {
            get => sessionID;
        }

        public int IndexOfChannel{ get => indexOfChanel; set => indexOfChanel = value; }

        public int NumberOfChannels{ get => numberOfChanels; set => numberOfChanels = value; }

        public List<int> Fiber_IDs{ get => FiberIDs; set => FiberIDs = value; }

        public int LastChannel()
        {
            return indexOfChanel+numberOfChanels-1;
        }

        public Session(int SessionID, int IndexOfChanel, int NumberOfChanels, List<int> FiberIDs, double Band, System.Net.IPAddress Source, IPAddress Destination)
        {
            this.sessionID = SessionID;
            this.indexOfChanel = IndexOfChanel;
            this.numberOfChanels = NumberOfChanels;
            this.FiberIDs = FiberIDs;
            this.Band = Band;
            this.Source = Source;
            this.Destinantion = Destination;
            isTerminated = false;
            Timestamp = DateTime.Now;
        }
        public void resetTimer()
        {
            Timestamp = DateTime.Now;
        }
        public bool isActivee()
        {
            if ((DateTime.Now - Timestamp).Minutes < 1) return true;
            else throw new InactiveException("Inactive");
        }
        public void terminate()
        {
            isTerminated = true;
        }

    }

    public class InactiveException : Exception
    {
        public InactiveException(string message) : base(message) { }
    }

    public class Sessions
    {
        List<Session> sessions = new List<Session>();

        public Session findSession(int id)
        {
            foreach(Session s in sessions)
            {
                if (s.sessionID == id)
                    return s;
            }
            return null;
        }

    }

}
