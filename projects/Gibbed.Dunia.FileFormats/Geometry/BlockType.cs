﻿/* Copyright (c) 2021 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

namespace Gibbed.Dunia.FileFormats.Geometry
{
    public enum BlockType : uint
    {
        // ReSharper disable InconsistentNaming
        Root = 0x00000000,
        MaterialReference = 0x524D544C, // RMTL
        Nodes = 0x4E4F4445,
        O2BM = 0x4F32424D,
        SKID = 0x534B4944,
        SKND = 0x534B4E44,
        CLUS = 0x434C5553,
        LODs = 0x04C4F4453, // LODS
        BoundingBox = 0x42424F58, // BBOX
        BSPH = 0x42535048,
        LODInfo = 0x004C4F44, // LOD\0
        PCMP = 0x50434D50,
        UCMP = 0x55434D50,
        IKDA = 0x494B4441,
        MaterialDescriptor = 0x444D544C, // DMTL
        // ReSharper restore InconsistentNaming
    }
}
