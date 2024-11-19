// EducationalServices, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// EducationalServices.Hubs.ChatHub
using Microsoft.AspNet.SignalR;

public class ChatHub : Hub
{
    public void SendMessage(string user, string message)
    {
        base.Clients.All.receiveMessage(user, message);
    }
}
