using System;

namespace Deepnoid_PLC
{
	public class CPLCInterfaceMelsecParameter : ICloneable
	{
		private CPLCInterfaceMelsecParameterAbstract m_objAbstract;

		public CPLCInterfaceMelsecParameter( CPLCInterfaceMelsecParameterAbstract objAbstract )
		{
			m_objAbstract = objAbstract;
		}

		public object Clone()
		{
			CPLCInterfaceMelsecParameterAbstract objAbstract = ( CPLCInterfaceMelsecParameterAbstract )this.m_objAbstract.Clone();
			return new CPLCInterfaceMelsecParameter( objAbstract );
		}

		public CPLCInterfaceMelsecParameterAbstract GetParameter()
		{
			return m_objAbstract;
		}
	}
}