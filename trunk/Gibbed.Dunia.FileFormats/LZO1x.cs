/* Copyright (c) 2011 Rick (rick 'at' gibbed 'dot' us)
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
using System.Runtime.InteropServices;

namespace Gibbed.Dunia.FileFormats
{
    public static class LZO1x
    {
        private static bool Is64Bit = DetectIs64Bit();
        private static bool DetectIs64Bit()
        {
            return Marshal.SizeOf(IntPtr.Zero) == 8;
        }

        private sealed class Native32
        {
            [DllImport("lzo1x_32.dll",
                EntryPoint = "#67",
                CallingConvention = CallingConvention.StdCall)]
            internal static extern int Compress(
                byte[] inbuf,
                uint inlen,
                byte[] outbuf,
                ref uint outlen,
                byte[] workbuf);

            [DllImport("lzo1x_32.dll",
                EntryPoint = "#68",
                CallingConvention = CallingConvention.StdCall)]
            internal static extern int Decompress(
                byte[] inbuf,
                uint inlen,
                byte[] outbuf,
                ref uint outlen);
        }

        private sealed class Native64
        {
            [DllImport("lzo1x_64.dll",
                EntryPoint = "#67",
                CallingConvention = CallingConvention.StdCall)]
            internal static extern int Compress(
                byte[] inbuf,
                uint inlen,
                byte[] outbuf,
                ref uint outlen,
                byte[] workbuf);

            [DllImport("lzo1x_64.dll",
                EntryPoint = "#68",
                CallingConvention = CallingConvention.StdCall)]
            internal static extern int Decompress(
                byte[] inbuf,
                uint inlen,
                byte[] outbuf,
                ref uint outlen);
        }

        private const int lzo_sizeof_dict_t = 2;
        private const int LZO1X_MEM_COMPRESS = LZO1X_1_MEM_COMPRESS;
        private const int LZO1X_1_MEM_COMPRESS = (16384 * lzo_sizeof_dict_t);
        private const int LZO1X_MEM_DECOMPRESS = 0;

        private static byte[] CompressWork = new byte[LZO1X_1_MEM_COMPRESS];

        public static int Compress(
            byte[] inbuf, uint inlen, byte[] outbuf, ref uint outlen)
        {
            lock (CompressWork)
            {
                if (Is64Bit == true)
                {
                    return Native64.Compress(inbuf, inlen, outbuf, ref outlen, CompressWork);
                }
                else
                {
                    return Native32.Compress(inbuf, inlen, outbuf, ref outlen, CompressWork);
                }
            }
        }

        public static int Decompress(
            byte[] inbuf, uint inlen, byte[] outbuf, ref uint outlen)
        {
            if (Is64Bit == true)
            {
                return Native64.Decompress(inbuf, inlen, outbuf, ref outlen);
            }
            else
            {
                return Native32.Decompress(inbuf, inlen, outbuf, ref outlen);
            }
        }
    }
}
