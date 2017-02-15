using System.IO;

namespace map2agb.Library.Event
{
    public class Sign
    {
        public Sign(BinaryReader reader)
        {
            X = reader.ReadUInt16();
            Y = reader.ReadUInt16();
            Height = reader.ReadByte();
            Type = reader.ReadByte();
            Unknown = reader.ReadUInt16();
            Script = reader.ReadUInt32();
        }

        public ushort X { get; set; }
        public ushort Y { get; set; }
        public byte Height { get; set; }
        public byte Type { get; set; }
        public ushort Unknown { get; set; }
        public uint Script { get; set; }

    }
}
