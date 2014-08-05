namespace ExifOrganizer.UI
{
    partial class Main
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
            this.organize = new System.Windows.Forms.Button();
            this.source = new System.Windows.Forms.GroupBox();
            this.recursive = new System.Windows.Forms.CheckBox();
            this.destination = new System.Windows.Forms.GroupBox();
            this.copyMode = new System.Windows.Forms.ComboBox();
            this.localization = new System.Windows.Forms.ComboBox();
            this.destinationPath = new ExifOrganizer.UI.FileBrowseControl();
            this.sourcePath = new ExifOrganizer.UI.FileBrowseControl();
            this.patternImage = new System.Windows.Forms.TextBox();
            this.patternVideo = new System.Windows.Forms.TextBox();
            this.patternAudio = new System.Windows.Forms.TextBox();
            this.source.SuspendLayout();
            this.destination.SuspendLayout();
            this.SuspendLayout();
            // 
            // organize
            // 
            this.organize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.organize.Location = new System.Drawing.Point(303, 332);
            this.organize.Name = "organize";
            this.organize.Size = new System.Drawing.Size(75, 23);
            this.organize.TabIndex = 0;
            this.organize.Text = "Organize";
            this.organize.UseVisualStyleBackColor = true;
            this.organize.Click += new System.EventHandler(this.organize_Click);
            // 
            // source
            // 
            this.source.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.source.Controls.Add(this.recursive);
            this.source.Controls.Add(this.sourcePath);
            this.source.Location = new System.Drawing.Point(12, 12);
            this.source.Name = "source";
            this.source.Size = new System.Drawing.Size(366, 100);
            this.source.TabIndex = 3;
            this.source.TabStop = false;
            this.source.Text = "Source";
            // 
            // recursive
            // 
            this.recursive.AutoSize = true;
            this.recursive.Checked = true;
            this.recursive.CheckState = System.Windows.Forms.CheckState.Checked;
            this.recursive.Location = new System.Drawing.Point(6, 48);
            this.recursive.Name = "recursive";
            this.recursive.Size = new System.Drawing.Size(74, 17);
            this.recursive.TabIndex = 4;
            this.recursive.Text = "Recursive";
            this.recursive.UseVisualStyleBackColor = true;
            // 
            // destination
            // 
            this.destination.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.destination.Controls.Add(this.patternAudio);
            this.destination.Controls.Add(this.patternVideo);
            this.destination.Controls.Add(this.patternImage);
            this.destination.Controls.Add(this.localization);
            this.destination.Controls.Add(this.copyMode);
            this.destination.Controls.Add(this.destinationPath);
            this.destination.Location = new System.Drawing.Point(12, 118);
            this.destination.Name = "destination";
            this.destination.Size = new System.Drawing.Size(366, 209);
            this.destination.TabIndex = 0;
            this.destination.TabStop = false;
            this.destination.Text = "Destination";
            // 
            // copyMode
            // 
            this.copyMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.copyMode.FormattingEnabled = true;
            this.copyMode.Location = new System.Drawing.Point(6, 48);
            this.copyMode.Name = "copyMode";
            this.copyMode.Size = new System.Drawing.Size(121, 21);
            this.copyMode.TabIndex = 4;
            // 
            // localization
            // 
            this.localization.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.localization.FormattingEnabled = true;
            this.localization.Location = new System.Drawing.Point(6, 75);
            this.localization.Name = "localization";
            this.localization.Size = new System.Drawing.Size(121, 21);
            this.localization.TabIndex = 5;
            // 
            // destinationPath
            // 
            this.destinationPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.destinationPath.BrowseType = ExifOrganizer.UI.FileBrowseType.Directory;
            this.destinationPath.DialogTitle = "Destination directory";
            this.destinationPath.Location = new System.Drawing.Point(6, 19);
            this.destinationPath.Name = "destinationPath";
            this.destinationPath.Path = "C:\\temp\\backup";
            this.destinationPath.ReadOnly = false;
            this.destinationPath.Size = new System.Drawing.Size(354, 23);
            this.destinationPath.TabIndex = 2;
            // 
            // sourcePath
            // 
            this.sourcePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sourcePath.BrowseType = ExifOrganizer.UI.FileBrowseType.Directory;
            this.sourcePath.DialogTitle = "Source directory";
            this.sourcePath.Location = new System.Drawing.Point(6, 19);
            this.sourcePath.Name = "sourcePath";
            this.sourcePath.Path = "C:\\temp";
            this.sourcePath.ReadOnly = false;
            this.sourcePath.Size = new System.Drawing.Size(354, 23);
            this.sourcePath.TabIndex = 1;
            // 
            // patternImage
            // 
            this.patternImage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.patternImage.Location = new System.Drawing.Point(6, 102);
            this.patternImage.Name = "patternImage";
            this.patternImage.Size = new System.Drawing.Size(354, 20);
            this.patternImage.TabIndex = 6;
            this.patternImage.Text = "%y/%m/%t/%n";
            // 
            // patternVideo
            // 
            this.patternVideo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.patternVideo.Location = new System.Drawing.Point(6, 128);
            this.patternVideo.Name = "patternVideo";
            this.patternVideo.Size = new System.Drawing.Size(354, 20);
            this.patternVideo.TabIndex = 7;
            this.patternVideo.Text = "%y/%m/Video/%t/%n";
            // 
            // patternAudio
            // 
            this.patternAudio.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.patternAudio.Location = new System.Drawing.Point(6, 154);
            this.patternAudio.Name = "patternAudio";
            this.patternAudio.Size = new System.Drawing.Size(354, 20);
            this.patternAudio.TabIndex = 8;
            this.patternAudio.Text = "%y/%m/Audio/%t/%n";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(390, 367);
            this.Controls.Add(this.destination);
            this.Controls.Add(this.source);
            this.Controls.Add(this.organize);
            this.Name = "Main";
            this.Text = "ExifOrganizer";
            this.Load += new System.EventHandler(this.Main_Load);
            this.source.ResumeLayout(false);
            this.source.PerformLayout();
            this.destination.ResumeLayout(false);
            this.destination.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button organize;
        private FileBrowseControl sourcePath;
        private FileBrowseControl destinationPath;
        private System.Windows.Forms.GroupBox source;
        private System.Windows.Forms.GroupBox destination;
        private System.Windows.Forms.CheckBox recursive;
        private System.Windows.Forms.ComboBox copyMode;
        private System.Windows.Forms.ComboBox localization;
        private System.Windows.Forms.TextBox patternAudio;
        private System.Windows.Forms.TextBox patternVideo;
        private System.Windows.Forms.TextBox patternImage;
    }
}

