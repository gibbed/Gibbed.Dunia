#include "declarations.h"

void __declspec(naked) h_XInputGetState() { _asm{ jmp p_XInputGetState } }
void __declspec(naked) h_XInputSetState() { _asm{ jmp p_XInputSetState } }
void __declspec(naked) h_XInputGetCapabilities() { _asm{ jmp p_XInputGetCapabilities } }
void __declspec(naked) h_XInputEnable() { _asm{ jmp p_XInputEnable } }
void __declspec(naked) h_XInputGetDSoundAudioDeviceGuids() { _asm{ jmp p_XInputGetDSoundAudioDeviceGuids } }
void __declspec(naked) h_XInputGetBatteryInformation() { _asm{ jmp p_XInputGetBatteryInformation } }
void __declspec(naked) h_XInputGetKeystroke() { _asm{ jmp p_XInputGetKeystroke } }
void __declspec(naked) h_XInputGetStateEx() { _asm{ jmp p_XInputGetStateEx } }
void __declspec(naked) h_XInputWaitForGuideButton() { _asm{ jmp p_XInputWaitForGuideButton } }
void __declspec(naked) h_XInputCancelGuideButtonWait() { _asm{ jmp p_XInputCancelGuideButtonWait } }
void __declspec(naked) h_XInputPowerOffController() { _asm{ jmp p_XInputPowerOffController } }
