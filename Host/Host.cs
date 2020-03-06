using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using Simulation;

namespace Host
{
    class Host
    {
        public string HostName;
        private IPAddress IP;
        public IPAddress logicIP;
        private IPAddress UNI;
        private int FiberID;
        private int recPort;
        private int cloudPort;
        private int UNIPort;
        private Socket Receiver;
        private Socket Sender;
        public List<string> remoteHostIPs = new List<string>();
        public Queue<string> messageQueue = new Queue<string>();
        private ManualResetEvent AllDone = new ManualResetEvent(false);
        private Thread t;

        private int sessionID;
        private System.Timers.Timer sessionTimer;


        public Host(string filePath,string structure)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            HostName = doc.SelectSingleNode("/Host/HostName").InnerText;
            IP = IPAddress.Parse(doc.SelectSingleNode("/Host/IP").InnerText);
            logicIP = IPAddress.Parse(doc.SelectSingleNode("/Host/logIP").InnerText);
            recPort = Convert.ToInt32(doc.SelectSingleNode("/Host/RecPort").InnerText);
            //FiberID = Convert.ToInt32(doc.SelectSingleNode("/Host/Gateway").InnerText);//DOROBIC
            cloudPort = Convert.ToInt32(doc.SelectSingleNode("/Host/CloudPort").InnerText);
            UNIPort = 2137;
            UNI = IPAddress.Parse("8.8.8.8");
            string temp = doc.SelectSingleNode("/Host/logIP").InnerText;

