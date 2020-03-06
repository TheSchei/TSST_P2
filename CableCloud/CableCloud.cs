using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.Threading;
using Simulation;

namespace CableCloud
{
    class CableCloud
    {
        private int cloudPort;
        private int driverPort;
        public IPAddress cloudIP;
        private IPAddress DriverIP;
        private ManualResetEvent AllDone = new ManualResetEvent(false);
        private Queue<string> bufor = new Queue<string>();
        public Queue<string> messageQueue = new Queue<string>();

        private Socket Receiver;
        private Thread t;

        private CloudFields fields = new CloudFields();

        public CableCloud(string filePath, string structurePath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            cloudPort = Convert.ToInt32(doc.SelectSingleNode("/Cloud/Port").InnerText);
            cloudIP = IPAddress.Parse(doc.SelectSingleNode("/Cloud/IP").InnerText);
            driverPort = Convert.ToInt32(doc.SelectSingleNode("/Cloud/DriverPort").InnerText);
            DriverIP = IPAddress.Parse(doc.SelectSingleNode("/Cloud/DriverIP").InnerText);

            doc.Load(structurePath);
            XmlNodeList Edges = doc.DocumentElement.SelectNodes("/Structure/Edges/Edge");
            foreach (XmlNode Edge in Edges)
            {
                fields.addFiber(Edge.SelectSingleNode("ID").InnerText,
                    Edge.SelectSingleNode("IP1").InnerText,
                    Edge.SelectSingleNode("IP2").InnerText,
                    Edge.SelectSingleNode("Port1").InnerText,
                    Edge.SelectSingleNode("Port2").InnerText,
                    Edge.SelectSingleNode("Enable").InnerText);
            }
            Receiver = new Socket(new IPEndPoint(cloudIP, cloudPort).AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            t = new Thread(listen);
            t.Start();
        }


        private void listen()
        {
            Receiver.Bind(new IPEndPoint(cloudIP, cloudPort));
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
                while (reader.Available == 0);
                while (reader.Available > 0)
                {
                    reader.Receive(buffer, buffer.Length, SocketFlags.Partial);
                    temp.AddRange(buffer);
                }
                Package package = new Package(temp.ToArray());

                //Fields

                int port = fields.getDest(package.LastNode, package.FiberID);
                sendMessage(package, port);
            }
            catch (Exception e)
            {
                messageQueue.Enqueue(Logger.Log(e.Message, LogType.ERROR));
                
            }
            finally { if (reader != null) reader.Close(); }
        }

        public bool sendMessage(Package package, int port)
        {
            Socket Sender = new Socket(new IPEndPoint(cloudIP, port).AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                Sender.Connect(new IPEndPoint(cloudIP, port));
                Sender.Send(package.toBytes());
                Sender.Close();
                messageQueue.Enqueue(Logger.Log("Package from " + package.LastNode + " sent to port " + port, LogType.INFO));
            }
            catch (Exception e)
            {
                messageQueue.Enqueue(Logger.Log(e.Message, LogType.ERROR));
                if (Sender != null) Sender.Close();
                return false;
            }
            return true;
        }
        public void ReverseFieldStatus(int i)
        {
            Socket Sender = new Socket(new IPEndPoint(cloudIP, driverPort).AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                Sender.Connect(new IPEndPoint(cloudIP, driverPort));
                byte[] package;
                if (fields.reverseStatus(i, out int fiberID)) package = Interpreter.LinkUp(fiberID);
                else package = Interpreter.LinkDown(fiberID);
                Sender.Send(package);
                messageQueue.Enqueue(Logger.Log("Status sent to LRM", LogType.INFO));
            }
            catch (Exception e)
            {
                messageQueue.Enqueue(Logger.Log(e.Message, LogType.ERROR));
            }
            finally
            {
                if (Sender != null) Sender.Close();
            }
        }
        public int CountFields()
        {
            return fields.Count();
        }
        public bool isFieldActive(int i)
        {
            return fields.isActive(i);
        }
        public string[] GetFieldStrings()
        {
            return fields.GetStrings();
        }
        public void Dispose()
        {
            t.Abort();
        }
    }
}
