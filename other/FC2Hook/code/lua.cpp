#include <Windows.h>
#include "lua.h"

void *p_lua_gettop;
void *p_lua_getglobalstate;
void *p_luaL_error;
void *p_luaX_getstring;
void *p_luaX_getint;
void *p_luaX_getfloat;

int lua_gettop(void *L)
{
	__asm
	{
		mov ecx, L
		call p_lua_gettop
	}
}

void *lua_getglobalstate(void)
{
	__asm
	{
		call p_lua_getglobalstate
	}
}

bool luaX_getstring(void *L, int narg, const char **value)
{
	__asm
	{
		push value
		push narg
		mov ecx, L
		call p_luaX_getstring
	}
}

bool luaX_getint(void *L, int narg, int *value)
{
	__asm
	{
		push value
		push narg
		mov ecx, L
		call p_luaX_getint
	}
}

bool luaX_getfloat(void *L, int narg, float *value)
{
	__asm
	{
		push value
		push narg
		mov ecx, L
		call p_luaX_getfloat
	}
}

typedef int (__cdecl *LUAL_ERROR)(void *L, const char *fmt, ...);
LUAL_ERROR luaL_error;

// Base Address = 0x10000000
#define PTR(x) ((void *)((unsigned int)module + (x - 0x10000000)))
bool SetupLuaFunctions(HMODULE module)
{
	if (module == NULL)
	{
		return false;
	}

	p_lua_gettop = PTR(0x102BE150);
	p_lua_getglobalstate = PTR(0x102A9760);
	luaL_error = (LUAL_ERROR)PTR(0x102AA0B0);
	p_luaX_getstring = PTR(0x102BE260);
	p_luaX_getint = PTR(0x102BE1B0);
	p_luaX_getfloat = PTR(0x102BE220);

	return true;
}
#undef PTR
