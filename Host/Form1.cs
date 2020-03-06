using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using Simulation;

namespace Host
{
    public partial class Form1 : Form
    {
        private Host host;

        public Form1(string filePath, string structure)
        {
            try
            {
                host = new Host(filePath, structure);
                InitializeComponent();
                InfoBox.Text = "Host: " + host.logicIP.ToString();//do wpisania
                this.Text = host.HostName;
                this.Name = host.HostName;
                DestinationSelector.Items.AddRange(host.remoteHostIPs.ToArray());
                LogBox.Text += Logger.Log("Host started working", LogType.INFO);
            }
            catch (System.IO.FileNotFoundException e)
            {
                DialogResult result = MessageBox.Show(e.Message, "Failed to start application", MessageBoxButtons.OK);
                this.Close();
            }
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (host.sendMesage(MessageTextBox.Text, IPAddress.Parse(DestinationSelector.Text)))
                    MessageTextBox.Clear();
            }
            catch(System.FormatException ex)
            {
                host.messageQueue.Enqueue(Logger.Log(ex.Message, LogType.ERROR));
            }
        }

        private void Refresher_Tick(object sender, EventArgs e)
        {
            while (host.messageQueue.Count > 0)
            {
                if(host.messageQueue.Peek() == "TIMEOUT")
                {
                    host.messageQueue.Dequeue();
                    LogBox.AppendText(Logger.Log("Session timed out", LogType.INFO));
                    InitState();
                    return;//albo continuie :p
                }
                else if (host.messageQueue.Peek() == "TERMINATE")
                {
                    host.messageQueue.Dequeue();
                    LogBox.AppendText(Logger.Log("Session terminated", LogType.INFO));
                    InitState();
                    return;
                }
                LogBox.AppendText(host.messageQueue.Dequeue());
            }
        }
       
   
        private void StartButton_Click(object sender, EventArgs e)
        {
            
            IPAddress Destination;
            double Bandwidth;
            if (DestinationSelector.Text == "" || !IPAddress.TryParse(DestinationSelector.Text, out Destination))//jak niżej
            {
                host.messageQueue.Enqueue(Logger.Log("Not Selected Destination.", LogType.ERROR));
                return;
            }

            if (BandBox.Text == "" || !double.TryParse(BandBox.Text, out Bandwidth))//prawdopodobnie można wywalić sprawdanie pustego
            {
                host.messageQueue.Enqueue(Logger.Log("Wrong band width.", LogType.ERROR));
                return;
            }

            host.messageQueue.Enqueue(Logger.Log("Call request sent", LogType.INFO));
            AllLockedState();
            try
            {
                
                if (host.BeginSession(Destination, Bandwidth))
                {
                    host.messageQueue.Enqueue(Logger.Log("Session started", LogType.INFO));
                    SessionState();
                }
                else 
                {
                    host.messageQueue.Enqueue(Logger.Log("Sesion start failed.", LogType.ERROR));
                    InitState();
                }
                
            }
            catch (Exception ex)
            {
                host.messageQueue.Enqueue(Logger.Log(ex.Message, LogType.ERROR));
                InitState();
            }
            
        }
        private void EditButton_Click(object sender, EventArgs e)
        {
            double Bandwidth;
            if (BandBox.Text == "" || !double.TryParse(BandBox.Text, out Bandwidth))//prawdopodobnie można wywalić sprawdanie pustego
            {
                host.messageQueue.Enqueue(Logger.Log("Wrong band width.", LogType.ERROR));
                return;
            }
            AllLockedState();
            try
            {
                if (host.ReSetSession(Bandwidth))
                {
                    host.messageQueue.Enqueue(Logger.Log("Band changed to " + Bandwidth.ToString() + " Mbps.", LogType.INFO));
                    SessionState();
                }
                else 
                {
                    host.messageQueue.Enqueue(Logger.Log("Failed to change bandwidth", LogType.ERROR));
                    SessionState();
                }
                
            }
            catch (Exception ex)
            {
                host.messageQueue.Enqueue(Logger.Log(ex.Message, LogType.ERROR));
                InitState();
            }
        }
        private void TerminateButton_Click(object sender, EventArgs e)
        {
            AllLockedState();
            try
            {

                host.TerminateSession();
                host.messageQueue.Enqueue(Logger.Log("Session terminated", LogType.INFO));
                InitState();
                
            }
            catch (Exception ex)
            {
                host.messageQueue.Enqueue(Logger.Log(ex.Message, LogType.ERROR));
                InitState();
            }
        }

        private void InitState()
        {
            SendButton.Enabled = false;
            DestinationSelector.Enabled = true;
            BandBox.Enabled = true;
            StartButton.Enabled = true;
            EditButton.Enabled = false;
            TerminateButton.Enabled = false;
        }
        private void AllLockedState()
        {
            SendButton.Enabled = false;
            DestinationSelector.Enabled = false;
            BandBox.Enabled = false;
            StartButton.Enabled = false;
            EditButton.Enabled = false;
            TerminateButton.Enabled = false;
        }
        private void SessionState()
        {
            SendButton.Enabled = true;
            DestinationSelector.Enabled = false;
            BandBox.Enabled = true;
            StartButton.Enabled = false;
            EditButton.Enabled = true;
            TerminateButton.Enabled = true;
        }
        
    }
}
