#ifndef __toupcam_dshow_h__
#define __toupcam_dshow_h__

/* Version: 1.6.5660.20150520 */

#ifdef _WIN32
#ifndef _INC_WINDOWS
#include <windows.h>
#endif
#endif

#ifndef __TOUPCAM_CALLBACK_DEFINED__
typedef void (__stdcall* PITOUPCAM_EXPOSURE_CALLBACK)(void* pCtx);
typedef void (__stdcall* PITOUPCAM_WHITEBALANCE_CALLBACK)(const int aGain[3], void* pCtx);
typedef void (__stdcall* PITOUPCAM_TEMPTINT_CALLBACK)(const int nTemp, const int nTint, void* pCtx);
typedef void (__stdcall* PITOUPCAM_HISTOGRAM_CALLBACK)(const double aHistY[256], const double aHistR[256], const double aHistG[256], const double aHistB[256], void* pCtx);
typedef void (__stdcall* PITOUPCAM_CHROME_CALLBACK)(void* pCtx);
#endif

#ifndef __ITOUPCAM_DEFINED__
#define __ITOUPCAM_DEFINED__
    /* Both the DirectShow source filter and the output pin support this interface.
        That is to say, you can use QueryInterface on the filter or the pin to get the IToupcam interface. */
    // {66B8EAB5-B594-4141-95FC-691A81B445F6}
    DEFINE_GUID(IID_IToupcam, 0x66b8eab5, 0xb594, 0x4141, 0x95, 0xfc, 0x69, 0x1a, 0x81, 0xb4, 0x45, 0xf6);

    DECLARE_INTERFACE_(IToupcam, IUnknown)
    {
        /*
            put_Size, put_eSize, can be used to set the video output resolution BEFORE the DirectShow source filter is connected.
            put_Size use width and height parameters, put_eSize use the index parameter.
            for example, UCMOS03100KPA support the following resolutions:
                    index 0:    2048,   1536
                    index 1:    1024,   768
                    index 2:    680,    510
            so, we can use put_Size(1024, 768) or put_eSize(1). Both have the same effect.
            
            ------------------------------------------------------------------|
            | Parameter               |   Range       |   Default             |
            |-----------------------------------------------------------------|
            | Auto Exposure Target    |   16~235      |   120                 |
            | Temp                    |   2000~15000  |   6503                |
            | Tint                    |   200~2500    |   1000                |
            | LevelRange              |   0~255       |   Low = 0, High = 255 |
            | Contrast                |   -100~100    |   0                   |
            | Hue                     |   -180~180    |   0                   |
            | Saturation              |   0~255       |   128                 |
            | Brightness              |   -64~64      |   0                   |
            | Gamma                   |   20~180      |   100                 |
            | VidgetAmount            |   -100~100    |   0                   |
            | VignetMidPoint          |   0~100       |   50                  |
            ------------------------------------------------------------------|
        */
        STDMETHOD(put_Size) (THIS_
                    int nWidth, int nHeight) PURE;

        STDMETHOD(get_Size) (THIS_
                    int* pWidth, int* pHeight) PURE;

        STDMETHOD(put_eSize) (THIS_
                    unsigned nIndex) PURE;

        STDMETHOD(get_eSize) (THIS_
                    unsigned* pIndex) PURE;

        STDMETHOD(LevelRangeAuto) (THIS_) PURE;

        STDMETHOD(GetHistogram) (THIS_
                    PITOUPCAM_HISTOGRAM_CALLBACK fnHistogramProc, void* pHistogramCtx
                    ) PURE;
            
        STDMETHOD(put_ExpoCallback) (THIS_
                    PITOUPCAM_EXPOSURE_CALLBACK fnExpoProc, void* pExpoCtx
                    ) PURE;

        STDMETHOD(get_AutoExpoEnable) (THIS_
                    BOOL* bAutoExposure
                    ) PURE;
                
        STDMETHOD(put_AutoExpoEnable) (THIS_
                    BOOL bAutoExposure
                    ) PURE;

        STDMETHOD(get_AutoExpoTarget) (THIS_
                    unsigned short* Target
                    ) PURE;
                
        STDMETHOD(put_AutoExpoTarget) (THIS_
                    unsigned short Target
                    ) PURE;

        /* Auto White Balance, RGB Gain mode */
        STDMETHOD(AwbInit) (THIS_
                    PITOUPCAM_WHITEBALANCE_CALLBACK fnWBProc, void* pWBCtx
                    ) PURE;

        STDMETHOD(put_AuxRect) (THIS_
                    const RECT* pAuxRect
                    ) PURE;
                
        STDMETHOD(get_AuxRect) (THIS_
                    RECT* pAuxRect
                    ) PURE;
                
        STDMETHOD(put_AuxRectShow) (THIS_
                    BOOL bShow
                    ) PURE;
            
        STDMETHOD(get_AuxRectShow) (THIS_
                    BOOL* bShow
                    ) PURE;
                    
        STDMETHOD(put_VignetEnable) (THIS_
                    BOOL bEnable
                    ) PURE;
                
        STDMETHOD(get_VignetEnable) (THIS_
                    BOOL* bEnable
                    ) PURE;
        
        /* reserved, please use put_VignetAmountInt */
        STDMETHOD(put_VignetAmount) (THIS_
                    double dAmount
                    ) PURE;
        
        /* reserved, please use get_VignetAmountInt */
        STDMETHOD(get_VignetAmount) (THIS_
                    double* dAmount
                    ) PURE;

        /* reserved, please use put_VignetMidPointInt */
        STDMETHOD(put_VignetMidPoint) (THIS_
                    double dMidPoint
                    ) PURE;
                
        /* reserved, please use get_VignetMidPointInt */
        STDMETHOD(get_VignetMidPoint) (THIS_
                    double* dMidPoint
                    ) PURE;
        
        /* White Balance, RGB Gain mode */
        STDMETHOD(put_WhiteBalanceGain) (THIS_
                    int aGain[3]
                    ) PURE;

        /* White Balance, RGB Gain mode */
        STDMETHOD(get_WhiteBalanceGain) (THIS_
                    int aGain[3]
                    ) PURE;
                
        STDMETHOD(put_Hue) (THIS_
                    int Hue
                    ) PURE;
                
        STDMETHOD(get_Hue) (THIS_
                    int* Hue
                    ) PURE;

        STDMETHOD(put_Saturation) (THIS_
                    int Saturation
                    ) PURE;
                
        STDMETHOD(get_Saturation) (THIS_
                    int* Saturation
                    ) PURE;

        STDMETHOD(put_Brightness) (THIS_
                    int Brightness
                    ) PURE;
                
        STDMETHOD(get_Brightness) (THIS_
                    int* Brightness
                    ) PURE;
        
        /* microsecond */
        STDMETHOD(get_ExpoTime) (THIS_
                    unsigned* Time
                    ) PURE;
        
        /* microsecond */
        STDMETHOD(put_ExpoTime) (THIS_
                    unsigned Time
                    ) PURE;
        
        /* percent */
        STDMETHOD(get_ExpoAGain) (THIS_
                    unsigned short* AGain
                    ) PURE;
    
        /* percent */
        STDMETHOD(put_ExpoAGain) (THIS_
                    unsigned short AGain
                    ) PURE;

        STDMETHOD(put_LevelRange) (THIS_
                    unsigned short aLow[4], unsigned short aHigh[4]
                    ) PURE;
                
        STDMETHOD(get_LevelRange) (THIS_
                    unsigned short aLow[4], unsigned short aHigh[4]
                    ) PURE;
                
        STDMETHOD(get_Contrast) (THIS_
                    int* Contrast
                    ) PURE;
                
        STDMETHOD(put_Contrast) (THIS_
                    int Contrast
                    ) PURE;
                
        STDMETHOD(get_Gamma) (THIS_
                    int* Gamma
                    ) PURE;
                
        STDMETHOD(put_Gamma) (THIS_
                    int Gamma
                    ) PURE;

        STDMETHOD(get_Chrome) (THIS_
                    BOOL* bChrome
                    ) PURE;
                
        /* monochromatic */
        STDMETHOD(put_Chrome) (THIS_
                    BOOL bChrome
                    ) PURE;
                
        STDMETHOD(get_VFlip) (THIS_
                    BOOL* bVFlip
                    ) PURE;
        
        /* vertical flip */
        STDMETHOD(put_VFlip) (THIS_
                    BOOL bVFlip
                    ) PURE;
                
        STDMETHOD(get_HFlip) (THIS_
                    BOOL* bHFlip
                    ) PURE;
                
        /* horizontal flip */
        STDMETHOD(put_HFlip) (THIS_
                    BOOL bHFlip
                    ) PURE;

        STDMETHOD(put_Speed) (THIS_
                    unsigned short nSpeed
                    ) PURE;

        STDMETHOD(get_Speed) (THIS_
                    unsigned short* pSpeed
                    ) PURE;
                
        /* get the maximum speed, see Misc page, "Frame Speed Level", speed range = [0, max] */
        STDMETHOD(get_MaxSpeed) (THIS_) PURE;

        /* power supply: 
                0 -> 60HZ AC
                1 -> 50Hz AC
                2 -> DC
        */
        STDMETHOD(put_HZ) (THIS_
                    int nHZ
                    ) PURE;

        STDMETHOD(get_HZ) (THIS_
                    int* nHZ
                    ) PURE;
                
        /* skip or bin */
        STDMETHOD(put_Mode) (THIS_
                    BOOL bSkip
                    ) PURE;

        STDMETHOD(get_Mode) (THIS_
                    BOOL* bSkip
                    ) PURE;

        STDMETHOD(put_ChromeCallback) (THIS_
                    PITOUPCAM_CHROME_CALLBACK fnChromeProc, void* pChromeCtx
                    ) PURE;
                
        STDMETHOD(get_ExpTimeRange) (THIS_
                    unsigned* nMin, unsigned* nMax, unsigned* nDef
                    ) PURE;
                
        STDMETHOD(get_ExpoAGainRange) (THIS_
                    unsigned short* nMin, unsigned short* nMax, unsigned short* nDef
                    ) PURE;
                
        STDMETHOD(get_ResolutionNumber) (THIS_
                    ) PURE;
                
        STDMETHOD(get_Resolution) (THIS_
                    unsigned nIndex, int* pWidth, int* pHeight
                    ) PURE;
        
        /* White Balance, Temp/Tint mode */
        STDMETHOD(put_TempTint) (THIS_
                    int nTemp, int nTint
                    ) PURE;

        /* White Balance, Temp/Tint mode */
        STDMETHOD(get_TempTint) (THIS_
                    int* nTemp, int* nTint
                    ) PURE;

        STDMETHOD(put_VignetAmountInt) (THIS_
                    int nAmount
                    ) PURE;
                
        STDMETHOD(get_VignetAmountInt) (THIS_
                    int* nAmount
                    ) PURE;

        STDMETHOD(put_VignetMidPointInt) (THIS_
                    int nMidPoint
                    ) PURE;
                
        STDMETHOD(get_VignetMidPointInt) (THIS_
                    int* nMidPoint
                    ) PURE;

        /* Auto White Balance, Temp/Tint Mode */
        STDMETHOD(AwbOnePush) (THIS_
                    PITOUPCAM_TEMPTINT_CALLBACK fnTTProc, void* pTTCtx
                    ) PURE;

        STDMETHOD(put_AWBAuxRect) (THIS_
                    const RECT* pAuxRect
                    ) PURE;
                
        STDMETHOD(get_AWBAuxRect) (THIS_
                    RECT* pAuxRect
                    ) PURE;

        STDMETHOD(put_AEAuxRect) (THIS_
                    const RECT* pAuxRect
                    ) PURE;
                
        STDMETHOD(get_AEAuxRect) (THIS_
                    RECT* pAuxRect
                    ) PURE;

        /*
            S_FALSE:    color mode
            S_OK:       mono mode, such as EXCCD00300KMA
        */
        STDMETHOD(get_MonoMode) (THIS_
                    ) PURE;
                    
        STDMETHOD(put_MaxAutoExpoTimeAGain) (THIS_
                    unsigned maxTime, unsigned short maxAGain) PURE;

        STDMETHOD(get_RealExpoTime) (THIS_
                    unsigned* Time
                    ) PURE;

        /*
            return: 8, 10, 12, 14, 16
        */
        STDMETHOD(get_MaxBitDepth) (THIS_
                    ) PURE;

        STDMETHOD(get_BitDepth) (THIS_
                    BOOL* bBitDepth) PURE;

        STDMETHOD(put_BitDepth) (THIS_
                    BOOL bBitDepth) PURE;

        /* get the temperature of sensor, in 0.1 degrees Celsius (32 means 3.2 degrees Celsius)
            return E_NOTIMPL if not supported
        */
        STDMETHOD(get_Temperature) (THIS_
                    short* pTemperature) PURE;

        /* set the temperature of sensor, in 0.1 degrees Celsius (32 means 3.2 degrees Celsius)
            return E_NOTIMPL if not supported
        */
        STDMETHOD(put_Temperature) (THIS_
                    short nTemperature) PURE;

        STDMETHOD(put_Roi) (THIS_
                    unsigned xOffset, unsigned yOffset, unsigned xWidth, unsigned yHeight) PURE;

        STDMETHOD(get_Roi) (THIS_
                    unsigned* pxOffset, unsigned* pyOffset, unsigned* pxWidth, unsigned* pyHeight) PURE;
                    
        STDMETHOD(get_Fan) (THIS_
                    BOOL* bFan) PURE;

        STDMETHOD(put_Fan) (THIS_
                    BOOL bFan) PURE;
                    
        STDMETHOD(get_Cooler) (THIS_
                    BOOL* bCooler) PURE;

        STDMETHOD(put_Cooler) (THIS_
                    BOOL bCooler) PURE;

        STDMETHOD(get_ResolutionRatio) (THIS_
                    unsigned nIndex, int* pNumerator, int* pDenominator) PURE;
    };
