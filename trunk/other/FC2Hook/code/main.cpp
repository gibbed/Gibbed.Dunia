#include <windows.h>
#include "declarations.h"
#include "hook.h"

HMODULE LoadStubs(void);
HMODULE hOriginal;

BOOL WINAPI DllMain(HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpvReserved)
{
	switch (fdwReason)
	{
		case DLL_PROCESS_ATTACH:
		{
			hOriginal = LoadStubs();
			if (hOriginal == NULL)
			{
				MessageBox(
					0,
					TEXT("Failed to load original xinput1_3.dll!\n\nThe game will now exit."),
					TEXT("Error"),
					MB_OK | MB_ICONERROR);
				ExitProcess(0);
			}

			if (HookGame() == false)
			{
				MessageBox(
					0,
					TEXT("FC2Hook failed to initialize.\n\nThe game will now exit."),
					TEXT("Critical Error"),
					MB_OK | MB_ICONERROR);
				ExitProcess(0);
			}

			break;
		}

		case DLL_PROCESS_DETACH:
		{
			if (hOriginal != NULL)
			{
				FreeLibrary(hOriginal);
				hOriginal = NULL;
			}
			
			break;
		}
	}

	return TRUE;
}

HMODULE LoadStubs(void)
{
	char path[MAX_PATH];
	GetSystemDirectoryA(path, MAX_PATH);
	strcat(path, "\\xinput1_3.dll");

	HMODULE original = LoadLibraryA(path);
	if (original == NULL)
	{
		return NULL;
	}

	p_XInputGetState = (APIWRAPPER)GetProcAddress(original, "XInputGetState");
	p_XInputSetState = (APIWRAPPER)GetProcAddress(original, "XInputSetState");
	p_XInputGetCapabilities = (APIWRAPPER)GetProcAddress(original, "XInputGetCapabilities");
	p_XInputEnable = (APIWRAPPER)GetProcAddress(original, "XInputEnable");
	p_XInputGetDSoundAudioDeviceGuids = (APIWRAPPER)GetProcAddress(original, "XInputGetDSoundAudioDeviceGuids");
	p_XInputGetBatteryInformation = (APIWRAPPER)GetProcAddress(original, "XInputGetBatteryInformation");
	p_XInputGetKeystroke = (APIWRAPPER)GetProcAddress(original, "XInputGetKeystroke");
	p_XInputGetStateEx = (APIWRAPPER)GetProcAddress(original, MAKEINTRESOURCEA(100));
	p_XInputWaitForGuideButton = (APIWRAPPER)GetProcAddress(original, MAKEINTRESOURCEA(101));
	p_XInputCancelGuideButtonWait = (APIWRAPPER)GetProcAddress(original, MAKEINTRESOURCEA(102));
	p_XInputPowerOffController = (APIWRAPPER)GetProcAddress(original, MAKEINTRESOURCEA(103));

	if (p_XInputGetState == NULL ||
		p_XInputSetState == NULL ||
		p_XInputGetCapabilities == NULL ||
		p_XInputEnable == NULL ||
		p_XInputGetDSoundAudioDeviceGuids == NULL ||
		p_XInputGetBatteryInformation == NULL ||
		p_XInputGetKeystroke == NULL ||
		p_XInputGetStateEx == NULL ||
		p_XInputWaitForGuideButton == NULL ||
		p_XInputCancelGuideButtonWait == NULL ||
		p_XInputPowerOffController == NULL)
	{
		FreeLibrary(original);
		return NULL;
	}

	return original;
}
