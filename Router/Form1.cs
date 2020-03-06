using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Simulation;

namespace Router
{
    public partial class Form1 : Form
    {
        private Router router;
       
        public Form1(string filePath)
        {
            try
            {
                router = new Router(filePath);
                InitializeComponent();
                this.Text = router.RouterName;
                LogBox.Text += Logger.Log("Router started working", LogType.INFO);
            }
            catch (System.IO.FileNotFoundException e)
            {
                DialogResult result = MessageBox.Show(e.Message, "Failed to start application", MessageBoxButtons.OK);
                this.Close();
            }
        }

        private void Refresher_Tick(object sender, EventArgs e)
        {
            while (router.messageQueue.Count > 0)
            {
                LogBox.AppendText(router.messageQueue.Dequeue());
            }
        }

        private void ShowTable(ControlParamRouter x)
        {
            hideAll();
            DataView.AutoResizeColumns();
            switch (x)
            {
                case ControlParamRouter.ShowEdgeRoutingTable:
                    SetupEdgeRoutingDataGridView();
                    break;
                case ControlParamRouter.ShowRoutingTable:
                    SetupRoutingDataGridView();
                    break;  
            }
        }
        private void hideAll()
        {
            DataView.Visible = false;
        }

        public void SetupEdgeRoutingDataGridView()
        {
            this.Controls.Add(DataView);
            DataView.ColumnCount = 3;
            DataView.Name = "EdgeRouterTable";

            DataView.Columns[0].Name = "sessionID";
            DataView.Columns[1].Name = "indexOfChannel";
            DataView.Columns[2].Name = "outFiber";
            for (int i=0; i<router.PubEdgeRoutingTable.Count; i++)
            {
                EdgeRoutingRecord EdgeRoutingRow = new EdgeRoutingRecord(router.PubEdgeRoutingTable.ElementAt(i).SessionID,
                    router.PubEdgeRoutingTable.ElementAt(i).IndexOfChannel,
                    router.PubEdgeRoutingTable.ElementAt(i).OutFiberID);
                string[] row = { EdgeRoutingRow.SessionID.ToString(),
                    EdgeRoutingRow.IndexOfChannel.ToString(),
                    EdgeRoutingRow.OutFiberID.ToString()};
                DataView.Rows.Add(row);
            }
            DataView.Show();
        }

        public void SetupRoutingDataGridView()
        {
            this.Controls.Add(DataView);
            DataView.ColumnCount = 4;
            DataView.Name = "RouterTable";

            DataView.Columns[0].Name = "sessionID";
            DataView.Columns[1].Name = "inFiber";
            DataView.Columns[2].Name = "indexOfChannel";
            DataView.Columns[3].Name = "outFiber";
            for (int i = 0; i < router.PubRoutingTable.Count; i++)
            {
                RoutingRecord RoutingRow = new RoutingRecord(router.PubRoutingTable.ElementAt(i).SessionID,
                    router.PubRoutingTable.ElementAt(i).InFiberID,
                    router.PubRoutingTable.ElementAt(i).IndexOfChannel,
                    router.PubRoutingTable.ElementAt(i).OutFiberID);
                string[] row = { RoutingRow.SessionID.ToString(),
                    RoutingRow.InFiberID.ToString(),
                    RoutingRow.IndexOfChannel.ToString(),
                    RoutingRow.OutFiberID.ToString()};
                DataView.Rows.Add(row);
            }
            DataView.Show();
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            DataView.Columns.Clear();
            try
            {
                if (EdgeRoutingButton.Checked)
                    SetupEdgeRoutingDataGridView();
                else if (RoutingButton.Checked)
                    SetupRoutingDataGridView();
            }
            catch (Exception ex)
            {
                LogBox.AppendText(Logger.Log(ex.Message, LogType.ERROR));
            }

        }
    }
}
