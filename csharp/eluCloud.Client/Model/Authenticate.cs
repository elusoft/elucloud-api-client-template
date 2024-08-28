using ServiceStack;

namespace elusoft.eluCloud.Client.Model;

public class Authenticate : IReturn<AuthenticateResponse>
{
    public string? Provider { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
}
