using System.IO;

namespace map2agb.Library.Event
{
    public class Script
    {
        public Script(BinaryReader reader)
        {
            X = reader.ReadUInt16();
            Y = reader.ReadUInt16();
            Height = reader.ReadByte();
            UnknownOne = reader.ReadByte();
            Variable = reader.ReadUInt16();
            Value = reader.ReadUInt16();
            UnknownTwo = reader.ReadUInt16();
            ScriptOffset = reader.ReadUInt32();
        }

        public ushort X { get; set; }
        public ushort Y { get; set; }
        public byte Height { get; set; }
        public byte UnknownOne { get; set; }
        public ushort Variable { get; set; }
        public ushort Value { get; set; }
        public ushort UnknownTwo { get; set; }
        public uint ScriptOffset { get; set; }
    }
}
