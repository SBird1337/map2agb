using CommandLine;
using CommandLine.Text;
using System.IO;
using System;
using Newtonsoft.Json;

namespace map2agb
{
    class Options
    {
        [Option('f', Required = true)]
        public string ConfigFile { get; set; }

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
                Map m = Map.FromFile(config.MapFile);
                Console.WriteLine("Mapheader of {0}:\nWidth: {1}\nHeigth: {2}\nTileset 0: {3}\nTileset 1: {4}\nBorder Width: {5}\nBorder Heigth: {5}", config.MapFile, m.Width, m.Heigth, m.FirstTileset, m.SecondTileset, m.BorderWidth, m.BorderHeigth);
                Console.WriteLine("Border Block:");
                Console.WriteLine();
                for (int y = 0; y < m.BorderHeigth; ++y)
                {
                    for (int x = 0; x < m.BorderWidth; ++x)
                    {
                        Console.Write("[{0}] ", m.BorderEntries[x][y].ToString("X4"));

                    }
                    Console.WriteLine();
                }

                Console.WriteLine("Map:");
                Console.WriteLine();
                for (int y = 0; y < m.Heigth; ++y)
                {
                    for (int x = 0; x < m.Width; ++x)
                    {
                        Console.Write("[{0}] ", m.Entries[x][y].ToString("X4"));

                    }
                    Console.WriteLine();
                }
            }
        }
    }
}
