using System.Collections.Generic;

namespace utcp
{
    public interface IToolSearchStrategy
    {
        IEnumerable<Tool> Search(IEnumerable<Tool> tools, string query);
    }
}
