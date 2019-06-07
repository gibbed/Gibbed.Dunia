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
using System.IO;
using Gibbed.IO;

namespace Gibbed.Dunia.FileFormats.Geometry
{
    public class Root : IBlock
    {
        public MaterialReference MaterialReference;
        public Nodes Nodes;
        public O2BM O2BM;
        public SKID SKID;
        public SKND SKND;
        public LODs LODs;
        public BoundingBox BoundingBox;
        public BSPH BSPH;
        public LODInfo LOD;
        public PCMP PCMP;
        public UCMP UCMP;
        public IKDA IKDA;
        public List<MaterialDescriptor> MaterialDescriptors = new List<MaterialDescriptor>();

        public BlockType Type
        {
            get { return BlockType.Root; }
        }

        public void Deserialize(IBlock parent, Stream input, Endian endian)
        {
        }

        public void Serialize(IBlock parent, Stream output, Endian endian)
        {
        }

        public IBlock CreateBlock(BlockType type)
        {
            switch (type)
            {
                case BlockType.MaterialReference:
                {
                    return new MaterialReference();
                }

                case BlockType.Nodes:
                {
                    return new Nodes();
                }

                case BlockType.O2BM:
                {
                    return new O2BM();
                }

                case BlockType.SKID:
                {
                    return new SKID();
                }

                case BlockType.SKND:
                {
                    return new SKND();
                }

                case BlockType.LODs:
                {
                    return new LODs();
                }

                case BlockType.BoundingBox:
                {
                    return new BoundingBox();
                }

                case BlockType.BSPH:
                {
                    return new BSPH();
                }

                case BlockType.LODInfo:
                {
                    return new LODInfo();
                }

                case BlockType.PCMP:
                {
                    return new PCMP();
                }

                case BlockType.UCMP:
                {
                    return new UCMP();
                }

                case BlockType.IKDA:
                {
                    return new IKDA();
                }

                case BlockType.MaterialDescriptor:
                {
                    return new MaterialDescriptor();
                }
            }

            throw new NotSupportedException();
        }

        private static void SetChild<TType>(IBlock child, ref TType value)
            where TType : class, IBlock
        {
            if (child is TType)
            {
                if (value != null)
                {
                    throw new InvalidOperationException();
                }

                value = (TType)child;
            }
        }

        public void AddChild(IBlock child)
        {
            SetChild(child, ref this.MaterialReference);
            SetChild(child, ref this.Nodes);
            SetChild(child, ref this.O2BM);
            SetChild(child, ref this.SKID);
            SetChild(child, ref this.SKND);
            SetChild(child, ref this.LODs);
            SetChild(child, ref this.BoundingBox);
            SetChild(child, ref this.BSPH);
            SetChild(child, ref this.LOD);
            SetChild(child, ref this.PCMP);
            SetChild(child, ref this.UCMP);
            SetChild(child, ref this.IKDA);

            var materialDescriptor = child as MaterialDescriptor;
            if (materialDescriptor != null)
            {
                this.MaterialDescriptors.Add(materialDescriptor);
            }
        }

        private static void GetChild(List<IBlock> blocks, IBlock value)
        {
            if (value != null)
            {
                blocks.Add(value);
            }
        }

        public IEnumerable<IBlock> GetChildren()
        {
            var children = new List<IBlock>();
            GetChild(children, this.MaterialReference);
            GetChild(children, this.Nodes);
            GetChild(children, this.O2BM);
            GetChild(children, this.SKID);
            GetChild(children, this.SKND);
            GetChild(children, this.LODs);
            GetChild(children, this.BoundingBox);
            GetChild(children, this.BSPH);
            GetChild(children, this.LOD);
            GetChild(children, this.PCMP);
            GetChild(children, this.UCMP);
            GetChild(children, this.IKDA);
            children.AddRange(this.MaterialDescriptors);
            return children;
        }
    }
}
