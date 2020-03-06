using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CableCloud
{
    public partial class Form1 : Form
    {
        CableCloud Cloud;
        public Form1(string filePath, string structurePath)
        {
            InitializeComponent();
            Cloud = new CableCloud(filePath, structurePath);
            CableViewer.Items.AddRange(Cloud.GetFieldStrings());
            for (int i = 0; i < Cloud.CountFields(); i++)
                if (Cloud.isFieldActive(i)) CableViewer.SetItemChecked(i, true);
            CableViewer.CheckOnClick = true;
            CableViewer.SelectedIndexChanged += new System.EventHandler(CableViewer_SelectedIndexChanged);
        }

        private void Refresher_Tick(object sender, EventArgs e)
        {
            while (Cloud.messageQueue.Count > 0)
            {
                LogBox.AppendText(Cloud.messageQueue.Dequeue());
            }
        }

        private void CableViewer_SelectedIndexChanged(object sender, EventArgs e)
        {
            Cloud.ReverseFieldStatus(((CheckedListBox)sender).SelectedIndex);
        }
    }
}
