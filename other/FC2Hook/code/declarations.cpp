#include "declarations.h"

extern "C"
{
	APIWRAPPER p_XInputGetState;
	APIWRAPPER p_XInputSetState;
	APIWRAPPER p_XInputGetCapabilities;
	APIWRAPPER p_XInputEnable;
	APIWRAPPER p_XInputGetDSoundAudioDeviceGuids;
	APIWRAPPER p_XInputGetBatteryInformation;
	APIWRAPPER p_XInputGetKeystroke;
	APIWRAPPER p_XInputGetStateEx;
	APIWRAPPER p_XInputWaitForGuideButton;
	APIWRAPPER p_XInputCancelGuideButtonWait;
	APIWRAPPER p_XInputPowerOffController;
}
