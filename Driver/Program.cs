﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Driver
{
    static class Program
    {
        /// <summary>
        /// Główny punkt wejścia dla aplikacji.
        /// </summary>
        [STAThread]
        static void Main(string[] Args)
        {
            if (Args.Length == 2)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                try { Application.Run(new Form1(Args[0], Args[1])); }
                catch (System.ObjectDisposedException) { }
                catch (Exception e) { DialogResult result = MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK); }
            }
        }
    }
}
