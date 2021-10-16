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

namespace Gibbed.FarCry2.ConvertBinaryObject.MemberDefinitions
{
    internal class StringHandler : IMemberDefinition
    {
        public string Name { get; private set; }
        public MemberType Type { get { return MemberType.String; } }

        public StringHandler(string name)
        {
            this.Name = name;
        }

        public string Deserialize(byte[] value)
        {
            if (value.Length < 1)
            {
                throw new FormatException();
            }
            else if (value[value.Length - 1] != 0)
            {
                throw new FormatException();
            }

            return Encoding.UTF8.GetString(value, 0, value.Length - 1);
        }

        public byte[] Serialize(string value)
        {
            var data = Encoding.UTF8.GetBytes(value);
            Array.Resize(ref data, data.Length + 1);
            return data;
        }

        public void LoadDefinition(System.Xml.XPath.XPathNavigator nav)
        {
            throw new NotImplementedException();
        }
    }
}
