using System;

namespace Dll_Test {
    public class CUserInformation : ICloneable {
        /// <summary>
        /// 유저 아이디
        /// </summary>
        public string m_strID;
        /// <summary>
        /// 유저 이름
        /// </summary>
        public string m_strName;
        /// <summary>
        /// 패스워드
        /// </summary>
        public string m_strPassword;
        /// <summary>
        /// 권한 등급
        /// </summary>
        public CConfig.enumUserAuthorityLevel m_eAuthorityLevel;

        /// <summary>
        /// 생성자
        /// </summary>
        public CUserInformation()
        {
            m_strID = "Default";
            m_strName = "Default";
            m_strPassword = "";
            m_eAuthorityLevel = CConfig.enumUserAuthorityLevel.USER_AUTHORITY_LEVEL_FINAL;
        }

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="strID"></param>
        /// <param name="strName"></param>
        /// <param name="strPassword"></param>
        /// <param name="eAuthorityLevel"></param>
        public CUserInformation( string strID, string strName, string strPassword, CConfig.enumUserAuthorityLevel eAuthorityLevel )
        {
            m_strID = strID;
            m_strName = strName;
            m_strPassword = strPassword;
            m_eAuthorityLevel = eAuthorityLevel;
        }

        /// <summary>
        /// 복사
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            CUserInformation objUserInformation = new CUserInformation();
            objUserInformation.m_strID = this.m_strID;
            objUserInformation.m_strName = this.m_strName;
            objUserInformation.m_strPassword = this.m_strPassword;
            objUserInformation.m_eAuthorityLevel = this.m_eAuthorityLevel;
            return objUserInformation;
        }
    }
}
