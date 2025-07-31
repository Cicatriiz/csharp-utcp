using System.Collections.Generic;

namespace utcp
{
    public interface IToolRepository
    {
        void AddTool(Tool tool);
        Tool? GetTool(string name);
        IEnumerable<Tool> GetAllTools();
    }
}
