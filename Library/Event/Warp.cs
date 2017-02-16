using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace map2agb.Library.Event
{
    public class Warp
    {
        public Warp(BinaryReader reader)
        {
            X = reader.ReadUInt16();
            Y = reader.ReadUInt16();
            Height = reader.ReadByte();
            WarpNumber = reader.ReadByte();
            MapNumber = reader.ReadByte();
            MapBank = reader.ReadByte();
        }

        public ushort X { get; set; }
        public ushort Y { get; set; }
        public byte Height { get; set; }
        public byte WarpNumber { get; set; }
        public byte MapNumber { get; set; }
        public byte MapBank { get; set; }
    }
}
