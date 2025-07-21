using System.Collections.Generic;
using System.Linq;

namespace csharp_utcp
{
    public class TagSearch : IToolSearchStrategy
    {
        public IEnumerable<Tool> Search(IEnumerable<Tool> tools, string query)
        {
            return tools.Where(tool => tool.Tags != null && tool.Tags.Contains(query));
        }
    }
}
