namespace Deepnoid_Communication
{
	public class CCommunicationVirtual : CCommunicationAbstract
	{
		public override bool Initialize( CCommunicationParameter objParameter )
		{
			return true;
		}

		public override void DeInitialize()
		{
		}

		public override bool IsConnected()
		{
			return true;
		}

		public override void Connect()
		{
		}

		public override void Disconnect()
		{
		}

		public override bool Send( string strData )
		{
			return true;
		}

		public override bool Send( byte[] byteData )
		{
			return true;
		}
	}
}