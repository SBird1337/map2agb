using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace map2agb
{
    /// <summary>
    /// Configuration File for the tool to work with, specifies general information on the map
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// The Advance Map *.map file to use
        /// </summary>
        public string MapFile { get; set; }

        /// <summary>
        /// The Advance Map *.evt file to use
        /// </summary>
        public string EventFile { get; set; }

        /// <summary>
        /// The internal name to use for symbolic references
        /// </summary>
        public string InternalName { get; set; }
    }
}
