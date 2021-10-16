/* Copyright (c) 2021 Rick (rick 'at' gibbed 'dot' us)
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

using System;
using System.Text;
using Gibbed.IO;

namespace Gibbed.Dunia.FileFormats
{
    public static class FileDetection
    {
        public static bool TryDetect(byte[] guess, int read, out FileDetectionInfo info)
        {
            if (read == 0)
            {
                info = ("null", null);
                return true;
            }

            if (read >= 5 &&
                guess[0] == 'M' &&
                guess[1] == 'A' &&
                guess[2] == 'G' &&
                guess[3] == 'M' &&
                guess[4] == 'A')
            {
                info = ("ui", "mgb");
                return true;
            }

            if (read >= 3 &&
                guess[0] == 'B' &&
                guess[1] == 'I' &&
                guess[2] == 'K')
            {
                info = ("gfx", "bik");
                return true;
            }

            if (read >= 3 &&
                guess[0] == 'U' &&
                guess[1] == 'E' &&
                guess[2] == 'F')
            {
                info = ("ui", "feu");
                return true;
            }

            if (read >= 3 &&
                guess[0] == 0 &&
                guess[1] == 0 &&
                guess[2] == 0xFF)
            {
                info = ("misc", "maybe.rml");
                return true;
            }

            if (read >= 8 &&
                guess[4] == 'h' &&
                guess[5] == 'M' &&
                guess[6] == 'v' &&
                guess[7] == 'N')
            {
                info = ("gfx", "hMvN");
                return true;
            }

            if (read >= 8 &&
                guess[4] == 'Q' &&
                guess[5] == 'E' &&
                guess[6] == 'S' &&
                guess[7] == 0)
            {
                info = ("game", "cseq");
                return true;
            }

            if (read >= 20 &&
                guess[16] == 'W' &&
                guess[17] == 0xE0 &&
                guess[18] == 0xE0 &&
                guess[19] == 'W')
            {
                info = ("gfx", "hkx");
                return true;
            }

            if (read >= 48 &&
                guess[44] == 'S' &&
                guess[45] == 'D' &&
                guess[46] == 'K' &&
                guess[47] == 'V')
            {
                info = ("gfx", "hkx");
                return true;
            }

            if (read >= 24 &&
                guess[20] == 0xC8 &&
                guess[21] == 0xEF &&
                guess[22] == 0x1D &&
                guess[23] == 0x3E)
            {
                info = ("terrain", "terrainnode.bdl");
                return true;
            }

            if (read >= 8 &&
                guess[0] == 0xEF &&
                guess[1] == 0xBB &&
                guess[2] == 0xBF &&
                guess[3] == '<' &&
                guess[4] == '?' &&
                guess[5] == 'x' &&
                guess[6] == 'm' &&
                guess[7] == 'l')
            {
                info = ("misc", "xml");
                return true;
            }

            if (read >= 20)
            {
                uint magic = BitConverter.ToUInt32(guess, 16);
                var result = DetectMagic(magic) ?? DetectMagic(magic.Swap());
                if (result != null)
                {
                    info = result.Value;
                    return true;
                }
            }

            if (read >= 8)
            {
                uint magic = BitConverter.ToUInt32(guess, 4);
                var result = DetectMagic(magic) ?? DetectMagic(magic.Swap());
                if (result != null)
                {
                    info = result.Value;
                    return true;
                }
            }

            if (read >= 4)
            {
                uint magic = BitConverter.ToUInt32(guess, 0);
                var result = DetectMagic(magic) ?? DetectMagic(magic.Swap());
                if (result != null)
                {
                    info = result.Value;
                    return true;
                }
            }

            string text = Encoding.ASCII.GetString(guess, 0, read);

            if (read >= 3 && text.StartsWith("-- ") == true)
            {
                info = ("scripts", "lua");
                return true;
            }

            if (read >= 6 && text.StartsWith("<root>") == true)
            {
                info = ("misc", "root.xml");
                return true;
            }

            if (read >= 9 && text.StartsWith("<package>") == true)
            {
                info = ("ui", "mbg.desc");
                return true;
            }

            if (read >= 12 && text.StartsWith("<NewPartLib>") == true)
            {
                info = ("misc", "NewPartLib.xml");
                return true;
            }

            if (read >= 14 && text.StartsWith("<BarkDataBase>") == true)
            {
                info = ("misc", "BarkDataBase.xml");
                return true;
            }

            if (read >= 13 && text.StartsWith("<BarkManager>") == true)
            {
                info = ("misc", "BarkManager.xml");
                return true;
            }

            if (read >= 17 && text.StartsWith("<ObjectInventory>") == true)
            {
                info = ("misc", "ObjectInventory.xml");
                return true;
            }

            if (read >= 21 && text.StartsWith("<CollectionInventory>") == true)
            {
                info = ("misc", "CollectionInventory.xml");
                return true;
            }

            if (read >= 14 && text.StartsWith("<SoundRegions>") == true)
            {
                info = ("misc", "SoundRegions.xml");
                return true;
            }

            if (read >= 11 && text.StartsWith("<MovieData>") == true)
            {
                info = ("misc", "MovieData.xml");
                return true;
            }

            if (read >= 8 && text.StartsWith("<Profile") == true)
            {
                info = ("misc", "Profile.xml");
                return true;
            }

            if (read >= 12 && text.StartsWith("<MinimapInfo") == true)
            {
                info = ("misc", "MinimapInfo.xml");
                return true;
            }

            if (read >= 12 && text.StartsWith("<stringtable") == true)
            {
                info = ("text", "xml");
                return true;
            }

            if (read >= 5 && text.StartsWith("<?xml") == true)
            {
                info = ("misc", "xml");
                return true;
            }

            if (read >= 1 && text.StartsWith("<Sequence>") == true)
            {
                info = ("game", "seq");
                return true;
            }

            if (read >= 8 && text.StartsWith("<Binary>") == true)
            {
                info = ("pilot", "pnm");
                return true;
            }

            if (read >= 15 && text.StartsWith("SQLite format 3") == true)
            {
                info = ("db", "sqlite3");
                return true;
            }

            if (read >= 2 &&
                guess[0] == 'p' &&
                guess[1] == 'A')
            {
                info = ("animations", "dpax");
                return true;
            }

            info = default;
            return false;
        }

        private static FileDetectionInfo? DetectMagic(uint magic)
        {
            return magic switch
            {
#pragma warning disable format
                /* 'Strm'    */ 0x5374726D => ("strm", "bin"),
                /* '\0XBT'   */ 0x00584254 => ("gfx", "xbt"),
                /* 'MESH'    */ 0x4D455348 => ("gfx", "xbg"),
                /* '\0MAT'   */ 0x54414D00 => ("gfx", "material.bin"),
                /* 'SPK\2'   */ 0x53504B02 => ("sfx", "spk"),
                /* 'FCbn'    */ 0x4643626E => ("game", "fcb"),
                /* 'SNdN'    */ 0x534E644E => ("game", "rnv"),
                /* 'PNG\x89' */ 0x474E5089 => ("gfx", "png"),
                /* 'MVM\0'   */ 0x4D564D00 => ("gfx", "MvN"),
                /* 'Lua\x1B' */ 0x61754C1B => ("scripts", "luab"),
                /* 'LUAC'    */ 0x4341554C => ("scripts", "luac"),
                /* 'GEOM'    */ 0x47454F4D => ("gfx", "xbg"),
                /* 'BTCH'    */ 0x42544348 => ("cbatch", "cbatch"),
                /* 'SRHR'    */ 0x53524852 => ("srhr", "bin"),
                /* 'SRLR'    */ 0x53524C52 => ("srlr", "bin"),
                /* 'SCTR'    */ 0x53435452 => ("sctr", "bin"),
                /* 'TREE'    */ 0x54524545 => ("tree", "bin"),
                /* 'PIMG'    */ 0x50494D47 => ("pimg", "bin"),
                /* 'BASE'    */ 0x45534142 => ("wlu", "fcb"),
                /* 'f1A0'    */ 0x66314130 => ("dialog", "stimuli.dsc.pack"),
                /* 'g2BK'    */ 0x6732424B => ("bink", "bik"),
                /* 'j2BK'    */ 0x6A32424B => ("bink", "bik"),
                /* 'ano\n'   */ 0x0A6F6E61 => ("annotation", "ano"),
                /* 'LiPr'    */ 0x4C695072 => ("lightprobe", "lipr.bin"),
                /* 'Mv2\x11' */ 0x4D763211 => ("move", "bin"),
                /* 'SLHR'    */ 0x534C4852 => ("roadresources", "hgfx"),
                /* 'GMIP'    */ 0x474D4950 => ("gfx", "xbgmip"),
                /* 'LPMT'    */ 0x4C504D54 => ("bin", "lpmt"),
                /* 'BKHD'    */ 0x424B4844 => ("bin", "bkhd"),
                /* 'CKTA'    */ 0x434B5441 => ("bin", "ckta"),
                /* 'OTTO'    */ 0x4F54544F => ("fonts", "otf"),
                /* 'MGRF'    */ 0x4D475246 => ("bin", "mgrf"),
                /* 'DXBC'    */ 0x43425844 => ("shaders", "bin"),
                /* "RIFF"    */ 0x46464952 => ("sounds", "wem"),
                /*           */ 0x00014C53 => ("languages", "loc"),
                /*           */ 0x00032A02 => ("sfx", "sbao"),
                /*           */ 0x0000389C => ("eight", "bin"),
                /*           */ _ => null,
#pragma warning restore format
            };
        }
    }
}
