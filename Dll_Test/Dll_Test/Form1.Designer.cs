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
			this.btnServer = new System.Windows.Forms.Button();
			this.btnClient = new System.Windows.Forms.Button();
			this.btnConnectServer = new System.Windows.Forms.Button();
			this.btnConnectClient = new System.Windows.Forms.Button();
			this.textBoxServer = new System.Windows.Forms.TextBox();
			this.textBoxClient = new System.Windows.Forms.TextBox();
			this.btnSendServer = new System.Windows.Forms.Button();
			this.btnSendClient = new System.Windows.Forms.Button();
			this.btnVirtual = new System.Windows.Forms.Button();
			this.btnConnectVirtual = new System.Windows.Forms.Button();
			this.btnSendVirtual = new System.Windows.Forms.Button();
			this.dataGridViewPLC = new System.Windows.Forms.DataGridView();
			this.btnReceiveClient = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewPLC)).BeginInit();
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
			this.btnWaitMessage.Location = new System.Drawing.Point(12, 187);
			this.btnWaitMessage.Name = "btnWaitMessage";
			this.btnWaitMessage.Size = new System.Drawing.Size(119, 66);
			this.btnWaitMessage.TabIndex = 0;
			this.btnWaitMessage.Text = "Wait Message";
			this.btnWaitMessage.UseVisualStyleBackColor = true;
			this.btnWaitMessage.Click += new System.EventHandler(this.btnWaitMessage_Click);
			// 
			// btnLoadingScreen
			// 
			this.btnLoadingScreen.Location = new System.Drawing.Point(12, 259);
			this.btnLoadingScreen.Name = "btnLoadingScreen";
			this.btnLoadingScreen.Size = new System.Drawing.Size(119, 66);
			this.btnLoadingScreen.TabIndex = 0;
			this.btnLoadingScreen.Text = "Loading Screen";
			this.btnLoadingScreen.UseVisualStyleBackColor = true;
			this.btnLoadingScreen.Click += new System.EventHandler(this.btnLoadingScreen_Click);
			// 
			// textBoxWaitMessage
			// 
			this.textBoxWaitMessage.Location = new System.Drawing.Point(137, 187);
			this.textBoxWaitMessage.Name = "textBoxWaitMessage";
			this.textBoxWaitMessage.Size = new System.Drawing.Size(303, 21);
			this.textBoxWaitMessage.TabIndex = 2;
			// 
			// btnServer
			// 
			this.btnServer.Location = new System.Drawing.Point(12, 362);
			this.btnServer.Name = "btnServer";
			this.btnServer.Size = new System.Drawing.Size(119, 66);
			this.btnServer.TabIndex = 0;
			this.btnServer.Text = "Server OnOff";
			this.btnServer.UseVisualStyleBackColor = true;
			this.btnServer.Click += new System.EventHandler(this.btnServer_Click);
			// 
			// btnClient
			// 
			this.btnClient.Location = new System.Drawing.Point(12, 434);
			this.btnClient.Name = "btnClient";
			this.btnClient.Size = new System.Drawing.Size(119, 66);
			this.btnClient.TabIndex = 0;
			this.btnClient.Text = "Client OnOff";
			this.btnClient.UseVisualStyleBackColor = true;
			this.btnClient.Click += new System.EventHandler(this.btnClient_Click);
			// 
			// btnConnectServer
			// 
			this.btnConnectServer.Location = new System.Drawing.Point(137, 362);
			this.btnConnectServer.Name = "btnConnectServer";
			this.btnConnectServer.Size = new System.Drawing.Size(119, 66);
			this.btnConnectServer.TabIndex = 0;
			this.btnConnectServer.Text = "Connect";
			this.btnConnectServer.UseVisualStyleBackColor = true;
			// 
			// btnConnectClient
			// 
			this.btnConnectClient.Location = new System.Drawing.Point(137, 434);
			this.btnConnectClient.Name = "btnConnectClient";
			this.btnConnectClient.Size = new System.Drawing.Size(119, 66);
			this.btnConnectClient.TabIndex = 0;
			this.btnConnectClient.Text = "Connect";
			this.btnConnectClient.UseVisualStyleBackColor = true;
			// 
			// textBoxServer
			// 
			this.textBoxServer.Location = new System.Drawing.Point(262, 362);
			this.textBoxServer.Name = "textBoxServer";
			this.textBoxServer.Size = new System.Drawing.Size(77, 21);
			this.textBoxServer.TabIndex = 2;
			// 
			// textBoxClient
			// 
			this.textBoxClient.Location = new System.Drawing.Point(262, 434);
			this.textBoxClient.Name = "textBoxClient";
			this.textBoxClient.Size = new System.Drawing.Size(77, 21);
			this.textBoxClient.TabIndex = 2;
			// 
			// btnSendServer
			// 
			this.btnSendServer.Location = new System.Drawing.Point(345, 362);
			this.btnSendServer.Name = "btnSendServer";
			this.btnSendServer.Size = new System.Drawing.Size(119, 66);
			this.btnSendServer.TabIndex = 0;
			this.btnSendServer.Text = "Send";
			this.btnSendServer.UseVisualStyleBackColor = true;
			this.btnSendServer.Click += new System.EventHandler(this.btnSendServer_Click);
			// 
			// btnSendClient
			// 
			this.btnSendClient.Location = new System.Drawing.Point(345, 434);
			this.btnSendClient.Name = "btnSendClient";
			this.btnSendClient.Size = new System.Drawing.Size(119, 66);
			this.btnSendClient.TabIndex = 0;
			this.btnSendClient.Text = "Send";
			this.btnSendClient.UseVisualStyleBackColor = true;
			this.btnSendClient.Click += new System.EventHandler(this.btnSendClient_Click);
			// 
			// btnVirtual
			// 
			this.btnVirtual.Location = new System.Drawing.Point(12, 506);
			this.btnVirtual.Name = "btnVirtual";
			this.btnVirtual.Size = new System.Drawing.Size(119, 66);
			this.btnVirtual.TabIndex = 0;
			this.btnVirtual.Text = "Virtual OnOff";
			this.btnVirtual.UseVisualStyleBackColor = true;
			this.btnVirtual.Click += new System.EventHandler(this.btnVirtual_Click);
			// 
			// btnConnectVirtual
			// 
			this.btnConnectVirtual.Location = new System.Drawing.Point(137, 506);
			this.btnConnectVirtual.Name = "btnConnectVirtual";
			this.btnConnectVirtual.Size = new System.Drawing.Size(119, 66);
			this.btnConnectVirtual.TabIndex = 0;
			this.btnConnectVirtual.Text = "Connect";
			this.btnConnectVirtual.UseVisualStyleBackColor = true;
			// 
			// btnSendVirtual
			// 
			this.btnSendVirtual.Location = new System.Drawing.Point(262, 506);
			this.btnSendVirtual.Name = "btnSendVirtual";
			this.btnSendVirtual.Size = new System.Drawing.Size(119, 66);
			this.btnSendVirtual.TabIndex = 0;
			this.btnSendVirtual.Text = "Send";
			this.btnSendVirtual.UseVisualStyleBackColor = true;
			this.btnSendVirtual.Click += new System.EventHandler(this.btnSendVirtual_Click);
			// 
			// dataGridViewPLC
			// 
			this.dataGridViewPLC.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridViewPLC.Location = new System.Drawing.Point(650, 12);
			this.dataGridViewPLC.Name = "dataGridViewPLC";
			this.dataGridViewPLC.RowTemplate.Height = 23;
			this.dataGridViewPLC.Size = new System.Drawing.Size(792, 768);
			this.dataGridViewPLC.TabIndex = 1;
			// 
			// btnReceiveClient
			// 
			this.btnReceiveClient.Location = new System.Drawing.Point(470, 434);
			this.btnReceiveClient.Name = "btnReceiveClient";
			this.btnReceiveClient.Size = new System.Drawing.Size(119, 66);
			this.btnReceiveClient.TabIndex = 0;
			this.btnReceiveClient.Text = "Receive";
			this.btnReceiveClient.UseVisualStyleBackColor = true;
			this.btnReceiveClient.Click += new System.EventHandler(this.btnReceiveClient_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1454, 792);
			this.Controls.Add(this.textBoxClient);
			this.Controls.Add(this.textBoxServer);
			this.Controls.Add(this.textBoxWaitMessage);
			this.Controls.Add(this.dataGridViewPLC);
			this.Controls.Add(this.dataGridView1);
			this.Controls.Add(this.btnSelect);
			this.Controls.Add(this.btnLoadingScreen);
			this.Controls.Add(this.btnClient);
			this.Controls.Add(this.btnReceiveClient);
			this.Controls.Add(this.btnSendClient);
			this.Controls.Add(this.btnConnectVirtual);
			this.Controls.Add(this.btnConnectClient);
			this.Controls.Add(this.btnSendServer);
			this.Controls.Add(this.btnConnectServer);
			this.Controls.Add(this.btnSendVirtual);
			this.Controls.Add(this.btnVirtual);
			this.Controls.Add(this.btnServer);
			this.Controls.Add(this.btnWaitMessage);
			this.Controls.Add(this.btnInsert);
			this.Name = "Form1";
			this.Text = "Form1";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewPLC)).EndInit();
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
		private System.Windows.Forms.Button btnServer;
		private System.Windows.Forms.Button btnClient;
		private System.Windows.Forms.Button btnConnectServer;
		private System.Windows.Forms.Button btnConnectClient;
		private System.Windows.Forms.TextBox textBoxServer;
		private System.Windows.Forms.TextBox textBoxClient;
		private System.Windows.Forms.Button btnSendServer;
		private System.Windows.Forms.Button btnSendClient;
		private System.Windows.Forms.Button btnVirtual;
		private System.Windows.Forms.Button btnConnectVirtual;
		private System.Windows.Forms.Button btnSendVirtual;
		private System.Windows.Forms.DataGridView dataGridViewPLC;
		private System.Windows.Forms.Button btnReceiveClient;
	}
}

