using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace csharp_utcp
{
    public class OpenApiConverter
    {
        public static UTCPManual Convert(string openApiSpec)
        {
            var openApiDocument = new OpenApiStringReader().Read(openApiSpec, out var diagnostic);

            var manual = new UTCPManual
            {
                UtcpVersion = "1.0.0",
                ManualVersion = openApiDocument.Info.Version,
                Tools = new List<Tool>()
            };

            foreach (var path in openApiDocument.Paths)
            {
                foreach (var operation in path.Value.Operations)
                {
                    var tool = new Tool
                    {
                        Name = operation.Value.OperationId,
                        Description = operation.Value.Summary ?? operation.Value.Description,
                        Inputs = new JsonObject(),
                        Outputs = new JsonObject(),
                        ToolTransport = new HttpTransport
                        {
                            TransportType = "http",
                            Url = $"{openApiDocument.Servers[0].Url}{path.Key}",
                            HttpMethod = operation.Key.ToString().ToUpper()
                        }
                    };

                    foreach (var parameter in operation.Value.Parameters)
                    {
                        tool.Inputs[parameter.Name] = new JsonObject
                        {
                            ["type"] = parameter.Schema.Type
                        };
                    }

                    manual.Tools.Add(tool);
                }
            }

            return manual;
        }
    }
}
