using System;

namespace Deepnoid_PLC
{
	public abstract class CPLCInterfaceMelsecParameterAbstract : ICloneable
	{
		public CPLCInterfaceMelsecParameterAbstract()
		{
		}

		public abstract object Clone();
	}
}