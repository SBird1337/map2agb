using CommandLine;
using CommandLine.Text;
using System.IO;
using System;
using Newtonsoft.Json;
using map2agb.Library.Map;
using map2agb.Library.Tileset;

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
                            sw.WriteLine();
                            sw.WriteLine(".align 2");
                            sw.WriteLine(".global " + mapFooterSymbol);
                            sw.WriteLine(mapFooterSymbol + ":");
                            sw.WriteLine();

                            /* write the map footer */
                            string mapBorderSymbol = config.InternalName + "_" + "map_border";
                            string mapTileOrderSymbol = config.InternalName + "_" + "map_tile_order";
                            string firstTilesetName = "tileset_header" + m.FirstTileset.ToString();
                            string secondTilesetName = "tileset_header" + m.SecondTileset.ToString();

                            sw.WriteLine(".word " + "0x" + m.Width.ToString("X8"));
                            sw.WriteLine(".word " + "0x" + m.Heigth.ToString("X8"));
                            sw.WriteLine(".word " + mapBorderSymbol);
                            sw.WriteLine(".word " + mapTileOrderSymbol);
                            sw.WriteLine(".word " + firstTilesetName);
                            sw.WriteLine(".word " + secondTilesetName);
                            sw.WriteLine(".byte " + "0x" + m.BorderWidth);
                            sw.WriteLine(".byte " + "0x" +m.BorderHeight);
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
