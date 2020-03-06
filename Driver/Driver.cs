using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using Simulation;

namespace Driver
{
    class Driver
    {
        private IPAddress IP;
        public IPAddress logicIP;
        private int recPort;
        private Socket Receiver;
        private Socket Sender;

        public Queue<string> messageQueue = new Queue<string>();
        //private Dictionary<IPAddress, int> Hosts = new Dictionary<IPAddress, int>();

        private int NextSessionID = 1;
        private List<Session> Sessions = new List<Session>();

        //Topology
        private List<Node> nodes = new List<Node>();
        private List<Edge> edges = new List<Edge>();

        private System.Timers.Timer sessionTimer;
        private ManualResetEvent AllDone = new ManualResetEvent(false);
        private Thread t;

        public RoutingController RC;
        public LRM lrm;

        public Driver(string filePath, string structFilename)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            IP = IPAddress.Parse(doc.SelectSingleNode("/Driver/IP").InnerText);
            logicIP = IPAddress.Parse(doc.SelectSingleNode("/Driver/LogicIP").InnerText);
            recPort = Convert.ToInt32(doc.SelectSingleNode("/Driver/RecPort").InnerText);

            //WCZYTYWANIE HOSTOW ZE STRUKTURY
            //WCZYTYWANIE NODES I EDGES Z XML

            doc.Load(structFilename);
            XmlNodeList Nodes = doc.DocumentElement.SelectNodes("/Structure/Nodes/Node");
            foreach (XmlNode Node in Nodes)
                if ((Node.SelectSingleNode("Type").InnerText == "HOST"))
                    nodes.Add(new Node(Int32.Parse(Node.SelectSingleNode("ID").InnerText), IPAddress.Parse(Node.SelectSingleNode("IP").InnerText), Int32.Parse(Node.SelectSingleNode("Port").InnerText), Type.HOST));
                else if(Node.SelectSingleNode("Type").InnerText == "ROUTER")
                    nodes.Add(new Node(Int32.Parse(Node.SelectSingleNode("ID").InnerText), IPAddress.Parse(Node.SelectSingleNode("IP").InnerText), Int32.Parse(Node.SelectSingleNode("Port").InnerText), Type.ROUTER));
            Nodes = doc.DocumentElement.SelectNodes("/Structure/Edges/Edge");
            foreach (XmlNode Node in Nodes)
                edges.Add(new Edge(Int32.Parse(Node.SelectSingleNode("ID").InnerText), IPAddress.Parse(Node.SelectSingleNode("IP1").InnerText), IPAddress.Parse(Node.SelectSingleNode("IP2").InnerText), Int32.Parse(Node.SelectSingleNode("Length").InnerText)));


            sessionTimer = new System.Timers.Timer(1000);//1 sekunda
            sessionTimer.Elapsed += OnTimedEvent;
            sessionTimer.AutoReset = true;
            sessionTimer.Enabled = true;

            RC = new RoutingController(nodes, edges);
            lrm = new LRM(structFilename);
            t = new Thread(Listen);
            t.Start();
        }

