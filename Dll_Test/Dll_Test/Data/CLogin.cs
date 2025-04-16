using Newtonsoft.Json;
using System;
using System.Windows.Forms;
using System.IO;
using System.Linq;

namespace Data
{
	public class CLogin
	{
		private class CLogInParameter : ICloneable
		{
			public string[] strLoginParameter;

			public CLogInParameter()
			{
				strLoginParameter = new string[ Enum.GetNames( typeof( CConfig.enumUserAuthorityLevel ) ).Length ];
			}

			public object Clone()
			{
				CLogInParameter obj = new CLogInParameter();
				obj.strLoginParameter = this.strLoginParameter.ToArray();
				return obj;
			}
		}

		/// <summary>
		/// 로그인 파라미터
		/// </summary>
		private CLogInParameter m_objLogInParameter;

		/// <summary>
		/// 생성자
		/// </summary>
		public CLogin()
		{
		}

		/// <summary>
		/// 초기화
		/// </summary>
		/// <returns></returns>
		public bool Initialize()
		{
			bool bReturn = false;

			do {
				m_objLogInParameter = new CLogInParameter();
				LoadUserInformationParameter();
				SaveUserInformationParameter();

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 해제
		/// </summary>
		public void DeInitialize()
		{
		}

		/// <summary>
		/// 유저 정보 불러오기
		/// </summary>
		private void LoadUserInformationParameter()
		{
			do {
				string strPath = string.Format( @"{0}\Config\{1}", Application.StartupPath, "UserInformation.Json" );
				// 유저정보 파일이 없을 경우
				if( false == File.Exists( strPath ) ) {
					// 비밀번호는 1로 만들자.
					m_objLogInParameter.strLoginParameter[ 0 ] = "1";
					m_objLogInParameter.strLoginParameter[ 1 ] = "1";
					m_objLogInParameter.strLoginParameter[ 2 ] = "1";
					SaveUserInformationParameter();
				} else {
					// 유저정보 파일 읽기
					string json = File.ReadAllText( strPath );
					m_objLogInParameter = JsonConvert.DeserializeObject<CLogInParameter>( json );
					string strLogInPassword = "";
					for( int iLoopCount = 0; iLoopCount < m_objLogInParameter.strLoginParameter.Length; iLoopCount++ ) {
						strLogInPassword = AEC256.AESDecrypt256( m_objLogInParameter.strLoginParameter[ iLoopCount ] );
						m_objLogInParameter.strLoginParameter[ iLoopCount ] = strLogInPassword;
					}
				}
			} while( false );
		}

		/// <summary>
		/// 유저 정보 저장
		/// </summary>
		private void SaveUserInformationParameter()
		{

			do {
				string strPath = string.Format( @"{0}\Config\{1}", Application.StartupPath, "UserInformation.Json" );
				string[] strLogInPassword = { "", "", "" };
				for( int iLoopCount = 0; iLoopCount < m_objLogInParameter.strLoginParameter.Length; iLoopCount++ ) {
					// 데이터 암호화
					strLogInPassword[ iLoopCount ] = AEC256.AESEncrypt256( m_objLogInParameter.strLoginParameter[ iLoopCount ] );
				}
				CLogInParameter obj = new CLogInParameter();
				obj.strLoginParameter[ 0 ] = strLogInPassword[ 0 ];
				obj.strLoginParameter[ 1 ] = strLogInPassword[ 1 ];
				obj.strLoginParameter[ 2 ] = strLogInPassword[ 2 ];
				string json = JsonConvert.SerializeObject( obj, Formatting.Indented );
				File.WriteAllText( strPath, json );

			} while( false );
		}

		/// <summary>
		/// 로그인 체크
		/// </summary>
		/// <param name="eAuthorityLevel"></param>
		/// <param name="strPassword"></param>
		/// <returns></returns>
		public bool SetLogin( CConfig.enumUserAuthorityLevel eAuthorityLevel, string strPassword )
		{
			bool bResult = false;

			do {
				if( strPassword != m_objLogInParameter.strLoginParameter[ ( int )eAuthorityLevel ] ) break;

				bResult = true;
			} while( false );

			return bResult;
		}

		/// <summary>
		/// 패스워드 변경
		/// </summary>
		/// <param name="eAuthorityLevel"></param>
		/// <param name="strPassword"></param>
		/// <returns></returns>
		public bool SetChangePassword( CConfig.enumUserAuthorityLevel eAuthorityLevel, string strPassword )
		{
			bool bResult = false;

			do {
				// 암호 변경해서 데이터 저장
				m_objLogInParameter.strLoginParameter[ ( int )eAuthorityLevel ] = strPassword;
				SaveUserInformationParameter();

				bResult = true;
			} while( false );

			return bResult;
		}
	}
}
