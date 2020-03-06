namespace Host
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
                host.Dispose();
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
            this.InfoBox = new System.Windows.Forms.TextBox();
            this.LogBox = new System.Windows.Forms.TextBox();
            this.MessageTextBox = new System.Windows.Forms.TextBox();
            this.SendButton = new System.Windows.Forms.Button();
            this.Refresher = new System.Windows.Forms.Timer(this.components);
            this.DestinationSelector = new System.Windows.Forms.ComboBox();
            this.BandBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.StartButton = new System.Windows.Forms.Button();
            this.EditButton = new System.Windows.Forms.Button();
            this.TerminateButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // InfoBox
            // 
            this.InfoBox.Location = new System.Drawing.Point(12, 12);
            this.InfoBox.Name = "InfoBox";
            this.InfoBox.ReadOnly = true;
            this.InfoBox.Size = new System.Drawing.Size(547, 20);
            this.InfoBox.TabIndex = 0;
            // 
            // LogBox
            // 
            this.LogBox.Location = new System.Drawing.Point(13, 40);
            this.LogBox.Multiline = true;
            this.LogBox.Name = "LogBox";
            this.LogBox.ReadOnly = true;
            this.LogBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.LogBox.Size = new System.Drawing.Size(546, 333);
            this.LogBox.TabIndex = 1;
            // 
            // MessageTextBox
            // 
            this.MessageTextBox.Location = new System.Drawing.Point(12, 379);
            this.MessageTextBox.Multiline = true;
            this.MessageTextBox.Name = "MessageTextBox";
            this.MessageTextBox.Size = new System.Drawing.Size(216, 73);
            this.MessageTextBox.TabIndex = 2;
            // 
            // SendButton
            // 
            this.SendButton.Enabled = false;
            this.SendButton.Location = new System.Drawing.Point(234, 379);
            this.SendButton.Name = "SendButton";
            this.SendButton.Size = new System.Drawing.Size(73, 73);
            this.SendButton.TabIndex = 3;
            this.SendButton.Text = "Send";
            this.SendButton.UseVisualStyleBackColor = true;
            this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
            // 
            // Refresher
            // 
            this.Refresher.Enabled = true;
            this.Refresher.Interval = 300;
            this.Refresher.Tick += new System.EventHandler(this.Refresher_Tick);
            // 
            // DestinationSelector
            // 
            this.DestinationSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DestinationSelector.FormattingEnabled = true;
            this.DestinationSelector.Location = new System.Drawing.Point(313, 401);
            this.DestinationSelector.Name = "DestinationSelector";
            this.DestinationSelector.Size = new System.Drawing.Size(121, 21);
            this.DestinationSelector.TabIndex = 4;
            // 
            // BandBox
            // 
            this.BandBox.Location = new System.Drawing.Point(440, 401);
            this.BandBox.Name = "BandBox";
            this.BandBox.Size = new System.Drawing.Size(121, 20);
            this.BandBox.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(314, 380);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Destination";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(437, 380);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Band Width [Mbps]";
            // 
            // StartButton
            // 
            this.StartButton.Location = new System.Drawing.Point(313, 429);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(80, 23);
            this.StartButton.TabIndex = 8;
            this.StartButton.Text = "Start";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // EditButton
            // 
            this.EditButton.Enabled = false;
            this.EditButton.Location = new System.Drawing.Point(395, 429);
            this.EditButton.Name = "EditButton";
            this.EditButton.Size = new System.Drawing.Size(80, 23);
            this.EditButton.TabIndex = 9;
            this.EditButton.Text = "Edit";
            this.EditButton.UseVisualStyleBackColor = true;
            this.EditButton.Click += new System.EventHandler(this.EditButton_Click);
            // 
            // TerminateButton
            // 
            this.TerminateButton.Enabled = false;
            this.TerminateButton.Location = new System.Drawing.Point(479, 429);
            this.TerminateButton.Name = "TerminateButton";
            this.TerminateButton.Size = new System.Drawing.Size(80, 23);
            this.TerminateButton.TabIndex = 10;
            this.TerminateButton.Text = "Terminate";
            this.TerminateButton.UseVisualStyleBackColor = true;
            this.TerminateButton.Click += new System.EventHandler(this.TerminateButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(571, 457);
            this.Controls.Add(this.TerminateButton);
            this.Controls.Add(this.EditButton);
            this.Controls.Add(this.StartButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.BandBox);
            this.Controls.Add(this.DestinationSelector);
            this.Controls.Add(this.SendButton);
            this.Controls.Add(this.MessageTextBox);
            this.Controls.Add(this.LogBox);
            this.Controls.Add(this.InfoBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Form2";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox InfoBox;
        private System.Windows.Forms.TextBox LogBox;
        private System.Windows.Forms.TextBox MessageTextBox;
        private System.Windows.Forms.Button SendButton;
        private System.Windows.Forms.Timer Refresher;
        private System.Windows.Forms.ComboBox DestinationSelector;
        private System.Windows.Forms.TextBox BandBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.Button EditButton;
        private System.Windows.Forms.Button TerminateButton;
    }
}

