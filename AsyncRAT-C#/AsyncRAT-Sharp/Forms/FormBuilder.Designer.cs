namespace AsyncRAT_Sharp.Forms
{
    partial class FormBuilder
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormBuilder));
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textPort = new System.Windows.Forms.TextBox();
            this.textIP = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.comboBoxFolder = new System.Windows.Forms.ComboBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.textFilename = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.txtMutex = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.chkAnti = new System.Windows.Forms.CheckBox();
            this.txtPastebin = new System.Windows.Forms.TextBox();
            this.chkPastebin = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(10, 818);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(446, 50);
            this.button1.TabIndex = 0;
            this.button1.Text = "Build";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkPastebin);
            this.groupBox1.Controls.Add(this.txtPastebin);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.textPort);
            this.groupBox1.Controls.Add(this.textIP);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(16, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(440, 281);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Connection";
            // 
            // textPort
            // 
            this.textPort.Location = new System.Drawing.Point(106, 99);
            this.textPort.Name = "textPort";
            this.textPort.Size = new System.Drawing.Size(271, 26);
            this.textPort.TabIndex = 6;
            // 
            // textIP
            // 
            this.textIP.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::AsyncRAT_Sharp.Properties.Settings.Default, "IP", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.textIP.Location = new System.Drawing.Point(106, 41);
            this.textIP.Name = "textIP";
            this.textIP.Size = new System.Drawing.Size(271, 26);
            this.textIP.TabIndex = 5;
            this.textIP.Text = global::AsyncRAT_Sharp.Properties.Settings.Default.IP;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 102);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 20);
            this.label2.TabIndex = 3;
            this.label2.Text = "Port";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(24, 20);
            this.label1.TabIndex = 4;
            this.label1.Text = "IP";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.comboBoxFolder);
            this.groupBox2.Controls.Add(this.checkBox1);
            this.groupBox2.Controls.Add(this.textFilename);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Location = new System.Drawing.Point(16, 329);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(440, 211);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Install";
            // 
            // comboBoxFolder
            // 
            this.comboBoxFolder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFolder.Enabled = false;
            this.comboBoxFolder.FormattingEnabled = true;
            this.comboBoxFolder.Items.AddRange(new object[] {
            "%AppData%",
            "%Temp%"});
            this.comboBoxFolder.Location = new System.Drawing.Point(106, 150);
            this.comboBoxFolder.Name = "comboBoxFolder";
            this.comboBoxFolder.Size = new System.Drawing.Size(271, 28);
            this.comboBoxFolder.TabIndex = 8;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(18, 40);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(67, 24);
            this.checkBox1.TabIndex = 7;
            this.checkBox1.Text = "OFF";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // textFilename
            // 
            this.textFilename.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::AsyncRAT_Sharp.Properties.Settings.Default, "Filename", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.textFilename.Enabled = false;
            this.textFilename.Location = new System.Drawing.Point(106, 92);
            this.textFilename.Name = "textFilename";
            this.textFilename.Size = new System.Drawing.Size(271, 26);
            this.textFilename.TabIndex = 5;
            this.textFilename.Text = global::AsyncRAT_Sharp.Properties.Settings.Default.Filename;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 153);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 20);
            this.label3.TabIndex = 3;
            this.label3.Text = "Folder";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 95);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 20);
            this.label4.TabIndex = 4;
            this.label4.Text = "Filename";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.txtMutex);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.chkAnti);
            this.groupBox3.Location = new System.Drawing.Point(16, 581);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(440, 174);
            this.groupBox3.TabIndex = 9;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "MISC";
            // 
            // txtMutex
            // 
            this.txtMutex.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::AsyncRAT_Sharp.Properties.Settings.Default, "Mutex", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.txtMutex.Location = new System.Drawing.Point(106, 99);
            this.txtMutex.Name = "txtMutex";
            this.txtMutex.Size = new System.Drawing.Size(271, 26);
            this.txtMutex.TabIndex = 11;
            this.txtMutex.Text = global::AsyncRAT_Sharp.Properties.Settings.Default.Mutex;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(14, 102);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(52, 20);
            this.label5.TabIndex = 10;
            this.label5.Text = "Mutex";
            // 
            // chkAnti
            // 
            this.chkAnti.AutoSize = true;
            this.chkAnti.Location = new System.Drawing.Point(21, 40);
            this.chkAnti.Name = "chkAnti";
            this.chkAnti.Size = new System.Drawing.Size(125, 24);
            this.chkAnti.TabIndex = 9;
            this.chkAnti.Text = "Anti Analysis";
            this.chkAnti.UseVisualStyleBackColor = true;
            // 
            // txtPastebin
            // 
            this.txtPastebin.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::AsyncRAT_Sharp.Properties.Settings.Default, "Pastebin", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.txtPastebin.Enabled = false;
            this.txtPastebin.Location = new System.Drawing.Point(106, 200);
            this.txtPastebin.Name = "txtPastebin";
            this.txtPastebin.Size = new System.Drawing.Size(271, 26);
            this.txtPastebin.TabIndex = 8;
            this.txtPastebin.Text = global::AsyncRAT_Sharp.Properties.Settings.Default.Pastebin;
            this.toolTip1.SetToolTip(this.txtPastebin, "IP:PORT .. Example 127.0.0.1:6606");
            // 
            // chkPastebin
            // 
            this.chkPastebin.AutoSize = true;
            this.chkPastebin.Location = new System.Drawing.Point(264, 164);
            this.chkPastebin.Name = "chkPastebin";
            this.chkPastebin.Size = new System.Drawing.Size(130, 24);
            this.chkPastebin.TabIndex = 9;
            this.chkPastebin.Text = "Use Pastebin";
            this.toolTip1.SetToolTip(this.chkPastebin, "IP:PORT .. Example 127.0.0.1:6606");
            this.chkPastebin.UseVisualStyleBackColor = true;
            this.chkPastebin.CheckedChanged += new System.EventHandler(this.CheckBox2_CheckedChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(14, 203);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(71, 20);
            this.label6.TabIndex = 7;
            this.label6.Text = "Pastebin";
            // 
            // FormBuilder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(470, 893);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormBuilder";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Builder";
            this.Load += new System.EventHandler(this.Builder_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textPort;
        private System.Windows.Forms.TextBox textIP;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox textFilename;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.ComboBox comboBoxFolder;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox chkAnti;
        private System.Windows.Forms.TextBox txtMutex;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtPastebin;
        private System.Windows.Forms.CheckBox chkPastebin;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}