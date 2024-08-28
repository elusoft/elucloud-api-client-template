using elusoft.eluCloud.Client;
using elusoft.eluCloud.Client.Model;


string user = "vendor";
string apiKey = "key";

Client client = new(
    baseUrl: "http://localhost:31750",
    user,
    apiKey);

// SignalR: can be registered before authentication
client.On<Test>(o => Console.WriteLine("Test! " + o.Timestamp + o.Message));

// You can provide a SessionId here from previous connection.
// If no SessionId provided, this will trigger the authentication flow.
await client.ConnectAsync();

// SignalR: Or after authentication
client.On<Test>(o => Console.WriteLine("Test again!"));

Console.WriteLine($"Connected to the server (v{client.ApiVersion}).");
Console.WriteLine($"Session ID: {client.SessionId}");
Console.WriteLine($"SignalR Connection ID: {client.SignalRConnectionId}");
Console.Read();


// registering to partEnd event
client.On<PartEnd>(o =>
{
    o.Parts?.ForEach(async x =>
    {
        var part = await client.GetAsync<Part>($"/api/parts/{x.PartId}", token: default);
        Console.WriteLine("Barcode: " + part.Barcode);
    });
});