            doc.Load(structure);
            XmlNodeList Nodes = doc.DocumentElement.SelectNodes("/Structure/Nodes/Node");
            foreach (XmlNode Node in Nodes)
                if((Node.SelectSingleNode("Type").InnerText == "HOST") && (Node.SelectSingleNode("IP").InnerText != temp))
                    remoteHostIPs.Add(Node.SelectSingleNode("IP").InnerText);
            Nodes = doc.DocumentElement.SelectNodes("/Structure/Edges/Edge");
            foreach (XmlNode Node in Nodes)
                if ((Node.SelectSingleNode("IP1").InnerText == temp) || (Node.SelectSingleNode("IP2").InnerText == temp))
                    FiberID = Convert.ToInt32(Node.SelectSingleNode("ID").InnerText);
            Receiver = new Socket((new IPEndPoint(IP, recPort)).AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            t = new Thread(Listen);
            t.Start();
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
                while (reader.Available == 0) ;// Thread.Sleep(3);
                while (reader.Available > 0)
                {
                    reader.Receive(buffer, buffer.Length, SocketFlags.Partial);
                    temp.AddRange(buffer);
                }
                Package package = new Package(temp.ToArray());
                if (package.Source.Equals(UNI)) InterpreteMessage(package, reader);
                else messageQueue.Enqueue(package.Source.ToString() + ": " + package.Payload + Environment.NewLine);
                reader.Disconnect(true);
            }
            catch (Exception e)
            {
                messageQueue.Enqueue(Logger.Log(e.Message, LogType.ERROR));
            }
            finally { if (reader != null) reader.Close(); }
        }
        public bool sendMesage(string message, IPAddress destination)
        {
            try
            {
                Sender = new Socket((new IPEndPoint(IP, cloudPort)).AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                Sender.Connect(new IPEndPoint(IP, cloudPort));
                Sender.Send(new Package(logicIP, FiberID, sessionID, message).toBytes());
                messageQueue.Enqueue(Logger.Log("Sent to " + destination.ToString() + ": " + message, LogType.INFO));
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
        private void InterpreteMessage(Package package, Socket soc)
        {
            string payload = package.Payload;
            switch (Protocol.getControlParamAndDelete(ref payload))
            {
                case ControlParam.TimedOut:
                    //if (Interpreter.TimeOut(payload) == sessionID)
                    //{
                        sessionTimer.Stop();
                        messageQueue.Enqueue("TIMEOUT");
                    //}//else ignore :p
                    break;
                case ControlParam.SessionTerminated:
                    //if (Interpreter.Terminate(payload) == sessionID)
                    //{
                        sessionTimer.Stop();
                        messageQueue.Enqueue("TERMINATE");
                    //}//else ignore :p//jebać te warunki
                    break;
                case ControlParam.CallAccept://W payloadzie powinien iść 1 bajt numerku, a dalej SRC IP stringiem
                   soc.Send(Acceptor(payload));
                        break;
                default:
                    break;
            }
        }

        private byte[] Acceptor(string payload)
        {
            System.Windows.Forms.MessageBoxButtons buttons = System.Windows.Forms.MessageBoxButtons.YesNo;
            System.Windows.Forms.DialogResult result;

            // Displays the MessageBox.
            result = System.Windows.Forms.MessageBox.Show("From " + payload, "Do you accept a call?", buttons);
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                messageQueue.Enqueue(Logger.Log("Call from " + payload + " accepted", LogType.INFO));
                return Protocol.CreateResponse(ControlResponse.OK);
            }
            else
            {
                messageQueue.Enqueue(Logger.Log("Call from " + payload + " rejected", LogType.INFO));
                return Protocol.CreateResponse(ControlResponse.ERROR);
            }
        }

        public bool BeginSession(IPAddress dest, double band)
        {
            try
            {
                Sender = new Socket((new IPEndPoint(IP, cloudPort)).AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                Sender.Connect(new IPEndPoint(IP, UNIPort));
                Sender.Send(Interpreter.Path(logicIP, dest, band));
                messageQueue.Enqueue(Logger.Log("Sent Path message", LogType.INFO));
                byte[] package = WaitAndRead(Sender);
                Sender.Close();
                switch (Protocol.getControlResponse(package))
                {
                    case ControlResponse.OK:
                        Interpreter.PathOK(package, out sessionID);
                        sessionTimer = new System.Timers.Timer(50000);//50 sekund
                        sessionTimer.Elapsed += OnTimedEvent;
                        sessionTimer.AutoReset = true;
                        sessionTimer.Enabled = true;
                        return true;
                    case ControlResponse.ERROR:
                        return false;
                    default:
                        return false;
                }
            }
            catch (Exception e)
            {
                messageQueue.Enqueue(Logger.Log(e.Message, LogType.ERROR));
                if (Sender != null) Sender.Close();
                return false;
            }
        }
        public bool ReSetSession(double band)
        {
            try
            {
                sessionTimer.Stop();
                Sender = new Socket((new IPEndPoint(IP, cloudPort)).AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                Sender.Connect(new IPEndPoint(IP, UNIPort));
                Sender.Send(Interpreter.PathReSet(sessionID, band));
                messageQueue.Enqueue(Logger.Log("Sent Path reset message", LogType.INFO));
                byte[] package = WaitAndRead(Sender);
                Sender.Close();
                switch (Protocol.getControlResponse(package))
                {
                    case ControlResponse.OK://udało się jej
                        sessionTimer = new System.Timers.Timer(50000);//50 sekund
                        sessionTimer.Elapsed += OnTimedEvent;
                        sessionTimer.AutoReset = true;
                        sessionTimer.Enabled = true;
                        return true;
                    case ControlResponse.ERROR://nie udało się zwiększyć prędkości
                        sessionTimer = new System.Timers.Timer(50000);//50 sekund
                        sessionTimer.Elapsed += OnTimedEvent;
                        sessionTimer.AutoReset = true;
                        sessionTimer.Enabled = true;
                        return false;
                    case ControlResponse.UnknownError://całkiem wyjebało
                        throw new Exception("An error occured, czy jakoś tak");
                    default:
                        return false;
                }
            }
            catch (Exception e)
            {
                messageQueue.Enqueue(Logger.Log(e.Message, LogType.ERROR));
                if (Sender != null) Sender.Close();
                return false;
            }
        }
        public bool TerminateSession()
        {
            try
            {
                Sender = new Socket((new IPEndPoint(IP, cloudPort)).AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                Sender.Connect(new IPEndPoint(IP, UNIPort));
                Sender.Send(Interpreter.PathTerminate(sessionID));
                messageQueue.Enqueue(Logger.Log("Sent Path terminate message", LogType.INFO));
                byte[] package = WaitAndRead(Sender);
                Sender.Close();
                switch (Protocol.getControlResponse(package))
                {
                    case ControlResponse.OK://udało się jej
                        sessionTimer.Stop();
                        sessionID = 0;
                        return true;
                    case ControlResponse.ERROR://nie udało się zakończyć
                        return false;
                    case ControlResponse.UnknownError://całkiem wyjebało
                        throw new Exception("An error occured, czy jakoś tak");
                    default:
                        return false;
                }
            }
            catch (Exception e)
            {
                messageQueue.Enqueue(Logger.Log(e.Message, LogType.ERROR));
                if (Sender != null) Sender.Close();
                return false;
            }
        }
        public void Dispose()
        {
            t.Abort();
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
        private void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            //przedłużenie sesji
            try
            {
                Sender = new Socket((new IPEndPoint(IP, cloudPort)).AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                Sender.Connect(new IPEndPoint(IP, UNIPort));
                Sender.Send(Interpreter.PathUpdate(sessionID));
                messageQueue.Enqueue(Logger.Log("Sent PathUpdate message", LogType.INFO));
                /*while (Sender.Available == 0);
                List<byte> temp = new List<byte>();
                byte[] buffer = new byte[128];
                while (Sender.Available > 0)
                {
                    Sender.Receive(buffer, buffer.Length, SocketFlags.Partial);
                    temp.AddRange(buffer);
                }*/
                byte[] package = WaitAndRead(Sender);
                Sender.Close();
                //Package package = new Package(temp.ToArray());
                switch (Protocol.getControlResponse(package))
                {
                    case ControlResponse.OK:
                        messageQueue.Enqueue(Logger.Log("Session updated", LogType.INFO));
                        return;
                    case ControlResponse.ERROR:
                        return;
                    default:
                        return;
                }
            }
            catch (Exception ex)
            {
                messageQueue.Enqueue(Logger.Log(ex.Message, LogType.ERROR));
                if (Sender != null) Sender.Close();
            }
        }
    }
}
