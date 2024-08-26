using elusoft.eluCloud;
using elusoft.eluCloud.Model;
using System;
using System.Threading.Tasks;

namespace Exe
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Create a new client for the eluCloud server connection.
            // See the implementation for details.
            // Feel free to make any changes.
            var client = new Client(port: 31750);

            // SignalR: can be registered before authentication
            client.On<Dummy>("dummy", (o) => {
                Console.WriteLine("Dummy!" + o.Timestamp + o.Message);
            });

            // authentication with provided vendor and key pair
            await client.Authenticate("vendor", "key");

            // SignalR: Or after authentication
            client.On<Dummy>("dummy", (o) =>
            {
                Console.WriteLine("Dummy again!");
            });

            // registering to partEnd event
            client.On<PartEnd>("partEnd", o =>
            { 
                o.Parts.ForEach(x =>
                {
                    var part = client.Get<Part>($"/api/parts/{x.PartId}");
                    Console.WriteLine("Barcode: " + part.Barcode);
                });
            });

            Console.WriteLine($"Connected to the server (v{client.ApiVersion}).");
            Console.WriteLine($"Session ID: {client.SessionId}");
            Console.WriteLine($"SignalR Connection ID: {client.SignalRConnectionId}");
            Console.ReadKey();
        }
    }
}
