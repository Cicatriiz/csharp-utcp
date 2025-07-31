using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace utcp
{
    public class UtcpClientConfig
    {
        public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();

        public string? ResolveVariable(string? value)
        {
            if (value == null)
            {
                return null;
            }

            var match = Regex.Match(value, @"\${(.*?)}");
            if (match.Success)
            {
                var varName = match.Groups[1].Value;
                if (Variables.TryGetValue(varName, out var resolvedValue))
                {
                    return resolvedValue;
                }
                return System.Environment.GetEnvironmentVariable(varName);
            }

            if (value.StartsWith("$"))
            {
                var varName = value.Substring(1);
                if (Variables.TryGetValue(varName, out var resolvedValue))
                {
                    return resolvedValue;
                }
                return System.Environment.GetEnvironmentVariable(varName);
            }

            return value;
        }
    }
}
