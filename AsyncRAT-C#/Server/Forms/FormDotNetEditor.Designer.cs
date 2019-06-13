namespace Server.Forms
{
    partial class FormDotNetEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDotNetEditor));
            this.panel1 = new System.Windows.Forms.Panel();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.comboHelper = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.listBoxReferences = new System.Windows.Forms.ListBox();
            this.menuReferences = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.comboLang = new System.Windows.Forms.ComboBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.txtBox = new FastColoredTextBoxNS.FastColoredTextBox();
            this.panel1.SuspendLayout();
            this.menuReferences.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtBox)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.comboHelper);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.listBoxReferences);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.comboLang);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(914, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(215, 633);
            this.panel1.TabIndex = 0;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(375, 6);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(95, 43);
            this.button3.TabIndex = 8;
            this.button3.Text = "Test";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.Button3_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(148, 6);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(95, 43);
            this.button2.TabIndex = 7;
            this.button2.Text = "Reset";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.Button2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(606, 6);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(95, 43);
            this.button1.TabIndex = 6;
            this.button1.Text = "Execute";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 522);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 20);
            this.label3.TabIndex = 5;
            this.label3.Text = "Helper:";
            // 
            // comboHelper
            // 
            this.comboHelper.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboHelper.FormattingEnabled = true;
            this.comboHelper.Items.AddRange(new object[] {
            "Download and execute",
            "Hello world"});
            this.comboHelper.Location = new System.Drawing.Point(8, 554);
            this.comboHelper.Name = "comboHelper";
            this.comboHelper.Size = new System.Drawing.Size(195, 28);
            this.comboHelper.TabIndex = 4;
            this.comboHelper.SelectedIndexChanged += new System.EventHandler(this.ComboHelper_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 96);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 20);
            this.label2.TabIndex = 3;
            this.label2.Text = "References:";
            // 
            // listBoxReferences
            // 
            this.listBoxReferences.ContextMenuStrip = this.menuReferences;
            this.listBoxReferences.FormattingEnabled = true;
            this.listBoxReferences.ItemHeight = 20;
            this.listBoxReferences.Items.AddRange(new object[] {
            "System.dll",
            "System.Windows.Forms.dll",
            "Microsoft.VisualBasic.dll",
            "System.Management.dll",
            "System.Drawing.dll"});
            this.listBoxReferences.Location = new System.Drawing.Point(8, 128);
            this.listBoxReferences.Name = "listBoxReferences";
            this.listBoxReferences.Size = new System.Drawing.Size(195, 364);
            this.listBoxReferences.TabIndex = 2;
            // 
            // menuReferences
            // 
            this.menuReferences.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuReferences.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToolStripMenuItem,
            this.removeToolStripMenuItem});
            this.menuReferences.Name = "menuReferences";
            this.menuReferences.ShowImageMargin = false;
            this.menuReferences.Size = new System.Drawing.Size(125, 68);
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.Size = new System.Drawing.Size(124, 32);
            this.addToolStripMenuItem.Text = "Add";
            this.addToolStripMenuItem.Click += new System.EventHandler(this.AddToolStripMenuItem_Click);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(124, 32);
            this.removeToolStripMenuItem.Text = "Remove";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.RemoveToolStripMenuItem_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "Language:";
            // 
            // comboLang
            // 
            this.comboLang.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboLang.FormattingEnabled = true;
            this.comboLang.Items.AddRange(new object[] {
            "C#",
            "VB.NET"});
            this.comboLang.Location = new System.Drawing.Point(8, 40);
            this.comboLang.Name = "comboLang";
            this.comboLang.Size = new System.Drawing.Size(195, 28);
            this.comboLang.TabIndex = 0;
            this.comboLang.SelectedIndexChanged += new System.EventHandler(this.ComboLang_SelectedIndexChanged);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.txtBox);
            this.panel2.Location = new System.Drawing.Point(12, 12);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(884, 554);
            this.panel2.TabIndex = 1;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.button1);
            this.panel3.Controls.Add(this.button3);
            this.panel3.Controls.Add(this.button2);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel3.Location = new System.Drawing.Point(0, 572);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(914, 61);
            this.panel3.TabIndex = 1;
            // 
            // txtBox
            // 
            this.txtBox.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
            this.txtBox.AutoIndentCharsPatterns = "\r\n^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;]+);\r\n^\\s*(case|default)\\s*[^:]" +
    "*(?<range>:)\\s*(?<range>[^;]+);\r\n";
            this.txtBox.AutoScrollMinSize = new System.Drawing.Size(599, 264);
            this.txtBox.BackBrush = null;
            this.txtBox.BracketsHighlightStrategy = FastColoredTextBoxNS.BracketsHighlightStrategy.Strategy2;
            this.txtBox.CharHeight = 22;
            this.txtBox.CharWidth = 12;
            this.txtBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtBox.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.txtBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtBox.IsReplaceMode = false;
            this.txtBox.Language = FastColoredTextBoxNS.Language.CSharp;
            this.txtBox.LeftBracket = '(';
            this.txtBox.LeftBracket2 = '{';
            this.txtBox.Location = new System.Drawing.Point(0, 0);
            this.txtBox.Name = "txtBox";
            this.txtBox.Paddings = new System.Windows.Forms.Padding(0);
            this.txtBox.RightBracket = ')';
            this.txtBox.RightBracket2 = '}';
            this.txtBox.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.txtBox.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("txtBox.ServiceColors")));
            this.txtBox.Size = new System.Drawing.Size(884, 554);
            this.txtBox.TabIndex = 2;
            this.txtBox.Text = resources.GetString("txtBox.Text");
            this.txtBox.Zoom = 100;
            // 
            // FormDotNetEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1129, 633);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormDotNetEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Dot Net Editor";
            this.Load += new System.EventHandler(this.FormDotNetEditor_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.menuReferences.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboHelper;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox listBoxReferences;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboLang;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ContextMenuStrip menuReferences;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.Button button3;
        private FastColoredTextBoxNS.FastColoredTextBox txtBox;
        private System.Windows.Forms.Panel panel3;
    }
}