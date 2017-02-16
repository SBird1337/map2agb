using System.Collections.Generic;

namespace map2agb.Library.Map
{
    /// <summary>
    /// Specifies a map Configuration File, mainly containing information about the map header
    /// </summary>
    public class MapConfiguration
    {
        #region Structures

        public struct Connection
        {
            public uint Direction;
            public int Offset;
            public byte Bank;
            public byte MapNumber;
            public ushort Padding;
        }

        public struct Script
        {
            public byte Type;
            public ushort Variable;
            public ushort Value;
        }
        
        #endregion
        #region Properties

        public string MapFile { get; set; }
        public string EventFile { get; set; }
        public ushort Music { get; set; }
        public ushort MapIndex { get; set; }
        public byte NameIndex { get; set; }
        public byte Cave { get; set; }
        public byte Weather { get; set; }
        public byte Light { get; set; }
        public byte Unknown { get; set; }
        public byte EscapeRope { get; set; }
        public byte ShowName { get; set; }
        public byte BattleType { get; set; }
        
        public IList<Connection> Connections {get;set;}
        public IList<Script> Scripts { get; set; }
        #endregion
    }
}
