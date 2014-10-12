namespace easyGateGunnebo
{
    partial class easyGate
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(easyGate));
            this.labelENG = new System.Windows.Forms.Label();
            this.labelITA = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panelErrorBarcodeReader = new System.Windows.Forms.Panel();
            this.panelErrorGunnabo = new System.Windows.Forms.Panel();
            this.panelClose = new System.Windows.Forms.Panel();
            this.panelReset = new System.Windows.Forms.Panel();
            this.labelBusy = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // labelENG
            // 
            this.labelENG.AutoSize = true;
            this.labelENG.Font = new System.Drawing.Font("Arial", 32.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelENG.Location = new System.Drawing.Point(4, 150);
            this.labelENG.Name = "labelENG";
            this.labelENG.Size = new System.Drawing.Size(304, 49);
            this.labelENG.TabIndex = 0;
            this.labelENG.Text = "Boarding Pass";
            this.labelENG.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelITA
            // 
            this.labelITA.AutoSize = true;
            this.labelITA.Font = new System.Drawing.Font("Arial", 32.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelITA.Location = new System.Drawing.Point(2, 65);
            this.labelITA.Name = "labelITA";
            this.labelITA.Size = new System.Drawing.Size(327, 49);
            this.labelITA.TabIndex = 0;
            this.labelITA.Text = "Carta d\'Imbarco";
            this.labelITA.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(95, 306);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(286, 262);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // panelErrorBarcodeReader
            // 
            this.panelErrorBarcodeReader.BackColor = System.Drawing.Color.Lime;
            this.panelErrorBarcodeReader.Location = new System.Drawing.Point(0, 760);
            this.panelErrorBarcodeReader.Name = "panelErrorBarcodeReader";
            this.panelErrorBarcodeReader.Size = new System.Drawing.Size(46, 42);
            this.panelErrorBarcodeReader.TabIndex = 3;
            this.panelErrorBarcodeReader.Click += new System.EventHandler(this.panelErrorBarcodeReader_Click);
            // 
            // panelErrorGunnabo
            // 
            this.panelErrorGunnabo.BackColor = System.Drawing.Color.Lime;
            this.panelErrorGunnabo.Location = new System.Drawing.Point(52, 760);
            this.panelErrorGunnabo.Name = "panelErrorGunnabo";
            this.panelErrorGunnabo.Size = new System.Drawing.Size(46, 42);
            this.panelErrorGunnabo.TabIndex = 3;
            this.panelErrorGunnabo.Click += new System.EventHandler(this.panelErrorGunnabo_Click);
            // 
            // panelClose
            // 
            this.panelClose.BackColor = System.Drawing.Color.Lime;
            this.panelClose.Location = new System.Drawing.Point(437, 0);
            this.panelClose.Name = "panelClose";
            this.panelClose.Size = new System.Drawing.Size(42, 36);
            this.panelClose.TabIndex = 4;
            this.panelClose.Click += new System.EventHandler(this.panelClose_Click);
            // 
            // panelReset
            // 
            this.panelReset.BackColor = System.Drawing.Color.Lime;
            this.panelReset.Location = new System.Drawing.Point(0, -1);
            this.panelReset.Name = "panelReset";
            this.panelReset.Size = new System.Drawing.Size(46, 36);
            this.panelReset.TabIndex = 5;
            this.panelReset.Click += new System.EventHandler(this.panelReset_Click);
            // 
            // labelBusy
            // 
            this.labelBusy.AutoSize = true;
            this.labelBusy.Location = new System.Drawing.Point(220, 9);
            this.labelBusy.Name = "labelBusy";
            this.labelBusy.Size = new System.Drawing.Size(0, 13);
            this.labelBusy.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(32, 616);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "label3";
            this.label3.Visible = false;
            // 
            // easyGate
            // 
            this.AccessibleRole = System.Windows.Forms.AccessibleRole.TitleBar;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Lime;
            this.ClientSize = new System.Drawing.Size(480, 800);
            this.Controls.Add(this.labelBusy);
            this.Controls.Add(this.panelReset);
            this.Controls.Add(this.panelClose);
            this.Controls.Add(this.panelErrorGunnabo);
            this.Controls.Add(this.panelErrorBarcodeReader);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.labelITA);
            this.Controls.Add(this.labelENG);
            this.Cursor = System.Windows.Forms.Cursors.Cross;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "easyGate";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "easyGate";
            this.TopMost = true;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.easyGate_FormClosed);
            this.DoubleClick += new System.EventHandler(this.easyGate_DoubleClick);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelENG;
        private System.Windows.Forms.Label labelITA;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel panelErrorBarcodeReader;
        private System.Windows.Forms.Panel panelErrorGunnabo;
        private System.Windows.Forms.Panel panelClose;
        private System.Windows.Forms.Panel panelReset;
        private System.Windows.Forms.Label labelBusy;
        private System.Windows.Forms.Label label3;
    }
}

