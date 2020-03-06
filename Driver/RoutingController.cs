using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Simulation;

namespace Driver
{
    class RoutingController
    {
        private List<Node> nodes = new List<Node>();
        private List<Edge> edges = new List<Edge>();
        public Modulation mod;

        public List<Node> Nodes
        {
            get => nodes;
        }

        public List<Edge> Edges
        {
            get => edges;
        }

        public RoutingController(List<Node> nodes, List<Edge> edges)
        {
            foreach(Node node in nodes)
            {
                this.nodes.Add(node);
            }
            foreach(Edge edge in edges)
            {
                this.edges.Add(edge);
            }
        }

        public void UpdateEdges(List<Edge> edges)
        {
            this.edges = edges;
        }

        public void UpdateNodes(List<Node> nodes)
        {
            this.nodes = nodes;
        }

        public Path Dijkstra(IPAddress src, IPAddress dst, double band)
        {
            Path path = new Path();

            //Zwraca gotowa sciezke wyliczona
            List<Edge> DijkstraPath = ShortestDijkstraPath(src, dst);
            int pathLength = 0;
            foreach (Edge edge in DijkstraPath) pathLength += edge.Length;

            //Trzeba wybrac modulacje na podstawie dlugosci
            mod = Modulation(pathLength); // Modulacja
            double TotBand = totBand(band, mod); //Pasmo zajęte po uwzględnieniu efektywności i modulacji
            double Lifts = Math.Ceiling(TotBand / Constants.LIFTING); //Liczba podnośnych
            if (Lifts > Constants.MAX_NUM_OF_SLOTS) throw (new Exception("Nie znaleziono sciezki"));
            double LiftWidth = TotBand / Lifts; //Szerokość jednej podnośnej

            //Obliczyc ile potrzebnych slotów -> requiredSlots
            int requiredSlots = 0;
            if (Constants.LIFTING - LiftWidth > Constants.DISTANCE) requiredSlots = (int)Lifts;
            else requiredSlots = (int)Lifts * 2;

            int firstSlot = -1;
            bool notFound = false;

            for(int i=0; i<Constants.MAX_NUM_OF_SLOTS-requiredSlots; i++)
            {
                foreach(Edge edge in DijkstraPath)
                {
                    if(!edge.CheckSlotsAvailability(i, requiredSlots))
                    {
                        notFound = true;
                        break;
                    }
                }
                if(!notFound)
                {
                    firstSlot = i;
                    break;
                }
                else
                {
                    notFound = false;
                }
            }

            if(firstSlot != -1)
            {
                foreach(Edge edge in DijkstraPath)
                {
                    path.AddEdge(edge);
                }
                path.StartID = firstSlot;
                path.NumOfSlots = requiredSlots;
            }
            else
            {
                throw (new Exception("Nie znaleziono sciezki"));
            }

            return path;
        }

        public Path RecalcDijkstra(IPAddress src, IPAddress dst, List<int> usedEdges, int firstUsedSlot, int usedSlots, double band)
        {
            //Path to return
            Path path = new Path();

            //Zwraca gotowa sciezke wyliczona
            List<Edge> DijkstraPath = ShortestDijkstraPath(src, dst);
            int pathLength = 0;
            foreach (Edge edge in DijkstraPath) pathLength += edge.Length;

            //Trzeba wybrac modulacje na podstawie dlugosci
            mod = Modulation(pathLength); // Modulacja
            double TotBand = totBand(band, mod); //Pasmo zajęte po uwzględnieniu efektywności i modulacji
            double Lifts = Math.Ceiling(TotBand / Constants.LIFTING); //Liczba podnośnych
            if (Lifts > Constants.MAX_NUM_OF_SLOTS) throw (new Exception("Nie znaleziono sciezki"));
            double LiftWidth = TotBand / Lifts; //Szerokość jednej podnośnej

            //Obliczyc ile potrzebnych slotów -> requiredSlots
            int requiredSlots = 0;
            if (Constants.LIFTING - LiftWidth > Constants.DISTANCE) requiredSlots = (int)Lifts;
            else requiredSlots = (int)Lifts * 2;

            int firstSlot = -1;
            bool notFound = false;

            foreach(int usedEdge in usedEdges)
            {
                Edge found = DijkstraPath.Find(item => item.ID == usedEdge);
                if (found != null)found.ReleaseSlots(firstUsedSlot, usedSlots);
            }

            for (int i = 0; i < Constants.MAX_NUM_OF_SLOTS - requiredSlots; i++)
            {
                foreach (Edge edge in DijkstraPath)
                {
                    if (!edge.CheckSlotsAvailability(i, requiredSlots))
                    {
                        notFound = true;
                        break;
                    }
                }
                if (!notFound)
                {
                    firstSlot = i;
                    break;
                }
                else
                {
                    notFound = false;
                }
            }

            if (firstSlot != -1)
            {
                foreach (Edge edge in DijkstraPath)
                {
                    path.AddEdge(edge);
                }
                path.StartID = firstSlot;
                path.NumOfSlots = requiredSlots;
            }
            else
            {
                throw (new Exception("Nie znaleziono sciezki"));
            }

            return path;
        }