#endif

#ifndef __ITOUPCAMSTILLIMAGE_DEFINED__
#define __ITOUPCAMSTILLIMAGE_DEFINED__
    /* most Touptek Camera DirectShow source filters has two output pins, one is pin category PIN_CATEGORY_PREVIEW, and the other is pin category PIN_CATEGORY_STILL.
        Please refrence MSDN library "Capturing an Image From a Still Image Pin". http://msdn.microsoft.com/en-us/library/dd318622(VS.85).aspx.
        Please see the example toupcamdemo which is in the install directory.
        */
    // {E978EEA0-A0FF-465f-AB35-65907B8C62AC}
    DEFINE_GUID(IID_IToupcamStillImage, 0xe978eea0, 0xa0ff, 0x465f, 0xab, 0x35, 0x65, 0x90, 0x7b, 0x8c, 0x62, 0xac);

    DECLARE_INTERFACE_(IToupcamStillImage, IUnknown)
    {
        /*
            similar to put_Size, put_eSize
        */
        STDMETHOD(put_StillSize) (THIS_
                    int nWidth, int nHeight
                    ) PURE;

        STDMETHOD(get_StillSize) (THIS_
                    int* pWidth, int* pHeight
                    ) PURE;

        STDMETHOD(put_eStillSize) (THIS_
                    unsigned nIndex
                    ) PURE;

        STDMETHOD(get_eStillSize) (THIS_
                    unsigned* pIndex
                    ) PURE;
                    
        STDMETHOD(get_StillResolutionNumber) (THIS_
                    ) PURE;
                
        STDMETHOD(get_StillResolution) (THIS_
                    unsigned nIndex, int* pWidth, int* pHeight
                    ) PURE;
    };
