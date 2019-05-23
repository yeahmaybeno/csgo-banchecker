namespace yes
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
            this.listView1 = new System.Windows.Forms.ListView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openSteamToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyLoginToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Location = new System.Drawing.Point(7, 10);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(332, 500);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.ListView1_ColumnClick);
            this.listView1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ListView1_MouseClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openSteamToolStripMenuItem,
            this.copyLoginToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(179, 48);
            // 
            // openSteamToolStripMenuItem
            // 
            this.openSteamToolStripMenuItem.Name = "openSteamToolStripMenuItem";
            this.openSteamToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.openSteamToolStripMenuItem.Text = "Open Steam/CS:GO";
            this.openSteamToolStripMenuItem.Click += new System.EventHandler(this.OpenSteamToolStripMenuItem_Click);
            // 
            // copyLoginToolStripMenuItem
            // 
            this.copyLoginToolStripMenuItem.Name = "copyLoginToolStripMenuItem";
            this.copyLoginToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.copyLoginToolStripMenuItem.Text = "Copy Login";
            this.copyLoginToolStripMenuItem.Click += new System.EventHandler(this.CopyLoginToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(351, 522);
            this.Controls.Add(this.listView1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem openSteamToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyLoginToolStripMenuItem;
    }
}

