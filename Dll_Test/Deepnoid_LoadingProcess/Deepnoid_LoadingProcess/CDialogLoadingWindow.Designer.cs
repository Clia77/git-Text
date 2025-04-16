namespace Deepnoid_LoadingProcess
{
	partial class CDialogLoadingWindow
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if( disposing && ( components != null ) ) {
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CDialogLoadingWindow));
            this.labelMessage = new System.Windows.Forms.Label();
            this.labelCopyLight = new System.Windows.Forms.Label();
            this.labelTitle = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.pictureProgress = new System.Windows.Forms.PictureBox();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.richTextBoxLog = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureProgress)).BeginInit();
            this.SuspendLayout();
            // 
            // labelMessage
            // 
            this.labelMessage.AutoSize = true;
            this.labelMessage.BackColor = System.Drawing.Color.Transparent;
            this.labelMessage.Font = new System.Drawing.Font("맑은 고딕", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelMessage.ForeColor = System.Drawing.Color.Black;
            this.labelMessage.Location = new System.Drawing.Point(5, 91);
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Size = new System.Drawing.Size(124, 32);
            this.labelMessage.TabIndex = 1;
            this.labelMessage.Text = "Loading...";
            // 
            // labelCopyLight
            // 
            this.labelCopyLight.AutoSize = true;
            this.labelCopyLight.BackColor = System.Drawing.Color.Transparent;
            this.labelCopyLight.Font = new System.Drawing.Font("굴림", 7F);
            this.labelCopyLight.ForeColor = System.Drawing.Color.Black;
            this.labelCopyLight.Location = new System.Drawing.Point(13, 480);
            this.labelCopyLight.Name = "labelCopyLight";
            this.labelCopyLight.Size = new System.Drawing.Size(262, 10);
            this.labelCopyLight.TabIndex = 1;
            this.labelCopyLight.Text = "CopyLight ⓒ 2018-2024 Deepnoid. All rights reserved. ";
            // 
            // labelTitle
            // 
            this.labelTitle.AutoSize = true;
            this.labelTitle.BackColor = System.Drawing.Color.Transparent;
            this.labelTitle.Font = new System.Drawing.Font("맑은 고딕", 30F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelTitle.ForeColor = System.Drawing.Color.Black;
            this.labelTitle.Location = new System.Drawing.Point(12, 18);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(457, 54);
            this.labelTitle.TabIndex = 1;
            this.labelTitle.Text = "INSPECTION PROGRAM";
            // 
            // progressBar1
            // 
            this.progressBar1.BackColor = System.Drawing.Color.White;
            this.progressBar1.Location = new System.Drawing.Point(447, 80);
            this.progressBar1.MarqueeAnimationSpeed = 10;
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(119, 41);
            this.progressBar1.Step = 1;
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar1.TabIndex = 5;
            this.progressBar1.Visible = false;
            // 
            // pictureProgress
            // 
            this.pictureProgress.BackColor = System.Drawing.Color.White;
            this.pictureProgress.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureProgress.Location = new System.Drawing.Point(12, 420);
            this.pictureProgress.Name = "pictureProgress";
            this.pictureProgress.Size = new System.Drawing.Size(555, 50);
            this.pictureProgress.TabIndex = 7;
            this.pictureProgress.TabStop = false;
            this.pictureProgress.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureProgress_Paint);
            // 
            // timer
            // 
            this.timer.Interval = 10;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // richTextBoxLog
            // 
            this.richTextBoxLog.Font = new System.Drawing.Font("굴림", 12F);
            this.richTextBoxLog.Location = new System.Drawing.Point(11, 126);
            this.richTextBoxLog.Name = "richTextBoxLog";
            this.richTextBoxLog.Size = new System.Drawing.Size(555, 288);
            this.richTextBoxLog.TabIndex = 8;
            this.richTextBoxLog.Text = "";
            // 
            // CDialogLoadingWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.Silver;
            this.ClientSize = new System.Drawing.Size(578, 499);
            this.ControlBox = false;
            this.Controls.Add(this.richTextBoxLog);
            this.Controls.Add(this.pictureProgress);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.labelCopyLight);
            this.Controls.Add(this.labelTitle);
            this.Controls.Add(this.labelMessage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CDialogLoadingWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CDialogLoadingWindow";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CDialogLoadingWindow_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.pictureProgress)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label labelMessage;
		private System.Windows.Forms.Label labelCopyLight;
		private System.Windows.Forms.Label labelTitle;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.PictureBox pictureProgress;
		private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.RichTextBox richTextBoxLog;
    }
}