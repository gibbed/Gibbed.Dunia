#include <windows.h>
#include "patch.h"

void PatchData(unsigned int address, void *data, int length)
{
	DWORD old, junk;
	VirtualProtect((void *)address, length, PAGE_EXECUTE_READWRITE, &old);
	memcpy((void *)address, data, length);
	VirtualProtect((void *)address, length, old, &junk);
	FlushInstructionCache(GetCurrentProcess(), (void *)address, length);
}

void PatchAddress(unsigned int address, void *target)
{
	PatchData(address, &target, 4);
}

void PatchFunction(unsigned int address, void *target)
{
	unsigned char code[5];
	code[0] = 0xE9;

	unsigned int *jump = (unsigned int *)&code[1];
	*jump = (unsigned int)target - address - 5;

	PatchData(address, code, 5);
}
