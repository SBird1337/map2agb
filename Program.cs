using CommandLine;
using CommandLine.Text;
using System.IO;
using System;
using Newtonsoft.Json;
using map2agb.Library.Map;
using map2agb.Library.Event;
using map2agb.Library.Tileset;
using System.Collections;
using System.Collections.Generic;

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
                Map m = null;
                Event e = null;
                Tileset t = null;

                if (config.InternalName == null)
                {
                    Error("configuration file needs to have 'InternalName' field");
                    return;
                }
                MapConfiguration mapConfig = null;
                if (config.MapConfigurationFile != null)
                {
                    try
                    {
                        mapConfig = JsonConvert.DeserializeObject<MapConfiguration>(File.ReadAllText(config.MapConfigurationFile));
                    }
                    catch (Exception ex)
                    {
                        Error("could not read map configuration");
                        return;
                    }
                    
                    m = Map.FromFile(mapConfig.MapFile);
                    e = Event.FromFile(mapConfig.EventFile);
                    if (m == null || e == null)
                    {
                        Error("'MapFile' or 'EventFile' not specified in your map configuration or failed loading");
                        return;
                    }
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

                            /* write a map header */
                            sw.WriteLine(".global " + mapHeaderSymbol);
                            sw.WriteLine(mapHeaderSymbol + ":");
                            sw.WriteLine();
                            sw.WriteLine(".word " + mapFooterSymbol);
                            sw.WriteLine(".word " + mapEventSymbol);
                            sw.WriteLine(".word " + mapScriptSymbol);
                            sw.WriteLine(".word " + ((mapConfig.Connections == null) ? "0x00000000" : mapConnectionsSymbol));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Error("could not open output file for writing");
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
