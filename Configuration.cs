namespace map2agb
{
    /// <summary>
    /// Configuration File for the tool to work with, specifies general information on the map
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// The *.mcfg file to use for the map data
        /// </summary>
        public string MapConfigurationFile { get; set; }

        /// <summary>
        /// The internal name to use for symbolic references
        /// </summary>
        public string InternalName { get; set; }

        /// <summary>
        /// The tileset (*.bvt) file to parse, optional
        /// </summary>
        public string TilesetFile { get; set; }
    }
}