        public List<Edge> ShortestDijkstraPath(IPAddress src, IPAddress dst)
        {
            List<Edge> dijkstraEdges = new List<Edge>();

            List<DijkstraRecord> dijkstraNodes = new List<DijkstraRecord>();
            foreach(Node node in nodes)
            {
                dijkstraNodes.Add(new DijkstraRecord(node.IP));
            }

            dijkstraNodes.Find(item => item.ip.Equals(src)).cost = 0;
            DijkstraRecord currentNode = dijkstraNodes.Find(item => item.ip.Equals(src));

            while(!dijkstraNodes.Find(item => item.ip.Equals(dst)).visited)
            {
                foreach(Edge edge in edges)
                {
                    DijkstraRecord remoteNode;
                    if (edge.Node1.Equals(currentNode.ip) && !edge.Node2.Equals(currentNode.from))
                    {
                        remoteNode = dijkstraNodes.Find(item => item.ip.Equals(edge.Node2));
                    }
                    else if (edge.Node2.Equals(currentNode.ip) && !edge.Node1.Equals(currentNode.from))
                    {
                        remoteNode = dijkstraNodes.Find(item => item.ip.Equals(edge.Node1));
                    }
                    else continue;

                    if(remoteNode.cost > currentNode.cost + edge.Length)
                    {
                        remoteNode.cost = currentNode.cost + edge.Length;
                        remoteNode.from = currentNode.ip;
                    }
                }
                currentNode.visited = true;
                if (currentNode.ip.Equals(dst)) break;
                int minCost = int.MaxValue; 
                foreach(var node in dijkstraNodes)
                    if (!node.visited && node.cost < minCost) minCost = node.cost;

                currentNode = dijkstraNodes.Find(item => (item.cost.Equals(minCost) && item.visited.Equals(false)));
            }

            while(!currentNode.ip.Equals(src))
            {
                Edge edge = edges.Find(item => ((item.Node1.Equals(currentNode.ip) && item.Node2.Equals(currentNode.from) && !dijkstraEdges.Contains(item)) || (item.Node2.Equals(currentNode.ip) && item.Node1.Equals(currentNode.from) && !dijkstraEdges.Contains(item))));
                currentNode = dijkstraNodes.Find(item => item.ip.Equals(currentNode.from));
                dijkstraEdges.Add(edge);
            }

            return dijkstraEdges;
        }

        public Modulation Modulation(int length)
        {
            if (length <= 100) return global::Modulation.QAM64;
            else if (length <= 200) return global::Modulation.QAM32;
            else if (length <= 300) return global::Modulation.QAM16;
            else if (length <= 400) return global::Modulation.QAM8;
            else if (length <= 500) return global::Modulation.QAM4;
            else return global::Modulation.BPSK;
        }

        public double totBand(double band, Modulation mod)
        {
            double ret = Constants.EFFICIENCY * band / 1024;
            int divider = 1;

            switch(mod)
            {
                case global::Modulation.BPSK:
                    divider = 1;
                    break;
                case global::Modulation.QAM4:
                    divider = 2;
                    break;
                case global::Modulation.QAM8:
                    divider = 3;
                    break;
                case global::Modulation.QAM16:
                    divider = 4;
                    break;
                case global::Modulation.QAM32:
                    divider = 5;
                    break;
                case global::Modulation.QAM64:
                    divider = 6;
                    break;
                default:
                    divider = 1;
                    break;
            }

            ret /= divider;

            return ret;
        }
    }

    class DijkstraRecord
    {
        public IPAddress ip;
        public bool visited;
        public int cost;
        public IPAddress from;

        public DijkstraRecord(IPAddress ip)
        {
            this.ip = ip;
            this.visited = false;
            this.cost = int.MaxValue;
            this.from = IPAddress.Parse("0.0.0.0");
        }
    }
}
