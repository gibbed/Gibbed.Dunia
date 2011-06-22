#include <windows.h>
#include <stdio.h>
#include <string.h>

#include "hook.h"
#include "patch.h"
#include "lua.h"

void **pConsole;
typedef void (__cdecl *PRINTTOCONSOLE)(void *console, int level, const char *fmt, ...);
PRINTTOCONSOLE PrintToConsole;

int __stdcall LuaSystemLog(void *L)
{
	if (lua_gettop(L) != 1)
	{
		luaL_error(lua_getglobalstate(), "%s.%s wrong number of arguments", "System", "Log");
		return 0;
	}

	const char *text;
	luaX_getstring(L, 1, &text);
	PrintToConsole(*pConsole, 0, "[Log] %s", text);
	return 0;
}

int __stdcall LuaSystemLogToConsole(void *L)
{
	if (lua_gettop(L) != 1)
	{
		luaL_error(lua_getglobalstate(), "%s.%s wrong number of arguments", "System", "LogToConsole");
		return 0;
	}

	const char *text;
	luaX_getstring(L, 1, &text);
	PrintToConsole(*pConsole, 0, "[L2C] %s", text);
	return 0;
}

int __stdcall LuaSystemWarning(void *L)
{
	if (lua_gettop(L) != 1)
	{
		luaL_error(lua_getglobalstate(), "%s.%s wrong number of arguments", "System", "Warning");
		return 0;
	}

	const char *text;
	luaX_getstring(L, 1, &text);
	PrintToConsole(*pConsole, 0, "[Wrn] %s", text);
	return 0;
}

int __stdcall LuaSystemTrace(void *L)
{
	if (lua_gettop(L) != 1)
	{
		luaL_error(lua_getglobalstate(), "%s.%s wrong number of arguments", "System", "Trace");
		return 0;
	}

	const char *text;
	luaX_getstring(L, 1, &text);
	PrintToConsole(*pConsole, 0, "[Trc] %s", text);
	return 0;
}

int __stdcall LuaSystemDebugLine(void *L)
{
	if (lua_gettop(L) != 1)
	{
		luaL_error(lua_getglobalstate(), "%s.%s wrong number of arguments", "System", "DebugLine");
		return 0;
	}

	const char *text;
	luaX_getstring(L, 1, &text);
	PrintToConsole(*pConsole, 0, "[Dbg] %s", text);
	return 0;
}

// Base Address = 0x10000000
#define PTR(x) ((unsigned int)((unsigned int)dunia + (x - 0x10000000)))
bool HookGame(void)
{
	HMODULE dunia = GetModuleHandle(TEXT("Dunia.dll"));
	if (dunia == NULL)
	{
		return false;
	}
	
	pConsole = (void **)PTR(0x11606280);
	PrintToConsole = (PRINTTOCONSOLE)PTR(0x102956F0);

	if (SetupLuaFunctions(dunia) == false)
	{
		return false;
	}

	PatchAddress(PTR(0x105F8E01) + 1, LuaSystemLogToConsole);
	PatchAddress(PTR(0x105F8E10) + 1, LuaSystemLog);
	PatchAddress(PTR(0x105F8E3D) + 1, LuaSystemWarning);
	PatchAddress(PTR(0x105F8E4C) + 1, LuaSystemTrace);
	PatchAddress(PTR(0x105F8E5B) + 1, LuaSystemDebugLine);
	return true;
}
#undef PTR
