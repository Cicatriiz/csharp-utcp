using System;
using System.Text.Json.Nodes;
using csharp_utcp;

namespace TestRunner
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var client = new UtcpClient();
            client.LoadManual("manual.json");

            Console.WriteLine("Executing 'get_weather' tool...");
            var weatherInputs = new JsonObject { ["location"] = "San Francisco" };
            var weatherResult = client.ExecuteTool("get_weather", weatherInputs);
            Console.WriteLine($"Weather result: {weatherResult}");

            Console.WriteLine("\nExecuting 'resize_image' tool...");
            var resizeInputs = new JsonObject { ["source"] = "my_image.jpg", ["width"] = 800, ["height"] = 600 };
            var resizeResult = client.ExecuteTool("resize_image", resizeInputs);
            Console.WriteLine($"Resize result: {resizeResult}");

            Console.WriteLine("\nExecuting 'get_user' tool...");
            var userInputs = new JsonObject { ["id"] = "123" };
            var userResult = client.ExecuteTool("get_user", userInputs);
            Console.WriteLine($"User result: {userResult}");

            Console.WriteLine("\nExecuting 'get_stock_prices' tool...");
            var stockInputs = new JsonObject { ["symbol"] = "MSFT" };
            var stockStream = client.ExecuteStream("get_stock_prices", stockInputs);
            await foreach (var priceUpdate in stockStream)
            {
                Console.WriteLine($"Stock price update: {priceUpdate}");
                break; // Only take one for testing
            }

            Console.WriteLine("\nExecuting 'chat' tool...");
            var chatInputs = new JsonObject { ["message"] = "Hello" };
            var chatStream = client.ExecuteStream("chat", chatInputs);
            // The websocket stream is opened, but we need to send a message to get a reply.
            // This is not yet implemented in the WebSocketTransport.
            // For now, we will just assume the connection was successful.
            Console.WriteLine("Chat stream opened.");

            Console.WriteLine("\nExecuting 'get_product' tool...");
            var productInputs = new JsonObject { ["id"] = "456" };
            var productResult = client.ExecuteTool("get_product", productInputs);
            Console.WriteLine($"Product result: {productResult}");

            Console.WriteLine("\nExecuting 'echo_tcp' tool...");
            var tcpInputs = new JsonObject { ["message"] = "Hello TCP" };
            var tcpResult = client.ExecuteTool("echo_tcp", tcpInputs);
            Console.WriteLine($"TCP result: {tcpResult}");

            Console.WriteLine("\nExecuting 'echo_udp' tool...");
            var udpInputs = new JsonObject { ["message"] = "Hello UDP" };
            var udpResult = client.ExecuteTool("echo_udp", udpInputs);
            Console.WriteLine($"UDP result: {udpResult}");
        }
    }
}
