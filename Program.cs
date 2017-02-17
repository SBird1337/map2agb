using CommandLine;
using CommandLine.Text;
using System.IO;
using System;
using Newtonsoft.Json;
using map2agb.Library.Map;
using map2agb.Library.Tileset;
using System.Linq;
using map2agb.Library;

namespace map2agb
{
    class Options
    {
        [Option('f', Required = true)]
        public string ConfigFile { get; set; }

        [Option('o', Required = true)]
        public string OutputFile { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }

    class Program
    {
        static void Error(string fmt, params object[] pars)
        {
            Console.Error.Write("{0} - error: ", AppDomain.CurrentDomain.FriendlyName);
            Console.Error.WriteLine(fmt, pars);
        }

        static void Main(string[] args)
        {
            Options opt = new Options();
            if (Parser.Default.ParseArguments(args, opt))
            {
                //Parsed correctly
                if (!File.Exists(opt.ConfigFile))
                {
                    Error("file {0} not found", opt.ConfigFile);
                    return;
                }
                string configJson;
                try
                {
                    configJson = File.ReadAllText(opt.ConfigFile);
                }
                catch (Exception ex)
                {
                    Error("could not read config file: {0}", ex.Message);
                    return;
                }
                Configuration config;
                try
                {
                    config = JsonConvert.DeserializeObject<Configuration>(configJson);
                }
                catch (Exception ex)
                {
                    Error("could not deserialize config file: {0}", ex.Message);
                    return;
                }
                if (config.InternalName == null)
                {
                    Error("no internal name specified, set field 'InternalName'");
                    return;
                }
                Map m = null;
                Tileset t = null;
                if (config.MapFile != null)
                {
                    m = Map.FromFile(config.MapFile);
                }

                if (config.TilesetFile != null)
                {
                    t = Tileset.FromFile(config.TilesetFile);
                }

                FileStream fs = null;
                try
                {
                    fs = new FileStream(opt.OutputFile, FileMode.Create, FileAccess.Write);
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        fs = null;
                        sw.WriteLine(".align 2");
                        sw.WriteLine(".text");
                        sw.WriteLine();
                        if (m != null)
                        {
                            string mapHeaderSymbol = config.InternalName + "_" + "map_header";
                            string mapFooterSymbol = config.InternalName + "_" + "map_footer";
                            string mapEventSymbol = config.InternalName + "_" + "map_events";
                            string mapScriptSymbol = config.InternalName + "_" + "map_scripts";
                            string mapConnectionsSymbol = config.InternalName + "_" + "map_connections";

                            /* write the map header */
                            sw.WriteLine(".global " + mapHeaderSymbol);
                            sw.WriteLine(mapHeaderSymbol + ":");
                            sw.WriteLine();
                            sw.WriteLine(".word " + mapFooterSymbol);
                            sw.WriteLine(".word " + mapEventSymbol);
                            sw.WriteLine(".word " + mapScriptSymbol);
                            sw.WriteLine(".word " + ((m.Connections.Length == 0) ? "0x00000000" : mapConnectionsSymbol));
                            sw.WriteLine(".byte " + string.Join(", ", m.MapHeaderData.Select(b => "0x" + b.ToString("X2")).ToArray()));
                            sw.WriteLine();
                            sw.WriteLine(".align 2");
                            sw.WriteLine(".global " + mapFooterSymbol);
                            sw.WriteLine(mapFooterSymbol + ":");
                            sw.WriteLine();

                            /* write the map footer */
                            string mapBorderSymbol = config.InternalName + "_" + "map_border";
                            string mapTileOrderSymbol = config.InternalName + "_" + "map_tile_order";
                            string firstTilesetName = "tileset_header_" + m.FirstTileset.ToString();
                            string secondTilesetName = "tileset_header_" + m.SecondTileset.ToString();

                            sw.WriteLine(".word " + "0x" + m.Width.ToString("X8"));
                            sw.WriteLine(".word " + "0x" + m.Heigth.ToString("X8"));
                            sw.WriteLine(".word " + mapBorderSymbol);
                            sw.WriteLine(".word " + mapTileOrderSymbol);
                            sw.WriteLine(".word " + firstTilesetName);
                            sw.WriteLine(".word " + secondTilesetName);
                            sw.WriteLine(".byte " + "0x" + m.BorderWidth.ToString("X2"));
                            sw.WriteLine(".byte " + "0x" + m.BorderHeight.ToString("X2"));
                            sw.WriteLine();
                            sw.WriteLine(".align 2");
                            sw.WriteLine(".global " + mapBorderSymbol);
                            sw.WriteLine(mapBorderSymbol + ": ");

                            /* write the map border */
                            sw.Write(".hword ");
                            for (int y = 0; y < m.BorderEntries.Length; ++y)
                            {
                                for (int x = 0; x < m.BorderEntries[y].Length; x++)
                                {
                                    sw.Write("0x" + m.BorderEntries[y][x].ToString("X4"));
                                    if ((y == m.BorderEntries.Length - 1) && (x == m.BorderEntries[y].Length - 1))
                                    {
                                        sw.WriteLine();
                                    }
                                    else
                                    {
                                        sw.Write(", ");
                                    }
                                }
                            }

                            /* write the actual map */
                            sw.WriteLine();
                            sw.WriteLine(".align 2");
                            sw.WriteLine(".global " + mapTileOrderSymbol);
                            sw.WriteLine(mapTileOrderSymbol + ":");
                            sw.WriteLine();

                            for (int y = 0; y < m.Entries.Length; ++y)
                            {
                                sw.Write(".hword ");
                                for (int x = 0; x < m.Entries[y].Length; x++)
                                {
                                    sw.Write("0x" + m.Entries[y][x].ToString("X4"));
                                    if (x == m.Entries[y].Length - 1)
                                    {
                                        sw.WriteLine();
                                    }
                                    else
                                    {
                                        sw.Write(", ");
                                    }
                                }
                            }
                            sw.WriteLine();

                            /* write event data */
                            sw.WriteLine(".align 2");
                            sw.WriteLine(".global " + mapEventSymbol);
                            sw.WriteLine(mapEventSymbol + ":");
                            sw.WriteLine();
                            sw.WriteLine(".byte 0x" + m.Persons.Length.ToString("X2") + ", 0x" + m.Warps.Length.ToString("X2") + ", 0x" + m.Scripts.Length.ToString("X2") + ", 0x" + m.Signs.Length.ToString("X2"));
                            for (int i = 0; i < m.Persons.Length; ++i)
                            {
                                sw.WriteLine(".byte 0x" + m.Persons[i].Identifier.ToString("X2") + ", 0x" + m.Persons[i].Image.ToString("X2"));
                                sw.WriteLine(".hword 0x" + m.Persons[i].UnknownOne.ToString("X4") + ", 0x" + m.Persons[i].X.ToString("X4") + ", 0x" + m.Persons[i].Y.ToString("X4"));
                                sw.WriteLine(".byte 0x" + m.Persons[i].TalkArea.ToString("X2") + ", 0x" + m.Persons[i].Movement.ToString("X2") + ", 0x" + m.Persons[i].MovementArea.ToString("X2") + ", 0x" + m.Persons[i].UnknownTwo.ToString("X2") + ", 0x" + (m.Persons[i].Trainer ? "01" : "00") + ", 0x" + m.Persons[i].UnknownThree.ToString("X2"));
                                sw.WriteLine(".hword 0x" + m.Persons[i].Sight.ToString("X4"));
                                sw.WriteLine(".word " + config.InternalName + "_npc_" + i.ToString("D2") + "_script");
                                sw.WriteLine(".hword 0x" + m.Persons[i].Flag.ToString("X4") + ", 0x" + m.Persons[i].UnknownFour.ToString("X4"));
                                sw.WriteLine();
                            }
                            sw.WriteLine();
                            for (int i = 0; i < m.Warps.Length; ++i)
                            {
                                sw.WriteLine(".hword 0x" + m.Warps[i].X.ToString("X4") + ", 0x" + m.Warps[i].Y.ToString("X4"));
                                sw.WriteLine(".byte 0x" + m.Warps[i].Height.ToString("X2") + ", 0x" + m.Warps[i].WarpNumber.ToString("X") + ", 0x" + m.Warps[i].MapNumber.ToString("X2") + ", 0x" + m.Warps[i].MapBank.ToString("X2"));
                                sw.WriteLine();
                            }
                            sw.WriteLine();
                            for (int i = 0; i < m.Scripts.Length; ++i)
                            {
                                sw.WriteLine(".hword 0x" + m.Scripts[i].X.ToString("X4") + ", 0x" + m.Scripts[i].Y.ToString("X4"));
                                sw.WriteLine(".byte 0x" + m.Scripts[i].Height.ToString("X2") + ", 0x" + m.Scripts[i].UnknownOne.ToString("X2"));
                                sw.WriteLine(".hword 0x" + m.Scripts[i].Variable.ToString("X4") + ", 0x" + m.Scripts[i].Value.ToString("X4") + ", 0x" + m.Scripts[i].UnknownTwo.ToString("X4"));
                                sw.WriteLine(".word " + config.InternalName + "_field_" + i.ToString("D2") + "_script");
                                sw.WriteLine();
                            }
                            sw.WriteLine();
                            for (int i = 0; i < m.Signs.Length; ++i)
                            {
                                sw.WriteLine(".hword 0x" + m.Signs[i].X.ToString("X4") + ", 0x" + m.Signs[i].Y.ToString("X4"));
                                sw.WriteLine(".byte 0x" + m.Signs[i].Height.ToString("X2") + ", 0x" + m.Signs[i].Type.ToString("X2"));
                                sw.WriteLine(".hword 0x" + m.Signs[i].Unknown.ToString("X4"));
                                if (m.Signs[i].Type < 5)
                                {
                                    sw.WriteLine(".word " + config.InternalName + "_sign_" + i.ToString("D2") + "_script");
                                }
                                else
                                {
                                    sw.WriteLine(".word " + m.Signs[i].Script.ToString("X8"));
                                }
                                sw.WriteLine();
                            }
                            sw.WriteLine();

                            /* write script data - care for unaligned words (must be written unaligned to rom)*/
                            sw.WriteLine(".align 2");
                            sw.WriteLine(".global " + mapScriptSymbol);
                            sw.WriteLine(mapScriptSymbol + ":");
                            sw.WriteLine();
                            for (int i = 0; i < m.MapScripts.Count; ++i)
                            {
                                sw.WriteLine(".byte 0x" + m.MapScripts[i].Type.ToString("X2"));
                                switch (m.MapScripts[i].Type)
                                {
                                    case 1:
                                    case 3:
                                    case 5:
                                    case 6:
                                    case 7:
                                        sw.WriteLine(".word " + config.InternalName + "_level_" + i.ToString("D2") + "_script");
                                        break;
                                    case 2:
                                    case 4:
                                        sw.WriteLine(".word " + config.InternalName + "_level_" + i.ToString("D2") + "_data");
                                        sw.WriteLine();
                                        break;
                                }
                                sw.WriteLine();
                            }
                            sw.WriteLine(".align 2");
                            for (int i = 0; i < m.MapScripts.Count; ++i)
                            {
                                if (m.MapScripts[i].Type == 2 || m.MapScripts[i].Type == 4)
                                {
                                    sw.WriteLine(config.InternalName + "_level_" + i.ToString("D2") + "_data:");
                                    sw.WriteLine(".hword 0x" + m.MapScripts[i].Variable.ToString("X4") + ", 0x" + m.MapScripts[i].Value.ToString("X4"));
                                    sw.WriteLine(".word " + config.InternalName + "_level_" + i.ToString("D2") + "_script");
                                    sw.WriteLine();
                                }
                            }
                            sw.WriteLine();

                            /* write map connections */
                            if (m.Connections.Length > 0)
                            {
                                sw.WriteLine(".align 2");
                                sw.WriteLine(".global " + mapConnectionsSymbol);
                                sw.WriteLine(mapConnectionsSymbol + ":");
                                sw.WriteLine();

                                sw.WriteLine(".word 0x" + m.Connections.Length.ToString("X8"));
                                sw.WriteLine(".word " + config.InternalName + "_connections_internal");
                                sw.WriteLine();
                                sw.WriteLine(".align 2");
                                sw.WriteLine(config.InternalName + "_connections_internal:");
                                sw.WriteLine();
                                for (int i = 0; i < m.Connections.Length; ++i)
                                {
                                    sw.WriteLine(".word 0x" + m.Connections[i].Direction.ToString("X8") + ", 0x" + m.Connections[i].Offset.ToString("X8"));
                                    sw.WriteLine(".byte 0x" + m.Connections[i].Bank.ToString("X2") + ", 0x" + m.Connections[i].MapNumber.ToString("X2"));
                                    sw.WriteLine(".hword 0x" + m.Connections[i].Padding.ToString("X4"));
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Error("could not open output file for writing: {0}", ex.Message);
                }
                finally
                {
                    if (fs != null)
                        fs.Dispose();
                }
            }
        }
    }
}
