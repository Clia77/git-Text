namespace Dll_Test
{
	partial class Form1
	{
		/// <summary>
		/// 필수 디자이너 변수입니다.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 사용 중인 모든 리소스를 정리합니다.
		/// </summary>
		/// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
		protected override void Dispose( bool disposing )
		{
			if( disposing && ( components != null ) ) {
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form 디자이너에서 생성한 코드

		/// <summary>
		/// 디자이너 지원에 필요한 메서드입니다. 
		/// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
		/// </summary>
		private void InitializeComponent()
		{
            this.btnInsert = new System.Windows.Forms.Button();
            this.btnSelect = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.btnWaitMessage = new System.Windows.Forms.Button();
            this.btnLoadingScreen = new System.Windows.Forms.Button();
            this.textBoxWaitMessage = new System.Windows.Forms.TextBox();
            this.BtnScanStart = new System.Windows.Forms.Button();
            this.BtnScanStop = new System.Windows.Forms.Button();
            this.BtnMeasure = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnInsert
            // 
            this.btnInsert.Location = new System.Drawing.Point(12, 12);
            this.btnInsert.Name = "btnInsert";
            this.btnInsert.Size = new System.Drawing.Size(119, 66);
            this.btnInsert.TabIndex = 0;
            this.btnInsert.Text = "Insert";
            this.btnInsert.UseVisualStyleBackColor = true;
            this.btnInsert.Click += new System.EventHandler(this.btnInsert_Click);
            // 
            // btnSelect
            // 
            this.btnSelect.Location = new System.Drawing.Point(12, 84);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(119, 66);
            this.btnSelect.TabIndex = 0;
            this.btnSelect.Text = "Select";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(137, 12);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(507, 149);
            this.dataGridView1.TabIndex = 1;
            // 
            // btnWaitMessage
            // 
            this.btnWaitMessage.Location = new System.Drawing.Point(12, 224);
            this.btnWaitMessage.Name = "btnWaitMessage";
            this.btnWaitMessage.Size = new System.Drawing.Size(119, 66);
            this.btnWaitMessage.TabIndex = 0;
            this.btnWaitMessage.Text = "Wait Message";
            this.btnWaitMessage.UseVisualStyleBackColor = true;
            this.btnWaitMessage.Click += new System.EventHandler(this.btnWaitMessage_Click);
            // 
            // btnLoadingScreen
            // 
            this.btnLoadingScreen.Location = new System.Drawing.Point(12, 296);
            this.btnLoadingScreen.Name = "btnLoadingScreen";
            this.btnLoadingScreen.Size = new System.Drawing.Size(119, 66);
            this.btnLoadingScreen.TabIndex = 0;
            this.btnLoadingScreen.Text = "Loading Screen";
            this.btnLoadingScreen.UseVisualStyleBackColor = true;
            this.btnLoadingScreen.Click += new System.EventHandler(this.btnLoadingScreen_Click);
            // 
            // textBoxWaitMessage
            // 
            this.textBoxWaitMessage.Location = new System.Drawing.Point(137, 224);
            this.textBoxWaitMessage.Name = "textBoxWaitMessage";
            this.textBoxWaitMessage.Size = new System.Drawing.Size(303, 21);
            this.textBoxWaitMessage.TabIndex = 2;
            // 
            // BtnScanStart
            // 
            this.BtnScanStart.Location = new System.Drawing.Point(461, 224);
            this.BtnScanStart.Name = "BtnScanStart";
            this.BtnScanStart.Size = new System.Drawing.Size(119, 66);
            this.BtnScanStart.TabIndex = 0;
            this.BtnScanStart.Text = "Scan Start";
            this.BtnScanStart.UseVisualStyleBackColor = true;
            this.BtnScanStart.Click += new System.EventHandler(this.BtnScanStart_Click);
            // 
            // BtnScanStop
            // 
            this.BtnScanStop.Location = new System.Drawing.Point(461, 296);
            this.BtnScanStop.Name = "BtnScanStop";
            this.BtnScanStop.Size = new System.Drawing.Size(119, 66);
            this.BtnScanStop.TabIndex = 0;
            this.BtnScanStop.Text = "Scan Stop";
            this.BtnScanStop.UseVisualStyleBackColor = true;
            this.BtnScanStop.Click += new System.EventHandler(this.BtnScanStop_Click);
            // 
            // BtnMeasure
            // 
            this.BtnMeasure.Location = new System.Drawing.Point(461, 368);
            this.BtnMeasure.Name = "BtnMeasure";
            this.BtnMeasure.Size = new System.Drawing.Size(119, 66);
            this.BtnMeasure.TabIndex = 0;
            this.BtnMeasure.Text = "Measure";
            this.BtnMeasure.UseVisualStyleBackColor = true;
            this.BtnMeasure.Click += new System.EventHandler(this.BtnMeasure_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1347, 571);
            this.Controls.Add(this.textBoxWaitMessage);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.BtnMeasure);
            this.Controls.Add(this.BtnScanStop);
            this.Controls.Add(this.BtnScanStart);
            this.Controls.Add(this.btnLoadingScreen);
            this.Controls.Add(this.btnWaitMessage);
            this.Controls.Add(this.btnInsert);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnInsert;
		private System.Windows.Forms.Button btnSelect;
		private System.Windows.Forms.DataGridView dataGridView1;
		private System.Windows.Forms.Button btnWaitMessage;
		private System.Windows.Forms.Button btnLoadingScreen;
		private System.Windows.Forms.TextBox textBoxWaitMessage;
        private System.Windows.Forms.Button BtnScanStart;
        private System.Windows.Forms.Button BtnScanStop;
        private System.Windows.Forms.Button BtnMeasure;
    }
}

