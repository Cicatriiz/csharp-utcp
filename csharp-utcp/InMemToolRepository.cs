using System.Collections.Generic;

namespace csharp_utcp
{
    public class InMemToolRepository : IToolRepository
    {
        private readonly Dictionary<string, Tool> _tools = new Dictionary<string, Tool>();

        public void AddTool(Tool tool)
        {
            _tools[tool.Name] = tool;
        }

        public Tool GetTool(string name)
        {
            _tools.TryGetValue(name, out var tool);
            return tool;
        }

        public IEnumerable<Tool> GetAllTools()
        {
            return _tools.Values;
        }
    }
}
