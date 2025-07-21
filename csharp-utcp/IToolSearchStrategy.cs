using System.Collections.Generic;

namespace csharp_utcp
{
    public interface IToolSearchStrategy
    {
        IEnumerable<Tool> Search(IEnumerable<Tool> tools, string query);
    }
}
