using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace map2agb.Library.Tileset
{
    public class Tileset
    {
        public uint Image { get; set; }
        public uint Palette { get; set; }
        public ushort[][] Blocks { get; set; }
        public uint Animation { get; set; }
        public uint[] Behavior { get; set; }

        public Tileset(uint numberOfBlocks)
        {
            Blocks = new ushort[numberOfBlocks][];
            for (int i = 0; i < numberOfBlocks; ++i)
            {
                Blocks[i] = new ushort[8];
            }
            Behavior = new uint[numberOfBlocks];
            Image = 0;
            Palette = 0;
            Animation = 0;
        }

        /// <summary>
        /// Reads a tileset from a *.bvd file - This might only work for firered, be careful with other formats
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Tileset FromFile(string path)
        {
            FileStream fs = null;
            Tileset output = null;
            try
            {
                fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                using (BinaryReader br = new BinaryReader(fs))
                {
                    fs = null;
                    br.BaseStream.Seek(-4, SeekOrigin.End);
                    uint magic = br.ReadUInt32();
                    if (magic == 0x474C5246)
                    {
                        //Magic number matches FRLG
                        br.BaseStream.Seek(0, SeekOrigin.Begin);
                        uint numberOfBlocks = br.ReadUInt32();
                        output = new Tileset(numberOfBlocks);
                        for (int i = 0; i < numberOfBlocks; ++i)
                        {
                            for (int j = 0; j < 8; ++j)
                            {
                                output.Blocks[i][j] = br.ReadUInt16();
                            }
                        }
                        for (int i = 0; i < numberOfBlocks; ++i)
                        {
                            output.Behavior[i] = br.ReadUInt32();
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
    }
}
