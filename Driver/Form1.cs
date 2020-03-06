using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using Simulation;

namespace Driver
{
    public partial class Form1 : Form
    {
        private Driver driver;
        public Form1(string filename, string structFilename)
        {
            try
            {
                driver = new Driver(filename, structFilename);
                InitializeComponent();
                this.Text = "Driver";
                this.Name = "Driver";
                LogBox.Text += Logger.Log("Driver started working", LogType.INFO);
            }
            catch (System.IO.FileNotFoundException e)
            {
                DialogResult result = MessageBox.Show(e.Message, "Failed to start application", MessageBoxButtons.OK);
                this.Close();
            }
        }

        private void Refresher_Tick(object sender, EventArgs e)
        {
            while (driver.messageQueue.Count > 0)
               LogBox.AppendText(driver.messageQueue.Dequeue());
        }
    }
}