        private void Listen()
        {
            Receiver = new Socket((new IPEndPoint(IP, recPort)).AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Receiver.Bind(new IPEndPoint(IP, recPort));
            Receiver.Listen(10);
            while (true)
            {
                AllDone.Reset();
                Receiver.BeginAccept(new AsyncCallback(readMessage), Receiver);
                AllDone.WaitOne();
            }
        }

        public void Dispose()
        {
            t.Abort();
        }

        private void readMessage(IAsyncResult result)
        {
            List<byte> temp = new List<byte>();
            byte[] buffer = new byte[128];
            Socket reader = ((Socket)result.AsyncState).EndAccept(result);
            AllDone.Set();
            try
            {
                while (reader.Available == 0) ;
                while (reader.Available > 0)
                {
                    reader.Receive(buffer, buffer.Length, SocketFlags.Partial);
                    temp.AddRange(buffer);
                }
                InterpreteMessage(reader, temp.ToArray());
            }
            catch (Exception e)
            {
                messageQueue.Enqueue(Logger.Log(e.Message, LogType.ERROR));
            }
            finally { if (reader != null) reader.Close(); }
        }

        private bool sendMesage(string message, IPAddress destination)
        {
            try
            {
                //Sender = new Socket(new IPEndPoint(destination, Hosts[destination]).AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                //Sender.Connect(new IPEndPoint(destination, Hosts[destination]));
                int port = nodes.Find(item => item.IP.Equals(destination)).Port;
                Sender = new Socket(new IPEndPoint(IP, port).AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                Sender.Connect(new IPEndPoint(IP, port));
                Sender.Send(new Package(logicIP, 0, 0, message).toBytes());
                Sender.Close();
                return true;
            }
            catch { }
            finally { if (Sender != null) Sender.Close(); }
            return false;
        }

        private void InterpreteMessage(Socket socket, byte[] package)
        {
            switch (Protocol.getControlParam(package))
            {
                case ControlParam.Path:
                    socket.Send(NewSession(package));
                    break;
                case ControlParam.PathUpdate:
                    socket.Send(UpdateSession(package));
                    break;
                case ControlParam.PathTerminate:
                    socket.Send(TerminateSession(package));
                    break;
                case ControlParam.PathReSet:
                    socket.Send(ReSetSession(package));
                    break;
                case ControlParam.LinkUp:
                    linkUp(checkLinkId(package));
                    break;
                case ControlParam.LinkDown:
                    linkDown(checkLinkId(package));
                    break;
                default:
                    break;
            }
        }

        private byte[] NewSession(byte[] package)
        {
           
            Interpreter.Path(package, out IPAddress src, out IPAddress dst, out double band);
            string edges = "";
            string nodes = "";
            List<int> edgesID = new List<int>();
            foreach(Edge e in RC.ShortestDijkstraPath(src, dst))
            {
                int egdeID = e.ID;
                edgesID.Add(egdeID);
                edges += e.ID.ToString() + " ";
                nodes += e.Node1.ToString() + " ";
            }

           
            messageQueue.Enqueue(Logger.Log("RECEIVED CALL REQUEST FROM CPCC " + src + " TO " + dst + " [" + band + "[Mbs]]", LogType.INFO));
            messageQueue.Enqueue(Logger.Log("NCC SENDS CONNECTION REQUEST TO CC [" + src + ", " + dst + ", " + band + "[Mbs]]", LogType.INFO));
            try
            {
                messageQueue.Enqueue(Logger.Log("CC RECEIVED CONNECTION REQUEST", LogType.INFO));
                messageQueue.Enqueue(Logger.Log("CC SENDS ROUTE TABLE QUERY TO RC: PATH FROM " + src + " TO " + dst, LogType.INFO));
                Path path = RC.Dijkstra(src, dst, band);
                messageQueue.Enqueue(Logger.Log("RC RECEIVED ROUTE TABLE QUERY: CALCULATING PATH", LogType.INFO));
                messageQueue.Enqueue(Logger.Log("EDGES: [ " + edges + "]", LogType.INFO));
                messageQueue.Enqueue(Logger.Log("NODES: [ " + nodes.Remove(nodes.Length - 10) + "]", LogType.INFO));
                if (path.NumOfSlots == 1)
                    messageQueue.Enqueue(Logger.Log("USED CHANNEL: " + path.StartID.ToString(), LogType.INFO));
                else
                    messageQueue.Enqueue(Logger.Log("USED CHANNELS FROM " + path.StartID.ToString() + " TO " + (path.StartID + path.NumOfSlots - 1).ToString(), LogType.INFO));

                messageQueue.Enqueue(Logger.Log("RC SENDS MESSAGE TO CC WITH CALCULATED PATH", LogType.INFO));
                //Dijkstra throws an error if path is not found
                messageQueue.Enqueue(Logger.Log("CC RECEIVED RESPONSE FROM RC", LogType.INFO));

                //KomunikacjaZLRM
                List<Edge> UpdatedEdges = new List<Edge>();

                messageQueue.Enqueue(Logger.Log("CC SENDS LINK CONNECTION REQUEST TO LRM: OCCUPY RESOURCES", LogType.INFO));

                messageQueue.Enqueue(Logger.Log("LRM OCCUPIES RESOURCES", LogType.INFO));
                UpdatedEdges = lrm.OfficialOccupy(path.Edges, path.StartID, path.NumOfSlots);
                messageQueue.Enqueue(Logger.Log("LRM SENDS CONFIRMATION OF RESOURCE ALLOCATION TO CC", LogType.INFO));
                if (UpdatedEdges.Count > 0) RC.UpdateEdges(UpdatedEdges);
                messageQueue.Enqueue(Logger.Log("LRM SENDS LOCAL TOPOLOGY TO RC", LogType.INFO));
                messageQueue.Enqueue(Logger.Log("RC RECEIVED LOCAL TOPOLOGY FROM LRM, UPDATING TOPOLOGY", LogType.INFO));
                messageQueue.Enqueue(Logger.Log("CC RECEIVED RESPONSE FROM LRM: PATH IS RESERVED", LogType.INFO));
                messageQueue.Enqueue(Logger.Log("CC SENDS CONNECTION REQUEST RESPONSE TO NCC [" + src + ", " + dst + "]", LogType.INFO));
                messageQueue.Enqueue(Logger.Log("NCC RECEIVED RESPONSE FROM CC", LogType.INFO));
                messageQueue.Enqueue(Logger.Log("NCC SENDS CALL ACCEPT REQUEST TO CPCC [" + src + ", " + dst + ", " + band + "[Mbs]]", LogType.INFO));
                if (AskOtherSide(src, dst) == true)
                {
                    messageQueue.Enqueue(Logger.Log("NCC RECEIVED CONNECTION CONFIRMED", LogType.INFO));
                    messageQueue.Enqueue(Logger.Log("NCC SENDS CALL REQUEST RESPONSE TO CPCC [" + dst + "]", LogType.INFO));
                    int session = NextSessionID++;//to jest raczej jakiś synchronizacyjny shit, nie pytajcie
                    List<int> ids = new List<int>();
                    for (int i = 0; i < path.Edges.Count; i++)
                    {
                        ids.Add(path.Edges[i].ID);
                    }
                    Session temp = new Session(session, path.StartID, path.NumOfSlots, ids, band, src, dst);
                    Sessions.Add(temp);

                    messageQueue.Enqueue(Logger.Log("SENDING ROUTING RECORDS TO ROUTERS", LogType.INFO));
                    setRoutingTables(temp);
                    //WYSYLANIE INFO DO ROUTEROW
                    package = Interpreter.PathOK(session);
                    messageQueue.Enqueue(Logger.Log("SESSION " + temp.sessionID + " IS STARTED", LogType.INFO));
                }
                else
                {
                    lrm.FreeResources(edgesID, path.StartID, path.NumOfSlots);
                    messageQueue.Enqueue(Logger.Log("NCC SENDS CONNECTION REQUEST RELEASE TO CC [" + src + ", " + dst + "]", LogType.INFO));
                    messageQueue.Enqueue(Logger.Log("CC SENDING MESSAGE TO LRM : LINK CONNECTION DEALLOCATION", LogType.INFO));
                    messageQueue.Enqueue(Logger.Log("LRM RECEIVED MESSAGE: LINK CONNECTION DEALLOCATION", LogType.INFO));
                    messageQueue.Enqueue(Logger.Log("LRM SENDS LOCAL TOPOLOGY TO RC", LogType.INFO));
                    messageQueue.Enqueue(Logger.Log("RC RECEIVED LOCAL TOPOLOGY FROM LRM, UPDATING TOPOLOGY", LogType.INFO));
                    messageQueue.Enqueue(Logger.Log("LRM SENDS RESPONSE TO CC", LogType.INFO));
                    messageQueue.Enqueue(Logger.Log("NCC SENDS CALL TEARDOWN TO CPCC", LogType.INFO));
                }
                
            }
            catch (Exception e)
            {
                package = Protocol.CreateResponse(ControlResponse.ERROR);
            }
            return package;
        }

        private byte[] UpdateSession(byte[] package)
        {

            messageQueue.Enqueue(Logger.Log("UPDATE SESSION MESSAGE RECEIVED", LogType.INFO));
            Interpreter.PathUpdate(package, out int sessionID);
            try
            {
                messageQueue.Enqueue(Logger.Log("REFRESHING SESSION TIMEOUT", LogType.INFO));
                Sessions.Find(item => item.SessionID == sessionID).isActivee();//Throws exception if not found(chyba), or inactive
                Sessions.Find(item => item.SessionID == sessionID).resetTimer();
                package = Protocol.CreateResponse(ControlResponse.OK);

                messageQueue.Enqueue(Logger.Log("SESSION UPDATED", LogType.INFO));
            }
            catch
            {
                messageQueue.Enqueue(Logger.Log("COULD NOT REFRESH THE SESSION", LogType.INFO));
                package = Protocol.CreateResponse(ControlResponse.ERROR);
            }
            return package;
        }

        private byte[] TerminateSession(byte[] package)
        {

            messageQueue.Enqueue(Logger.Log("TERMINATE SESSION MESSAGE RECEIVED", LogType.INFO));
            Interpreter.PathTerminate(package, out int sessionID);
            try
            {
                Sessions.Find(item => item.SessionID == sessionID).isActivee();//Throws exception if not found(chyba), or inactive
                Session temp = Sessions.Find(item => item.SessionID == sessionID);
                Sessions.Remove(temp);

                messageQueue.Enqueue(Logger.Log("REMOVING ROUTING RECORDS FROM ROUTERS", LogType.INFO));
                deleteRoutingTables(temp);


                messageQueue.Enqueue(Logger.Log("CC SENDS MESSAGE TO LRM : LINK CONNECTION DEALLOCATION", LogType.INFO));
                List<Edge> UpdatedEdges = lrm.FreeResources(temp.Fiber_IDs, temp.IndexOfChannel, temp.NumberOfChannels);

                messageQueue.Enqueue(Logger.Log("LRM RECEIVED MESSAGE: LINK CONNECTION DEALLOCATION", LogType.INFO));
                messageQueue.Enqueue(Logger.Log("LRM SENDS LOCAL TOPOLOGY TO RC", LogType.INFO));
                RC.UpdateEdges(UpdatedEdges);
                messageQueue.Enqueue(Logger.Log("RC RECEIVED RESPONSE FROM LRM, , UPDATING TOPOLOGY", LogType.INFO));
                messageQueue.Enqueue(Logger.Log("LRM SENDS RESPONSE TO CC", LogType.INFO));
                messageQueue.Enqueue(Logger.Log("NCC SENDS CALL ACCEPT TO CPCC", LogType.INFO));
                package = Protocol.CreateResponse(ControlResponse.OK);
            }
            catch
            {
                package = Protocol.CreateResponse(ControlResponse.ERROR);
            }
            return package;
        }

        private byte[] ReSetSession(byte[] package)
        {          
            messageQueue.Enqueue(Logger.Log("MODIFY SESSION MESSAGE RECEIVED", LogType.INFO));
            Interpreter.PathReSet(package, out int sessionID, out double band);
            Session FoundSession = Sessions.Find(item => item.SessionID.Equals(sessionID));
            string edges = "";
            string nodes = "";
            foreach (Edge e in RC.ShortestDijkstraPath(FoundSession.Source, FoundSession.Destinantion))
            {
                edges += e.ID.ToString() + " ";
                nodes += e.Node1.ToString() + " ";
            }

            try
            {
                FoundSession.isActivee();//Throws exception if not found(chyba), or inactive
                FoundSession.resetTimer();

                try
                {
                    messageQueue.Enqueue(Logger.Log("CC SENDS ROUTE TABLE QUERY TO RC: MODIFY EXISTING SESSION", LogType.INFO));
                    messageQueue.Enqueue(Logger.Log("RC CALCULATES PATH FOR MODIFIED SESSION", LogType.INFO));
                    Path path = RC.RecalcDijkstra(FoundSession.Source, FoundSession.Destinantion, FoundSession.Fiber_IDs, FoundSession.IndexOfChannel, FoundSession.NumberOfChannels, band);
                    //RecalcDijkstra throws an exception if path is not found
                    //Session temp = new Session(sessionID, path.StartID, path.NumOfSlots, FoundSession.Fiber_IDs, band, FoundSession.Source, FoundSession.Destinantion);
                    List<int> FiberIDs = new List<int>();
                    foreach (Edge e in path.Edges) FiberIDs.Add(e.ID);
                    messageQueue.Enqueue(Logger.Log("EDGES: [ " + edges + "]", LogType.INFO));
                    messageQueue.Enqueue(Logger.Log("NODES: [ " + nodes.Remove(nodes.Length - 10) + "]", LogType.INFO));
                    if (path.NumOfSlots == 1)
                        messageQueue.Enqueue(Logger.Log("USED CHANNEL: " + path.StartID.ToString(), LogType.INFO));
                    else
                        messageQueue.Enqueue(Logger.Log("USED CHANNELS FROM " + path.StartID.ToString() + " TO " + (path.StartID + path.NumOfSlots - 1).ToString(), LogType.INFO));
                    messageQueue.Enqueue(Logger.Log("RC SENDS MESSAGE TO CC WITH CALCULATED PATH", LogType.INFO));
                    messageQueue.Enqueue(Logger.Log("CC RECEIVED MESSAGE FROM RC: SESSION CAN BE MODIFIED", LogType.INFO));
                    messageQueue.Enqueue(Logger.Log("MODIFYING ROUTING RECORDS", LogType.INFO));
                    messageQueue.Enqueue(Logger.Log("SENDS MESSAGE TO ROUTERS: DELETE ROUTING RECORDS", LogType.INFO));
                    deleteRoutingTables(FoundSession);

                    messageQueue.Enqueue(Logger.Log("CC SENDS MESSAGE TO LRM: LINK CONNECTION DEALLOCATION", LogType.INFO));
                    lrm.FreeResources(FoundSession.Fiber_IDs, FoundSession.IndexOfChannel, FoundSession.NumberOfChannels);
                    FoundSession.Band = band;
                    FoundSession.Fiber_IDs = FiberIDs;
                    FoundSession.IndexOfChannel = path.StartID;
                    FoundSession.NumberOfChannels = path.NumOfSlots;

                    messageQueue.Enqueue(Logger.Log("CC SENDING LINK CONNECTION REQUEST TO LRM: OCCUPY RESOURCES", LogType.INFO));
                    //tego chyba brakowalo ale sprawdzcie
                    messageQueue.Enqueue(Logger.Log("LRM OCCUPIES RESOURCES", LogType.INFO));
                    messageQueue.Enqueue(Logger.Log("LRM SENDS CONFIRMATION OF RESOURCE ALLOCATION TO CC", LogType.INFO));
                    messageQueue.Enqueue(Logger.Log("CC RECEIVED LOCAL TOPOLOGY FROM LRM", LogType.INFO));

                    messageQueue.Enqueue(Logger.Log("CC SENDS LOCAL TOPOLOGY TO RC: REFRESH TOPOLOGY INFORMATION", LogType.INFO));
                    messageQueue.Enqueue(Logger.Log("SENDING ROUTING RECORDS TO ROUTERS", LogType.INFO));
                    setRoutingTables(FoundSession);
                    //
                    //KomunikacjaZLRM();
                    List <Edge> UpdatedEdges = new List<Edge>();
                    UpdatedEdges = lrm.OfficialOccupy(path.Edges, path.StartID, path.NumOfSlots);


                    if (UpdatedEdges.Count > 0) RC.UpdateEdges(UpdatedEdges);
                    
                    //trzeba będzie gdzieś przechowywać pary SourcePort, IP (czyli IP, port hostów)
                    //KOMUNIKACJA Z LRM 
                    //WYSYLANIE INFO DO ROUTEROW

                    messageQueue.Enqueue(Logger.Log("NCC SENDS CALL ACCEPT TO CPCC", LogType.INFO));
                    messageQueue.Enqueue(Logger.Log("SESSION " + FoundSession.sessionID + " IS EDITED", LogType.INFO));
                    package = Protocol.CreateResponse(ControlResponse.OK);//Zmiana banda
                }
                catch
                {
                    //KOMUNIKACJA Z LRM
                    //INFO DO ROUTEROW
                    package = Protocol.CreateResponse(ControlResponse.ERROR);//stary band
                }
                

                Sessions.Find(item => item.SessionID == sessionID).resetTimer();
            }
            catch
            {
                package = Protocol.CreateResponse(ControlResponse.UnknownError);
            }
            return package;
        }

        private void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {

            foreach (Session x in Sessions)
            {
                try
                {
                    if (x.isTerminated)
                    {
                        sendMesage(Interpreter.Terminate(x.sessionID), x.Source);
                        Sessions.Remove(x);
                        //continue;
                    }
                    else x.isActivee();
                }
                catch
                {
                    //KOMUNIKACJA Z LRM
                    //INFO DO ROUTEROW
                    deleteRoutingTables(x);
                    List<Edge> UpdatedEdges = lrm.FreeResources(x.Fiber_IDs, x.IndexOfChannel, x.NumberOfChannels);
                    RC.UpdateEdges(UpdatedEdges);
                    Sessions.Remove(x);
                    sendMesage(Protocol.CreateMessage(ControlParam.TimedOut), x.Source);//trzeba skądś wziąść IP i port hosta
                }
            }
        }

        private void setRoutingTables(Session session)
        {
            List<IPAddress> nodesToChange = getNodes(session.Fiber_IDs);
            for (int i = 0; i < nodesToChange.Count() - 1; i++)
            {
                messageQueue.Enqueue(Logger.Log("SET RECORD IN " + nodesToChange[i], LogType.INFO));
                sendMesage(Protocol.CreateMessage(ControlParam.SetRouting, new RoutingRecord(session.SessionID, session.Fiber_IDs[i + 1], session.IndexOfChannel, session.Fiber_IDs[i]).toBytes()), nodesToChange[i]);
            }
            messageQueue.Enqueue(Logger.Log("SET RECORD IN " + nodesToChange[nodesToChange.Count() - 1], LogType.INFO));
            sendMesage(Protocol.CreateMessage(ControlParam.SetEdgeRouting, new EdgeRoutingRecord(session.SessionID, session.IndexOfChannel, session.Fiber_IDs[nodesToChange.Count() - 1]).toBytes()), nodesToChange[nodesToChange.Count() - 1]);//ojezu
        }

        private void deleteRoutingTables(Session session)
        {
            List<IPAddress> nodesToChange = getNodes(session.Fiber_IDs);
            /*foreach(IPAddress RouterIP in nodesToChange)
            {
                sendMesage(Protocol.CreateMessage(ControlParam.DeleteRoutingbyID))
            }*///Patrzcie jakie to by było łądne jakby @Marek z kolektywizował usuwanie wpisów
            for (int i = 0; i < nodesToChange.Count() - 1; i++)
            {
                sendMesage(Protocol.CreateMessage(ControlParam.DeleteRoutingbyID, session.SessionID), nodesToChange[i]);
                messageQueue.Enqueue(Logger.Log("DELETE RECORD IN" + nodesToChange[i], LogType.INFO));
            }
            sendMesage(Protocol.CreateMessage(ControlParam.DeleteEdgeRoutingbyID, session.SessionID), nodesToChange[nodesToChange.Count() - 1]);
            messageQueue.Enqueue(Logger.Log("DELETE RECORD IN" + nodesToChange[nodesToChange.Count() - 1], LogType.INFO));

        }

        private List<IPAddress> getNodes(List<int> IDs)
        {
            List<IPAddress> output = new List<IPAddress>();
            Edge edge = edges.Find(item => item.ID == IDs[0]);//find first edge

            if (nodes.Find(item => item.IP.Equals(edge.Node1)).Type == Type.ROUTER) output.Add(edge.Node1);//check which end is connected to Host, and choose the other one
            else output.Add(edge.Node2);

            for (int i = 1; i < IDs.Count() - 1; i++)// for each internal edge (from 1 to N-1)
            {
                edge = edges.Find(item => item.ID == IDs[i]);//find edge by ID
                if (edge.Node1.Equals(output[i - 1])) output.Add(edge.Node2);//check which end was previously added, and choose the other one
                else output.Add(edge.Node1);
            }
            return output;//return nodes which tables will be changed
        }

        //wyłuskanie id łącza, które wstało/padło <-- nie jestem pewien zamiany na int
        private int checkLinkId(byte[] package)
        {
            int linkId;
            List<byte> tmp = new List<byte>();

            for (int i = 1; i < package.Length - 1; i++)
            {
                tmp.Add(package[i]);
            }

            linkId = BitConverter.ToInt32(tmp.ToArray(), 0);
            return linkId;
        }

        private void linkDown(int id)
        {

            messageQueue.Enqueue(Logger.Log("DETECTED LINK DOWN: LINK " + id, LogType.INFO));
            List<Edge> UpdatedEdges = new List<Edge>();
            messageQueue.Enqueue(Logger.Log("SENDING INFORMATION TO LRM", LogType.INFO));
            UpdatedEdges = lrm.linkDown(id);
            messageQueue.Enqueue(Logger.Log("SENDING LOCAL TOPOLOGY INFORMATION TO RC", LogType.INFO));
            messageQueue.Enqueue(Logger.Log("RECEIVED LOCAL TOPOLOGY INFORMATION FROM LRM", LogType.INFO));
            if (UpdatedEdges.Count > 0) RC.UpdateEdges(UpdatedEdges);
            checkLinkUse(id);
        }

        private void linkUp(int id)
        {

            messageQueue.Enqueue(Logger.Log("DETECTED LINK UP: LINK " + id, LogType.INFO));
            List<Edge> UpdatedEdges = new List<Edge>();
            messageQueue.Enqueue(Logger.Log("SENDING INFORMATION TO LRM", LogType.INFO));
            UpdatedEdges = lrm.linkUp(id);
            if (UpdatedEdges.Count > 0) RC.UpdateEdges(UpdatedEdges);
        }

        //metoda do sprawdzenia, czy coś korzysta z zepsutego łącza i do ewentualnej naprawy ścieżki
        //nazwa może troche myląca
        private void checkLinkUse(int id)
        {
            messageQueue.Enqueue(Logger.Log("CHECKING IF LINK DOWN CAUSED SESSION DESTROY", LogType.INFO));
            //przejście po aktywnych sesjach i sprawdzenie, czy któraś używa linka, który padł
            foreach (Session s in Sessions)
            {
                s.resetTimer();
                List<int> IDs = new List<int>();
                IDs = s.Fiber_IDs;
                if (IDs.Exists(x => x.Equals(id)))
                {
                    //przypisanie source, dest i band sesji do zmiennych
                    IPAddress src = s.Source;
                    IPAddress dst = s.Destinantion;
                    double band = s.Band;
                    //ubicie sesji korzystającej z nieaktywnego łącza
                    //sendMesage(Protocol.CreateMessage(ControlParam.TimedOut), s.Source);
                    
                    //wyznaczenie nowej ścieżki
                    Interpreter.Path(src, dst, band);
                    try
                    {
                        string edges = "";
                        string nodes = "";
                        foreach (Edge e in RC.ShortestDijkstraPath(src, dst))
                        {
                            edges += e.ID.ToString() + " ";
                            nodes += e.Node1.ToString() + " ";
                        }
                        //Path path = RC.Dijkstra(src, dst, band);
                        List<int> FiberIDsToSend = new List<int>();
                        foreach (int x in s.Fiber_IDs) if (x != id) FiberIDsToSend.Add(x);
                        messageQueue.Enqueue(Logger.Log("TRYING TO FIND ALTERNATIVE PATH FOR SESSION", LogType.INFO));
                        Path path = RC.RecalcDijkstra(src, dst, FiberIDsToSend, s.IndexOfChannel, s.NumberOfChannels, band);
                        //Dijkstra throws an error if path is not found
                        messageQueue.Enqueue(Logger.Log("CC RECEIVED CONNECTION REQUEST", LogType.INFO));
                        messageQueue.Enqueue(Logger.Log("CC SENDS ROUTE TABLE QUERY TO RC: PATH FROM " + src + " TO " + dst, LogType.INFO));
                        //messageQueue.Enqueue(Logger.Log("RECEIVED NEW PATH FROM RC", LogType.INFO));
                        //Komunikacja z LRM, on już zaktualizował dostepne edge
                        List<Edge> UpdatedEdges = new List<Edge>();
                        messageQueue.Enqueue(Logger.Log("RC RECEIVED ROUTE TABLE QUERY: CALCULATING PATH", LogType.INFO));
                        messageQueue.Enqueue(Logger.Log("EDGES: [ " + edges + "]", LogType.INFO));
                        messageQueue.Enqueue(Logger.Log("NODES: [ " + nodes.Remove(nodes.Length - 10) + "]", LogType.INFO));
                        if (path.NumOfSlots == 1)
                            messageQueue.Enqueue(Logger.Log("USED CHANNEL: " + path.StartID.ToString(), LogType.INFO));
                        else
                            messageQueue.Enqueue(Logger.Log("USED CHANNELS FROM " + path.StartID.ToString() + " TO " + (path.StartID + path.NumOfSlots - 1).ToString(), LogType.INFO));

                        messageQueue.Enqueue(Logger.Log("RC SENDS MESSAGE TO CC WITH CALCULATED PATH", LogType.INFO));
                        messageQueue.Enqueue(Logger.Log("CC RECEIVED RESPONSE FROM RC", LogType.INFO));
                        messageQueue.Enqueue(Logger.Log("SENDING MESSAGE TO LRM: LINK CONNECTION DEALLOCATION", LogType.INFO));
                        lrm.FreeResources(s.Fiber_IDs, s.IndexOfChannel, s.NumberOfChannels);
                        messageQueue.Enqueue(Logger.Log("CC SENDS LINK CONNECTION REQUEST TO LRM: OCCUPY RESOURCES", LogType.INFO));
                        //messageQueue.Enqueue(Logger.Log("SENDING MESSAGE TO LRM: LINK CONNECTION REQUEST", LogType.INFO));
                        UpdatedEdges = lrm.OfficialOccupy(path.Edges, path.StartID, path.NumOfSlots);
                        messageQueue.Enqueue(Logger.Log("LRM OCCUPIES RESOURCES", LogType.INFO));
                        messageQueue.Enqueue(Logger.Log("LRM SENDS CONFIRMATION OF RESOURCE ALLOCATION TO CC", LogType.INFO));
                        messageQueue.Enqueue(Logger.Log("CC RECEIVED LOCAL TOPOLOGY INFORMATION FROM LRM", LogType.INFO));
                        messageQueue.Enqueue(Logger.Log("CC SENDS LOCAL TOPOLOGY INFORMATION TO RC", LogType.INFO));
                        if (UpdatedEdges.Count > 0) RC.UpdateEdges(UpdatedEdges);

                        int session = s.SessionID;//to jest raczej jakiś synchronizacyjny shit, nie pytajcie -> otóż nie
                        List<int> ids = new List<int>();
                        for (int i = 0; i < path.Edges.Count; i++)
                            ids.Add(path.Edges[i].ID);
                        messageQueue.Enqueue(Logger.Log("SENDING MESSAGES TO ROUTERS: DELETE ROUTING RECORDS WITH SESSION ID: " + session.ToString(), LogType.INFO));
                        deleteRoutingTables(s);
                        s.Fiber_IDs = ids;
                        s.IndexOfChannel = path.StartID;
                        s.NumberOfChannels = path.NumOfSlots;
                        messageQueue.Enqueue(Logger.Log("SENDING MESSAGES TO ROUTERS: ADD ROUTING RECORDS WITH SESSION ID: " + session.ToString(), LogType.INFO));
                        setRoutingTables(s);
                        messageQueue.Enqueue(Logger.Log("SESSION " + s.sessionID + " IS UPDATED", LogType.INFO));
                    }
                    catch (Exception e)
                    {
                        messageQueue.Enqueue(Logger.Log("COULD NOT FIND ALTERNATIVE PATH", LogType.INFO));
                        List<Edge> UpdatedEdges = new List<Edge>();
                        messageQueue.Enqueue(Logger.Log("SENDING MESSAGE TO ROUTERS: DELETE ROUTING RECORDS", LogType.INFO));
                        messageQueue.Enqueue(Logger.Log("SENDING MESSAGE TO LRM: LINK CONNECTION DEALLOCATION", LogType.INFO));
                        UpdatedEdges = lrm.FreeResources(s.Fiber_IDs, s.IndexOfChannel, s.NumberOfChannels);
                        messageQueue.Enqueue(Logger.Log("RECEIVED LOCAL TOPOLOGY INFORMATION FROM LRM", LogType.INFO));
                        messageQueue.Enqueue(Logger.Log("SENDING LOCAL TOPOLOGY INFORMATION TO RC", LogType.INFO));
                        RC.UpdateEdges(UpdatedEdges);

                        deleteRoutingTables(s);
                        s.terminate();
                        messageQueue.Enqueue(Logger.Log("Failed to recalc Path", LogType.ERROR));
                    }

                }

            }
        }
        /*
        public void allInfo(Session s)
        {
            string fibers = "";
            foreach(int fiberID in s.Fiber_IDs)
            {
                fibers = fibers + " " + fiberID.ToString();
            }
            fibers.Reverse();
            messageQueue.Enqueue("================================================================\n");
            messageQueue.Enqueue(Logger.Log("SESSION ID: " + s.SessionID.ToString(), LogType.INFO));
            messageQueue.Enqueue(Logger.Log("SESSION BETWEEN " + s.Source.ToString() + " AND " + s.Destinantion.ToString(), LogType.INFO));
            if (s.NumberOfChannels == 1)
                messageQueue.Enqueue(Logger.Log("USED CHANNEL: " + s.IndexOfChannel.ToString(), LogType.INFO));
            else
                messageQueue.Enqueue(Logger.Log("USED CHANNELS FROM " + s.IndexOfChannel.ToString() + " TO " + s.LastChannel().ToString(), LogType.INFO));
            messageQueue.Enqueue("================================================================\n");
        }
        */

        private bool AskOtherSide(IPAddress src, IPAddress dst)
        {
            try
            {
                int port = nodes.Find(item => item.IP.Equals(dst)).Port;
                Sender = new Socket(new IPEndPoint(IP, port).AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                Sender.Connect(new IPEndPoint(IP, port));
                Sender.Send(new Package(logicIP, 0, 0, Protocol.CreateMessage(ControlParam.CallAccept, Encoding.ASCII.GetBytes(src.ToString()))).toBytes());
                switch (Protocol.getControlResponse(WaitAndRead(Sender)))
                {
                    case ControlResponse.OK:
                        Sender.Close();
                        return true;
                    case ControlResponse.ERROR:
                        Sender.Close();
                        return false;
                    default:
                        break;
                }
                Sender.Close();
            }
            catch { }
            finally { if (Sender != null) Sender.Close(); }
            return false;
            throw new NotImplementedException();
        }
        private byte[] WaitAndRead(Socket readerek)
        {
            while (readerek.Available == 0) ;
            List<byte> temp = new List<byte>();
            byte[] buffer = new byte[128];
            while (readerek.Available > 0)
            {
                readerek.Receive(buffer, buffer.Length, SocketFlags.Partial);
                temp.AddRange(buffer);
            }
            readerek.Close();
            return temp.ToArray();
        }
    }
}
