using map2agb.Library.Event;
using System.Collections.Generic;
using System.IO;

namespace map2agb.Library.Map
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

    public struct MapScript
    {
        public byte Type;
        public ushort Variable;
        public ushort Value;
    }

    #endregion
    /// <summary>
    /// Represents a map object which can be read from a Advance Map file and written as a code representation of an ingame object.
    /// </summary>
    public class Map
    {
        #region Properties

        /// <summary>
        /// Width of the Map
        /// </summary>
        public uint Width { get; set; }

        /// <summary>
        /// Heigth of the Map
        /// </summary>
        public uint Heigth { get; set; }

        public byte BorderWidth { get; set; }

        public byte BorderHeight { get; set; }

        /// <summary>
        /// First Tileset (Number) - Will Receive Symbolic Value
        /// </summary>
        public uint FirstTileset { get; set; }

        /// <summary>
        /// Second Tileset (Number) - Will Receive Symbolic Value
        /// </summary>
        public uint SecondTileset { get; set; }

        /// <summary>
        /// 2D Map Array
        /// </summary>
        public ushort[][] Entries { get; private set; }

        /// <summary>
        /// 2D Array of Border Tiles
        /// </summary>
        public ushort[][] BorderEntries { get; private set; }

        public Person[] Persons { get; private set; }
        public Script[] Scripts { get; private set; }
        public Warp[] Warps { get; private set; }
        public Sign[] Signs { get; private set; }
        public Connection[] Connections { get; private set; }
        public IList<MapScript> MapScripts { get; set; }
        public byte[] MapHeaderData { get; set; }

        #endregion

        #region Constructor

        public Map(uint width, uint height, uint ts0, uint ts1, byte borderWidth, byte borderHeight, byte cPerson, byte cScript, byte cWarp, byte cSign, uint cConnection)
        {
            Width = width;
            Heigth = height;
            FirstTileset = ts0;
            SecondTileset = ts1;
            BorderWidth = borderWidth;
            BorderHeight = borderHeight;
            Entries = new ushort[height][];
            BorderEntries = new ushort[borderHeight][];
            Persons = new Person[cPerson];
            Warps = new Warp[cWarp];
            Scripts = new Script[cScript];
            Signs = new Sign[cSign];
            MapScripts = new List<MapScript>();
            Connections = new Connection[cConnection];
            for (int i = 0; i < height; ++i)
            {
                Entries[i] = new ushort[width];
            }
            for (int i = 0; i < borderHeight; ++i)
            {
                BorderEntries[i] = new ushort[borderWidth];
            }
        }

        #endregion

        #region Statics

        /// <summary>
        /// Reads a Advance Map *.map file and creates a Map structure, returns null on error
        /// </summary>
        /// <param name="path">Path to Advance Map *.map file</param>
        /// <returns>Parsed Map Structure or null</returns>
        public static Map FromFile(string path)
        {
            FileStream fs = null;
            Map output = null;
            try
            {
                fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                using (BinaryReader br = new BinaryReader(fs))
                {
                    fs = null;
                    uint width = br.ReadUInt32();
                    uint height = br.ReadUInt32();
                    uint ts0 = br.ReadUInt32();
                    uint ts1 = br.ReadUInt32();
                    byte borderWidth = br.ReadByte();
                    byte borderHeight = br.ReadByte();

                    ushort padding = br.ReadUInt16();
                    uint magic = br.ReadUInt32();
                    if (magic != 0x45525042)
                    {
                        //what do
                    }
                    uint borderPointer = br.ReadUInt32();
                    uint eventPointer = br.ReadUInt32();
                    uint scriptPointer = br.ReadUInt32();
                    uint connectionPointer = br.ReadUInt32();
                    byte[] mapHeaderBytes = br.ReadBytes(12);

                    /* read event header data */
                    br.BaseStream.Seek(eventPointer, SeekOrigin.Begin);

                    byte cPerson = br.ReadByte();
                    byte cWarp = br.ReadByte();
                    byte cScript = br.ReadByte();
                    byte cSign = br.ReadByte();
                    uint eventDataPointer = br.ReadUInt32();

                    br.BaseStream.Seek(connectionPointer, SeekOrigin.Begin);
                    uint cConnection = br.ReadUInt32();
                    uint connectionDataPointer = br.ReadUInt32();

                    output = new Map(width, height, ts0, ts1, borderWidth, borderHeight, cPerson, cScript, cWarp, cSign, cConnection);
                    output.MapHeaderData = mapHeaderBytes;

                    /* read border and map data */
                    br.BaseStream.Seek(borderPointer, SeekOrigin.Begin);
                    for (int y = 0; y < output.BorderHeight; ++y)
                    {
                        for (int x = 0; x < output.BorderWidth; ++x)
                        {
                            output.BorderEntries[y][x] = br.ReadUInt16();
                        }
                    }
                    for (int y = 0; y < output.Heigth; ++y)
                    {
                        for (int x = 0; x < output.Width; ++x)
                        {
                            output.Entries[y][x] = br.ReadUInt16();
                        }
                    }

                    /* read event data */
                    br.BaseStream.Seek(eventDataPointer, SeekOrigin.Begin);

                    for (int i = 0; i < cPerson; ++i)
                    {
                        output.Persons[i] = new Person(br);
                    }
                    for (int i = 0; i < cWarp; ++i)
                    {
                        output.Warps[i] = new Warp(br);
                    }
                    for (int i = 0; i < cScript; ++i)
                    {
                        output.Scripts[i] = new Script(br);
                    }
                    for (int i = 0; i < cSign; ++i)
                    {
                        output.Signs[i] = new Sign(br);
                    }

                    /* read script data */
                    br.BaseStream.Seek(scriptPointer, SeekOrigin.Begin);
                    byte scriptType;
                    while ((scriptType = br.ReadByte()) != 0)
                    {
                        MapScript s = new MapScript();
                        s.Type = scriptType;
                        switch (scriptType)
                        {
                            case 1:
                            case 3:
                            case 5:
                            case 6:
                            case 7:
                                /* we do not need the script offset because we will make it a symbol */
                                br.ReadUInt32();
                                break;
                            case 2:
                            case 4:
                                uint scriptDataPointer = br.ReadUInt32();
                                long currentOffset = br.BaseStream.Position;
                                br.BaseStream.Seek(scriptDataPointer, SeekOrigin.Begin);
                                s.Variable = br.ReadUInt16();
                                s.Value = br.ReadUInt16();
                                /* we do not need the script offset because we will make it a symbol */
                                br.ReadUInt32();
                                br.BaseStream.Seek(currentOffset, SeekOrigin.Begin);
                                break;
                        }
                        output.MapScripts.Add(s);
                    }
                    br.BaseStream.Seek(connectionDataPointer, SeekOrigin.Begin);
                    for (int i = 0; i < cConnection; ++i)
                    {
                        Connection c = new Connection();
                        uint connectionType = br.ReadUInt32();
                        int connectionOffset = br.ReadInt32();
                        byte bank = br.ReadByte();
                        byte map = br.ReadByte();
                        ushort cPadding = br.ReadUInt16();
                        c.Direction = connectionType;
                        c.Offset = connectionOffset;
                        c.Bank = bank;
                        c.MapNumber = map;
                        c.Padding = cPadding;
                        output.Connections[i] = c;
                    }
                }
            }
            finally
            {
                if (fs != null)
                    fs.Dispose();
            }
            return output;
        }

        #endregion
    }
}
