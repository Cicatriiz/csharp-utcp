using System.Text.Json;
using System.Text.Json.Nodes;
using Grpc.Net.Client;

namespace utcp
{
    public class GrpcTransportLogic : ITransport
    {
        public JsonNode Execute(Transport transport, JsonObject inputs)
        {
            var grpcTransport = (GrpcTransport)transport;
            var channel = GrpcChannel.ForAddress(grpcTransport.Address);
            var assembly = System.Reflection.Assembly.Load("GrpcServer");
            var clientType = assembly.GetType(grpcTransport.Service);
            if (clientType == null) throw new System.TypeLoadException($"Service type '{grpcTransport.Service}' not found in assembly.");

            var client = System.Activator.CreateInstance(clientType, channel);
            if (client == null) throw new System.InvalidOperationException($"Failed to create instance of service '{grpcTransport.Service}'.");

            var method = client.GetType().GetMethod(grpcTransport.Method);
            if (method == null) throw new System.MissingMethodException($"Method '{grpcTransport.Method}' not found on service '{grpcTransport.Service}'.");

            var requestType = method.GetParameters()[0].ParameterType;
            var request = JsonSerializer.Deserialize(inputs.ToString(), requestType);
            var responseTask = method.Invoke(client, new object[] { request!, new Grpc.Core.CallOptions() });
            if (responseTask == null) throw new System.InvalidOperationException("gRPC method invocation returned null.");

            var response = ((dynamic)responseTask).Result;
            return JsonSerializer.SerializeToNode(response)!;
        }
    }
}
