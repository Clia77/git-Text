using System;
using System.Drawing;
using System.Windows.Forms;
using Deepnoid_MemoryMap;

namespace Deepnoid_LoadingProcess
{
	public partial class CDialogLoadingWindow : Form
	{
		// 메시지 타입
		public enum TypeOfMessage
		{
			Success = 0,
			Warning,
			Error,
		}
		private string m_strPreMessage = "";
		private object m_objLock = new object();

		/// <summary>
		/// 생성자
		/// </summary>
		public CDialogLoadingWindow()
		{
			InitializeComponent();
			var objMemoryMap = Deepnoid_MemoryMap.CMemoryMapManager.Instance;
			Initialize( objMemoryMap[ CMemoryMapManager.enumPage.LOADING_PROCESS ].strProgramName, objMemoryMap[ CMemoryMapManager.enumPage.LOADING_PROCESS ].strProgramVersion );
		}

		/// <summary>
		/// 초기화
		/// </summary>
		/// <param name="strProgramName"></param>
		/// <param name="strVersion"></param>
		/// <returns></returns>
		public bool Initialize( string strProgramName, string strVersion )
		{
			bool bReturn = false;

			do {
				// 프로그램 이름
				this.labelTitle.Text = strProgramName;
				// 버전
				//this.labelVersion.Text = strVersion;
				// 라벨 색상 추가.
				this.labelMessage.Text = "WAITING";
				this.labelMessage.ForeColor = Color.Green;

				timer.Interval = 10;
				timer.Enabled = true;

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 상태 업데이트
		/// </summary>
		/// <param name="iIndex"></param>
		/// <param name="Text"></param>
		public void UpdateStatusText( int iIndex, string Text )
		{
			labelMessage.ForeColor = Color.Green;
			labelMessage.Text = Text;
			progressBar1.Value = iIndex;
		}

		/// <summary>
		/// 상태 업데이트
		/// </summary>
		/// <param name="iIndex"></param>
		/// <param name="Text"></param>
		/// <param name="objTypeOfMessage"></param>
		public void UpdateStatusTextWithStatus( int iIndex, string Text, TypeOfMessage objTypeOfMessage )
		{
			lock( m_objLock ) {
				switch( objTypeOfMessage ) {
					case TypeOfMessage.Success:
						labelMessage.ForeColor = Color.Green;
						break;
					case TypeOfMessage.Warning:
						labelMessage.ForeColor = Color.Yellow;
						break;
					case TypeOfMessage.Error:
						labelMessage.ForeColor = Color.Red;
						break;
				}
				labelMessage.Text = Text;
				if( progressBar1.Maximum >= iIndex )
					progressBar1.Value = iIndex;

				if( Text != m_strPreMessage && Text != "" ) {
					m_strPreMessage = Text;

					if( richTextBoxLog.Lines.Length > 10000 ) {
						richTextBoxLog.Select( 0, richTextBoxLog.GetFirstCharIndexFromLine( richTextBoxLog.Lines.Length - 10000 ) );
						richTextBoxLog.SelectedText = "";
					}

					richTextBoxLog.SelectionStart = richTextBoxLog.Text.Length;

					richTextBoxLog.AppendText( string.Format( "[{0}] {1} \r\n", System.DateTime.Now.ToString( "yyyyMMdd-HH:mm:ss.fff" ), Text ) );
					richTextBoxLog.ScrollToCaret();
				}
				this.pictureProgress.Refresh();
			}
		}

		/// <summary>
		/// 폼 종료
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CDialogLoadingWindow_FormClosing( object sender, FormClosingEventArgs e )
		{
			timer.Enabled = false;
		}

		/// <summary>
		/// 현재 프로그래스바 업데이트
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void pictureProgress_Paint( object sender, PaintEventArgs e )
		{
			// Clear the background.
			e.Graphics.Clear( pictureProgress.BackColor );

			// Draw the progress bar.
			float fraction =
				( float )( progressBar1.Value - progressBar1.Minimum ) /
				( progressBar1.Maximum - progressBar1.Minimum );
			int wid = ( int )( fraction * pictureProgress.ClientSize.Width );
			e.Graphics.FillRectangle(
				Brushes.LimeGreen, 0, 0, wid,
				pictureProgress.ClientSize.Height );

			// Draw the text.
			e.Graphics.TextRenderingHint =
				System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
			using( StringFormat sf = new StringFormat() ) {
				sf.Alignment = StringAlignment.Center;
				sf.LineAlignment = StringAlignment.Center;
				int percent = ( int )( fraction * 100 );
				e.Graphics.DrawString(
					percent.ToString() + "%",
					this.labelMessage.Font, Brushes.Black,
					pictureProgress.ClientRectangle, sf );
			}
		}

		private void timer_Tick( object sender, EventArgs e )
		{
			var objMemoryMap = Deepnoid_MemoryMap.CMemoryMapManager.Instance;
			UpdateStatusTextWithStatus(
				objMemoryMap[ CMemoryMapManager.enumPage.LOADING_PROCESS ].iProgressIndex,
				objMemoryMap[ CMemoryMapManager.enumPage.LOADING_PROCESS ].strProgressMessage,
				( TypeOfMessage )objMemoryMap[ CMemoryMapManager.enumPage.LOADING_PROCESS ].iMessageType );
		}
	}
}
