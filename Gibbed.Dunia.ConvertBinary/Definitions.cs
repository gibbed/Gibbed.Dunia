/* Copyright (c) 2012 Rick (rick 'at' gibbed 'dot' us)
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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.XPath;
using Gibbed.Dunia.FileFormats;

namespace Gibbed.Dunia.ConvertBinary
{
    internal class Definitions : IDefinitionProvider
    {
        //private static Dictionary<MemberType, 

        public static void LoadNameAndHash(
                            XPathNavigator node, out string name, out uint hash)
        {
            var _name = node.GetAttribute("name", "");
            var _hash = node.GetAttribute("hash", "");

            if (string.IsNullOrWhiteSpace(_name) == true &&
                string.IsNullOrWhiteSpace(_hash) == true)
            {
                throw new FormatException();
            }

            name = string.IsNullOrWhiteSpace(_name) == false ? _name : null;
            hash = name != null ? name.HashCRC32() : uint.Parse(_hash, NumberStyles.AllowHexSpecifier);
        }

        public static void LoadTypeAndHash(
                    XPathNavigator node, out string type, out uint hash)
        {
            var _type = node.GetAttribute("type", "");
            var _hash = node.GetAttribute("hash", "");

            if (string.IsNullOrWhiteSpace(_type) == true &&
                string.IsNullOrWhiteSpace(_hash) == true)
            {
                throw new FormatException();
            }

            type = string.IsNullOrWhiteSpace(_type) == false ? _type : null;
            hash = type != null ? type.HashCRC32() : uint.Parse(_hash, NumberStyles.AllowHexSpecifier);
        }

        private Definitions()
        {
            this.DummyClass = new ClassDefinition(this, null);
        }

        private ClassDefinition DummyClass;
        private Dictionary<uint, ClassDefinition> ClassDefinitions
            = new Dictionary<uint, ClassDefinition>();

        public IClassDefinition GetClassDefinition(uint type)
        {
            if (this.ClassDefinitions.ContainsKey(type) == true)
            {
                return this.ClassDefinitions[type];
            }

            return this.DummyClass;
        }

        public static IDefinitionProvider Load(string path)
        {
            var defs = new Definitions();
            if (File.Exists(path) == false)
            {
                return defs;
            }

            using (var input = File.OpenRead(path))
            {
                var doc = new XPathDocument(input);
                var nav = doc.CreateNavigator();

                var classes = nav.Select("/classes/class");
                defs.ClassDefinitions = defs.LoadClasses(classes, null);
            }

            defs.ResolveSupers();
            return defs;
        }

        private void ResolveSupers()
        {
            var queue = new Queue<ClassDefinition>();
            var processed = new List<ClassDefinition>();

            foreach (var kv in this.ClassDefinitions)
            {
                queue.Enqueue(kv.Value);
            }

            while (queue.Count > 0)
            {
                var def = queue.Dequeue();
                processed.Add(def);

                if (string.IsNullOrWhiteSpace(def.SuperName) == false)
                {
                    var superHash = def.SuperName.HashCRC32();

                    var current = def.Parent;
                    while (current != null)
                    {
                        if (current.Children.ContainsKey(superHash) == true)
                        {
                            def.Super = current.Children[superHash];
                            break;
                        }

                        current = current.Parent;
                    }

                    if (def.Super == null &&
                        this.ClassDefinitions.ContainsKey(superHash) == true)
                    {
                        def.Super = this.ClassDefinitions[superHash];
                    }
                }

                foreach (var kv in def.Children)
                {
                    if (queue.Contains(kv.Value) == true ||
                        processed.Contains(kv.Value) == true)
                    {
                        continue;
                    }

                    queue.Enqueue(kv.Value);
                }
            }
        }

        private Dictionary<uint, ClassDefinition> LoadClasses(
            XPathNodeIterator nodes, ClassDefinition parent)
        {
            var defs = new Dictionary<uint, ClassDefinition>();

            while (nodes.MoveNext() == true)
            {
                var classDef = new ClassDefinition(this, parent);

                string className;
                uint classHash;

                LoadNameAndHash(nodes.Current, out className, out classHash);
                classDef.Name = className;

                var classParent = nodes.Current.GetAttribute("extends", "");
                classDef.SuperName = string.IsNullOrWhiteSpace(classParent) == true ?
                    null : classParent;

                var members = nodes.Current.Select("member");
                while (members.MoveNext() == true)
                {
                    string memberName;
                    uint memberHash;

                    LoadNameAndHash(members.Current, out memberName, out memberHash);

                    var type = members.Current.Value;
                    if (Enum.IsDefined(typeof(MemberType), type) == false)
                    {
                        throw new FormatException();
                    }

                    classDef.Members.Add(memberHash, new MemberDefinition()
                    {
                        Name = memberName,
                        Type = (MemberType)Enum.Parse(typeof(MemberType), type),
                    });
                }

                var children = nodes.Current.Select("class");
                classDef.Children = LoadClasses(children, classDef);

                defs.Add(classHash, classDef);
            }

            return defs;
        }

        private class ClassDefinition : IClassDefinition
        {
            public Definitions Master;
            public ClassDefinition Parent;

            public string Name { get; set; }
            public IClassDefinition Super { get; set; }

            public string SuperName;

            public Dictionary<uint, MemberDefinition> Members
                = new Dictionary<uint, MemberDefinition>();

            public Dictionary<uint, ClassDefinition> Children
                = new Dictionary<uint, ClassDefinition>();

            public ClassDefinition(Definitions master, ClassDefinition parent)
            {
                this.Master = master;
                this.Parent = parent;
            }

            public IClassDefinition GetClassDefinition(uint hash)
            {
                if (this.Children.ContainsKey(hash) == true)
                {
                    return this.Children[hash];
                }

                IClassDefinition current = this.Super;
                while (current != null)
                {
                    var def = current.GetClassDefinition(hash);
                    if (def != null)
                    {
                        return def;
                    }

                    current = current.Super;
                }

                return this.Master.GetClassDefinition(hash);
            }

            public IMemberDefinition GetMemberDefinition(uint hash)
            {
                if (this.Members.ContainsKey(hash) == true)
                {
                    return this.Members[hash];
                }

                IClassDefinition current = this.Super;
                while (current != null)
                {
                    var member = current.GetMemberDefinition(hash);
                    if (member != null)
                    {
                        return member;
                    }

                    current = current.Super;
                }

                return null;
            }
        }

        private class MemberDefinition : IMemberDefinition
        {
            public string Name { get; set; }
            public MemberType Type { get; set; }

            public string Deserialize(byte[] value)
            {
                throw new NotImplementedException();
            }

            public byte[] Serialize(string value)
            {
                throw new NotImplementedException();
            }
        }
    }
}
