using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using utcp;

namespace TestRunner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices(services =>
                    {
                        services.AddGrpc();
                    });
                    webBuilder.Configure(app =>
                    {
                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapGrpcService<ProductServiceImpl>();
                        });
                    });
                })
                .Build();

            await host.StartAsync();

            StartEchoServer();

            var client = new UtcpClient();
            client.LoadManual("manual.json");

            Console.WriteLine("Executing 'get_weather' tool...");
            try
            {
                var weatherInputs = new JsonObject { ["location"] = "San Francisco" };
                var weatherResult = await client.ExecuteToolAsync("get_weather", weatherInputs);
                Console.WriteLine($"Weather result: {weatherResult}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing 'get_weather': {ex.Message}");
            }


            Console.WriteLine("\nExecuting 'resize_image' tool...");
            try
            {
                var resizeInputs = new JsonObject { ["source"] = "my_image.jpg", ["width"] = 800, ["height"] = 600 };
                var resizeResult = await client.ExecuteToolAsync("resize_image", resizeInputs);
                Console.WriteLine($"Resize result: {resizeResult}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing 'resize_image': {ex.Message}");
            }

            Console.WriteLine("\nExecuting 'get_user' tool...");
            try
            {
                var userInputs = new JsonObject { ["id"] = "123" };
                var userResult = await client.ExecuteToolAsync("get_user", userInputs);
                Console.WriteLine($"User result: {userResult}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing 'get_user': {ex.Message}");
            }


            Console.WriteLine("\nExecuting 'get_stock_prices' tool...");
            try
            {
                var stockInputs = new JsonObject { ["symbol"] = "MSFT" };
                var stockStream = client.ExecuteStream("get_stock_prices", stockInputs);
                await foreach (var priceUpdate in stockStream)
                {
                    Console.WriteLine($"Stock price update: {priceUpdate}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing 'get_stock_prices': {ex.Message}");
            }


            Console.WriteLine("\nExecuting 'chat' tool...");
            try
            {
                var chatInputs = new JsonObject { ["message"] = "Hello" };
                var chatStream = client.ExecuteStream("chat", chatInputs);
                // The websocket stream is opened, but we need to send a message to get a reply.
                // This is not yet implemented in the WebSocketTransport.
                // For now, we will just assume the connection was successful.
                Console.WriteLine("Chat stream opened.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing 'chat': {ex.Message}");
            }


            Console.WriteLine("\nExecuting 'get_product' tool...");
            try
            {
                var productInputs = new JsonObject { ["id"] = "456", ["assembly"] = "TestRunner" };
                var productResult = await client.ExecuteToolAsync("get_product", productInputs);
                Console.WriteLine($"Product result: {productResult}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing 'get_product': {ex.Message}");
            }


            Console.WriteLine("\nExecuting 'echo_tcp' tool...");
            try
            {
                var tcpInputs = new JsonObject { ["message"] = "Hello TCP" };
                var tcpResult = await client.ExecuteToolAsync("echo_tcp", tcpInputs);
                Console.WriteLine($"TCP result: {tcpResult}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing 'echo_tcp': {ex.Message}");
            }


            Console.WriteLine("\nExecuting 'echo_udp' tool...");
            try
            {
                var udpInputs = new JsonObject { ["message"] = "Hello UDP" };
                var udpResult = await client.ExecuteToolAsync("echo_udp", udpInputs);
                Console.WriteLine($"UDP result: {udpResult}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing 'echo_udp': {ex.Message}");
            }

            await host.StopAsync();
        }

        static void StartEchoServer()
        {
            var tcpListener = new TcpListener(IPAddress.Any, 12345);
            tcpListener.Start();
            Task.Run(async () =>
            {
                while (true)
                {
                    var client = await tcpListener.AcceptTcpClientAsync();
                    var stream = client.GetStream();
                    var buffer = new byte[1024];
                    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    await stream.WriteAsync(buffer, 0, bytesRead);
                    client.Close();
                }
            });

            var udpClient = new UdpClient(12345);
            Task.Run(async () =>
            {
                while (true)
                {
                    var result = await udpClient.ReceiveAsync();
                    await udpClient.SendAsync(result.Buffer, result.Buffer.Length, result.RemoteEndPoint);
                }
            });
        }
    }
}
