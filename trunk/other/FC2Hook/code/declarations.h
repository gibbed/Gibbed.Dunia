#ifndef __DECLARATIONS_H
#define __DECLARATIONS_H 1

typedef int (*APIWRAPPER)(void);

extern "C"
{
	extern APIWRAPPER p_XInputGetState;
	extern APIWRAPPER p_XInputSetState;
	extern APIWRAPPER p_XInputGetState;
	extern APIWRAPPER p_XInputSetState;
	extern APIWRAPPER p_XInputGetCapabilities;
	extern APIWRAPPER p_XInputEnable;
	extern APIWRAPPER p_XInputGetDSoundAudioDeviceGuids;
	extern APIWRAPPER p_XInputGetBatteryInformation;
	extern APIWRAPPER p_XInputGetKeystroke;
	extern APIWRAPPER p_XInputGetStateEx;
	extern APIWRAPPER p_XInputWaitForGuideButton;
	extern APIWRAPPER p_XInputCancelGuideButtonWait;
	extern APIWRAPPER p_XInputPowerOffController;
}

#endif
