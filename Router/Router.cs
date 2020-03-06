using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Xml;
using Simulation;
using System.Windows.Forms;

namespace Router
{

    enum Option { Input, EgdeRouting, Routing, Output };

    class Router
    {
        public string RouterName;
        private IPAddress IP;
        private IPAddress logicIP;
        private IPAddress DriverIP = IPAddress.Parse("8.8.8.8");
        private int recPort;
        private int cloudPort;
        private Socket Receiver;
        private Socket Sender;
        private List<RoutingRecord> RoutingTable = new List<RoutingRecord>();
        private List<EdgeRoutingRecord> EdgeRoutingTable = new List<EdgeRoutingRecord>();
        public Queue<String> messageQueue = new Queue<String>();
        private ManualResetEvent AllDone = new ManualResetEvent(false);
        private readonly Thread t;

        private List<IPAddress> HostsIP = new List<IPAddress>();

        public List<RoutingRecord> PubRoutingTable { get => RoutingTable; }
        public List<EdgeRoutingRecord> PubEdgeRoutingTable { get => EdgeRoutingTable; }

        public Router(string filePath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            RouterName = doc.SelectSingleNode("/Router/RouterName").InnerText;
            IP = IPAddress.Parse(doc.SelectSingleNode("/Router/IP").InnerText);
            logicIP = IPAddress.Parse(doc.SelectSingleNode("/Router/logIP").InnerText);
            recPort = Convert.ToInt32(doc.SelectSingleNode("/Router/RecPort").InnerText);
            cloudPort = Convert.ToInt32(doc.SelectSingleNode("/Router/CloudPort").InnerText);
            Receiver = new Socket((new IPEndPoint(IP, recPort)).AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            //moze sie przyda 
            /*doc.Load(structurePath);
            XmlNodeList HostsList = doc.DocumentElement.SelectNodes("/Structure/Nodes/Node");
            foreach (XmlNode Host in HostsList)
            {
                if (Host.SelectSingleNode("Type").InnerText == "HOST")
                {
                    IPAddress HostIP = IPAddress.Parse(Host["IP"].InnerText);
                    HostsIP.Add(HostIP);
                }
            }
            
            //FILL TABLE FROM XML - to tak narazie bo nwm jak bedzie z ta struktura
            XmlNodeList Routing = doc.DocumentElement.SelectNodes("/Router/Routing/Element");
            foreach(XmlNode RoutingRec in Routing)
            {
                int sessionID = Convert.ToInt32(RoutingRec["sessionID"].InnerText);
                int inFiberID = Convert.ToInt32(RoutingRec["inFiberID"].InnerText);
                int indexOfChannel = Convert.ToInt32(RoutingRec["indexOfChannel"].InnerText);
                int outFiber = Convert.ToInt32(RoutingRec["outFiberID"].InnerText);
                RoutingTable.Add(new RoutingRecord(sessionID, inFiberID, indexOfChannel, outFiber));
            }

            XmlNodeList EdgeRouting = doc.DocumentElement.SelectNodes("/Router/EdgeRouting/Element");
            foreach (XmlNode EdgeRoutingRec in EdgeRouting)
            {
                int sessionID = Convert.ToInt32(EdgeRoutingRec["sessionID"].InnerText);
                int indexOfChannel = Convert.ToInt32(EdgeRoutingRec["indexOfChannel"].InnerText);
                int outFiber = Convert.ToInt32(EdgeRoutingRec["outFiberID"].InnerText);
                EdgeRoutingTable.Add(new EdgeRoutingRecord(sessionID, indexOfChannel, outFiber));
            }*/

            //Run listening Thread
            t = new Thread(Listen);
            t.Start();
        }

        public void Dispose()
        {
            //End listening thread at the end
            t.Abort();
        }

        private void Listen()
        {
            Receiver.Bind(new IPEndPoint(IP, recPort));
            Receiver.Listen(10);
            while (true)
            {
                AllDone.Reset();
                Receiver.BeginAccept(new AsyncCallback(readMessage), Receiver);
                AllDone.WaitOne();
            }
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
                Package package = new Package(temp.ToArray());

                if (package.Source.Equals(DriverIP)) // || package.interfaceFrom.ToString() == "8.8.8.8" - ten waruenk chyba trzeba uwzglednic ale dla Drivera 
                {
                    //Configuration message arrived
                    byte[] Response = ProccessManagementMessage(package);
                    

                    

                    

                    //reader.Send(new Package(package.Source, package.FiberID, package.SessionID, Encoding.ASCII.GetString(Response)).toBytes());//Nie przewidzialem czekania na odpowiedz :p
                }
                else
                {
                    routeMessage(package);
                }
            }
            catch (Exception e)
            {
                messageQueue.Enqueue(Logger.Log(e.Message, LogType.ERROR));
            }
            finally { if (reader != null) reader.Close(); }
        }


        public bool routeMessage(Package package)
        {
            Option option = Option.Input;
            try
            {
                int packageSession = package.SessionID;
                messageQueue.Enqueue(Logger.Log("SOURCE: " + package.Source.ToString(), LogType.INFO));   
                while (option != Option.Output)
                {
                    switch (option)
                    {
                        case Option.Input:
                            if (package.IndexOfChanel == -1)
                                option = Option.EgdeRouting;
                            else
                                option = Option.Routing;
                            break;

                        case Option.EgdeRouting:
                            EdgeRoutingRecord MatchingEdgeRouting = EdgeRoutingTable.Find(item => item.SessionID.Equals(packageSession));
                            package.IndexOfChanel = MatchingEdgeRouting.IndexOfChannel;
                            package.FiberID = MatchingEdgeRouting.OutFiberID;
                            option = Option.Output;
                            break;

                        case Option.Routing:
                            RoutingRecord MatchingRouting = RoutingTable.Find(item => item.InFiberID.Equals(package.FiberID) && item.IndexOfChannel.Equals(package.IndexOfChanel));
                            if (MatchingRouting == null)
                                throw new Exception("Couldn't find any matching fiber witch proper channel");
                            package.FiberID = MatchingRouting.OutFiberID;
                            option = Option.Output;
                            break;
                    }
                }
                sendMessage(package);

            }
            catch (Exception e)
            {
                messageQueue.Enqueue(Logger.Log(e.Message, LogType.ERROR));
            }

            return true;
        }

