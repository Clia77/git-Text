using System;

namespace Deepnoid_Communication
{
	public class CCommunicationParameter : ICloneable
	{
		private CCommunicationParameterAbstract m_objAbstract;

		public CCommunicationParameter( CCommunicationParameterAbstract objAbstract )
		{
			m_objAbstract = objAbstract;
		}

		public object Clone()
		{
			CCommunicationParameterAbstract objAbstract = ( CCommunicationParameterAbstract )this.m_objAbstract.Clone();
			return new CCommunicationParameter( objAbstract );
		}

		public CCommunicationParameterAbstract GetParameter()
		{
			return m_objAbstract;
		}
	}
}