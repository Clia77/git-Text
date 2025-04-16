using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Algorithm {
    /// <summary>
    /// 탭돌출 검사
    /// </summary>
    public partial class CAlgorithm {
        
        [DllImport( "DeepNoid3d.dll", CallingConvention = CallingConvention.Cdecl )]
        // // 탭돌출 검사 15Point 일때 알고리즘
        public static extern void DeepNoid3DCellTapCheckHeight_USA( double[] p3Ddata, int nDataLength, int nTapcount,
                                                                [In, Out] double[] pOutput3Ddata, [In, Out] int[] nTapPos, [In, Out] double[] fCellTopHeight,
                                                                [In, Out] double[] fEmbosHeight, [In, Out] double[] fFullTopHeight,
                                                                int bTabType, // 4개부터 시작 하면 1 -> 전면, 2개 부터 시작 하면 0 -> 후면
                                                                [In, Out] double[] pEmbosprofiledata, int nShift, int nFilterSize, int nTapHeightFlag, [In, Out] int[] nEmbosPos );
        [DllImport( "DeepNoid3d.dll", CallingConvention = CallingConvention.Cdecl )]
        // 탭돌출 검사 8Point 일때 알고리즘 - 프로파일 원본 버전
        public static extern void DeepNoid3DCellTapCheckHeight_USA8um_2( double[] p3Ddata, int nDataLength, int nTapcount,
                                                                [In, Out] double[] pOutput3Ddata, [In, Out] int[] nTapPos, [In, Out] double[] fCellTopHeight,
                                                                [In, Out] double[] fEmbosHeight, [In, Out] double[] fFullTopHeight,
                                                                int bTabType, // 4개부터 시작 하면 1 -> 전면, 2개 부터 시작 하면 0 -> 후면
                                                                [In, Out] double[] pEmbosprofiledata, int nShift, int nFilterSize, int nTapHeightFlag, [In, Out] int[] nEmbosPos );
        [DllImport( "DeepNoid3d.dll", CallingConvention = CallingConvention.Cdecl )]
        // 탭돌출 검사 8Point 일때 알고리즘 - 프로파일 변환 버전.
        public static extern void DeepNoid3DCellTapCheckHeight_USA8um_3( double[] p3Ddata, int nDataLength, int nTapcount,
                                                                [In, Out] double[] pOutput3Ddata, [In, Out] int[] nTapPos, [In, Out] double[] fCellTopHeight,
                                                                [In, Out] double[] fEmbosHeight, [In, Out] double[] fFullTopHeight,
                                                                int bTabType, // 4개부터 시작 하면 1 -> 전면, 2개 부터 시작 하면 0 -> 후면
                                                                [In, Out] double[] pEmbosprofiledata, int nShift, int nFilterSize, int nTapHeightFlag, [In, Out] int[] nEmbosPos );

        [DllImport( "DeepNoid3d.dll", CallingConvention = CallingConvention.Cdecl )]
        // 사용 안함.
        public static extern void DeepNoid3DCellTapCheckHeight_GetTapPos( double[] p3Ddata, int nDataLength, int nTapcount,
                                                                                [In, Out] double[] pOutput3Ddata, [In, Out] int[] nTapPos, [In, Out] double[] fmaxval );

        [DllImport( "DeepNoid3d.dll", CallingConvention = CallingConvention.Cdecl )]
        // 사용 안함.
        public static extern void DeepNoid3DCellTapCheckHeight2MasterJig( double[] p3Ddata, int nDataLength, int nTapcount,
                                                                [In, Out] double[] pOutput3Ddata, [In, Out] int[] nTapPos, [In, Out] double[] fCellTopHeight,
                                                                [In, Out] double[] fEmbosHeight, [In, Out] double[] fFullTopHeight );

        [DllImport( "DeepNoid3d.dll", CallingConvention = CallingConvention.Cdecl )]
        // 탭돌출 15포인트 일때 마스터지그 알고리즘
        public static extern void DeepNoid3DCellTapCheckHeight2MasterJig_USA( double[] p3Ddata, int nDataLength, int nTapcount,
                                                            [In, Out] double[] pOutput3Ddata, [In, Out] int[] nTapPos, [In, Out] double[] fCellTopHeight,
                                                            [In, Out] double[] fEmbosHeight, [In, Out] double[] fFullTopHeight, int nFilterSize, int nTapHeightFlag, [In, Out] int[] nEmbosPos, [In, Out] double[] pEmbosprofiledata );
        [DllImport( "DeepNoid3d.dll", CallingConvention = CallingConvention.Cdecl )]
        // 탭돌출 8포인트 일때 마스터 지그 알고리즘
        public static extern void DeepNoid3DCellTapCheckHeight2MasterJig_USA8um( double[] p3Ddata, int nDataLength, int nTapcount,
                                                            [In, Out] double[] pOutput3Ddata, [In, Out] int[] nTapPos, [In, Out] double[] fCellTopHeight,
                                                            [In, Out] double[] fEmbosHeight, [In, Out] double[] fFullTopHeight, int nFilterSize, int nTapHeightFlag, [In, Out] int[] nEmbosPos, [In, Out] double[] pEmbosprofiledata );
    }
}
