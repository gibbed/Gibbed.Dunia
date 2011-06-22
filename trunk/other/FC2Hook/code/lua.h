#ifndef __LUA_H
#define __LUA_H

bool SetupLuaFunctions(HMODULE module);

typedef int (__cdecl *LUAL_ERROR)(void *L, const char *fmt, ...);
extern LUAL_ERROR luaL_error;

int lua_gettop(void *L);
void *lua_getglobalstate(void);
bool luaX_getstring(void *L, int narg, const char **value);
bool luaX_getint(void *L, int narg, int *value);
bool luaX_getfloat(void *L, int narg, float *value);

#endif
