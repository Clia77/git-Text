using System.Runtime.InteropServices;

namespace Algorithm
{
	/// <summary>
	/// NTC 검사 알고리즘
	/// </summary>
	public class CAlgorithmNTC
	{
		[DllImport( "DeepNoid3d.dll", CallingConvention = CallingConvention.Cdecl )]
		// NTC 검사
		public static extern void DeepNoid3DNTCInspect( double[] p3Ddata, byte[] pLuminanceData, int w, int h, int RoiCount, int[] RoiRect, int[] offsetxy, [In, Out] int[] findAlign, [In, Out] double[] fHeightData, int MinMeanMax2TYPE );
	}
}
