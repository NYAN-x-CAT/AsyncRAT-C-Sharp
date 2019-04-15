namespace AsyncRAT_Sharp
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.listView1 = new System.Windows.Forms.ListView();
            this.lv_ip = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lv_country = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lv_hwid = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lv_user = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lv_os = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lv_version = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cLIENTOPTIONSToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cLOSEToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uPDATEToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uNISTALLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.sENDMESSAGEBOXToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sENDFILEToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sENDFILETOMEMORYToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rEMOTEDESKTOPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.kEYLOGGERToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fILEMANAGERToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pROCESSMANAGERToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bOTKILLERToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uSBSPREADToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.bUILDERToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.ping = new System.Windows.Forms.Timer(this.components);
            this.UpdateUI = new System.Windows.Forms.Timer(this.components);
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.listView2 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.performanceCounter1 = new System.Diagnostics.PerformanceCounter();
            this.performanceCounter2 = new System.Diagnostics.PerformanceCounter();
            this.contextMenuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.performanceCounter1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.performanceCounter2)).BeginInit();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.lv_ip,
            this.lv_country,
            this.lv_hwid,
            this.lv_user,
            this.lv_os,
            this.lv_version});
            this.listView1.ContextMenuStrip = this.contextMenuStrip1;
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.Location = new System.Drawing.Point(3, 3);
            this.listView1.Name = "listView1";
            this.listView1.ShowGroups = false;
            this.listView1.ShowItemToolTips = true;
            this.listView1.Size = new System.Drawing.Size(988, 303);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listView1_KeyDown);
            this.listView1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseMove);
            // 
            // lv_ip
            // 
            this.lv_ip.Text = "IP";
            this.lv_ip.Width = 121;
            // 
            // lv_country
            // 
            this.lv_country.Text = "COUNTRY";
            this.lv_country.Width = 124;
            // 
            // lv_hwid
            // 
            this.lv_hwid.Text = "HWID";
            this.lv_hwid.Width = 117;
            // 
            // lv_user
            // 
            this.lv_user.Text = "USER";
            this.lv_user.Width = 117;
            // 
            // lv_os
            // 
            this.lv_os.Text = "OS";
            this.lv_os.Width = 179;
            // 
            // lv_version
            // 
            this.lv_version.Text = "VERSION";
            this.lv_version.Width = 126;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cLIENTOPTIONSToolStripMenuItem,
            this.toolStripSeparator1,
            this.sENDMESSAGEBOXToolStripMenuItem,
            this.sENDFILEToolStripMenuItem,
            this.sENDFILETOMEMORYToolStripMenuItem,
            this.rEMOTEDESKTOPToolStripMenuItem,
            this.kEYLOGGERToolStripMenuItem,
            this.fILEMANAGERToolStripMenuItem,
            this.pROCESSMANAGERToolStripMenuItem,
            this.bOTKILLERToolStripMenuItem,
            this.uSBSPREADToolStripMenuItem,
            this.toolStripSeparator2,
            this.bUILDERToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.ShowImageMargin = false;
            this.contextMenuStrip1.Size = new System.Drawing.Size(275, 346);
            // 
            // cLIENTOPTIONSToolStripMenuItem
            // 
            this.cLIENTOPTIONSToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cLOSEToolStripMenuItem,
            this.uPDATEToolStripMenuItem,
            this.uNISTALLToolStripMenuItem});
            this.cLIENTOPTIONSToolStripMenuItem.Name = "cLIENTOPTIONSToolStripMenuItem";
            this.cLIENTOPTIONSToolStripMenuItem.Size = new System.Drawing.Size(274, 30);
            this.cLIENTOPTIONSToolStripMenuItem.Text = "[$] CLIENT OPTIONS";
            // 
            // cLOSEToolStripMenuItem
            // 
            this.cLOSEToolStripMenuItem.Name = "cLOSEToolStripMenuItem";
            this.cLOSEToolStripMenuItem.Size = new System.Drawing.Size(173, 30);
            this.cLOSEToolStripMenuItem.Text = "CLOSE";
            this.cLOSEToolStripMenuItem.Click += new System.EventHandler(this.cLOSEToolStripMenuItem_Click);
            // 
            // uPDATEToolStripMenuItem
            // 
            this.uPDATEToolStripMenuItem.Name = "uPDATEToolStripMenuItem";
            this.uPDATEToolStripMenuItem.Size = new System.Drawing.Size(173, 30);
            this.uPDATEToolStripMenuItem.Text = "UPDATE";
            this.uPDATEToolStripMenuItem.Click += new System.EventHandler(this.uPDATEToolStripMenuItem_Click);
            // 
            // uNISTALLToolStripMenuItem
            // 
            this.uNISTALLToolStripMenuItem.Name = "uNISTALLToolStripMenuItem";
            this.uNISTALLToolStripMenuItem.Size = new System.Drawing.Size(173, 30);
            this.uNISTALLToolStripMenuItem.Text = "UNISTALL";
            this.uNISTALLToolStripMenuItem.Click += new System.EventHandler(this.uNISTALLToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(271, 6);
            // 
            // sENDMESSAGEBOXToolStripMenuItem
            // 
            this.sENDMESSAGEBOXToolStripMenuItem.Name = "sENDMESSAGEBOXToolStripMenuItem";
            this.sENDMESSAGEBOXToolStripMenuItem.Size = new System.Drawing.Size(274, 30);
            this.sENDMESSAGEBOXToolStripMenuItem.Text = "[1] SEND MESSAGEBOX";
            this.sENDMESSAGEBOXToolStripMenuItem.Click += new System.EventHandler(this.sENDMESSAGEBOXToolStripMenuItem_Click);
            // 
            // sENDFILEToolStripMenuItem
            // 
            this.sENDFILEToolStripMenuItem.Name = "sENDFILEToolStripMenuItem";
            this.sENDFILEToolStripMenuItem.Size = new System.Drawing.Size(274, 30);
            this.sENDFILEToolStripMenuItem.Text = "[2] SEND FILE TO DISK";
            this.sENDFILEToolStripMenuItem.Click += new System.EventHandler(this.sENDFILEToolStripMenuItem_Click_1);
            // 
            // sENDFILETOMEMORYToolStripMenuItem
            // 
            this.sENDFILETOMEMORYToolStripMenuItem.Name = "sENDFILETOMEMORYToolStripMenuItem";
            this.sENDFILETOMEMORYToolStripMenuItem.Size = new System.Drawing.Size(274, 30);
            this.sENDFILETOMEMORYToolStripMenuItem.Text = "[3] SEND FILE TO MEMORY";
            this.sENDFILETOMEMORYToolStripMenuItem.Click += new System.EventHandler(this.sENDFILETOMEMORYToolStripMenuItem_Click);
            // 
            // rEMOTEDESKTOPToolStripMenuItem
            // 
            this.rEMOTEDESKTOPToolStripMenuItem.Name = "rEMOTEDESKTOPToolStripMenuItem";
            this.rEMOTEDESKTOPToolStripMenuItem.Size = new System.Drawing.Size(274, 30);
            this.rEMOTEDESKTOPToolStripMenuItem.Text = "[4] REMOTE DESKTOP";
            this.rEMOTEDESKTOPToolStripMenuItem.Click += new System.EventHandler(this.rEMOTEDESKTOPToolStripMenuItem_Click);
            // 
            // kEYLOGGERToolStripMenuItem
            // 
            this.kEYLOGGERToolStripMenuItem.Name = "kEYLOGGERToolStripMenuItem";
            this.kEYLOGGERToolStripMenuItem.Size = new System.Drawing.Size(274, 30);
            this.kEYLOGGERToolStripMenuItem.Text = "[5] KEYLOGGER";
            this.kEYLOGGERToolStripMenuItem.Click += new System.EventHandler(this.KEYLOGGERToolStripMenuItem_Click);
            // 
            // fILEMANAGERToolStripMenuItem
            // 
            this.fILEMANAGERToolStripMenuItem.Name = "fILEMANAGERToolStripMenuItem";
            this.fILEMANAGERToolStripMenuItem.Size = new System.Drawing.Size(274, 30);
            this.fILEMANAGERToolStripMenuItem.Text = "[6] FILE MANAGER";
            this.fILEMANAGERToolStripMenuItem.Click += new System.EventHandler(this.fILEMANAGERToolStripMenuItem_Click);
            // 
            // pROCESSMANAGERToolStripMenuItem
            // 
            this.pROCESSMANAGERToolStripMenuItem.Name = "pROCESSMANAGERToolStripMenuItem";
            this.pROCESSMANAGERToolStripMenuItem.Size = new System.Drawing.Size(274, 30);
            this.pROCESSMANAGERToolStripMenuItem.Text = "[7] PROCESS MANAGER";
            this.pROCESSMANAGERToolStripMenuItem.Click += new System.EventHandler(this.pROCESSMANAGERToolStripMenuItem_Click);
            // 
            // bOTKILLERToolStripMenuItem
            // 
            this.bOTKILLERToolStripMenuItem.Name = "bOTKILLERToolStripMenuItem";
            this.bOTKILLERToolStripMenuItem.Size = new System.Drawing.Size(274, 30);
            this.bOTKILLERToolStripMenuItem.Text = "[8] BOT KILLER";
            this.bOTKILLERToolStripMenuItem.Click += new System.EventHandler(this.BOTKILLERToolStripMenuItem_Click);
            // 
            // uSBSPREADToolStripMenuItem
            // 
            this.uSBSPREADToolStripMenuItem.Name = "uSBSPREADToolStripMenuItem";
            this.uSBSPREADToolStripMenuItem.Size = new System.Drawing.Size(274, 30);
            this.uSBSPREADToolStripMenuItem.Text = "[9] USB SPREAD";
            this.uSBSPREADToolStripMenuItem.Click += new System.EventHandler(this.USBSPREADToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(271, 6);
            // 
            // bUILDERToolStripMenuItem
            // 
            this.bUILDERToolStripMenuItem.Name = "bUILDERToolStripMenuItem";
            this.bUILDERToolStripMenuItem.Size = new System.Drawing.Size(274, 30);
            this.bUILDERToolStripMenuItem.Text = "[#] BUILDER";
            this.bUILDERToolStripMenuItem.Click += new System.EventHandler(this.bUILDERToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 342);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1002, 30);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(24, 25);
            this.toolStripStatusLabel1.Text = "...";
            // 
            // ping
            // 
            this.ping.Enabled = true;
            this.ping.Interval = 50000;
            this.ping.Tick += new System.EventHandler(this.ping_Tick);
            // 
            // UpdateUI
            // 
            this.UpdateUI.Enabled = true;
            this.UpdateUI.Interval = 500;
            this.UpdateUI.Tick += new System.EventHandler(this.UpdateUI_Tick);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1002, 342);
            this.tabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.listView1);
            this.tabPage1.Location = new System.Drawing.Point(4, 29);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(994, 309);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Clients";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.listView2);
            this.tabPage2.Location = new System.Drawing.Point(4, 29);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(994, 309);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Logs";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // listView2
            // 
            this.listView2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listView2.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listView2.ContextMenuStrip = this.contextMenuStrip1;
            this.listView2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView2.FullRowSelect = true;
            this.listView2.GridLines = true;
            this.listView2.Location = new System.Drawing.Point(3, 3);
            this.listView2.Name = "listView2";
            this.listView2.ShowGroups = false;
            this.listView2.ShowItemToolTips = true;
            this.listView2.Size = new System.Drawing.Size(988, 303);
            this.listView2.TabIndex = 1;
            this.listView2.UseCompatibleStateImageBehavior = false;
            this.listView2.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Time";
            this.columnHeader1.Width = 150;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Message";
            this.columnHeader2.Width = 500;
            // 
            // performanceCounter1
            // 
            this.performanceCounter1.CategoryName = "Processor";
            this.performanceCounter1.CounterName = "% Processor Time";
            this.performanceCounter1.InstanceName = "_Total";
            // 
            // performanceCounter2
            // 
            this.performanceCounter2.CategoryName = "Memory";
            this.performanceCounter2.CounterName = "% Committed Bytes In Use";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1002, 372);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AsyncRAT-Sharp // NYAN CAT";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.performanceCounter1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.performanceCounter2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader lv_ip;
        private System.Windows.Forms.ColumnHeader lv_user;
        private System.Windows.Forms.ColumnHeader lv_os;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.Timer ping;
        private System.Windows.Forms.Timer UpdateUI;
        private System.Windows.Forms.ToolStripMenuItem cLIENTOPTIONSToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cLOSEToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uPDATEToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uNISTALLToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem sENDMESSAGEBOXToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sENDFILEToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sENDFILETOMEMORYToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rEMOTEDESKTOPToolStripMenuItem;
        public System.Windows.Forms.ColumnHeader lv_hwid;
        private System.Windows.Forms.ToolStripMenuItem pROCESSMANAGERToolStripMenuItem;
        private System.Windows.Forms.ColumnHeader lv_country;
        private System.Windows.Forms.ToolStripMenuItem bUILDERToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ColumnHeader lv_version;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        public System.Windows.Forms.ListView listView2;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ToolStripMenuItem fILEMANAGERToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem kEYLOGGERToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bOTKILLERToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uSBSPREADToolStripMenuItem;
        private System.Diagnostics.PerformanceCounter performanceCounter1;
        private System.Diagnostics.PerformanceCounter performanceCounter2;
    }
}

