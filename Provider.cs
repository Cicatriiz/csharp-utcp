using System.Collections.Generic;

namespace csharp_utcp
{
    public class Provider
    {
        public required string Name { get; set; }
        public List<Tool> Tools { get; set; } = new List<Tool>();
    }
}