        public bool sendMessage(Package package)
        {
            try
            {
                package.LastNode = logicIP;
                Sender = new Socket((new IPEndPoint(IP, cloudPort)).AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                Sender.Connect(new IPEndPoint(IP, cloudPort));
                Sender.Send(package.toBytes());
                Sender.Close();
            }
            catch (Exception e)
            {
                messageQueue.Enqueue(Logger.Log(e.Message, LogType.ERROR));
                if (Sender != null) Sender.Close();
                return false;
            }
            return true;
        }

        public byte[] ProccessManagementMessage(Package package)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(package.Payload);
            ControlParam Param = (ControlParam)bytes[0];
            bytes = Protocol.DeleteControlParam(bytes);
            List<RoutingRecord> temp = new List<RoutingRecord>();
            string Err = "";
            int ID = -1;
            string rowString = "";

            switch (Param)
            {
                case ControlParam.SetRouting:
                    messageQueue.Enqueue(Logger.Log("SETTING ROUTING RECORD", LogType.INFO));
                    try
                    {
                        RoutingTable.Add(new RoutingRecord(bytes));
                        temp.Add(new RoutingRecord(bytes));
                        for (int i = 0; i < temp.Count; i++)
                        {
                            RoutingRecord RoutingRow = new RoutingRecord(temp.ElementAt(i).SessionID,
                                temp.ElementAt(i).InFiberID,
                                temp.ElementAt(i).IndexOfChannel,
                                temp.ElementAt(i).OutFiberID);
                            string[] row = { RoutingRow.SessionID.ToString(),
                                RoutingRow.InFiberID.ToString(),
                                RoutingRow.IndexOfChannel.ToString(),
                                RoutingRow.OutFiberID.ToString()};

                            foreach (string s in row)
                            {
                                rowString += s + " ";
                            }

                            messageQueue.Enqueue(Logger.Log("ADD ROUTING RECORD ID : " + BitConverter.ToInt32(bytes, 0) + " [ " + rowString + "]", LogType.INFO));
                            temp.Remove(RoutingRow);
                        }
                    }
                    catch
                    {
                        Err = "Setting error";
                    }
                    break;

                case ControlParam.DeleteRoutingbyID:

                    try
                    {
                        ID = BitConverter.ToInt32(bytes, 0);

                        messageQueue.Enqueue(Logger.Log("DELETING ROUTING RECORD ID : " + ID, LogType.INFO));
                        RoutingRecord SessionFromID = RoutingTable.Find(item => item.SessionID.Equals(ID));
                        if (SessionFromID == null)
                            throw new Exception("This SessionID don't exist!");
                        int removedID = RoutingTable.IndexOf(SessionFromID);
                        RoutingTable.RemoveAt(removedID);
                    }
                    catch
                    {
                        Err = "Error occured";
                        break;
                    }
                    break;

                case ControlParam.SetEdgeRouting:
                    try
                    {
                        EdgeRoutingTable.Add(new EdgeRoutingRecord(bytes));
                        for (int i = 0; i < EdgeRoutingTable.Count; i++)
                        {
                            EdgeRoutingRecord EdgeRoutingRow = new EdgeRoutingRecord(EdgeRoutingTable.ElementAt(i).SessionID,
                                EdgeRoutingTable.ElementAt(i).IndexOfChannel,
                                EdgeRoutingTable.ElementAt(i).OutFiberID);
                            string[] row = { EdgeRoutingRow.SessionID.ToString(),
                                EdgeRoutingRow.IndexOfChannel.ToString(),
                                EdgeRoutingRow.OutFiberID.ToString()};

                            foreach (string s in row)
                            {
                                rowString += s + " ";
                            }

                            messageQueue.Enqueue(Logger.Log("ADD EDGE ROUTING RECORD ID : " + BitConverter.ToInt32(bytes, 0) + " [ " + rowString + "]", LogType.INFO));
                        }
                    }
                    catch
                    {
                        Err = "Setting error";
                    }
                    break;

                case ControlParam.DeleteEdgeRoutingbyID:
                    try
                    {
                        ID = BitConverter.ToInt32(bytes, 0);

                        messageQueue.Enqueue(Logger.Log("DELETING EDGE ROUTING RECORD ID : " + ID, LogType.INFO));
                        EdgeRoutingRecord SessionFromID = EdgeRoutingTable.Find(item => item.SessionID.Equals(ID));
                        if (SessionFromID == null)
                            throw new Exception("This SessionID don't exist!");
                        int removedID = EdgeRoutingTable.IndexOf(SessionFromID);
                        EdgeRoutingTable.RemoveAt(removedID);
                    }
                    catch
                    {
                        Err = "Error occured";
                        break;
                    }
                    break;

                default:
                    Err = "Nieobsługiwana opcja";
                    break;
            }
            if (Err == "")
                return Protocol.CreateResponse(ControlResponse.OK);
            else
                return Protocol.CreateResponse(ControlResponse.ERROR, Err);
        }
    }

}
