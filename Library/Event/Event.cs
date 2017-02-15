using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace map2agb.Library.Event
{
    public class Event
    {
        public Person[] Persons { get; private set; }
        public Script[] Scripts { get; private set; }
        public Warp[] Warps { get; private set; }
        public Sign[] Signs { get; private set; }

        public Event(int cPerson, int cScript, int cWarp, int cSign)
        {
            Persons = new Person[cPerson];
            Scripts = new Script[cScript];
            Warps = new Warp[cWarp];
            Signs = new Sign[cSign];
        }

        public static Event FromFile(string path)
        {
            FileStream fs = null;
            Event output = null;
            try
            {
                fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                using (BinaryReader br = new BinaryReader(fs))
                {
                    fs = null;
                    br.BaseStream.Seek(-4, SeekOrigin.End);
                    byte cPerson = br.ReadByte();
                    byte cWarp = br.ReadByte();
                    byte cScript = br.ReadByte();
                    byte cSign = br.ReadByte();

                    br.BaseStream.Seek(0, SeekOrigin.Begin);
                    output = new Event(cPerson, cScript, cWarp, cSign);
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
