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

using System;
using System.Xml;

namespace Gibbed.Dunia.ConvertBinary.MemberDefinitions
{
    internal class BinHexHandler : IMemberDefinition
    {
        public string Name { get; private set; }
        public MemberType Type { get { return MemberType.BinHex; } }

        public BinHexHandler(string name)
        {
            this.Name = name;
        }

        public string Deserialize(byte[] value)
        {
            return ToBinHexString(value);
        }

        public byte[] Serialize(string value)
        {
            return FromBinHexString(value);
        }

        #region c'est la vie
        private static readonly System.Reflection.MethodInfo FromBinHexStringMethod = typeof(XmlConvert).GetMethod("FromBinHexString", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic, null, new Type[] { typeof(string) }, null);
        private static byte[] FromBinHexString(string input)
        {
            return input == null ? null : (byte[])FromBinHexStringMethod.Invoke(null, new object[] { input });
        }

        private static readonly System.Reflection.MethodInfo ToBinHexStringMethod = typeof(XmlConvert).GetMethod("ToBinHexString", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic, null, new Type[] { typeof(byte[]) }, null);
        private static string ToBinHexString(byte[] input)
        {
            return input == null ? null : (string)ToBinHexStringMethod.Invoke(null, new object[] { input });
        }
        #endregion

        public void LoadDefinition(System.Xml.XPath.XPathNavigator nav)
        {
            throw new NotImplementedException();
        }
    }
}
