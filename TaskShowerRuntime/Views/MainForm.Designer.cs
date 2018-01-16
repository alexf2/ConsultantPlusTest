namespace ConsPlus.TaskShowerRuntime.Views
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.treeDirs = new System.Windows.Forms.TreeView();
            this.imglstMain = new System.Windows.Forms.ImageList(this.components);
            this.splitRight = new System.Windows.Forms.SplitContainer();
            this.lstCurDir = new System.Windows.Forms.ListView();
            this.clmName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblCurDir = new System.Windows.Forms.Label();
            this.htmlViewer = new System.Windows.Forms.WebBrowser();
            this.btnUp = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitRight)).BeginInit();
            this.splitRight.Panel1.SuspendLayout();
            this.splitRight.Panel2.SuspendLayout();
            this.splitRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnUp)).BeginInit();
            this.SuspendLayout();
            // 
            // splitMain
            // 
            this.splitMain.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitMain.Location = new System.Drawing.Point(0, 0);
            this.splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            this.splitMain.Panel1.Controls.Add(this.treeDirs);
            this.splitMain.Panel1MinSize = 50;
            // 
            // splitMain.Panel2
            // 
            this.splitMain.Panel2.Controls.Add(this.splitRight);
            this.splitMain.Size = new System.Drawing.Size(940, 635);
            this.splitMain.SplitterDistance = 313;
            this.splitMain.TabIndex = 0;
            // 
            // treeDirs
            // 
            this.treeDirs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeDirs.ImageIndex = 0;
            this.treeDirs.ImageList = this.imglstMain;
            this.treeDirs.Location = new System.Drawing.Point(0, 0);
            this.treeDirs.Margin = new System.Windows.Forms.Padding(5);
            this.treeDirs.Name = "treeDirs";
            this.treeDirs.SelectedImageIndex = 0;
            this.treeDirs.Size = new System.Drawing.Size(311, 633);
            this.treeDirs.TabIndex = 0;
            this.treeDirs.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.treeDirs_AfterCollapse);
            this.treeDirs.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeDirs_BeforeExpand);
            this.treeDirs.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.treeDirs_AfterExpand);
            this.treeDirs.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeDirs_AfterSelect);
            this.treeDirs.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeDirs_NodeMouseClick);
            // 
            // imglstMain
            // 
            this.imglstMain.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imglstMain.ImageStream")));
            this.imglstMain.TransparentColor = System.Drawing.Color.Magenta;
            this.imglstMain.Images.SetKeyName(0, "VSFolder_closed.bmp");
            this.imglstMain.Images.SetKeyName(1, "VSFolder_open.bmp");
            this.imglstMain.Images.SetKeyName(2, "VSProject_genericfile.bmp");
            this.imglstMain.Images.SetKeyName(3, "VSProject_xml.bmp");
            // 
            // splitRight
            // 
            this.splitRight.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitRight.Location = new System.Drawing.Point(0, 0);
            this.splitRight.Name = "splitRight";
            this.splitRight.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitRight.Panel1
            // 
            this.splitRight.Panel1.Controls.Add(this.btnUp);
            this.splitRight.Panel1.Controls.Add(this.lstCurDir);
            this.splitRight.Panel1.Controls.Add(this.lblCurDir);
            // 
            // splitRight.Panel2
            // 
            this.splitRight.Panel2.Controls.Add(this.htmlViewer);
            this.splitRight.Size = new System.Drawing.Size(621, 633);
            this.splitRight.SplitterDistance = 266;
            this.splitRight.TabIndex = 0;
            // 
            // lstCurDir
            // 
            this.lstCurDir.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clmName});
            this.lstCurDir.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstCurDir.Location = new System.Drawing.Point(0, 20);
            this.lstCurDir.Name = "lstCurDir";
            this.lstCurDir.Size = new System.Drawing.Size(617, 242);
            this.lstCurDir.SmallImageList = this.imglstMain;
            this.lstCurDir.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lstCurDir.TabIndex = 1;
            this.lstCurDir.UseCompatibleStateImageBehavior = false;
            this.lstCurDir.View = System.Windows.Forms.View.Details;
            this.lstCurDir.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lstCurDir_ColumnClick);
            this.lstCurDir.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lstCurDir_MouseDoubleClick);
            // 
            // clmName
            // 
            this.clmName.Text = "Имя";
            // 
            // lblCurDir
            // 
            this.lblCurDir.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblCurDir.Location = new System.Drawing.Point(0, 0);
            this.lblCurDir.Name = "lblCurDir";
            this.lblCurDir.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.lblCurDir.Size = new System.Drawing.Size(617, 20);
            this.lblCurDir.TabIndex = 0;
            this.lblCurDir.Text = "label1";
            this.lblCurDir.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // htmlViewer
            // 
            this.htmlViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.htmlViewer.Location = new System.Drawing.Point(0, 0);
            this.htmlViewer.MinimumSize = new System.Drawing.Size(20, 20);
            this.htmlViewer.Name = "htmlViewer";
            this.htmlViewer.Size = new System.Drawing.Size(617, 359);
            this.htmlViewer.TabIndex = 0;
            // 
            // btnUp
            // 
            this.btnUp.Image = global::ConsPlus.TaskShowerRuntime.Properties.Resources._112_UpArrowLong_Blue_16x16_72;
            this.btnUp.Location = new System.Drawing.Point(1, 2);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(16, 16);
            this.btnUp.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.btnUp.TabIndex = 2;
            this.btnUp.TabStop = false;
            this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(940, 635);
            this.Controls.Add(this.splitMain);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Xml Task  Shower";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.splitRight.Panel1.ResumeLayout(false);
            this.splitRight.Panel1.PerformLayout();
            this.splitRight.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitRight)).EndInit();
            this.splitRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.btnUp)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.SplitContainer splitRight;
        private System.Windows.Forms.TreeView treeDirs;
        private System.Windows.Forms.ImageList imglstMain;
        private System.Windows.Forms.ListView lstCurDir;
        private System.Windows.Forms.Label lblCurDir;
        private System.Windows.Forms.WebBrowser htmlViewer;
        private System.Windows.Forms.ColumnHeader clmName;
        private System.Windows.Forms.PictureBox btnUp;
    }
}

