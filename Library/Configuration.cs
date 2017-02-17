namespace map2agb
{
    /// <summary>
    /// Configuration File for the tool to work with, specifies general information on the map
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// The *.map file to use for the map data
        /// </summary>
        public string MapFile { get; set; }

        /// <summary>
        /// The tileset (*.bvd) file to parse
        /// </summary>
        public string TilesetFile { get; set; }

        public string InternalName { get; set; }
    }
}
