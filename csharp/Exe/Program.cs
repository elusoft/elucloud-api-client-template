using elusoft.eluCloud.Client;
using elusoft.eluCloud.Client.Model;


string user = "vendor";
string apiKey = "key";

Client client = new(
    baseUrl: "http://localhost:31750",
    user,
    apiKey);

// SignalR: can be registered before authentication
client.On<Dummy>("dummy", o => Console.WriteLine("Dummy!" + o.Timestamp + o.Message));

// You can provide a SessionId here from previous connection.
// If no SessionId provided, this will trigger the authentication flow.
await client.ConnectAsync();

// SignalR: Or after authentication
client.On<Dummy>("dummy", _ => Console.WriteLine("Dummy again!"));

Console.WriteLine($"Connected to the server (v{client.ApiVersion}).");
Console.WriteLine($"Session ID: {client.SessionId}");
Console.WriteLine($"SignalR Connection ID: {client.SignalRConnectionId}");
Console.ReadKey();


// registering to partEnd event
client.On<PartEnd>("partEnd", o =>
{
    o.Parts?.ForEach(async x =>
    {
        var part = await client.GetAsync<Part>($"/api/parts/{x.PartId}", token: default);
        Console.WriteLine("Barcode: " + part.Barcode);
    });
});