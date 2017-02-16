using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace map2agb.Library.Event
{
    /// <summary>
    /// Represents a person on a standard Map Event Page
    /// </summary>
    public class Person
    {

        public Person(BinaryReader reader)
        {
            Identifier = reader.ReadByte();
            Image = reader.ReadByte();
            UnknownOne = reader.ReadUInt16();
            X = reader.ReadUInt16();
            Y = reader.ReadUInt16();
            TalkArea = reader.ReadByte();
            Movement = reader.ReadByte();
            MovementArea = reader.ReadByte();
            UnknownTwo = reader.ReadByte();
            Trainer = (reader.ReadByte() == 0) ? false : true;
            UnknownThree = reader.ReadByte();
            Sight = reader.ReadUInt16();
            Script = reader.ReadUInt32();
            Flag = reader.ReadUInt16();
            UnknownFour = reader.ReadUInt16();
        }

        public byte Identifier { get; set; }
        public byte Image { get; set; }
        public ushort UnknownOne { get; set; }
        public ushort X { get; set; }
        public ushort Y { get; set; }
        public byte TalkArea { get; set; }
        public byte Movement { get; set; }
        public byte MovementArea { get; set; }
        public byte UnknownTwo { get; set; }
        public bool Trainer { get; set; }
        public byte UnknownThree { get; set; }
        public ushort Sight { get; set; }
        public uint Script { get; set; }
        public ushort Flag { get; set; }
        public ushort UnknownFour { get; set; }
    }
}
