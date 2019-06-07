/* Copyright (c) 2019 Rick (rick 'at' gibbed 'dot' us)
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

using System.Text;

namespace Gibbed.Dunia.FileFormats
{
    public static class FileExtensions
    {
        public static string Detect(byte[] guess, int read)
        {
            if (read == 0)
            {
                return "null";
            }

            if (read >= 5 &&
                guess[0] == 'M' &&
                guess[1] == 'A' &&
                guess[2] == 'G' &&
                guess[3] == 'M' &&
                guess[4] == 'A')
            {
                return "mgb";
            }

            if (read >= 4 &&
                guess[0] == 'T' &&
                guess[1] == 'B' &&
                guess[2] == 'X' &&
                guess[3] == 0)
            {
                return "xbt";
            }

            if (read >= 4 &&
                guess[0] == 'H' &&
                guess[1] == 'S' &&
                guess[2] == 'E' &&
                guess[3] == 'M')
            {
                return "xbg";
            }

            if (read >= 4 &&
                guess[0] == 1 &&
                guess[1] == 'K' &&
                guess[2] == 'P' &&
                guess[3] == 'S')
            {
                return "spk";
            }

            if (read >= 4 &&
                guess[0] == 'n' &&
                guess[1] == 'b' &&
                guess[2] == 'C' &&
                guess[3] == 'F')
            {
                return "fcb";
            }

            if (read >= 4 &&
                guess[0] == 0x89 &&
                guess[1] == 'P' &&
                guess[2] == 'N' &&
                guess[3] == 'G')
            {
                return "png";
            }

            if (read >= 3 &&
                guess[0] == 0 &&
                guess[1] == 0 &&
                guess[2] == 0xFF)
            {
                return "maybe.rml";
            }

            if (read >= 8 &&
                guess[4] == 0x68 &&
                guess[5] == 0x4D &&
                guess[6] == 0x76 &&
                guess[7] == 0x4E)
            {
                return "hMvN";
            }

            if (read >= 20 &&
                guess[16] == 0x57 &&
                guess[17] == 0xE0 &&
                guess[18] == 0xE0 &&
                guess[19] == 0x57)
            {
                return "hkx";
            }

            string text = Encoding.ASCII.GetString(guess, 0, read);
            
            if (read >= 6 && text.StartsWith("<root>") == true)
            {
                return "root.xml";
            }

            if (read >= 9 && text.StartsWith("<package>") == true)
            {
                return "mgb.desc";
            }

            if (read >= 6 && text.StartsWith("<root>") == true)
            {
                return "root.xml";
            }

            if (read >= 12 && text.StartsWith("<NewPartLib>") == true)
            {
                return "NewPartLib.xml";
            }

            if (read >= 11 && text.StartsWith("<MovieData>") == true)
            {
                return "MovieData.xml";
            }

            return "unknown";
        }
    }
}
