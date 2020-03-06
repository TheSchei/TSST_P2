using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Simulation;
using System.Threading;
using System.Net;

namespace Driver
{
    class LRM
    {
        
        // czy ja wgl potrzebuje tej listy i tego wczytywania?
        List<Edge> Myedges = new List <Edge>();
        List<Edge> Alledges = new List<Edge>();

        public LRM(string structurePath)
        {
            //Load all edges to know network structure
            XmlDocument doc = new XmlDocument();
            
            doc.Load(structurePath);
            XmlNodeList Edges = doc.DocumentElement.SelectNodes("/Structure/Edges/Edge");
            foreach (XmlNode Edge in Edges)
            {
                Edge edge = new Edge(Convert.ToInt32(Edge.SelectSingleNode("ID").InnerText),
                    IPAddress.Parse(Edge.SelectSingleNode("IP1").InnerText),
                    IPAddress.Parse(Edge.SelectSingleNode("IP2").InnerText),
                    Convert.ToInt32(Edge.SelectSingleNode("Length").InnerText));
                Myedges.Add(edge);
                Alledges.Add(edge);
            }
        }

        //dostaje info od cc żeby zająć łącza
        public List<Edge> OfficialOccupy(List<Edge> edges, int firstSlot, int numOfSlots)
        {
            //przejście po całej dostarczonej liście
            foreach(Edge edge in edges)
            {
                //wyszukuje w mojej liście edgy edga o takim ID jak aktualny z pętli, sprawdzam, czy ma wolne sloty, jak
                //ma to rezerwuje
                if (Myedges.Find(x => x.ID.Equals(edge.ID)).CheckSlotsAvailability(firstSlot, numOfSlots))
                    Myedges.Find(x => x.ID.Equals(edge.ID)).OccupySlots(firstSlot, numOfSlots);
                
            }
            return Myedges;
        }

        //dostaje info od CC z danymi troche innymi jak wyzej, bo dostaje ID Fiber'ów
        //Dodaje zasoby i informuje o tym RC
        public List<Edge> FreeResources(List<int> FiberIDs, int firstSlot, int numOfSlots)
        {
            for (int i = 0; i < FiberIDs.Count; i++)
            {
                //wyszukuje w mojej liście Edge o Id z listy dostarczonych i zwalniam zasoby
                if(Myedges.Exists(item => item.ID.Equals(FiberIDs[i])))
                {
                    Myedges.Find(x => x.ID.Equals(FiberIDs[i])).ReleaseSlots(firstSlot, numOfSlots);
                }
            }
            //zwracam moją listę edgy
            return Myedges;
        }

        //odebrane info od Clouda, że enable jest off, wiec odsyłam liste aktualnych łączy do RC
        public List<Edge> linkDown(int id)
        {   
            //znajdź łącze, zwolnij mu zasoby i usuń z aktualnie dostępnych łączy
            //chyba trzeba tu zrobic jakies zabezpieczenie
            if(Myedges.Find(x => x.ID.Equals(id)).ReleaseSlots(0,199))
                Myedges.Remove(Myedges.Find(x => x.ID.Equals(id)));
            
            return Myedges;

            
        }

        public List<Edge> linkUp(int id)
        {

            Myedges.Add(Alledges.Find(x => x.ID == id));

            return Myedges;

        }

    }
}
