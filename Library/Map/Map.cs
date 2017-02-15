using System.IO;

namespace map2agb.Library.Map
{
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

        public byte BorderHeigth { get; set; }

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

        public ushort[][] BorderEntries { get; private set; }
        #endregion

        #region Constructor

        public Map(uint width, uint heigth, uint ts0, uint ts1, byte borderWidth, byte borderHeight)
        {
            Width = width;
            Heigth = heigth;
            FirstTileset = ts0;
            SecondTileset = ts1;
            BorderWidth = borderWidth;
            BorderHeigth = borderHeight;
            Entries = new ushort[width][];
            BorderEntries = new ushort[borderWidth][];
            for (int i = 0; i < width; ++i)
            {
                Entries[i] = new ushort[heigth];
            }
            for (int i = 0; i < borderWidth; ++i)
            {
                BorderEntries[i] = new ushort[borderHeight];
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
                    output = new Map(br.ReadUInt32(), br.ReadUInt32(), br.ReadUInt32(), br.ReadUInt32(), br.ReadByte(), br.ReadByte());
                    ushort padding = br.ReadUInt16();
                    for (int y = 0; y < output.BorderHeigth; ++y)
                    {
                        for (int x = 0; x < output.BorderWidth; ++x)
                        {
                            output.BorderEntries[x][y] = br.ReadUInt16();
                        }
                    }
                    for (int y = 0; y < output.Heigth; ++y)
                    {
                        for (int x = 0; x < output.Width; ++x)
                        {
                            output.Entries[x][y] = br.ReadUInt16();
                        }
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
        //etcetc
    }
}
