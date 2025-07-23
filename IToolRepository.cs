using System.Collections.Generic;

namespace csharp_utcp
{
    public interface IToolRepository
    {
        void AddTool(Tool tool);
        Tool? GetTool(string name);
        IEnumerable<Tool> GetAllTools();
    }
}
