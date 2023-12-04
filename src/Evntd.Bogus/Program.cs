using System.Net.Sockets;
using Evntd.Bogus;
using Microsoft.Extensions.Configuration;

internal class Program
{
    private static async Task Main(string[] args)
    {
        IConfigurationRoot config = new ConfigurationManager()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var store = new InMemoryStore();
        var options = config.GetSection("Scenario").Get<ScenarioOptions>();
        var scenario = new Scenario(store, options);

        try 
        {
            var httpClient = new HttpClient();
            foreach (var request in scenario.Simulate())
            {
                Console.WriteLine($"sending {request.Method} {request.RequestUri}");
                await httpClient.SendAsync(request);
            }

            Console.WriteLine("done");
        }
        catch (HttpRequestException e) when ((e.InnerException as SocketException)?.SocketErrorCode == SocketError.ConnectionRefused)
        {
            Console.WriteLine("Please start the Evntd.EventStoreDB.WebApi project...it should listen for HTTPS on port 7177.");
        }
    }
}