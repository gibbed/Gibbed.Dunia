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
            [DllImport("lzo1x_32.dll", EntryPoint = "Compress")]
            internal static extern int Compress(
                [MarshalAs(UnmanagedType.LPArray)]
                byte[] inbuf,
                uint inlen,
                [MarshalAs(UnmanagedType.LPArray)]
                byte[] outbuf,
                ref uint outlen);

            [DllImport("lzo1x_32.dll", EntryPoint = "Decompress")]
            internal static extern int Decompress(
                [MarshalAs(UnmanagedType.LPArray)]
                byte[] inbuf,
                uint inlen,
                [MarshalAs(UnmanagedType.LPArray)]
                byte[] outbuf,
                ref uint outlen);
        }

        private sealed class Native64
        {
            [DllImport("lzo1x_64.dll", EntryPoint = "Compress")]
            internal static extern int Compress(
                [MarshalAs(UnmanagedType.LPArray)]
                byte[] inbuf,
                uint inlen,
                [MarshalAs(UnmanagedType.LPArray)]
                byte[] outbuf,
                ref uint outlen);

            [DllImport("lzo1x_64.dll", EntryPoint = "Decompress")]
            internal static extern int Decompress(
                [MarshalAs(UnmanagedType.LPArray)]
                byte[] inbuf,
                uint inlen,
                [MarshalAs(UnmanagedType.LPArray)]
                byte[] outbuf,
                ref uint outlen);
        }

        public static int Compress(
            byte[] inbuf, uint inlen, byte[] outbuf, ref uint outlen)
        {
            if (Is64Bit == true)
            {
                return Native64.Compress(inbuf, inlen, outbuf, ref outlen);
            }
            else
            {
                return Native32.Compress(inbuf, inlen, outbuf, ref outlen);
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
