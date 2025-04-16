using System;
using System.Windows.Forms;
using Deepnoid_MemoryMap;

namespace Deepnoid_Wait
{
	public partial class Wait : Form
	{
		private int m_iWaitingMillisecond;

		public Wait()
		{
			InitializeComponent();
			m_iWaitingMillisecond = 0;
			this.timer.Interval = 100;
			this.timer.Enabled = true;
		}

		private void timer_Tick( object sender, EventArgs e )
		{
			var objMemoryMap = Deepnoid_MemoryMap.CMemoryMapManager.Instance;
			if( true == objMemoryMap[ CMemoryMapManager.enumPage.WAIT_MESSAGE ].bWaitShow ) {
				string strLabel = objMemoryMap[ CMemoryMapManager.enumPage.WAIT_MESSAGE ].strWaitMessage;
				this.labelText.Text = strLabel;
				m_iWaitingMillisecond += this.timer.Interval;
				string strMessage = string.Format( "Waiting {0:F1} Sec ...", ( double )m_iWaitingMillisecond / 1000 );
				this.labelWait.Text = strMessage;
			} else {
				m_iWaitingMillisecond = 0;
			}
		}
	}
}