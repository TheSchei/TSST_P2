namespace Router
{
    partial class Form1
    {
        /// <summary>
        /// Wymagana zmienna projektanta.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Wyczyść wszystkie używane zasoby.
        /// </summary>
        /// <param name="disposing">prawda, jeżeli zarządzane zasoby powinny zostać zlikwidowane; Fałsz w przeciwnym wypadku.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                router.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Kod generowany przez Projektanta formularzy systemu Windows

        /// <summary>
        /// Metoda wymagana do obsługi projektanta — nie należy modyfikować
        /// jej zawartości w edytorze kodu.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.LogBox = new System.Windows.Forms.TextBox();
            this.Refresher = new System.Windows.Forms.Timer(this.components);
            this.DataView = new System.Windows.Forms.DataGridView();
            this.RefreshButton = new System.Windows.Forms.Button();
            this.EdgeRoutingButton = new System.Windows.Forms.RadioButton();
            this.RoutingButton = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.DataView)).BeginInit();
            this.SuspendLayout();
            // 
            // LogBox
            // 
            this.LogBox.Location = new System.Drawing.Point(16, 15);
            this.LogBox.Margin = new System.Windows.Forms.Padding(4);
            this.LogBox.Multiline = true;
            this.LogBox.Name = "LogBox";
            this.LogBox.ReadOnly = true;
            this.LogBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.LogBox.Size = new System.Drawing.Size(799, 300);
            this.LogBox.TabIndex = 2;
            // 
            // Refresher
            // 
            this.Refresher.Enabled = true;
            this.Refresher.Interval = 300;
            this.Refresher.Tick += new System.EventHandler(this.Refresher_Tick);
            // 
            // DataView
            // 
            this.DataView.AllowUserToAddRows = false;
            this.DataView.AllowUserToDeleteRows = false;
            this.DataView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.DataView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataView.Location = new System.Drawing.Point(16, 338);
            this.DataView.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.DataView.Name = "DataView";
            this.DataView.ReadOnly = true;
            this.DataView.RowTemplate.Height = 24;
            this.DataView.Size = new System.Drawing.Size(799, 131);
            this.DataView.TabIndex = 9;
            // 
            // RefreshButton
            // 
            this.RefreshButton.Location = new System.Drawing.Point(586, 490);
            this.RefreshButton.Name = "RefreshButton";
            this.RefreshButton.Size = new System.Drawing.Size(229, 41);
            this.RefreshButton.TabIndex = 10;
            this.RefreshButton.Text = "Refresh";
            this.RefreshButton.UseVisualStyleBackColor = true;
            this.RefreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
            // 
            // EdgeRoutingButton
            // 
            this.EdgeRoutingButton.AutoSize = true;
            this.EdgeRoutingButton.Location = new System.Drawing.Point(64, 500);
            this.EdgeRoutingButton.Name = "EdgeRoutingButton";
            this.EdgeRoutingButton.Size = new System.Drawing.Size(147, 21);
            this.EdgeRoutingButton.TabIndex = 12;
            this.EdgeRoutingButton.TabStop = true;
            this.EdgeRoutingButton.Text = "EdgeRoutingTable";
            this.EdgeRoutingButton.UseVisualStyleBackColor = true;
            // 
            // RoutingButton
            // 
            this.RoutingButton.AutoSize = true;
            this.RoutingButton.Location = new System.Drawing.Point(331, 500);
            this.RoutingButton.Name = "RoutingButton";
            this.RoutingButton.Size = new System.Drawing.Size(114, 21);
            this.RoutingButton.TabIndex = 13;
            this.RoutingButton.TabStop = true;
            this.RoutingButton.Text = "RoutingTable";
            this.RoutingButton.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(832, 543);
            this.Controls.Add(this.RoutingButton);
            this.Controls.Add(this.EdgeRoutingButton);
            this.Controls.Add(this.RefreshButton);
            this.Controls.Add(this.DataView);
            this.Controls.Add(this.LogBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.DataView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox LogBox;
        private System.Windows.Forms.Timer Refresher;
        private System.Windows.Forms.DataGridView DataView;
        private System.Windows.Forms.Button RefreshButton;
        private System.Windows.Forms.RadioButton EdgeRoutingButton;
        private System.Windows.Forms.RadioButton RoutingButton;
    }
}