#endif
    
#ifndef __ITOUPCAMSERIALNUMBER_DEFINED__
#define __ITOUPCAMSERIALNUMBER_DEFINED__
    /*
        get the serial number which is always 32 chars which is zero-terminated such as "TP110826145730ABCD1234FEDC56787"
    */
    // {E12D4B13-333F-4eae-BC89-0446D1FC634D}
    DEFINE_GUID(IID_IToupcamSerialNumber, 0xe12d4b13, 0x333f, 0x4eae, 0xbc, 0x89, 0x4, 0x46, 0xd1, 0xfc, 0x63, 0x4d);

    DECLARE_INTERFACE_(IToupcamSerialNumber, IUnknown)
    {
        STDMETHOD(get_SerialNumber) (THIS_
                    char sn[32]
                    ) PURE;
    };
#endif

#ifndef __ITOUPCAMVERSION_DEFINED__
#define __ITOUPCAMVERSION_DEFINED__

    // {FC69B0C7-6140-4FE7-B385-473E5EA87887}
    DEFINE_GUID(IID_IToupcamVersion, 0xfc69b0c7, 0x6140, 0x4fe7, 0xb3, 0x85, 0x47, 0x3e, 0x5e, 0xa8, 0x78, 0x87);

    DECLARE_INTERFACE_(IToupcamVersion, IUnknown)
    {
        /*
            get the camera firmware version, such as: 3.2.1.20140922
        */
        STDMETHOD(get_FwVersion) (THIS_
                    char fwver[16]
                    ) PURE;

        /*
            get the camera hardware version, such as: 3.2.1.20140922
        */
        STDMETHOD(get_HwVersion) (THIS_
                    char hwver[16]
                    ) PURE;

        /*
            get the production date, such as: 20150327
        */
        STDMETHOD(get_ProductionDate) (THIS_
                    char pdate[10]
                    ) PURE;
    };
