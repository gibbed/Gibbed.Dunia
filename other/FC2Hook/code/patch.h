#ifndef __PATCH_H
#define __PATCH_H

void PatchData(unsigned int address, void *data, int length);
void PatchAddress(unsigned int address, void *target);
void PatchFunction(unsigned int address, void *target);

#endif
