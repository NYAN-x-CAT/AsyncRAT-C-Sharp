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
            this.chkPastebin = new System.Windows.Forms.CheckBox();
            this.txtPastebin = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.comboBoxFolder = new System.Windows.Forms.ComboBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.textFilename = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.chkBdos = new System.Windows.Forms.CheckBox();
            this.txtMutex = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.chkAnti = new System.Windows.Forms.CheckBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.textPort = new System.Windows.Forms.TextBox();
            this.listBoxPort = new System.Windows.Forms.ListBox();
            this.btnAddPort = new System.Windows.Forms.Button();
            this.btnRemovePort = new System.Windows.Forms.Button();
            this.btnRemoveIP = new System.Windows.Forms.Button();
            this.btnAddIP = new System.Windows.Forms.Button();
            this.listBoxIP = new System.Windows.Forms.ListBox();
            this.textIP = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(3, 313);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(539, 49);
            this.button1.TabIndex = 0;
            this.button1.Text = "Build";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnRemoveIP);
            this.groupBox1.Controls.Add(this.btnAddIP);
            this.groupBox1.Controls.Add(this.listBoxIP);
            this.groupBox1.Controls.Add(this.textIP);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.btnRemovePort);
            this.groupBox1.Controls.Add(this.btnAddPort);
            this.groupBox1.Controls.Add(this.listBoxPort);
            this.groupBox1.Controls.Add(this.chkPastebin);
            this.groupBox1.Controls.Add(this.txtPastebin);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.textPort);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(6, 19);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(539, 363);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Connection";
            // 
            // chkPastebin
            // 
            this.chkPastebin.AutoSize = true;
            this.chkPastebin.Location = new System.Drawing.Point(264, 261);
            this.chkPastebin.Name = "chkPastebin";
            this.chkPastebin.Size = new System.Drawing.Size(130, 24);
            this.chkPastebin.TabIndex = 9;
            this.chkPastebin.Text = "Use Pastebin";
            this.toolTip1.SetToolTip(this.chkPastebin, "IP:PORT .. Example 127.0.0.1:6606");
            this.chkPastebin.UseVisualStyleBackColor = true;
            this.chkPastebin.CheckedChanged += new System.EventHandler(this.CheckBox2_CheckedChanged);
            // 
            // txtPastebin
            // 
            this.txtPastebin.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::AsyncRAT_Sharp.Properties.Settings.Default, "Pastebin", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.txtPastebin.Enabled = false;
            this.txtPastebin.Location = new System.Drawing.Point(106, 296);
            this.txtPastebin.Name = "txtPastebin";
            this.txtPastebin.Size = new System.Drawing.Size(271, 26);
            this.txtPastebin.TabIndex = 8;
            this.txtPastebin.Text = global::AsyncRAT_Sharp.Properties.Settings.Default.Pastebin;
            this.toolTip1.SetToolTip(this.txtPastebin, "IP:PORT .. Example 127.0.0.1:6606");
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(14, 299);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(71, 20);
            this.label6.TabIndex = 7;
            this.label6.Text = "Pastebin";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(308, 37);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 20);
            this.label2.TabIndex = 3;
            this.label2.Text = "Port";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.comboBoxFolder);
            this.groupBox2.Controls.Add(this.checkBox1);
            this.groupBox2.Controls.Add(this.textFilename);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Location = new System.Drawing.Point(6, 17);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(536, 365);
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
            this.comboBoxFolder.Location = new System.Drawing.Point(106, 149);
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
            this.label3.Location = new System.Drawing.Point(14, 152);
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
            this.groupBox3.Controls.Add(this.pictureBox1);
            this.groupBox3.Controls.Add(this.chkBdos);
            this.groupBox3.Controls.Add(this.txtMutex);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.chkAnti);
            this.groupBox3.Location = new System.Drawing.Point(6, 17);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(536, 279);
            this.groupBox3.TabIndex = 9;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "MISC";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::AsyncRAT_Sharp.Properties.Resources.uac;
            this.pictureBox1.Location = new System.Drawing.Point(181, 74);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(32, 32);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 13;
            this.pictureBox1.TabStop = false;
            // 
            // chkBdos
            // 
            this.chkBdos.AutoSize = true;
            this.chkBdos.Location = new System.Drawing.Point(21, 82);
            this.chkBdos.Name = "chkBdos";
            this.chkBdos.Size = new System.Drawing.Size(143, 24);
            this.chkBdos.TabIndex = 12;
            this.chkBdos.Text = "Process Critical";
            this.chkBdos.UseVisualStyleBackColor = true;
            // 
            // txtMutex
            // 
            this.txtMutex.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::AsyncRAT_Sharp.Properties.Settings.Default, "Mutex", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.txtMutex.Location = new System.Drawing.Point(106, 165);
            this.txtMutex.Name = "txtMutex";
            this.txtMutex.Size = new System.Drawing.Size(271, 26);
            this.txtMutex.TabIndex = 11;
            this.txtMutex.Text = global::AsyncRAT_Sharp.Properties.Settings.Default.Mutex;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(14, 169);
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
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(556, 421);
            this.tabControl1.TabIndex = 10;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Location = new System.Drawing.Point(4, 29);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(548, 388);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Connection";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.groupBox2);
            this.tabPage2.Location = new System.Drawing.Point(4, 29);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(548, 388);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Install";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.groupBox3);
            this.tabPage3.Controls.Add(this.button1);
            this.tabPage3.Location = new System.Drawing.Point(4, 29);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(548, 388);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Misc";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // textPort
            // 
            this.textPort.Location = new System.Drawing.Point(362, 34);
            this.textPort.Name = "textPort";
            this.textPort.Size = new System.Drawing.Size(161, 26);
            this.textPort.TabIndex = 6;
            // 
            // listBoxPort
            // 
            this.listBoxPort.FormattingEnabled = true;
            this.listBoxPort.ItemHeight = 20;
            this.listBoxPort.Location = new System.Drawing.Point(362, 66);
            this.listBoxPort.Name = "listBoxPort";
            this.listBoxPort.Size = new System.Drawing.Size(161, 84);
            this.listBoxPort.TabIndex = 10;
            // 
            // btnAddPort
            // 
            this.btnAddPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddPort.Location = new System.Drawing.Point(362, 156);
            this.btnAddPort.Name = "btnAddPort";
            this.btnAddPort.Size = new System.Drawing.Size(43, 26);
            this.btnAddPort.TabIndex = 12;
            this.btnAddPort.Text = "+";
            this.btnAddPort.UseVisualStyleBackColor = true;
            this.btnAddPort.Click += new System.EventHandler(this.BtnAddPort_Click);
            // 
            // btnRemovePort
            // 
            this.btnRemovePort.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRemovePort.Location = new System.Drawing.Point(480, 156);
            this.btnRemovePort.Name = "btnRemovePort";
            this.btnRemovePort.Size = new System.Drawing.Size(43, 26);
            this.btnRemovePort.TabIndex = 13;
            this.btnRemovePort.Text = "-";
            this.btnRemovePort.UseVisualStyleBackColor = true;
            this.btnRemovePort.Click += new System.EventHandler(this.BtnRemovePort_Click);
            // 
            // btnRemoveIP
            // 
            this.btnRemoveIP.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRemoveIP.Location = new System.Drawing.Point(190, 156);
            this.btnRemoveIP.Name = "btnRemoveIP";
            this.btnRemoveIP.Size = new System.Drawing.Size(43, 26);
            this.btnRemoveIP.TabIndex = 18;
            this.btnRemoveIP.Text = "-";
            this.btnRemoveIP.UseVisualStyleBackColor = true;
            this.btnRemoveIP.Click += new System.EventHandler(this.BtnRemoveIP_Click);
            // 
            // btnAddIP
            // 
            this.btnAddIP.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddIP.Location = new System.Drawing.Point(72, 156);
            this.btnAddIP.Name = "btnAddIP";
            this.btnAddIP.Size = new System.Drawing.Size(43, 26);
            this.btnAddIP.TabIndex = 17;
            this.btnAddIP.Text = "+";
            this.btnAddIP.UseVisualStyleBackColor = true;
            this.btnAddIP.Click += new System.EventHandler(this.BtnAddIP_Click);
            // 
            // listBoxIP
            // 
            this.listBoxIP.FormattingEnabled = true;
            this.listBoxIP.ItemHeight = 20;
            this.listBoxIP.Location = new System.Drawing.Point(72, 66);
            this.listBoxIP.Name = "listBoxIP";
            this.listBoxIP.Size = new System.Drawing.Size(161, 84);
            this.listBoxIP.TabIndex = 16;
            // 
            // textIP
            // 
            this.textIP.Location = new System.Drawing.Point(72, 34);
            this.textIP.Name = "textIP";
            this.textIP.Size = new System.Drawing.Size(161, 26);
            this.textIP.TabIndex = 15;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 20);
            this.label1.TabIndex = 14;
            this.label1.Text = "DNS";
            // 
            // FormBuilder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(580, 445);
            this.Controls.Add(this.tabControl1);
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
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
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
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.CheckBox chkBdos;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ListBox listBoxPort;
        private System.Windows.Forms.TextBox textPort;
        private System.Windows.Forms.Button btnAddPort;
        private System.Windows.Forms.Button btnRemovePort;
        private System.Windows.Forms.Button btnRemoveIP;
        private System.Windows.Forms.Button btnAddIP;
        private System.Windows.Forms.ListBox listBoxIP;
        private System.Windows.Forms.TextBox textIP;
        private System.Windows.Forms.Label label1;
    }
}