#endif

#ifndef __ITOUPCAMST4_DEFINED__
#define __ITOUPCAMST4_DEFINED__

    /* Please see: ASCOM Platform Help ITelescopeV3 */
    // {29867037-78A9-432F-BF33-A3CFE0D6C9B3}
    DEFINE_GUID(IID_IToupcamST4, 0x29867037, 0x78a9, 0x432f, 0xbf, 0x33, 0xa3, 0xcf, 0xe0, 0xd6, 0xc9, 0xb3);

    DECLARE_INTERFACE_(IToupcamST4, IUnknown)
    {      
        STDMETHOD(PlusGuide) (THIS_
                    unsigned nDirect, unsigned nDuration
                    ) PURE;

        /* S_OK: yes */
        /* S_FALSE: no */
        STDMETHOD(IsPulseGuiding) (THIS_
                    ) PURE;
    };
#endif

#ifndef __ITOUPCAMINDEX_DEFINED__
#define __ITOUPCAMINDEX_DEFINED__

    // {F670E70F-FDD0-4131-BAF7-E7F6C3EB66CF}
    DEFINE_GUID(IID_IToupcamIndex, 0xf670e70f, 0xfdd0, 0x4131, 0xba, 0xf7, 0xe7, 0xf6, 0xc3, 0xeb, 0x66, 0xcf);

    DECLARE_INTERFACE_(IToupcamIndex, IUnknown)
    {      
        STDMETHOD(get_Index) (THIS_ unsigned* pIndex) PURE;
        STDMETHOD(put_Index) (THIS_ unsigned nIndex) PURE;
    };
#endif

#if 0
/* return value: the number of connected camera */
unsigned __stdcall DshowEnumCamera();

/* return value: the camera directshow COM object, which can be used to QueryInterface(IID_IBaseFilter, ...). When failed, NULL is returned */
/* use OpenCamera(0) to open the first camera, use OpenCamera(1) to open the second camera, etc */
IUnknown* __stdcall DshowOpenCamera(unsigned index);
#endif

#endif
