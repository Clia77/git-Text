using System;

namespace Deepnoid_Communication
{
	public abstract class CCommunicationParameterAbstract : ICloneable
	{

		public CCommunicationParameterAbstract()
		{
		}

		public abstract object Clone();
	}
}