using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Deepnoid_Sensor {
    #region Enum

    /// <summary>
    /// Return value definition
    /// </summary>
    public enum Rc {
        /// <summary>Normal termination</summary>
        Ok = 0x0000,
        /// <summary>Failed to open the device</summary>
        ErrOpenDevice = 0x1000,
        /// <summary>Device not open</summary>
        ErrNoDevice,
        /// <summary>Command send error</summary>
        ErrSend,
        /// <summary>Response reception error</summary>
        ErrReceive,
        /// <summary>Timeout</summary>
        ErrTimeout,
        /// <summary>No free space</summary>
        ErrNomemory,
        /// <summary>Parameter error</summary>
        ErrParameter,
        /// <summary>Received header format error</summary>
        ErrRecvFmt,

        /// <summary>Not open error (for high-speed communication)</summary>
        ErrHispeedNoDevice = 0x1009,
        /// <summary>Already open error (for high-speed communication)</summary>
        ErrHispeedOpenYet,
        /// <summary>Already performing high-speed communication error (for high-speed communication)</summary>
        ErrHispeedRecvYet,
        /// <summary>Insufficient buffer size</summary>
        ErrBufferShort,
    }

    /// Definition that indicates the "setting type" in LJX8IF_TARGET_SETTING structure.
    public enum SettingType : byte {
        /// <summary>Environment setting</summary>
        Environment = 0x01,
        /// <summary>Common measurement setting</summary>
        Common = 0x02,
        /// <summary>Measurement Program setting</summary>
        Program00 = 0x10,
        Program01,
        Program02,
        Program03,
        Program04,
        Program05,
        Program06,
        Program07,
        Program08,
        Program09,
        Program10,
        Program11,
        Program12,
        Program13,
        Program14,
        Program15,
    };


    /// Get batch profile position specification method designation
    public enum LJX8IF_BATCH_POSITION : byte {
        /// <summary>From current</summary>
        LJX8IF_BATCH_POSITION_CURRENT = 0x00,
        /// <summary>Specify position</summary>
        LJX8IF_BATCH_POSITION_SPEC = 0x02,
        /// <summary>From current after commitment</summary>
        LJX8IF_BATCH_POSITION_COMMITED = 0x03,
        /// <summary>Current only</summary>
        LJX8IF_BATCH_POSITION_CURRENT_ONLY = 0x04,
    };

    /// Setting value storage level designation
    public enum LJX8IF_SETTING_DEPTH : byte {
        /// <summary>Settings write area</summary>
        LJX8IF_SETTING_DEPTH_WRITE = 0x00,
        /// <summary>Active measurement area</summary>
        LJX8IF_SETTING_DEPTH_RUNNING = 0x01,
        /// <summary>Save area</summary>
        LJX8IF_SETTING_DEPTH_SAVE = 0x02,
    };


    /// Get profile target buffer designation
    public enum LJX8IF_PROFILE_BANK : byte {
        /// <summary>Active surface</summary>
        LJX8IF_PROFILE_BANK_ACTIVE = 0x00,
        /// <summary>Inactive surface</summary>	
        LJX8IF_PROFILE_BANK_INACTIVE = 0x01,
    };

    /// Get profile position specification method designation
    public enum LJX8IF_PROFILE_POSITION : byte {
        /// <summary>From current</summary>
        LJX8IF_PROFILE_POSITION_CURRENT = 0x00,
        /// <summary>From oldest</summary>
        LJX8IF_PROFILE_POSITION_OLDEST = 0x01,
        /// <summary>Specify position</summary>
        LJX8IF_PROFILE_POSITION_SPEC = 0x02,
    };

    #endregion

    #region Structure
    /// <summary>
    /// Version Information
    /// </summary>
    [StructLayout( LayoutKind.Sequential )]
    public struct LJX8IF_VERSION_INFO {
        public int nMajorNumber;
        public int nMinorNumber;
        public int nRevisionNumber;
        public int nBuildNumber;
    };

    /// <summary>
    /// Ethernet settings structure
    /// </summary>
    [StructLayout( LayoutKind.Sequential )]
    public struct LJX8IF_ETHERNET_CONFIG {
        [MarshalAs( UnmanagedType.ByValArray, SizeConst = 4 )]
        public byte[] abyIpAddress;
        public ushort wPortNo;
        [MarshalAs( UnmanagedType.ByValArray, SizeConst = 2 )]
        public byte[] reserve;

    };

    /// <summary>
    /// Setting item designation structure
    /// </summary>
    [StructLayout( LayoutKind.Sequential )]
    public struct LJX8IF_TARGET_SETTING {
        public byte byType;
        public byte byCategory;
        public byte byItem;
        public byte reserve;
        public byte byTarget1;
        public byte byTarget2;
        public byte byTarget3;
        public byte byTarget4;
    };

    /// <summary>
    /// Profile information structure
    /// </summary>
    [StructLayout( LayoutKind.Sequential )]
    public struct LJX8IF_PROFILE_INFO {
        public byte byProfileCount;
        public byte reserve1;
        public byte byLuminanceOutput;
        public byte reserve2;
        public short nProfileDataCount;
        [MarshalAs( UnmanagedType.ByValArray, SizeConst = 2 )]
        public byte[] reserve3;
        public int lXStart;
        public int lXPitch;
    };

    /// <summary>
    /// Profile header information structure
    /// </summary>
    [StructLayout( LayoutKind.Sequential )]
    public struct LJX8IF_PROFILE_HEADER {
        public uint reserve;
        public uint dwTriggerCount;
        public int lEncoderCount;
        [MarshalAs( UnmanagedType.ByValArray, SizeConst = 3 )]
        public uint[] reserve2;
    };

    /// <summary>
    /// Profile footer information structure
    /// </summary>
    [StructLayout( LayoutKind.Sequential )]
    public struct LJX8IF_PROFILE_FOOTER {
        public uint reserve;
    };

    /// <summary>
    /// Get profile request structure (batch measurement: off)
    /// </summary>
    [StructLayout( LayoutKind.Sequential )]
    public struct LJX8IF_GET_PROFILE_REQUEST {
        public byte byTargetBank;
        public byte byPositionMode;
        [MarshalAs( UnmanagedType.ByValArray, SizeConst = 2 )]
        public byte[] reserve;
        public uint dwGetProfileNo;
        public byte byGetProfileCount;
        public byte byErase;
        [MarshalAs( UnmanagedType.ByValArray, SizeConst = 2 )]
        public byte[] reserve2;
    };

    /// <summary>
    /// Get profile request structure (batch measurement: on)
    /// </summary>
    [StructLayout( LayoutKind.Sequential )]
    public struct LJX8IF_GET_BATCH_PROFILE_REQUEST {
        public byte byTargetBank;
        public byte byPositionMode;
        [MarshalAs( UnmanagedType.ByValArray, SizeConst = 2 )]
        public byte[] reserve;
        public uint dwGetBatchNo;
        public uint dwGetProfileNo;
        public byte byGetProfileCount;
        public byte byErase;
        [MarshalAs( UnmanagedType.ByValArray, SizeConst = 2 )]
        public byte[] reserve2;
    };

    /// <summary>
    /// Get profile response structure (batch measurement: off)
    /// </summary>
    [StructLayout( LayoutKind.Sequential )]
    public struct LJX8IF_GET_PROFILE_RESPONSE {
        public uint dwCurrentProfileNo;
        public uint dwOldestProfileNo;
        public uint dwGetTopProfileNo;
        public byte byGetProfileCount;
        [MarshalAs( UnmanagedType.ByValArray, SizeConst = 3 )]
        public byte[] reserve;
    };

    /// <summary>
    /// Get profile response structure (batch measurement: on)
    /// </summary>
    [StructLayout( LayoutKind.Sequential )]
    public struct LJX8IF_GET_BATCH_PROFILE_RESPONSE {
        public uint dwCurrentBatchNo;
        public uint dwCurrentBatchProfileCount;
        public uint dwOldestBatchNo;
        public uint dwOldestBatchProfileCount;
        public uint dwGetBatchNo;
        public uint dwGetBatchProfileCount;
        public uint dwGetBatchTopProfileNo;
        public byte byGetProfileCount;
        public byte byCurrentBatchCommited;
        [MarshalAs( UnmanagedType.ByValArray, SizeConst = 2 )]
        public byte[] reserve;
    };

    /// <summary>
    /// High-speed communication start preparation request structure
    /// </summary>
    [StructLayout( LayoutKind.Sequential )]
    public struct LJX8IF_HIGH_SPEED_PRE_START_REQUEST {
        public byte bySendPosition;     // Send start position
        [MarshalAs( UnmanagedType.ByValArray, SizeConst = 3 )]
        public byte[] reserve;      // Reservation 
    };

    #endregion

    #region Define
    /// <summary>
	/// Constant class
	/// </summary>
	public static class Define {
        #region Constant

        public enum LjxHeadSamplingPeriod {
            LjxHeadSamplingPeriod10Hz = 0,
            LjxHeadSamplingPeriod20Hz,
            LjxHeadSamplingPeriod50Hz,
            LjxHeadSamplingPeriod100Hz,
            LjxHeadSamplingPeriod200Hz,
            LjxHeadSamplingPeriod500Hz,
            LjxHeadSamplingPeriod1KHz,
            LjxHeadSamplingPeriod1_5KHz,
            LjxHeadSamplingPeriod2KHz,
            LjxHeadSamplingPeriod2_5KHz,
            LjxHeadSamplingPeriod3KHz,
            LjxHeadSamplingPeriod3_5KHz,
            LjxHeadSamplingPeriod4KHz,
            LjxHeadSamplingPeriod4_5KHz,
            LjxHeadSamplingPeriod5KHz,
            LjxHeadSamplingPeriod6KHz,
            LjxHeadSamplingPeriod7KHz,
            LjxHeadSamplingPeriod8KHz,
            LjxHeadSamplingPeriod10KHz,
            LjxHeadSamplingPeriod12KHz,
            LjxHeadSamplingPeriod14KHz,
            LjxHeadSamplingPeriod16KHz,
        }

        public enum LuminanceOutput {
            LuminanceOutputOn,
            LuminanceOutputOff
        }

        public enum LjxAdvanced {
            LjxAdvancedOff,
            LjxAdvancedOn
        }

        public enum LjxXBinning {
            LjxXBinningOff,
            LjxXBinningOn
        }

        /// <summary>
        /// Maximum amount of data for 1 profile
        /// </summary>
        public const int MaxProfileCount = LjxHeadMeasureRangeFull;

        /// <summary>
        /// Device ID (fixed to 0)
        /// </summary>
        public const int DeviceId = 0;

        /// <summary>
        /// Maximum profile count that store to buffer.
        /// </summary>
#if WIN64
		public const int BufferFullCount = 120000;
#else
        public const int BufferFullCount = 30000;
#endif
        // @Point
        //  32-bit architecture cannot allocate huge memory and the buffer limit is more strict.

        /// <summary>
        /// Measurement range X direction of LJ-X Head
        /// </summary>
        public const int LjxHeadMeasureRangeFull = 3200;
        public const int LjxHeadMeasureRangeThreeFourth = 2400;
        public const int LjxHeadMeasureRangeHalf = 1600;
        public const int LjxHeadMeasureRangeQuarter = 800;

        public const int LjxHeadMeasureRange23_32 = 2300;
        public const int LjxHeadMeasureRange19_32 = 1900;
        public const int LjxHeadMeasureRange18_32 = 1800;
        public const int LjxHeadMeasureRange12_32 = 1200;

        /// <summary>
        /// Light reception characteristic
        /// </summary>
        public const int ReceivedBinningOff = 1;
        public const int ReceivedBinningOn = 2;

        public const int ThinningXOff = 1;
        public const int ThinningX2 = 2;
        public const int ThinningX4 = 4;


        /// <summary>
        /// Measurement range X direction of LJ-V Head
        /// </summary>
        public const int MeasureRangeFull = 800;
        public const int MeasureRangeMiddle = 600;
        public const int MeasureRangeSmall = 400;
        #endregion
    }
    #endregion

    #region ProfileData
    /// <summary>
	/// Profile data class
	/// </summary>
	public class ProfileData {
        #region constant
        private const int LUMINANCE_OUTPUT_ON_VALUE = 1;
        public const int MULTIPLE_VALUE_FOR_LUMINANCE_OUTPUT = 2;
        #endregion

        #region Enum
        /// <summary>
        /// Structure classification
        /// </summary>
        public enum TypeOfStructure {
            ProfileHeader,
            ProfileFooter,
        }
        #endregion

        #region Field
        /// <summary>
        /// Profile data
        /// </summary>
        private int[] _profData;

        /// <summary>
        /// Profile information
        /// </summary>
        private LJX8IF_PROFILE_INFO _profileInfo;

        #endregion

        #region Property
        /// <summary>
        /// Profile Data
        /// </summary>
        public int[] ProfData
        {
            get { return _profData; }
        }

        /// <summary>
        /// Profile Imformation
        /// </summary>
        public LJX8IF_PROFILE_INFO ProfInfo
        {
            get { return _profileInfo; }
        }
        #endregion

        #region Method
        #region Get the byte size

        /// <summary>
        /// Get the byte size of the structure.
        /// </summary>
        /// <param name="type">Structure whose byte size you want to get.</param>
        /// <returns>Byte size</returns>
        public int GetByteSize( TypeOfStructure type )
        {
            switch ( type ) {
                case TypeOfStructure.ProfileHeader:
                    LJX8IF_PROFILE_HEADER profileHeader = new LJX8IF_PROFILE_HEADER();
                    return Marshal.SizeOf( profileHeader );

                case TypeOfStructure.ProfileFooter:
                    LJX8IF_PROFILE_FOOTER profileFooter = new LJX8IF_PROFILE_FOOTER();
                    return Marshal.SizeOf( profileFooter );
            }

            return 0;
        }
        #endregion
        /// <summary>
        /// Constructor
        /// </summary>
        public ProfileData( int[] receiveBuffer, LJX8IF_PROFILE_INFO profileInfo )
        {
            SetData( receiveBuffer, profileInfo );
        }

        /// <summary>
        /// Constructor Overload
        /// </summary>
        /// <param name="receiveBuffer">Receive buffer</param>
        /// <param name="startIndex">Start position</param>
        /// <param name="profileInfo">Profile information</param>
        public ProfileData( int[] receiveBuffer, int startIndex, LJX8IF_PROFILE_INFO profileInfo )
        {
            int bufIntSize = CalculateDataSize( profileInfo );
            int[] bufIntArray = new int[ bufIntSize ];
            _profileInfo = profileInfo;

            Array.Copy( receiveBuffer, startIndex, bufIntArray, 0, bufIntSize );
            SetData( bufIntArray, profileInfo );
        }

        /// <summary>
        /// Set the members to the arguments.
        /// </summary>
        /// <param name="receiveBuffer">Receive buffer</param>
        /// <param name="profileInfo">Profile information</param>
        private void SetData( int[] receiveBuffer, LJX8IF_PROFILE_INFO profileInfo )
        {
            _profileInfo = profileInfo;

            // Extract the header.
            int headerSize = GetByteSize( TypeOfStructure.ProfileHeader ) / Marshal.SizeOf( typeof( int ) );
            int[] headerData = new int[ headerSize ];
            Array.Copy( receiveBuffer, 0, headerData, 0, headerSize );

            // Extract the footer.
            int footerSize = GetByteSize( TypeOfStructure.ProfileFooter ) / Marshal.SizeOf( typeof( int ) );
            int[] footerData = new int[ footerSize ];
            Array.Copy( receiveBuffer, receiveBuffer.Length - footerSize, footerData, 0, footerSize );

            // Extract the profile data.
            int profSize = receiveBuffer.Length - headerSize - footerSize;
            _profData = new int[ profSize ];
            Array.Copy( receiveBuffer, headerSize, _profData, 0, profSize );
        }

        /// <summary>
        /// Data size calculation
        /// </summary>
        /// <param name="profileInfo">Profile information</param>
        /// <returns>Profile data size</returns>
        public static int CalculateDataSize( LJX8IF_PROFILE_INFO profileInfo )
        {
            LJX8IF_PROFILE_HEADER header = new LJX8IF_PROFILE_HEADER();
            LJX8IF_PROFILE_FOOTER footer = new LJX8IF_PROFILE_FOOTER();

            int multipleValue = GetIsLuminanceOutput( profileInfo ) ? MULTIPLE_VALUE_FOR_LUMINANCE_OUTPUT : 1;
            return profileInfo.nProfileDataCount * multipleValue + ( Marshal.SizeOf( header ) + Marshal.SizeOf( footer ) ) / Marshal.SizeOf( typeof( int ) );
        }

        public static bool GetIsLuminanceOutput( LJX8IF_PROFILE_INFO profileInfo )
        {
            return profileInfo.byLuminanceOutput == LUMINANCE_OUTPUT_ON_VALUE;
        }

        /// <summary>
        /// Create the X-position string from the profile information.
        /// </summary>
        /// <param name="profileInfo">Profile information</param>
        /// <returns>X-position string</returns>
        public static string GetXPositionString( LJX8IF_PROFILE_INFO profileInfo )
        {
            StringBuilder sb = new StringBuilder();
            // Data position calculation
            double posX = profileInfo.lXStart;
            double deltaX = profileInfo.lXPitch;

            int singleProfileCount = profileInfo.nProfileDataCount;
            int dataCount = profileInfo.byProfileCount;

            for ( int i = 0; i < dataCount; i++ ) {
                for ( int j = 0; j < singleProfileCount; j++ ) {
                    sb.AppendFormat( "{0}\t", ( posX + deltaX * j ) );
                }
            }
            return sb.ToString();
        }

        #endregion
    }
    #endregion

    #region ThreadBuffer
    /// <summary>
    /// Thread-safe class for array storage
    /// </summary>
    public static class ThreadSafeBuffer {
        #region Constant
        private const int BatchFinalizeFlagBitCount = 16;
        #endregion

        #region Field
        /// <summary>Data buffer</summary>
        private static List<int[]>[] _buffer = new List<int[]>[ NativeMethodsLJX8000A.DeviceCount ];
        /// <summary>Buffer for the amount of data</summary>
        private static uint[] _count = new uint[ NativeMethodsLJX8000A.DeviceCount ];
        /// <summary>Object for exclusive control</summary>
        private static object[] _syncObject = new object[ NativeMethodsLJX8000A.DeviceCount ];
        /// <summary>Callback function notification parameter</summary>
        private static uint[] _notify = new uint[ NativeMethodsLJX8000A.DeviceCount ];
        /// <summary>Batch number</summary>
        private static int[] _batchNo = new int[ NativeMethodsLJX8000A.DeviceCount ];
        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        static ThreadSafeBuffer()
        {
            for ( int i = 0; i < NativeMethodsLJX8000A.DeviceCount; i++ ) {
                _buffer[ i ] = new List<int[]>();
                _syncObject[ i ] = new object();
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// Get buffer data count
        /// </summary>
        /// <returns>buffer data count</returns>
        public static int GetBufferDataCount( int index )
        {
            return _buffer[ index ].Count;
        }

        /// <summary>
        /// Element addition
        /// </summary>
        /// <param name="index">User information set when high-speed communication was initialized</param>
        /// <param name="value">Additional element</param>
        /// <param name="notify">Parameter for notification</param>
        public static void Add( int index, List<int[]> value, uint notify )
        {
            lock ( _syncObject[ index ] ) {
                _buffer[ index ].AddRange( value );
                _count[ index ] += ( uint )value.Count;
                _notify[ index ] |= notify;
                // Add the batch number if the batch has been finalized.
                if ( ( uint )( notify & ( 0x1 << BatchFinalizeFlagBitCount ) ) != 0 ) _batchNo[ index ]++;
            }
        }

        /// <summary>
        /// Clear elements.
        /// </summary>
        /// <param name="index">Device ID</param>
        public static void Clear( int index )
        {
            lock ( _syncObject[ index ] ) {
                _buffer[ index ].Clear();
            }
        }

        /// <summary>
        /// Clear the buffer.
        /// </summary>
        /// <param name="index">Device ID</param>
        public static void ClearBuffer( int index )
        {
            Clear( index );
            ClearCount( index );
            _batchNo[ index ] = 0;
            ClearNotify( index );
        }

        /// <summary>
        /// Get element.
        /// </summary>
        /// <param name="index">Device ID</param>
        /// <param name="notify">Parameter for notification</param>
        /// <param name="batchNo">Batch number</param>
        /// <returns>Element</returns>
        public static List<int[]> Get( int index, out uint notify, out int batchNo )
        {
            List<int[]> value = new List<int[]>();
            lock ( _syncObject[ index ] ) {
                value.AddRange( _buffer[ index ] );
                _buffer[ index ].Clear();
                notify = _notify[ index ];
                _notify[ index ] = 0;
                batchNo = _batchNo[ index ];
            }
            return value;
        }

        /// <summary>
        /// Add the count
        /// </summary>
        /// <param name="index">Device ID</param>
        /// <param name="count">Count</param>
        /// <param name="notify">Parameter for notification</param>
        internal static void AddCount( int index, uint count, uint notify )
        {
            lock ( _syncObject[ index ] ) {
                _count[ index ] += count;
                _notify[ index ] |= notify;
                // Add the batch number if the batch has been finalized.
                if ( ( uint )( notify & ( 0x1 << BatchFinalizeFlagBitCount ) ) != 0 ) _batchNo[ index ]++;
            }
        }

        /// <summary>
        /// Get the count
        /// </summary>
        /// <param name="index">Device ID</param>
        /// <param name="notify">Parameter for notification</param>
        /// <param name="batchNo">Batch number</param>
        /// <returns></returns>
        internal static uint GetCount( int index, out uint notify, out int batchNo )
        {
            lock ( _syncObject[ index ] ) {
                notify = _notify[ index ];
                _notify[ index ] = 0;
                batchNo = _batchNo[ index ];
                return _count[ index ];
            }
        }

        /// <summary>
        /// Clear the number of elements.
        /// </summary>
        /// <param name="index">Device ID</param>
        private static void ClearCount( int index )
        {
            lock ( _syncObject[ index ] ) {
                _count[ index ] = 0;
            }
        }

        /// <summary>
        /// Clear notifications.
        /// </summary>
        /// <param name="index">Device ID</param>
        private static void ClearNotify( int index )
        {
            lock ( _syncObject[ index ] ) {
                _notify[ index ] = 0;
            }
        }

        #endregion
    }
    #endregion

    #region Method
    /// <summary>
    /// Callback function for high-speed communication
    /// </summary>
    /// <param name="pBuffer">Received profile data pointer</param>
    /// <param name="dwSize">Size in units of bytes of one profile</param>
    /// <param name="dwCount">Number of profiles</param>
    /// <param name="dwNotify">Finalization condition</param>
    /// <param name="dwUser">Thread ID</param>
    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    public delegate void HighSpeedDataCallBack( IntPtr pBuffer, uint dwSize, uint dwCount, uint dwNotify, uint dwUser );

    /// <summary>
    /// Callback function for high-speed communication simple array
    /// </summary>
    /// <param name="pProfileHeaderArray">Received header data array pointer</param>
    /// <param name="pHeightProfileArray">Received profile data array pointer</param>
    /// <param name="pLuminanceProfileArray">Received luminance profile data array pointer</param>
    /// <param name="dwLuminanceEnable">The value indicating whether luminance data output is enable or not</param>
    /// <param name="dwProfileDataCount">The data count of one profile</param>
    /// <param name="dwCount">Number of profiles</param>
    /// <param name="dwNotify">Finalization condition</param>
    /// <param name="dwUser">Thread ID</param>
    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    public delegate void HighSpeedDataCallBackForSimpleArray( IntPtr pProfileHeaderArray, IntPtr pHeightProfileArray, IntPtr pLuminanceProfileArray, uint dwLuminanceEnable, uint dwProfileDataCount, uint dwCount, uint dwNotify, uint dwUser );

    /// <summary>
    /// Function definitions
    /// </summary>
    internal class NativeMethodsLJX8000A {
        /// <summary>
        /// Number of connectable devices
        /// </summary>
        internal static int DeviceCount
        {
            get { return 6; }
        }

        /// <summary>
        /// Fixed value for the bytes of environment settings data 
        /// </summary>
        internal static UInt32 EnvironmentSettingSize
        {
            get { return 60; }
        }

        /// <summary>
        /// Fixed value for the bytes of common measurement settings data 
        /// </summary>
        internal static UInt32 CommonSettingSize
        {
            get { return 20; }
        }

        /// <summary>
        /// Fixed value for the bytes of program settings data 
        /// </summary>
        internal static UInt32 ProgramSettingSize
        {
            get { return 10980; }
        }

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_Initialize();

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_Finalize();

        [DllImport( "LJX8_IF.dll" )]
        internal static extern LJX8IF_VERSION_INFO LJX8IF_GetVersion();

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_EthernetOpen( int lDeviceId, ref LJX8IF_ETHERNET_CONFIG pEthernetConfig );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_CommunicationClose( int lDeviceId );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_RebootController( int lDeviceId );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_ReturnToFactorySetting( int lDeviceId );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_ControlLaser( int lDeviceId, byte byState );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_GetError( int lDeviceId, byte byReceivedMax, ref byte pbyErrCount, IntPtr pwErrCode );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_ClearError( int lDeviceId, short wErrCode );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_TrgErrorReset( int lDeviceId );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_GetTriggerAndPulseCount( int lDeviceId, ref uint pdwTriggerCount, ref int plEncoderCount );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_SetTimerCount( int lDeviceId, uint dwTimerCount );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_GetTimerCount( int lDeviceId, ref uint pdwTimerCount );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_GetHeadTemperature( int lDeviceId, ref short pnSensorTemperature, ref short pnProcessorTemperature, ref short pnCaseTemperature );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_GetHeadModel( int lDeviceId, IntPtr pHeadModel );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_GetSerialNumber( int lDeviceId, IntPtr pControllerSerialNo, IntPtr pHeadSerialNo );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_GetAttentionStatus( int lDeviceId, ref ushort pwAttentionStatus );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_Trigger( int lDeviceId );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_StartMeasure( int lDeviceId );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_StopMeasure( int lDeviceId );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_ClearMemory( int lDeviceId );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_SetSetting( int lDeviceId, byte byDepth, LJX8IF_TARGET_SETTING targetSetting, IntPtr pData, uint dwDataSize, ref uint pdwError );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_GetSetting( int lDeviceId, byte byDepth, LJX8IF_TARGET_SETTING targetSetting, IntPtr pData, uint dwDataSize );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_InitializeSetting( int lDeviceId, byte byDepth, byte byTarget );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_ReflectSetting( int lDeviceId, byte byDepth, ref uint pdwError );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_RewriteTemporarySetting( int lDeviceId, byte byDepth );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_CheckMemoryAccess( int lDeviceId, ref byte pbyBusy );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_ChangeActiveProgram( int lDeviceId, byte byProgramNo );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_GetActiveProgram( int lDeviceId, ref byte pbyProgramNo );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_SetXpitch( int lDeviceId, uint dwXpitch );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_GetXpitch( int lDeviceId, ref uint pdwXpitch );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_GetProfile( int lDeviceId, ref LJX8IF_GET_PROFILE_REQUEST pReq,
        ref LJX8IF_GET_PROFILE_RESPONSE pRsp, ref LJX8IF_PROFILE_INFO pProfileInfo, IntPtr pdwProfileData, uint dwDataSize );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_GetBatchProfile( int lDeviceId, ref LJX8IF_GET_BATCH_PROFILE_REQUEST pReq,
        ref LJX8IF_GET_BATCH_PROFILE_RESPONSE pRsp, ref LJX8IF_PROFILE_INFO pProfileInfo,
        IntPtr pdwBatchData, uint dwDataSize );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_GetBatchSimpleArray( int lDeviceId, ref LJX8IF_GET_BATCH_PROFILE_REQUEST pReq,
        ref LJX8IF_GET_BATCH_PROFILE_RESPONSE pRsp, ref LJX8IF_PROFILE_INFO pProfileInfo,
        IntPtr pProfileHeaderArray, IntPtr pHeightProfileArray, IntPtr pLuminanceProfileArray );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_InitializeHighSpeedDataCommunication(
        int lDeviceId, ref LJX8IF_ETHERNET_CONFIG pEthernetConfig, ushort wHighSpeedPortNo,
        HighSpeedDataCallBack pCallBack, uint dwProfileCount, uint dwThreadId );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_InitializeHighSpeedDataCommunicationSimpleArray(
        int lDeviceId, ref LJX8IF_ETHERNET_CONFIG pEthernetConfig, ushort wHighSpeedPortNo,
        HighSpeedDataCallBackForSimpleArray pCallBackSimpleArray, uint dwProfileCount, uint dwThreadId );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_PreStartHighSpeedDataCommunication(
        int lDeviceId, ref LJX8IF_HIGH_SPEED_PRE_START_REQUEST pReq,
        ref LJX8IF_PROFILE_INFO pProfileInfo );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_StartHighSpeedDataCommunication( int lDeviceId );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_StopHighSpeedDataCommunication( int lDeviceId );

        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_FinalizeHighSpeedDataCommunication( int lDeviceId );
        [DllImport( "LJX8_IF.dll" )]
        internal static extern int LJX8IF_GetZUnitSimpleArray( int lDeviceId, ref ushort pwZUnit );
    }
    #endregion
}
