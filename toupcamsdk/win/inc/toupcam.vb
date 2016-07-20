Imports System.Runtime.InteropServices
Imports Microsoft.Win32.SafeHandles
Imports System.Security.Permissions
Imports System.Runtime.ConstrainedExecution
Imports System.Drawing

'    Versin: 1.6.5660.20150520
'
'    For Microsoft .NET Framework.
'
'    We use P/Invoke to call into the toupcam.dll API, the VB.net class ToupCam is a thin wrapper class to the native api of toupcam.dll.
'    So the manual en.html and hans.html are also applicable for programming with toupcam.vb.
'    See it in the 'doc' directory.
'

Namespace ToupTek
    Public Class SafeHToupCamHandle
        Inherits SafeHandleZeroOrMinusOneIsInvalid
        <DllImport("toupcam.dll", ExactSpelling := True, CallingConvention := CallingConvention.StdCall)> _
        Private Shared Sub Toupcam_Close(h As IntPtr)
        End Sub

        Public Sub New()
            MyBase.New(True)
        End Sub

        <ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)> _
        Protected Overrides Function ReleaseHandle() As Boolean
            ' Here, we must obey all rules for constrained execution regions.
            Toupcam_Close(handle)
            Return True
        End Function
    End Class

    Public Class ToupCam
        Implements IDisposable

        <Flags> _
        Public Enum eFALG As UInteger
            FLAG_CMOS = &H1                 ' cmos sensor
            FLAG_CCD_PROGRESSIVE = &H2      ' progressive ccd sensor
            FLAG_CCD_INTERLACED = &H4       ' interlaced ccd sensor
            FLAG_ROI_HARDWARE = &H8         ' support hardware ROI
            FLAG_MONO = &H10                ' monochromatic
            FLAG_BINSKIP_SUPPORTED = &H20   ' support bin/skip mode
            FLAG_USB30 = &H40               ' USB 3.0
            FLAG_COOLED = &H80              ' Cooled
            FLAG_USB30_OVER_USB20 = &H100   ' usb3.0 camera connected to usb2.0 port
            FLAG_ST4 = &H200                ' ST4
            FLAG_GETTEMPERATURE = &H400     ' support to get the temperature of sensor
            FLAG_PUTTEMPERATURE = &H800     ' support to put the temperature of sensor
            FLAG_BITDEPTH10 = &H1000        ' Maximum Bit Depth = 10
            FLAG_BITDEPTH12 = &H2000        ' Maximum Bit Depth = 12
            FLAG_BITDEPTH14 = &H4000        ' Maximum Bit Depth = 14
            FLAG_BITDEPTH16 = &H8000        ' Maximum Bit Depth = 16
            FLAG_FAN = &H10000              ' cooling fan
            FLAG_COOLERONOFF = &H20000      ' cooler can be turn on or off
            FLAG_ISP = &H40000              ' image signal processing supported
            FLAG_TRIGGER = &H80000          ' support the trigger mode
        End Enum

        Public Enum eEVENT As UInteger
            EVENT_EXPOSURE = &H1       ' exposure time changed
            EVENT_TEMPTINT = &H2       ' white balance changed, Temp/Tint mode
            EVENT_CHROME = &H3         ' reversed, do not use it
            EVENT_IMAGE = &H4          ' live image arrived, use Toupcam_PullImage to get this image
            EVENT_STILLIMAGE = &H5     ' snap (still) frame arrived, use Toupcam_PullStillImage to get this frame
            EVENT_WBGAIN = &H6         ' white balance changed, RGB Gain mode
            EVENT_ERROR = &H80         ' something error happens
            EVENT_DISCONNECTED = &H81  ' camera disconnected
        End Enum

        Public Enum ePROCESSMODE As UInteger
            PROCESSMODE_FULL = &H0 ' better image quality, more cpu usage. this is the default value
            PROCESSMODE_FAST = &H1 ' lower image quality, less cpu usage
        End Enum

        Public Enum eOPTION As UInteger
            OPTION_NOFRAME_TIMEOUT = &H1 ' iValue: 1 = enable; 0 = disable. default: enable
            OPTION_THREAD_PRIORITY = &H2 ' set the priority of the internal thread which grab data from the usb device. iValue: 0 = THREAD_PRIORITY_NORMAL; 1 = THREAD_PRIORITY_ABOVE_NORMAL; 2 = THREAD_PRIORITY_HIGHEST; default: 0; see: msdn SetThreadPriority
            OPTION_PROCESSMODE = &H3     ' better image quality, more cpu usage. this is the default value
                                         ' 1 = lower image quality, less cpu usage
            OPTION_RAW = &H4             ' raw mode, read the sensor data. This can be set only BEFORE Toupcam_StartXXX()
            OPTION_HISTOGRAM = &H5       ' 0 = only one, 1 = continue mode
            OPTION_BITDEPTH = &H6        ' 0 = 8bits mode, 1 = 16bits mode
            OPTION_FAN = &H7             ' 0 = turn off the cooling fan, 1 = turn on the cooling fan
            OPTION_COOLER = &H8          ' 0 = turn off cooler, 1 = turn on cooler
            OPTION_LINEAR = &H9          ' 0 = turn off tone linear, 1 = turn on tone linear
            OPTION_CURVE = &Ha           ' 0 = turn off tone curve, 1 = turn on tone curve
            OPTION_TRIGGER = &Hb         ' 0 = continuous mode, 1 = trigger mode, default value = 0
            OPTION_RGB48 = &Hc           ' enable RGB48 format when bitdepth > 8
        End Enum

        <StructLayout(LayoutKind.Sequential)> _
        Public Structure BITMAPINFOHEADER
            Public biSize As UInteger
            Public biWidth As Integer
            Public biHeight As Integer
            Public biPlanes As UShort
            Public biBitCount As UShort
            Public biCompression As UInteger
            Public biSizeImage As UInteger
            Public biXPelsPerMeter As Integer
            Public biYPelsPerMeter As Integer
            Public biClrUsed As UInteger
            Public biClrImportant As UInteger

            Public Sub Init()
                biSize = CUInt(Marshal.SizeOf(Me))
            End Sub
        End Structure

        Public Structure Resolution
            Public width As UInteger
            Public height As UInteger
        End Structure
        Public Structure Model
            Public name As String
            Public flag As eFALG
            Public maxspeed As UInteger
            Public preview As UInteger
            Public still As UInteger
            Public res As Resolution()
        End Structure
        Public Structure Instance
            Public displayname As String
            Public id As String
            Public model As Model
        End Structure

        <DllImport("kernel32.dll", EntryPoint:="CopyMemory")> _
        Public Shared Sub CopyMemory(Destination As IntPtr, Source As IntPtr, Length As UInteger)
        End Sub

        Public Delegate Sub DelegateEventCallback(nEvent As eEVENT)
        Public Delegate Sub DelegateDataCallback(pData As IntPtr, ByRef header As BITMAPINFOHEADER, bSnap As Boolean)
        Public Delegate Sub DelegateExposureCallback()
        Public Delegate Sub DelegateTempTintCallback(nTemp As Integer, nTint As Integer)
        Public Delegate Sub DelegateWhitebalanceCallback(aGain As Integer())
        Public Delegate Sub DelegateHistogramCallback(aHistY As Double(), aHistR As Double(), aHistG As Double(), aHistB As Double())
        Public Delegate Sub DelegateChromeCallback()

        <UnmanagedFunctionPointerAttribute(CallingConvention.StdCall)> _
        Friend Delegate Sub PTOUPCAM_DATA_CALLBACK(pData As IntPtr, pHeader As IntPtr, bSnap As Boolean, pCallbackCtx As IntPtr)
        <UnmanagedFunctionPointerAttribute(CallingConvention.StdCall)> _
        Friend Delegate Sub PITOUPCAM_EXPOSURE_CALLBACK(pCtx As IntPtr)
        <UnmanagedFunctionPointerAttribute(CallingConvention.StdCall)> _
        Friend Delegate Sub PITOUPCAM_TEMPTINT_CALLBACK(nTemp As Integer, nTint As Integer, pCtx As IntPtr)
        <UnmanagedFunctionPointerAttribute(CallingConvention.StdCall)> _
        Friend Delegate Sub PITOUPCAM_WHITEBALANCE_CALLBACK(aGain As IntPtr, pCtx As IntPtr)        
        <UnmanagedFunctionPointerAttribute(CallingConvention.StdCall)> _
        Friend Delegate Sub PITOUPCAM_HISTOGRAM_CALLBACK(aHistY As IntPtr, aHistR As IntPtr, aHistG As IntPtr, aHistB As IntPtr, pCtx As IntPtr)
        <UnmanagedFunctionPointerAttribute(CallingConvention.StdCall)> _
        Friend Delegate Sub PITOUPCAM_CHROME_CALLBACK(pCtx As IntPtr)
        <UnmanagedFunctionPointerAttribute(CallingConvention.StdCall)> _
        Friend Delegate Sub PTOUPCAM_EVENT_CALLBACK(nEvent As eEVENT, pCtx As IntPtr)

        <StructLayout(LayoutKind.Sequential)> _
        Private Structure RECT
            Public left As Integer, top As Integer, right As Integer, bottom As Integer
        End Structure

        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_Version() As IntPtr
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_Enum(ti As IntPtr) As UInteger
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_Open(<MarshalAs(UnmanagedType.LPWStr)> id As String) As SafeHToupCamHandle
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_StartPullModeWithWndMsg(h As SafeHToupCamHandle, hWnd As IntPtr, nMsg As UInteger) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_StartPullModeWithCallback(h As SafeHToupCamHandle, pEventCallback As PTOUPCAM_EVENT_CALLBACK, pCallbackCtx As IntPtr) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_PullImage(h As SafeHToupCamHandle, pImageData As IntPtr, bits As Integer, ByRef pnWidth As UInteger, ByRef pnHeight As UInteger) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_PullStillImage(h As SafeHToupCamHandle, pImageData As IntPtr, bits As Integer, ByRef pnWidth As UInteger, ByRef pnHeight As UInteger) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_StartPushMode(h As SafeHToupCamHandle, pDataCallback As PTOUPCAM_DATA_CALLBACK, pCallbackCtx As IntPtr) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_Stop(h As SafeHToupCamHandle) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_Pause(h As SafeHToupCamHandle, bPause As Integer) As Integer
        End Function

        ' for still image snap
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_Snap(h As SafeHToupCamHandle, nResolutionIndex As UInteger) As Integer
        End Function

        ' for soft trigger
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_Trigger(h As SafeHToupCamHandle) As Integer
        End Function
        
        '
        '  put_Size, put_eSize, can be used to set the video output resolution BEFORE Start.
        '  put_Size use width and height parameters, put_eSize use the index parameter.
        '  for example, UCMOS03100KPA support the following resolutions:
        '      index 0:    2048,   1536
        '      index 1:    1024,   768
        '      index 2:    680,    510
        '  so, we can use put_Size(h, 1024, 768) or put_eSize(h, 1). Both have the same effect.
        ' 
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_Size(h As SafeHToupCamHandle, nWidth As Integer, nHeight As Integer) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_Size(h As SafeHToupCamHandle, ByRef nWidth As Integer, ByRef nHeight As Integer) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_eSize(h As SafeHToupCamHandle, nResolutionIndex As UInteger) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_eSize(h As SafeHToupCamHandle, ByRef nResolutionIndex As UInteger) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_ResolutionNumber(h As SafeHToupCamHandle) As UInteger
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_Resolution(h As SafeHToupCamHandle, nResolutionIndex As UInteger, ByRef pWidth As Integer, ByRef pHeight As Integer) As UInteger
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_ResolutionRatio(h As SafeHToupCamHandle, nResolutionIndex As UInteger, ByRef pNumerator As Integer, ByRef pDenominator As Integer) As UInteger
        End Function
        
        ' FourCC:
        '   MAKEFOURCC('G', 'B', 'R', 'G')
        '   MAKEFOURCC('R', 'G', 'G', 'B')
        '   MAKEFOURCC('B', 'G', 'G', 'R')
        '   MAKEFOURCC('G', 'R', 'B', 'G')
        '   MAKEFOURCC('Y', 'U', 'Y', 'V')
        '   MAKEFOURCC('Y', 'Y', 'Y', 'Y')
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_RawFormat(h As SafeHToupCamHandle, ByRef nFourCC As UInteger, ByRef bitdepth As UInteger) As UInteger
        End Function

        '
        ' set or get the process mode: TOUPCAM_PROCESSMODE_FULL or TOUPCAM_PROCESSMODE_FAST
        '        
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_ProcessMode(h As SafeHToupCamHandle, nProcessMode As ePROCESSMODE) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_ProcessMode(h As SafeHToupCamHandle, ByRef pnProcessMode As ePROCESSMODE) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_RealTime(h As SafeHToupCamHandle, bEnable As Integer) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_RealTime(h As SafeHToupCamHandle, ByRef bEnable As Integer) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_Flush(h As SafeHToupCamHandle) As Integer
        End Function

        ' sensor Temperature
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_Temperature(h As SafeHToupCamHandle, ByRef pTemperature As Short) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_Temperature(h As SafeHToupCamHandle, nTemperature As Short) As Integer
        End Function
        
        ' ROI
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_Roi(h As SafeHToupCamHandle, ByRef xOffsett As UInteger, ByRef yOffsett As UInteger, ByRef xWidtht As UInteger, ByRef yHeightt As UInteger) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_Roi(h As SafeHToupCamHandle, pxOffset As UInteger, pyOffset As UInteger, pxWidth As UInteger, pyHeight As UInteger) As Integer
        End Function
        
        '
        '  ------------------------------------------------------------------|
        '  | Parameter               |   Range       |   Default             |
        '  |-----------------------------------------------------------------|
        '  | Auto Exposure Target    |   16~235      |   120                 |
        '  | Temp                    |   2000~15000  |   6503                |
        '  | Tint                    |   200~2500    |   1000                |
        '  | LevelRange              |   0~255       |   Low = 0, High = 255 |
        '  | Contrast                |   -100~100    |   0                   |
        '  | Hue                     |   -180~180    |   0                   |
        '  | Saturation              |   0~255       |   128                 |
        '  | Brightness              |   -64~64      |   0                   |
        '  | Gamma                   |   20~180      |   100                 |
        '  | WBGain                  |   -128~128    |   0                   |
        '  ------------------------------------------------------------------|
        '
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_AutoExpoEnable(h As SafeHToupCamHandle, ByRef bAutoExposure As Integer) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_AutoExpoEnable(h As SafeHToupCamHandle, bAutoExposure As Integer) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_AutoExpoTarget(h As SafeHToupCamHandle, ByRef Target As UShort) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_AutoExpoTarget(h As SafeHToupCamHandle, Target As UShort) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_MaxAutoExpoTimeAGain(h As SafeHToupCamHandle, maxTime As UInteger, maxAGain As UShort) As Integer
        End Function

        ' in microseconds
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_ExpoTime(h As SafeHToupCamHandle, ByRef Time As UInteger) As Integer
        End Function

        ' inmicroseconds
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_ExpoTime(h As SafeHToupCamHandle, Time As UInteger) As Integer
        End Function

        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_ExpTimeRange(h As SafeHToupCamHandle, ByRef nMin As UInteger, ByRef nMax As UInteger, ByRef nDef As UInteger) As Integer
        End Function

        ' percent, such as 300 
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_ExpoAGain(h As SafeHToupCamHandle, ByRef AGain As UShort) As Integer
        End Function

        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_ExpoAGain(h As SafeHToupCamHandle, AGain As UShort) As Integer
        End Function

        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_ExpoAGainRange(h As SafeHToupCamHandle, ByRef nMin As UShort, ByRef nMax As UShort, ByRef nDef As UShort) As Integer
        End Function

        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_LevelRange(h As SafeHToupCamHandle, <[In]> aLow As UShort(), <[In]> aHigh As UShort()) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_LevelRange(h As SafeHToupCamHandle, <Out> aLow As UShort(), <Out> aHigh As UShort()) As Integer
        End Function

        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_Hue(h As SafeHToupCamHandle, Hue As Integer) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_Hue(h As SafeHToupCamHandle, ByRef Hue As Integer) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_Saturation(h As SafeHToupCamHandle, Saturation As Integer) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_Saturation(h As SafeHToupCamHandle, ByRef Saturation As Integer) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_Brightness(h As SafeHToupCamHandle, Brightness As Integer) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_Brightness(h As SafeHToupCamHandle, ByRef Brightness As Integer) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_Contrast(h As SafeHToupCamHandle, ByRef Contrast As Integer) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_Contrast(h As SafeHToupCamHandle, Contrast As Integer) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_Gamma(h As SafeHToupCamHandle, ByRef Gamma As Integer) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_Gamma(h As SafeHToupCamHandle, Gamma As Integer) As Integer
        End Function

        ' monochromatic mode
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_Chrome(h As SafeHToupCamHandle, ByRef bChrome As Integer) As Integer
        End Function

        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_Chrome(h As SafeHToupCamHandle, bChrome As Integer) As Integer
        End Function

        ' vertical flip
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_VFlip(h As SafeHToupCamHandle, ByRef bVFlip As Integer) As Integer
        End Function

        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_VFlip(h As SafeHToupCamHandle, bVFlip As Integer) As Integer
        End Function

        ' horizontal flip
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_HFlip(h As SafeHToupCamHandle, ByRef bHFlip As Integer) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_HFlip(h As SafeHToupCamHandle, bHFlip As Integer) As Integer
        End Function

        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_Negative(h As SafeHToupCamHandle, ByRef bNegative As Integer) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_Negative(h As SafeHToupCamHandle, bNegative As Integer) As Integer
        End Function

        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_Speed(h As SafeHToupCamHandle, nSpeed As UShort) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_Speed(h As SafeHToupCamHandle, ByRef pSpeed As UShort) As Integer
        End Function

        ' get the maximum speed, "Frame Speed Level", speed range = [0, max]
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_MaxSpeed(h As SafeHToupCamHandle) As UInteger
        End Function

        ' get the max bit depth of this camera, such as 8, 10, 12, 14, 16
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_MaxBitDepth(h As SafeHToupCamHandle) As UInteger
        End Function
        
        ' power supply: 
        '   0 -> 60HZ AC
        '   1 -> 50Hz AC
        '   2 -> DC
        '        
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_HZ(h As SafeHToupCamHandle, nHZ As Integer) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_HZ(h As SafeHToupCamHandle, ByRef nHZ As Integer) As Integer
        End Function

        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_Mode(h As SafeHToupCamHandle, bSkip As Integer) As Integer
        End Function
        ' skip or bin 
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_Mode(h As SafeHToupCamHandle, ByRef bSkip As Integer) As Integer
        End Function

        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_TempTint(h As SafeHToupCamHandle, nTemp As Integer, nTint As Integer) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_TempTint(h As SafeHToupCamHandle, ByRef nTemp As Integer, ByRef nTint As Integer) As Integer
        End Function
        
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_WhiteBalanceGain(h As SafeHToupCamHandle, <[In]> aGain As Integer()) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_WhiteBalanceGain(h As SafeHToupCamHandle, <Out> aGain As Integer()) As Integer
        End Function

        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_AWBAuxRect(h As SafeHToupCamHandle, ByRef pAuxRect As RECT) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_AWBAuxRect(h As SafeHToupCamHandle, ByRef pAuxRect As RECT) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_AEAuxRect(h As SafeHToupCamHandle, ByRef pAuxRect As RECT) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_AEAuxRect(h As SafeHToupCamHandle, ByRef pAuxRect As RECT) As Integer
        End Function

        '
        '  S_FALSE:    color mode
        '  S_OK:       mono mode, such as EXCCD00300KMA and UHCCD01400KMA
        '        
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_MonoMode(h As SafeHToupCamHandle) As Integer
        End Function

        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_StillResolutionNumber(h As SafeHToupCamHandle) As UInteger
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_StillResolution(h As SafeHToupCamHandle, nIndex As UInteger, ByRef pWidth As Integer, ByRef pHeight As Integer) As Integer
        End Function

        '
        ' get the serial number which is always 32 chars which is zero-terminated such as "TP110826145730ABCD1234FEDC56787"
        '        
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_SerialNumber(h As SafeHToupCamHandle, sn As IntPtr) As Integer
        End Function

        '
        ' get the firmware version, such as: 3.2.1.20140922
        '        
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_FwVersion(h As SafeHToupCamHandle, fwver As IntPtr) As Integer
        End Function

        '
        ' get the hardware version, such as: 3.2.1.20140922
        '        
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_HwVersion(h As SafeHToupCamHandle, hwver As IntPtr) As Integer
        End Function

        '
        ' get the production date, such as: 20150327
        '        
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_ProductionDate(h As SafeHToupCamHandle, pdate As IntPtr) As Integer
        End Function
        
        '
        '  ------------------------------------------------------------|
        '  | Parameter         |   Range       |   Default             |
        '  |-----------------------------------------------------------|
        '  | VidgetAmount      |   -100~100    |   0                   |
        '  | VignetMidPoint    |   0~100       |   50                  |
        '  -------------------------------------------------------------
        '        
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_VignetEnable(h As SafeHToupCamHandle, bEnable As Integer) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_VignetEnable(h As SafeHToupCamHandle, ByRef bEnable As Integer) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_VignetAmountInt(h As SafeHToupCamHandle, nAmount As Integer) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_VignetAmountInt(h As SafeHToupCamHandle, ByRef nAmount As Integer) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_VignetMidPointInt(h As SafeHToupCamHandle, nMidPoint As Integer) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_VignetMidPointInt(h As SafeHToupCamHandle, ByRef nMidPoint As Integer) As Integer
        End Function

        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_ExpoCallback(h As SafeHToupCamHandle, fnExpoProc As PITOUPCAM_EXPOSURE_CALLBACK, pExpoCtx As IntPtr) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_ChromeCallback(h As SafeHToupCamHandle, fnChromeProc As PITOUPCAM_CHROME_CALLBACK, pChromeCtx As IntPtr) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_AwbOnePush(h As SafeHToupCamHandle, fnTTProc As PITOUPCAM_TEMPTINT_CALLBACK, pTTCtx As IntPtr) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_AwbInit(h As SafeHToupCamHandle, fnWBProc As PITOUPCAM_WHITEBALANCE_CALLBACK, pWBCtx As IntPtr) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_LevelRangeAuto(h As SafeHToupCamHandle) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_GetHistogram(h As SafeHToupCamHandle, fnHistogramProc As PITOUPCAM_HISTOGRAM_CALLBACK, pHistogramCtx As IntPtr) As Integer
        End Function

        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_LEDState(h As SafeHToupCamHandle, iLed As UShort, iState As UShort, iPeriod As UShort) As Integer
        End Function

        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_write_EEPROM(h As SafeHToupCamHandle, addr As UInteger, pData As IntPtr, nDataLen As UInteger) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_read_EEPROM(h As SafeHToupCamHandle, addr As UInteger, pBuffer As IntPtr, nBufferLen As UInteger) As Integer
        End Function

        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_put_Option(h As SafeHToupCamHandle, iOption As eOPTION, iValue As UInteger) As Integer
        End Function
        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_get_Option(h As SafeHToupCamHandle, iOption As eOPTION, ByRef iValue As UInteger) As Integer
        End Function

        <DllImport("toupcam.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function Toupcam_calc_ClarityFactor(pImageData As IntPtr, bits As Integer, nImgWidth As UInteger, nImgHeight As UInteger) As Double
        End Function

        Private _handle As SafeHToupCamHandle
        Private _gchandle As GCHandle
        Private _dDataCallback As DelegateDataCallback
        Private _dEventCallback As DelegateEventCallback
        Private _dExposureCallback As DelegateExposureCallback
        Private _dTempTintCallback As DelegateTempTintCallback
        Private _dWhitebalanceCallback As DelegateWhitebalanceCallback
        Private _dHistogramCallback As DelegateHistogramCallback
        Private _dChromeCallback As DelegateChromeCallback
        Private _pDataCallback As PTOUPCAM_DATA_CALLBACK
        Private _pEventCallback As PTOUPCAM_EVENT_CALLBACK
        Private _pExposureCallback As PITOUPCAM_EXPOSURE_CALLBACK
        Private _pTempTintCallback As PITOUPCAM_TEMPTINT_CALLBACK
        Private _pWhitebalanceCallback As PITOUPCAM_WHITEBALANCE_CALLBACK
        Private _pHistogramCallback As PITOUPCAM_HISTOGRAM_CALLBACK
        Private _pChromeCallback As PITOUPCAM_CHROME_CALLBACK

        Private Sub EventCallback(nEvent As eEVENT)
            _dEventCallback(nEvent)
        End Sub

        Private Sub DataCallback(pData As IntPtr, pHeader As IntPtr, bSnap As Boolean)
            If pData = IntPtr.Zero OrElse pHeader = IntPtr.Zero Then
                ' pData == 0 means that something error, we callback to tell the application 
                If _dDataCallback IsNot Nothing Then
                    Dim h As New BITMAPINFOHEADER()
                    _dDataCallback(IntPtr.Zero, h, bSnap)
                End If
            Else
                Dim h As BITMAPINFOHEADER = CType(Marshal.PtrToStructure(pHeader, GetType(BITMAPINFOHEADER)), BITMAPINFOHEADER)
                _dDataCallback(pData, h, bSnap)
            End If
        End Sub

        Private Sub ExposureCallback()
            _dExposureCallback()
        End Sub

        Private Sub TempTintCallback(nTemp As Integer, nTint As Integer)
            If _dTempTintCallback IsNot Nothing Then
                _dTempTintCallback(nTemp, nTint)
                _dTempTintCallback = Nothing
            End If
            _pTempTintCallback = Nothing
        End Sub

        Private Sub WhitebalanceCallback(aGain As Integer())
            If _dWhitebalanceCallback IsNot Nothing Then
                _dWhitebalanceCallback(aGain)
                _dWhitebalanceCallback = Nothing
            End If
            _pWhitebalanceCallback = Nothing
        End Sub

        Private Sub ChromeCallback()
            _dChromeCallback()
        End Sub

        Private Sub HistogramCallback(aHistY As Double(), aHistR As Double(), aHistG As Double(), aHistB As Double())
            If _dHistogramCallback IsNot Nothing Then
                _dHistogramCallback(aHistY, aHistR, aHistG, aHistB)
                _dHistogramCallback = Nothing
            End If
            _pHistogramCallback = Nothing
        End Sub

        Private Shared Sub DataCallback(pData As IntPtr, pHeader As IntPtr, bSnap As Boolean, pCallbackCtx As IntPtr)
            Dim gch As GCHandle = GCHandle.FromIntPtr(pCallbackCtx)
            Dim pthis As ToupCam = TryCast(gch.Target, ToupCam)
            If pthis IsNot Nothing Then
                pthis.DataCallback(pData, pHeader, bSnap)
            End If
        End Sub

        Private Shared Sub EventCallback(nEvent As eEVENT, pCallbackCtx As IntPtr)
            Dim gch As GCHandle = GCHandle.FromIntPtr(pCallbackCtx)
            Dim pthis As ToupCam = TryCast(gch.Target, ToupCam)
            If pthis IsNot Nothing Then
                pthis.EventCallback(nEvent)
            End If
        End Sub

        Private Shared Sub ExposureCallback(pCallbackCtx As IntPtr)
            Dim gch As GCHandle = GCHandle.FromIntPtr(pCallbackCtx)
            Dim pthis As ToupCam = TryCast(gch.Target, ToupCam)
            If pthis IsNot Nothing Then
                pthis.ExposureCallback()
            End If
        End Sub

        Private Shared Sub TempTintCallback(nTemp As Integer, nTint As Integer, pCallbackCtx As IntPtr)
            Dim gch As GCHandle = GCHandle.FromIntPtr(pCallbackCtx)
            Dim pthis As ToupCam = TryCast(gch.Target, ToupCam)
            If pthis IsNot Nothing Then
                pthis.TempTintCallback(nTemp, nTint)
            End If
        End Sub

        Private Shared Sub WhitebalanceCallback(aGain As IntPtr, pCallbackCtx As IntPtr)
            Dim gch As GCHandle = GCHandle.FromIntPtr(pCallbackCtx)
            Dim pthis As ToupCam = TryCast(gch.Target, ToupCam)
            If pthis IsNot Nothing Then
                Dim newGain As Integer() = New Integer(3) {}
                Marshal.Copy(aGain, newGain, 0, 3)
                pthis.WhitebalanceCallback(newGain)
            End If
        End Sub

        Private Shared Sub ChromeCallback(pCallbackCtx As IntPtr)
            Dim gch As GCHandle = GCHandle.FromIntPtr(pCallbackCtx)
            Dim pthis As ToupCam = TryCast(gch.Target, ToupCam)
            If pthis IsNot Nothing Then
                pthis.ChromeCallback()
            End If
        End Sub

        Private Shared Sub HistogramCallback(aHistY As IntPtr, aHistR As IntPtr, aHistG As IntPtr, aHistB As IntPtr, pCallbackCtx As IntPtr)
            Dim gch As GCHandle = GCHandle.FromIntPtr(pCallbackCtx)
            Dim pthis As ToupCam = TryCast(gch.Target, ToupCam)
            If pthis IsNot Nothing Then
                Dim arrHistY As Double() = New Double(255) {}
                Dim arrHistR As Double() = New Double(255) {}
                Dim arrHistG As Double() = New Double(255) {}
                Dim arrHistB As Double() = New Double(255) {}
                Marshal.Copy(aHistY, arrHistY, 0, 256)
                Marshal.Copy(aHistR, arrHistR, 0, 256)
                Marshal.Copy(aHistG, arrHistG, 0, 256)
                Marshal.Copy(aHistB, arrHistB, 0, 256)
                pthis.HistogramCallback(arrHistY, arrHistR, arrHistG, arrHistB)
            End If
        End Sub

        Protected Overrides Sub Finalize()
            Try
                Dispose(False)
            Finally
                MyBase.Finalize()
            End Try
        End Sub

        <SecurityPermission(SecurityAction.Demand, UnmanagedCode:=True)> _
        Protected Overridable Sub Dispose(disposing As Boolean)
            ' Note there are three interesting states here:
            ' 1) CreateFile failed, _handle contains an invalid handle
            ' 2) We called Dispose already, _handle is closed.
            ' 3) _handle is null, due to an async exception before
            '    calling CreateFile. Note that the finalizer runs
            '    if the constructor fails.
            If _handle IsNot Nothing AndAlso Not _handle.IsInvalid Then
                ' Free the handle
                _handle.Dispose()
            End If
            ' SafeHandle records the fact that we've called Dispose.
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' Follow the Dispose pattern - public nonvirtual.
            Dispose(True)
            If _gchandle.IsAllocated Then
                _gchandle.Free()
            End If
            GC.SuppressFinalize(Me)
        End Sub

        Public Sub Close()
            Dispose()
        End Sub

        ' get the version of this dll, which is: 1.6.5660.20150520
        Public Shared Function Version() As String
            Return Marshal.PtrToStringUni(Toupcam_Version())
        End Function

        ' enumerate ToupCam cameras that are currently connected to computer
        Public Shared Function [Enum]() As Instance()
            Dim ti As IntPtr = Marshal.AllocHGlobal(512 * 16)
            Dim cnt As UInteger = Toupcam_Enum(ti)
            Dim arr As Instance() = New Instance(cnt - 1) {}
            If cnt <> 0 Then
                Dim p As Int64 = ti.ToInt64()
                For i As UInteger = 0 To cnt - 1
                    arr(i).displayname = Marshal.PtrToStringUni(CType(p, IntPtr))
                    p += 2 * 64
                    arr(i).id = Marshal.PtrToStringUni(CType(p, IntPtr))
                    p += 2 * 64

                    Dim pm As IntPtr = Marshal.ReadIntPtr(CType(p, IntPtr))
                    p += IntPtr.Size

                    If True Then
                        Dim q As Int64 = pm.ToInt64()
                        Dim pmn As IntPtr = Marshal.ReadIntPtr(CType(q, IntPtr))
                        arr(i).model.name = Marshal.PtrToStringUni(pmn)
                        q += IntPtr.Size
                        arr(i).model.flag = CUInt(Marshal.ReadInt32(CType(q, IntPtr)))
                        q += 4
                        arr(i).model.maxspeed = CUInt(Marshal.ReadInt32(CType(q, IntPtr)))
                        q += 4
                        arr(i).model.preview = CUInt(Marshal.ReadInt32(CType(q, IntPtr)))
                        q += 4
                        arr(i).model.still = CUInt(Marshal.ReadInt32(CType(q, IntPtr)))
                        q += 4

                        Dim resn As UInteger = Math.Max(arr(i).model.preview, arr(i).model.still)
                        arr(i).model.res = New Resolution(resn - 1) {}
                        For j As UInteger = 0 To resn - 1
                            arr(i).model.res(j).width = CUInt(Marshal.ReadInt32(CType(q, IntPtr)))
                            q += 4
                            arr(i).model.res(j).height = CUInt(Marshal.ReadInt32(CType(q, IntPtr)))
                            q += 4
                        Next
                    End If
                Next
            End If
            Marshal.FreeHGlobal(ti)
            Return arr
        End Function

        ' id: enumerated by Enum
        Public Function Open(id As String) As Boolean
            Dim tmphandle As SafeHToupCamHandle = Toupcam_Open(id)
            If tmphandle Is Nothing OrElse tmphandle.IsInvalid OrElse tmphandle.IsClosed Then
                Return False
            End If
            _handle = tmphandle
            _gchandle = GCHandle.Alloc(Me)
            Return True
        End Function

        Public ReadOnly Property Handle() As SafeHToupCamHandle
            Get
                Return _handle
            End Get
        End Property

        Public ReadOnly Property ResolutionNumber() As UInteger
            Get
                If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                    Return 0
                End If
                Return Toupcam_get_ResolutionNumber(_handle)
            End Get
        End Property

        Public ReadOnly Property StillResolutionNumber() As UInteger
            Get
                If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                    Return 0
                End If
                Return Toupcam_get_StillResolutionNumber(_handle)
            End Get
        End Property

        Public ReadOnly Property MonoMode() As Boolean
            Get
                If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                    Return False
                End If
                Return (0 = Toupcam_get_MonoMode(_handle))
            End Get
        End Property

        ' get the maximum speed, see "Frame Speed Level"
        Public ReadOnly Property MaxSpeed() As UInteger
            Get
                If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                    Return 0
                End If
                Return Toupcam_get_MaxSpeed(_handle)
            End Get
        End Property

        Public ReadOnly Property MaxBitDepth() As UInteger
            Get
                If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                    Return 0
                End If
                Return Toupcam_get_MaxBitDepth(_handle)
            End Get
        End Property
        
        ' get the serial number which is always 32 chars which is zero-terminated such as "TP110826145730ABCD1234FEDC56787"
        Public ReadOnly Property SerialNumber() As String
            Get
                Dim sn As String = ""
                If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                    Return sn
                End If
                Dim ptr As IntPtr = Marshal.AllocHGlobal(64)
                If Toupcam_get_SerialNumber(_handle, ptr) < 0 Then
                    sn = ""
                Else
                    sn = Marshal.PtrToStringAnsi(ptr)
                End If

                Marshal.FreeHGlobal(ptr)
                Return sn
            End Get
        End Property

        ' get the camera firmware version, such as: 3.2.1.20140922
        Public ReadOnly Property FwVersion() As String
            Get
                Dim fwver As String = ""
                If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                    Return fwver
                End If
                Dim ptr As IntPtr = Marshal.AllocHGlobal(32)
                If Toupcam_get_FwVersion(_handle, ptr) < 0 Then
                    fwver = ""
                Else
                    fwver = Marshal.PtrToStringAnsi(ptr)
                End If

                Marshal.FreeHGlobal(ptr)
                Return fwver
            End Get
        End Property

        ' get the camera hardware version, such as: 3.2.1.20140922
        Public ReadOnly Property HwVersion() As String
            Get
                Dim hwver As String = ""
                If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                    Return hwver
                End If
                Dim ptr As IntPtr = Marshal.AllocHGlobal(32)
                If Toupcam_get_HwVersion(_handle, ptr) < 0 Then
                    hwver = ""
                Else
                    hwver = Marshal.PtrToStringAnsi(ptr)
                End If

                Marshal.FreeHGlobal(ptr)
                Return hwver
            End Get
        End Property
        
        ' such as: 20150327
        Public ReadOnly Property ProductionDate() As String
            Get
                Dim pdate As String = ""
                If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                    Return pdate
                End If
                Dim ptr As IntPtr = Marshal.AllocHGlobal(32)
                If Toupcam_get_ProductionDate(_handle, ptr) < 0 Then
                    pdate = ""
                Else
                    pdate = Marshal.PtrToStringAnsi(ptr)
                End If

                Marshal.FreeHGlobal(ptr)
                Return pdate
            End Get
        End Property

        Public Function StartPullModeWithWndMsg(hWnd As IntPtr, nMsg As UInteger) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If

            Return (Toupcam_StartPullModeWithWndMsg(_handle, hWnd, nMsg) >= 0)
        End Function

        Public Function StartPullModeWithCallback(edelegate As DelegateEventCallback) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If

            _dEventCallback = edelegate
            If edelegate Is Nothing Then
                Return (Toupcam_StartPullModeWithCallback(_handle, Nothing, IntPtr.Zero) >= 0)
            Else
                _pEventCallback = New PTOUPCAM_EVENT_CALLBACK(AddressOf EventCallback)
                Return (Toupcam_StartPullModeWithCallback(_handle, _pEventCallback, GCHandle.ToIntPtr(_gchandle)) >= 0)
            End If
        End Function

        '  bits: 24 (RGB24), 32 (RGB32), or 8 (Grey) 
        Public Function PullImage(pImageData As IntPtr, bits As Integer, ByRef pnWidth As UInteger, ByRef pnHeight As UInteger) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                pnWidth = 0
                Return False
            End If

            Return (Toupcam_PullImage(_handle, pImageData, bits, pnWidth, pnHeight) >= 0)
        End Function

        '  bits: 24 (RGB24), 32 (RGB32), or 8 (Grey) 
        Public Function PullStillImage(pImageData As IntPtr, bits As Integer, ByRef pnWidth As UInteger, ByRef pnHeight As UInteger) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                pnWidth = 0
                Return False
            End If

            Return (Toupcam_PullStillImage(_handle, pImageData, bits, pnWidth, pnHeight) >= 0)
        End Function

        Public Function StartPushMode(ddelegate As DelegateDataCallback) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If

            _dDataCallback = ddelegate
            _pDataCallback = New PTOUPCAM_DATA_CALLBACK(AddressOf DataCallback)
            Return (Toupcam_StartPushMode(_handle, _pDataCallback, GCHandle.ToIntPtr(_gchandle)) >= 0)
        End Function

        Public Function [Stop]() As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_Stop(_handle) >= 0)
        End Function

        Public Function Pause(bPause As Boolean) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_Pause(_handle, If(bPause, 1, 0)) >= 0)
        End Function

        Public Function Snap(nResolutionIndex As UInteger) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_Snap(_handle, nResolutionIndex) >= 0)
        End Function
        
        Public Function Trigger() As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_Trigger(_handle) >= 0)
        End Function

        Public Function put_Size(nWidth As Integer, nHeight As Integer) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_put_Size(_handle, nWidth, nHeight) >= 0)
        End Function

        Public Function get_Size(ByRef nWidth As Integer, ByRef nHeight As Integer) As Boolean
            nWidth = 0
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_get_Size(_handle, nWidth, nHeight) >= 0)
        End Function

        Public Function put_eSize(nResolutionIndex As UInteger) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_put_eSize(_handle, nResolutionIndex) >= 0)
        End Function

        Public Function get_eSize(ByRef nResolutionIndex As UInteger) As Boolean
            nResolutionIndex = 0
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_get_eSize(_handle, nResolutionIndex) >= 0)
        End Function

        Public Function get_Resolution(nResolutionIndex As UInteger, ByRef pWidth As Integer, ByRef pHeight As Integer) As Boolean
            pWidth = pHeight = 0
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_get_Resolution(_handle, nResolutionIndex, pWidth, pHeight) >= 0)
        End Function

        Public Function get_ResolutionRatio(nResolutionIndex As UInteger, ByRef pNumerator As Integer, ByRef pDenominator As Integer) As Boolean
            pNumerator = pDenominator = 1
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_get_ResolutionRatio(_handle, nResolutionIndex, pNumerator, pDenominator) >= 0)
        End Function
        
        Public Function get_RawFormat(ByRef nFourCC As UInteger, ByRef bitdepth As UInteger) As Boolean
            nFourCC = bitdepth = 0
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_get_RawFormat(_handle, nFourCC, bitdepth) >= 0)
        End Function

        Public Function put_ProcessMode(nProcessMode As ePROCESSMODE) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_put_ProcessMode(_handle, nProcessMode) >= 0)
        End Function

        Public Function get_ProcessMode(ByRef pnProcessMode As ePROCESSMODE) As Boolean
            pnProcessMode = 0
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_get_ProcessMode(_handle, pnProcessMode) >= 0)
        End Function

        Public Function put_RealTime(bEnable As Boolean) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_put_RealTime(_handle, If(bEnable, 1, 0)) >= 0)
        End Function

        Public Function get_RealTime(ByRef bEnable As Boolean) As Boolean
            bEnable = False
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If

            Dim iEnable As Integer = 0
            If Toupcam_get_RealTime(_handle, iEnable) < 0 Then
                Return False
            End If

            bEnable = (iEnable <> 0)
            Return True
        End Function

        Public Function Flush() As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_Flush(_handle) >= 0)
        End Function

        Public Function get_AutoExpoEnable(ByRef bAutoExposure As Boolean) As Boolean
            bAutoExposure = False
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If

            Dim iEnable As Integer = 0
            If Toupcam_get_AutoExpoEnable(_handle, iEnable) < 0 Then
                Return False
            End If

            bAutoExposure = (iEnable <> 0)
            Return True
        End Function

        Public Function put_AutoExpoEnable(bAutoExposure As Boolean) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_put_AutoExpoEnable(_handle, If(bAutoExposure, 1, 0)) >= 0)
        End Function

        Public Function get_AutoExpoTarget(ByRef Target As UShort) As Boolean
            Target = 0
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_get_AutoExpoTarget(_handle, Target) >= 0)
        End Function

        Public Function put_AutoExpoTarget(Target As UShort) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_put_AutoExpoTarget(_handle, Target) >= 0)
        End Function

        Public Function put_MaxAutoExpoTimeAGain(maxTime As UInteger, maxAGain As UShort) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_put_MaxAutoExpoTimeAGain(_handle, maxTime, maxAGain) >= 0)
        End Function

        Public Function get_ExpoTime(ByRef Time As UInteger) As Boolean
            ' in microseconds 
            Time = 0
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_get_ExpoTime(_handle, Time) >= 0)
        End Function

        Public Function put_ExpoTime(Time As UInteger) As Boolean
            ' in microseconds 
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_put_ExpoTime(_handle, Time) >= 0)
        End Function

        Public Function get_ExpTimeRange(ByRef nMin As UInteger, ByRef nMax As UInteger, ByRef nDef As UInteger) As Boolean
            nMin = nMax = nDef = 0
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_get_ExpTimeRange(_handle, nMin, nMax, nDef) >= 0)
        End Function

        Public Function get_ExpoAGain(ByRef AGain As UShort) As Boolean
            ' percent, such as 300 
            AGain = 0
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_get_ExpoAGain(_handle, AGain) >= 0)
        End Function

        Public Function put_ExpoAGain(AGain As UShort) As Boolean
            ' percent 
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_put_ExpoAGain(_handle, AGain) >= 0)
        End Function

        Public Function get_ExpoAGainRange(ByRef nMin As UShort, ByRef nMax As UShort, ByRef nDef As UShort) As Boolean
            nMin = nMax = nDef = 0
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_get_ExpoAGainRange(_handle, nMin, nMax, nDef) >= 0)
        End Function

        Public Function put_LevelRange(aLow As UShort(), aHigh As UShort()) As Boolean
            If aLow.Length <> 4 OrElse aHigh.Length <> 4 Then
                Return False
            End If
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_put_LevelRange(_handle, aLow, aHigh) >= 0)
        End Function

        Public Function get_LevelRange(aLow As UShort(), aHigh As UShort()) As Boolean
            If aLow.Length <> 4 OrElse aHigh.Length <> 4 Then
                Return False
            End If
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_get_LevelRange(_handle, aLow, aHigh) >= 0)
        End Function

        Public Function put_Hue(Hue As Integer) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_put_Hue(_handle, Hue) >= 0)
        End Function

        Public Function get_Hue(ByRef Hue As Integer) As Boolean
            Hue = 0
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_get_Hue(_handle, Hue) >= 0)
        End Function

        Public Function put_Saturation(Saturation As Integer) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_put_Saturation(_handle, Saturation) >= 0)
        End Function

        Public Function get_Saturation(ByRef Saturation As Integer) As Boolean
            Saturation = 0
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_get_Saturation(_handle, Saturation) >= 0)
        End Function

        Public Function put_Brightness(Brightness As Integer) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_put_Brightness(_handle, Brightness) >= 0)
        End Function

        Public Function get_Brightness(ByRef Brightness As Integer) As Boolean
            Brightness = 0
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_get_Brightness(_handle, Brightness) >= 0)
        End Function

        Public Function get_Contrast(ByRef Contrast As Integer) As Boolean
            Contrast = 0
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_get_Contrast(_handle, Contrast) >= 0)
        End Function

        Public Function put_Contrast(Contrast As Integer) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_put_Contrast(_handle, Contrast) >= 0)
        End Function

        Public Function get_Gamma(ByRef Gamma As Integer) As Boolean
            Gamma = 0
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_get_Gamma(_handle, Gamma) >= 0)
        End Function

        Public Function put_Gamma(Gamma As Integer) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_put_Gamma(_handle, Gamma) >= 0)
        End Function

        Public Function get_Chrome(ByRef bChrome As Boolean) As Boolean
            ' monochromatic mode 
            bChrome = False
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If

            Dim iEnable As Integer = 0
            If Toupcam_get_Chrome(_handle, iEnable) < 0 Then
                Return False
            End If

            bChrome = (iEnable <> 0)
            Return True
        End Function

        Public Function put_Chrome(bChrome As Boolean) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_put_Chrome(_handle, If(bChrome, 1, 0)) >= 0)
        End Function

        Public Function get_VFlip(ByRef bVFlip As Boolean) As Boolean
            ' vertical flip 
            bVFlip = False
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If

            Dim iVFlip As Integer = 0
            If Toupcam_get_VFlip(_handle, iVFlip) < 0 Then
                Return False
            End If

            bVFlip = (iVFlip <> 0)
            Return True
        End Function

        Public Function put_VFlip(bVFlip As Boolean) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_put_VFlip(_handle, If(bVFlip, 1, 0)) >= 0)
        End Function

        Public Function get_HFlip(ByRef bHFlip As Boolean) As Boolean
            bHFlip = False
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If

            Dim iHFlip As Integer = 0
            If Toupcam_get_HFlip(_handle, iHFlip) < 0 Then
                Return False
            End If

            bHFlip = (iHFlip <> 0)
            Return True
        End Function

        Public Function put_HFlip(bHFlip As Boolean) As Boolean
            ' horizontal flip 
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_put_HFlip(_handle, If(bHFlip, 1, 0)) >= 0)
        End Function

        ' negative film
        Public Function get_Negative(ByRef bNegative As Boolean) As Boolean
            bNegative = False
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If

            Dim iNegative As Integer = 0
            If Toupcam_get_Negative(_handle, iNegative) < 0 Then
                Return False
            End If

            bNegative = (iNegative <> 0)
            Return True
        End Function

        ' negative film
        Public Function put_Negative(bNegative As Boolean) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_put_Negative(_handle, If(bNegative, 1, 0)) >= 0)
        End Function

        Public Function put_Speed(nSpeed As UShort) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_put_Speed(_handle, nSpeed) >= 0)
        End Function

        Public Function get_Speed(ByRef pSpeed As UShort) As Boolean
            pSpeed = 0
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_get_Speed(_handle, pSpeed) >= 0)
        End Function

        Public Function put_HZ(nHZ As Integer) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_put_HZ(_handle, nHZ) >= 0)
        End Function

        Public Function get_HZ(ByRef nHZ As Integer) As Boolean
            nHZ = 0
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_get_HZ(_handle, nHZ) >= 0)
        End Function

        Public Function put_Mode(bSkip As Boolean) As Boolean
            ' skip or bin 
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_put_Mode(_handle, If(bSkip, 1, 0)) >= 0)
        End Function

        Public Function get_Mode(ByRef bSkip As Boolean) As Boolean
            bSkip = False
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If

            Dim iSkip As Integer = 0
            If Toupcam_get_Mode(_handle, iSkip) < 0 Then
                Return False
            End If

            bSkip = (iSkip <> 0)
            Return True
        End Function

        ' White Balance, Temp/Tint mode
        Public Function put_TempTint(nTemp As Integer, nTint As Integer) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_put_TempTint(_handle, nTemp, nTint) >= 0)
        End Function

        ' White Balance, Temp/Tint mode
        Public Function get_TempTint(ByRef nTemp As Integer, ByRef nTint As Integer) As Boolean
            nTemp = nTint = 0
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_get_TempTint(_handle, nTemp, nTint) >= 0)
        End Function

        ' White Balance, RGB Gain Mode
        Public Function put_WhiteBalanceGain(aGain As Integer()) As Boolean
            If aGain.Length <> 3 Then
                Return False
            End If
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_put_WhiteBalanceGain(_handle, aGain) >= 0)
        End Function

        ' White Balance, RGB Gain Mode
        Public Function get_WhiteBalanceGain(aGain As Integer()) As Boolean
            If aGain.Length <> 3 Then
                Return False
            End If
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_get_WhiteBalanceGain(_handle, aGain) >= 0)
        End Function
        
        Public Function put_AWBAuxRect(AuxRect As Rectangle) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If

            Dim rc As New RECT()
            rc.left = AuxRect.X
            rc.right = AuxRect.X + AuxRect.Width
            rc.top = AuxRect.Y
            rc.bottom = AuxRect.Y + AuxRect.Height
            Return (Toupcam_put_AWBAuxRect(_handle, rc) >= 0)
        End Function

        Public Function get_AWBAuxRect(ByRef pAuxRect As Rectangle) As Boolean
            pAuxRect = Rectangle.Empty
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If

            Dim rc As New RECT()
            If Toupcam_get_AWBAuxRect(_handle, rc) < 0 Then
                Return False
            End If

            pAuxRect.X = rc.left
            pAuxRect.Y = rc.top
            pAuxRect.Width = rc.right - rc.left
            pAuxRect.Height = rc.bottom - rc.top
            Return True
        End Function

        Public Function put_AEAuxRect(AuxRect As Rectangle) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If

            Dim rc As New RECT()
            rc.left = AuxRect.X
            rc.right = AuxRect.X + AuxRect.Width
            rc.top = AuxRect.Y
            rc.bottom = AuxRect.Y + AuxRect.Height
            Return (Toupcam_put_AEAuxRect(_handle, rc) >= 0)
        End Function

        Public Function get_AEAuxRect(ByRef pAuxRect As Rectangle) As Boolean
            pAuxRect = Rectangle.Empty
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If

            Dim rc As New RECT()
            If Toupcam_get_AEAuxRect(_handle, rc) < 0 Then
                Return False
            End If

            pAuxRect.X = rc.left
            pAuxRect.Y = rc.top
            pAuxRect.Width = rc.right - rc.left
            pAuxRect.Height = rc.bottom - rc.top
            Return True
        End Function

        Public Function get_StillResolution(nIndex As UInteger, ByRef pWidth As Integer, ByRef pHeight As Integer) As Boolean
            pWidth = pHeight = 0
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_get_StillResolution(_handle, nIndex, pWidth, pHeight) >= 0)
        End Function

        Public Function put_VignetEnable(bEnable As Boolean) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_put_VignetEnable(_handle, If(bEnable, 1, 0)) >= 0)
        End Function

        Public Function get_VignetEnable(ByRef bEnable As Boolean) As Boolean
            bEnable = False
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If

            Dim iEanble As Integer = 0
            If Toupcam_get_VignetEnable(_handle, iEanble) < 0 Then
                Return False
            End If

            bEnable = (iEanble <> 0)
            Return True
        End Function

        Public Function put_VignetAmountInt(nAmount As Integer) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_put_VignetAmountInt(_handle, nAmount) >= 0)
        End Function

        Public Function get_VignetAmountInt(ByRef nAmount As Integer) As Boolean
            nAmount = 0
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_get_VignetAmountInt(_handle, nAmount) >= 0)
        End Function

        Public Function put_VignetMidPointInt(nMidPoint As Integer) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_put_VignetMidPointInt(_handle, nMidPoint) >= 0)
        End Function

        Public Function get_VignetMidPointInt(ByRef nMidPoint As Integer) As Boolean
            nMidPoint = 0
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_get_VignetMidPointInt(_handle, nMidPoint) >= 0)
        End Function

        Public Function LevelRangeAuto() As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_LevelRangeAuto(_handle) >= 0)
        End Function

        ' led state:
        '    iLed: Led index, (0, 1, 2, ...)
        '    iState: 1 -> Ever bright; 2 -> Flashing; other -> Off
        '    iPeriod: Flashing Period (>= 500ms)
        Public Function put_LEDState(iLed As UShort, iState As UShort, iPeriod As UShort) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_put_LEDState(_handle, iLed, iState, iPeriod) >= 0)
        End Function

        Public Function write_EEPROM(h As SafeHToupCamHandle, addr As UInteger, pData As IntPtr, nDataLen As UInteger) As Integer
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return 0
            End If
            Return Toupcam_write_EEPROM(_handle, addr, pData, nDataLen)
        End Function

        Public Function read_EEPROM(h As SafeHToupCamHandle, addr As UInteger, pBuffer As IntPtr, nBufferLen As UInteger) As Integer
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return 0
            End If
            Return Toupcam_read_EEPROM(_handle, addr, pBuffer, nBufferLen)
        End Function

        Public Function put_Option(iOption As eOPTION, iValue As UInteger) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_put_Option(_handle, iOption, iValue) >= 0)
        End Function

        Public Function get_Option(iOption As eOPTION, ByRef iValue As UInteger) As Boolean
            iValue = 0
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_get_Option(_handle, iOption, iValue) >= 0)
        End Function

        ' get the temperature of sensor, in 0.1 degrees Celsius (32 means 3.2 degrees Celsius)
        Public Function get_Temperature(ByRef pTemperature As Short) As Boolean
            pTemperature = 0
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_get_Temperature(_handle, pTemperature) >= 0)
        End Function

        ' set the temperature of sensor, in 0.1 degrees Celsius (32 means 3.2 degrees Celsius)
        Public Function put_Temperature(nTemperature As Short) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_put_Temperature(_handle, nTemperature) >= 0)
        End Function

        Public Function get_Roi(ByRef pxOffset As UInteger, ByRef pyOffset As UInteger, ByRef pxWidth As UInteger, ByRef pyHeight As UInteger) As Boolean
            pxOffset = pyOffset = pxWidth = pyHeight = 0
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_get_Roi(_handle, pxOffset, pyOffset, pxWidth, pyHeight) >= 0)
        End Function

        Public Function put_Roi(xOffset As UInteger, yOffset As UInteger, xWidth As UInteger, yHeight As UInteger) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (Toupcam_put_Roi(_handle, xOffset, yOffset, xWidth, yHeight) >= 0)
        End Function
        
        Public Function put_ExpoCallback(fnExpoProc As DelegateExposureCallback) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If

            _dExposureCallback = fnExpoProc
            If fnExpoProc Is Nothing Then
                Return (Toupcam_put_ExpoCallback(_handle, Nothing, IntPtr.Zero) >= 0)
            Else
                _pExposureCallback = New PITOUPCAM_EXPOSURE_CALLBACK(AddressOf ExposureCallback)
                Return (Toupcam_put_ExpoCallback(_handle, _pExposureCallback, GCHandle.ToIntPtr(_gchandle)) >= 0)
            End If
        End Function

        Public Function put_ChromeCallback(fnChromeProc As DelegateChromeCallback) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If

            _dChromeCallback = fnChromeProc
            If fnChromeProc Is Nothing Then
                Return (Toupcam_put_ChromeCallback(_handle, Nothing, IntPtr.Zero) >= 0)
            Else
                _pChromeCallback = New PITOUPCAM_CHROME_CALLBACK(AddressOf ChromeCallback)
                Return (Toupcam_put_ChromeCallback(_handle, _pChromeCallback, GCHandle.ToIntPtr(_gchandle)) >= 0)
            End If
        End Function

        ' Auto White Balance, Temp/Tint Mode
        Public Function AwbOnePush(fnTTProc As DelegateTempTintCallback) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If

            _dTempTintCallback = fnTTProc
            If fnTTProc Is Nothing Then
                Return (Toupcam_AwbOnePush(_handle, Nothing, IntPtr.Zero) >= 0)
            Else
                _pTempTintCallback = New PITOUPCAM_TEMPTINT_CALLBACK(AddressOf TempTintCallback)
                Return (Toupcam_AwbOnePush(_handle, _pTempTintCallback, GCHandle.ToIntPtr(_gchandle)) >= 0)
            End If
        End Function

        ' put_TempTintInit is obsolete, it's a synonyms for AwbOnePush. They are exactly the same 
        Public Function put_TempTintInit(fnTTProc As DelegateTempTintCallback) As Boolean
            Return AwbOnePush(fnTTProc)
        End Function
        
        ' Auto White Balance, RGB Gain Mode
        Public Function AwbInit(fnWBProc As DelegateWhitebalanceCallback) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If

            _dWhitebalanceCallback = fnWBProc
            If fnWBProc Is Nothing Then
                Return (Toupcam_AwbInit(_handle, Nothing, IntPtr.Zero) >= 0)
            Else
                _pWhitebalanceCallback = New PITOUPCAM_WHITEBALANCE_CALLBACK(AddressOf WhitebalanceCallback)
                Return (Toupcam_AwbInit(_handle, _pWhitebalanceCallback, GCHandle.ToIntPtr(_gchandle)) >= 0)
            End If
        End Function

        Public Function GetHistogram(fnHistogramProc As DelegateHistogramCallback) As Boolean
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If

            _dHistogramCallback = fnHistogramProc
            _pHistogramCallback = New PITOUPCAM_HISTOGRAM_CALLBACK(AddressOf HistogramCallback)
            Return (Toupcam_GetHistogram(_handle, _pHistogramCallback, GCHandle.ToIntPtr(_gchandle)) >= 0)
        End Function

        Public Shared Function calcClarityFactor(pImageData As IntPtr, bits As Integer, nImgWidth As UInteger, nImgHeight As UInteger) As Double
            Return Toupcam_calc_ClarityFactor(pImageData, bits, nImgWidth, nImgHeight)
        End Function
    End Class
End Namespace
