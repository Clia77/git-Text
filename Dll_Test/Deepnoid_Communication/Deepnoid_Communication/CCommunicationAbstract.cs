using System;
using System.Linq;
using System.Threading;

namespace Deepnoid_Communication
{
	/// <summary>
	/// 수신 데이터 클래스
	/// </summary>
	public class CReceiveData : ICloneable
	{
		public string strData;
		public byte[] byteReceiveData;
		public int iByteLength;

		public CReceiveData()
		{
			strData = "";
			byteReceiveData = new byte[ CCommunicationDefine.DEF_BYTE_LENGTH ];
			iByteLength = 0;
		}

		public object Clone()
		{
			CReceiveData obj = new CReceiveData();

			obj.strData = this.strData;
			obj.byteReceiveData = this.byteReceiveData.ToArray();
			obj.iByteLength = this.iByteLength;

			return obj;
		}
	}

	public abstract class CCommunicationAbstract
    {
		/// <summary>
		/// 접속 유무
		/// </summary>
		protected bool m_bConnected;
		protected bool m_bThreadExit;
		protected object m_objLock;
		protected Thread m_threadConnect;
		protected byte[] m_byteReceivedData;

		/// <summary>
		/// 수신 데이터 콜백 처리
		/// </summary>
		/// <param name="objReceiveData"></param>
		public delegate void CallBackReceiveData( CReceiveData obj );
		protected CallBackReceiveData _callBackReceiveData;
		public void SetCallBackReceiveData( CallBackReceiveData callBack )
		{
			_callBackReceiveData = callBack;
		}

		/// <summary>
		/// Error Message 콜백 처리
		/// </summary>
		/// <param name="strErrorMessage"></param>
		public delegate void CallBackErrorMessage( string strErrorMessage );
		protected CallBackErrorMessage _callBackErrorMessage;
		public void SetCallBackErrorMessage( CallBackErrorMessage callBack )
		{
			_callBackErrorMessage = callBack;
		}

		/// <summary>
		/// 초기화
		/// </summary>
		/// <param name="objParameter"></param>
		/// <returns></returns>
		public abstract bool Initialize( CCommunicationParameter objParameter );

		/// <summary>
		/// 해제
		/// </summary>
		public abstract void DeInitialize();

		/// <summary>
		/// 접속 유무
		/// </summary>
		/// <returns></returns>
		public abstract bool IsConnected();

		/// <summary>
		/// 접속
		/// </summary>
		public abstract void Connect();

		/// <summary>
		/// 해제
		/// </summary>
		public abstract void Disconnect();

		/// <summary>
		/// 데이터 send
		/// </summary>
		/// <param name="strData"></param>
		/// <returns></returns>
		public abstract bool Send( string strData );

		/// <summary>
		/// 데이터 send
		/// </summary>
		/// <param name="byteData"></param>
		/// <returns></returns>
		public abstract bool Send( byte[] byteData );
	}
}