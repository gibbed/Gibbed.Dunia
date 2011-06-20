/* Copyright (c) 2011 Rick (rick 'at' gibbed 'dot' us)
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 * 
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 * 
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

namespace Gibbed.Dunia.ArchiveViewer
{
    partial class Viewer
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Viewer));
			this.openDialog = new System.Windows.Forms.OpenFileDialog();
			this.saveAllFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.mainToolStrip = new System.Windows.Forms.ToolStrip();
			this.openButton = new System.Windows.Forms.ToolStripButton();
			this.saveAllButton = new System.Windows.Forms.ToolStripSplitButton();
			this.saveOnlyknownFilesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveListOfKnownFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveFileButton = new System.Windows.Forms.ToolStripButton();
			this.fileList = new System.Windows.Forms.TreeView();
			this.fileListImages = new System.Windows.Forms.ImageList(this.components);
			this.saveFileListDialog = new System.Windows.Forms.SaveFileDialog();
			this.mainToolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// openDialog
			// 
			this.openDialog.Filter = "Far Cry 2 Archives (*.fat)|*.fat|All Files (*.*)|*.*";
			// 
			// saveAllFolderDialog
			// 
			this.saveAllFolderDialog.Description = "Select a directory to save all files from the archive to.";
			// 
			// mainToolStrip
			// 
			this.mainToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openButton,
            this.saveAllButton,
            this.saveFileButton});
			this.mainToolStrip.Location = new System.Drawing.Point(0, 0);
			this.mainToolStrip.Name = "mainToolStrip";
			this.mainToolStrip.Size = new System.Drawing.Size(792, 25);
			this.mainToolStrip.TabIndex = 2;
			this.mainToolStrip.Text = "toolStrip1";
			// 
			// openButton
			// 
			this.openButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.openButton.Image = global::Gibbed.Dunia.ArchiveViewer.Properties.Resources.OpenArchive;
			this.openButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.openButton.Name = "openButton";
			this.openButton.Size = new System.Drawing.Size(23, 22);
			this.openButton.Text = "Open";
			this.openButton.ToolTipText = "Open archive.";
			this.openButton.Click += new System.EventHandler(this.OnOpen);
			// 
			// saveAllButton
			// 
			this.saveAllButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.saveAllButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveOnlyknownFilesMenuItem,
            this.saveListOfKnownFilesToolStripMenuItem});
			this.saveAllButton.Image = global::Gibbed.Dunia.ArchiveViewer.Properties.Resources.SaveAllFiles;
			this.saveAllButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.saveAllButton.Name = "saveAllButton";
			this.saveAllButton.Size = new System.Drawing.Size(32, 22);
			this.saveAllButton.Text = "Save All";
			this.saveAllButton.ToolTipText = "Save all files in archive.";
			this.saveAllButton.ButtonClick += new System.EventHandler(this.OnSaveAll);
			// 
			// saveOnlyknownFilesMenuItem
			// 
			this.saveOnlyknownFilesMenuItem.Checked = true;
			this.saveOnlyknownFilesMenuItem.CheckOnClick = true;
			this.saveOnlyknownFilesMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
			this.saveOnlyknownFilesMenuItem.Name = "saveOnlyknownFilesMenuItem";
			this.saveOnlyknownFilesMenuItem.Size = new System.Drawing.Size(207, 22);
			this.saveOnlyknownFilesMenuItem.Text = "Save only &known files";
			// 
			// saveListOfKnownFilesToolStripMenuItem
			// 
			this.saveListOfKnownFilesToolStripMenuItem.Name = "saveListOfKnownFilesToolStripMenuItem";
			this.saveListOfKnownFilesToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
			this.saveListOfKnownFilesToolStripMenuItem.Text = "Save list of known files";
			this.saveListOfKnownFilesToolStripMenuItem.Click += new System.EventHandler(this.OnSaveKnownFileList);
			// 
			// saveFileButton
			// 
			this.saveFileButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.saveFileButton.Enabled = false;
			this.saveFileButton.Image = global::Gibbed.Dunia.ArchiveViewer.Properties.Resources.SaveFile;
			this.saveFileButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.saveFileButton.Name = "saveFileButton";
			this.saveFileButton.Size = new System.Drawing.Size(23, 22);
			this.saveFileButton.Text = "Save";
			this.saveFileButton.ToolTipText = "Save file from archive.";
			this.saveFileButton.Visible = false;
			// 
			// fileList
			// 
			this.fileList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.fileList.ImageIndex = 0;
			this.fileList.ImageList = this.fileListImages;
			this.fileList.Location = new System.Drawing.Point(0, 25);
			this.fileList.Name = "fileList";
			this.fileList.SelectedImageIndex = 0;
			this.fileList.Size = new System.Drawing.Size(792, 427);
			this.fileList.TabIndex = 3;
			// 
			// fileListImages
			// 
			this.fileListImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("fileListImages.ImageStream")));
			this.fileListImages.TransparentColor = System.Drawing.Color.Transparent;
			this.fileListImages.Images.SetKeyName(0, "rom.PNG");
			this.fileListImages.Images.SetKeyName(1, "info.png");
			this.fileListImages.Images.SetKeyName(2, "Directory");
			this.fileListImages.Images.SetKeyName(3, "File");
			// 
			// saveFileListDialog
			// 
			this.saveFileListDialog.DefaultExt = "filelist";
			this.saveFileListDialog.Filter = "File Lists (*.filelist)|*.filelist|All Files (*.*)|*.*";
			// 
			// Viewer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(792, 452);
			this.Controls.Add(this.fileList);
			this.Controls.Add(this.mainToolStrip);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "Viewer";
			this.Text = "Far Cry® 2 Archive Viewer";
			this.Load += new System.EventHandler(this.OnLoad);
			this.mainToolStrip.ResumeLayout(false);
			this.mainToolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

		private System.Windows.Forms.OpenFileDialog openDialog;
		private System.Windows.Forms.FolderBrowserDialog saveAllFolderDialog;
		private System.Windows.Forms.ToolStrip mainToolStrip;
		private System.Windows.Forms.ToolStripButton openButton;
		private System.Windows.Forms.ToolStripButton saveFileButton;
		private System.Windows.Forms.TreeView fileList;
		private System.Windows.Forms.ImageList fileListImages;
		private System.Windows.Forms.ToolStripSplitButton saveAllButton;
		private System.Windows.Forms.ToolStripMenuItem saveOnlyknownFilesMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveListOfKnownFilesToolStripMenuItem;
		private System.Windows.Forms.SaveFileDialog saveFileListDialog;
    }
}

