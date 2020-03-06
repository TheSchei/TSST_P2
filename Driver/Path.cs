using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Driver
{
    class Path
    {
        private List<Edge> edges = new List<Edge>();
        private int startID;
        private int numOfSlots;
        public int length;

        public List<Edge> Edges
        {
            get => edges;
        }

        public int StartID
        {
            get => startID;
            set => startID = value;
        }

        public int NumOfSlots
        {
            get => numOfSlots;
            set => numOfSlots = value;
        }

        public bool AddEdge(Edge edge)
        {
            if (edges.Contains(edge)) return false;
            
            edges.Add(edge);
            return true;
        }

        public bool RemoveEdge(Edge edge)
        {
            if (!edges.Contains(edge)) return false;

            edges.Remove(edge);
            return true;
        }

        public int getLength()
        {
            foreach(Edge e in edges)
            {
                length += e.Length;
            }
            return length;
        }
    }
}
