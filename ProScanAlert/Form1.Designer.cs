namespace ProScanAlert
{
    partial class Form1
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
            this.label1 = new System.Windows.Forms.Label();
            this.tbServerHost = new System.Windows.Forms.TextBox();
            this.tbServerPort = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btConnect = new System.Windows.Forms.Button();
            this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            this.btRetrieveSystems = new System.Windows.Forms.Button();
            this.lblLine1 = new System.Windows.Forms.Label();
            this.lblLine2 = new System.Windows.Forms.Label();
            this.lblLine3 = new System.Windows.Forms.Label();
            this.lblLine4 = new System.Windows.Forms.Label();
            this.lblLine5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tbLocalPort = new System.Windows.Forms.TextBox();
            this.btServerStart = new System.Windows.Forms.Button();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbPassword = new System.Windows.Forms.TextBox();
            this.listBox2 = new System.Windows.Forms.ListBox();
            this.tbMins = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbSecs = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "ProScan Host";
            // 
            // tbServerHost
            // 
            this.tbServerHost.Location = new System.Drawing.Point(91, 6);
            this.tbServerHost.Name = "tbServerHost";
            this.tbServerHost.Size = new System.Drawing.Size(116, 20);
            this.tbServerHost.TabIndex = 0;
            // 
            // tbServerPort
            // 
            this.tbServerPort.Location = new System.Drawing.Point(354, 6);
            this.tbServerPort.Name = "tbServerPort";
            this.tbServerPort.Size = new System.Drawing.Size(44, 20);
            this.tbServerPort.TabIndex = 2;
            this.tbServerPort.Text = "5000";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(322, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Port";
            // 
            // btConnect
            // 
            this.btConnect.Location = new System.Drawing.Point(404, 4);
            this.btConnect.Name = "btConnect";
            this.btConnect.Size = new System.Drawing.Size(60, 23);
            this.btConnect.TabIndex = 3;
            this.btConnect.Text = "Connect";
            this.btConnect.UseVisualStyleBackColor = true;
            this.btConnect.Click += new System.EventHandler(this.btConnect_Click);
            // 
            // checkedListBox1
            // 
            this.checkedListBox1.FormattingEnabled = true;
            this.checkedListBox1.Location = new System.Drawing.Point(12, 77);
            this.checkedListBox1.Name = "checkedListBox1";
            this.checkedListBox1.Size = new System.Drawing.Size(304, 184);
            this.checkedListBox1.TabIndex = 6;
            // 
            // btRetrieveSystems
            // 
            this.btRetrieveSystems.Location = new System.Drawing.Point(227, 267);
            this.btRetrieveSystems.Name = "btRetrieveSystems";
            this.btRetrieveSystems.Size = new System.Drawing.Size(89, 23);
            this.btRetrieveSystems.TabIndex = 9;
            this.btRetrieveSystems.Text = "Get Database";
            this.btRetrieveSystems.UseVisualStyleBackColor = true;
            this.btRetrieveSystems.Click += new System.EventHandler(this.btRetrieveSystems_Click);
            // 
            // lblLine1
            // 
            this.lblLine1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblLine1.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLine1.Location = new System.Drawing.Point(322, 77);
            this.lblLine1.Name = "lblLine1";
            this.lblLine1.Size = new System.Drawing.Size(142, 18);
            this.lblLine1.TabIndex = 8;
            this.lblLine1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblLine2
            // 
            this.lblLine2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblLine2.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLine2.Location = new System.Drawing.Point(322, 95);
            this.lblLine2.Name = "lblLine2";
            this.lblLine2.Size = new System.Drawing.Size(142, 18);
            this.lblLine2.TabIndex = 9;
            this.lblLine2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblLine3
            // 
            this.lblLine3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblLine3.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLine3.Location = new System.Drawing.Point(322, 113);
            this.lblLine3.Name = "lblLine3";
            this.lblLine3.Size = new System.Drawing.Size(142, 18);
            this.lblLine3.TabIndex = 10;
            this.lblLine3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblLine4
            // 
            this.lblLine4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblLine4.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLine4.Location = new System.Drawing.Point(322, 131);
            this.lblLine4.Name = "lblLine4";
            this.lblLine4.Size = new System.Drawing.Size(142, 18);
            this.lblLine4.TabIndex = 11;
            this.lblLine4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblLine5
            // 
            this.lblLine5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblLine5.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLine5.Location = new System.Drawing.Point(322, 149);
            this.lblLine5.Name = "lblLine5";
            this.lblLine5.Size = new System.Drawing.Size(142, 18);
            this.lblLine5.TabIndex = 12;
            this.lblLine5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(259, 31);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 13);
            this.label3.TabIndex = 14;
            this.label3.Text = "Local Server Port";
            // 
            // tbLocalPort
            // 
            this.tbLocalPort.Location = new System.Drawing.Point(354, 28);
            this.tbLocalPort.Name = "tbLocalPort";
            this.tbLocalPort.ReadOnly = true;
            this.tbLocalPort.Size = new System.Drawing.Size(44, 20);
            this.tbLocalPort.TabIndex = 6;
            this.tbLocalPort.Text = "15000";
            // 
            // btServerStart
            // 
            this.btServerStart.Location = new System.Drawing.Point(404, 26);
            this.btServerStart.Name = "btServerStart";
            this.btServerStart.Size = new System.Drawing.Size(60, 23);
            this.btServerStart.TabIndex = 7;
            this.btServerStart.Text = "Start";
            this.btServerStart.UseVisualStyleBackColor = true;
            this.btServerStart.Click += new System.EventHandler(this.btServerStart_Click);
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.HorizontalScrollbar = true;
            this.listBox1.Location = new System.Drawing.Point(12, 299);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(452, 95);
            this.listBox1.TabIndex = 17;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(213, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(30, 13);
            this.label4.TabIndex = 18;
            this.label4.Text = "Pass";
            // 
            // tbPassword
            // 
            this.tbPassword.Location = new System.Drawing.Point(249, 6);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.PasswordChar = '*';
            this.tbPassword.Size = new System.Drawing.Size(67, 20);
            this.tbPassword.TabIndex = 1;
            // 
            // listBox2
            // 
            this.listBox2.FormattingEnabled = true;
            this.listBox2.HorizontalScrollbar = true;
            this.listBox2.Location = new System.Drawing.Point(322, 168);
            this.listBox2.Name = "listBox2";
            this.listBox2.Size = new System.Drawing.Size(142, 121);
            this.listBox2.TabIndex = 21;
            // 
            // tbMins
            // 
            this.tbMins.Location = new System.Drawing.Point(119, 28);
            this.tbMins.Name = "tbMins";
            this.tbMins.Size = new System.Drawing.Size(30, 20);
            this.tbMins.TabIndex = 4;
            this.tbMins.Text = "60";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 31);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(101, 13);
            this.label5.TabIndex = 23;
            this.label5.Text = "Mins between alerts";
            // 
            // tbSecs
            // 
            this.tbSecs.Location = new System.Drawing.Point(119, 51);
            this.tbSecs.Name = "tbSecs";
            this.tbSecs.Size = new System.Drawing.Size(30, 20);
            this.tbSecs.TabIndex = 5;
            this.tbSecs.Text = "30";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 54);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(92, 13);
            this.label6.TabIndex = 25;
            this.label6.Text = "Secs total for alert";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 266);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 8;
            this.button1.Text = "Test";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(476, 402);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.tbSecs);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tbMins);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.listBox2);
            this.Controls.Add(this.tbPassword);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.btServerStart);
            this.Controls.Add(this.tbLocalPort);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblLine5);
            this.Controls.Add(this.lblLine4);
            this.Controls.Add(this.lblLine3);
            this.Controls.Add(this.lblLine2);
            this.Controls.Add(this.lblLine1);
            this.Controls.Add(this.btRetrieveSystems);
            this.Controls.Add(this.checkedListBox1);
            this.Controls.Add(this.btConnect);
            this.Controls.Add(this.tbServerPort);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbServerHost);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ProScanAlert 0.1.7";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbServerHost;
        private System.Windows.Forms.TextBox tbServerPort;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btConnect;
        private System.Windows.Forms.CheckedListBox checkedListBox1;
        private System.Windows.Forms.Button btRetrieveSystems;
        private System.Windows.Forms.Label lblLine1;
        private System.Windows.Forms.Label lblLine2;
        private System.Windows.Forms.Label lblLine3;
        private System.Windows.Forms.Label lblLine4;
        private System.Windows.Forms.Label lblLine5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbLocalPort;
        private System.Windows.Forms.Button btServerStart;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbPassword;
        private System.Windows.Forms.ListBox listBox2;
        private System.Windows.Forms.TextBox tbMins;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbSecs;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button button1;
    }
}

