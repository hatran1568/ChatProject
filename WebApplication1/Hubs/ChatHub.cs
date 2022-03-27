using ChatProject.Model;
using ChatProject.ViewModels;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatProject.Hubs
{
    public class ChatHub : Hub
    {
        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }
        public override async Task OnConnectedAsync()
        {
            if (!ConnectedUser.Ids.Contains(Context.User.Identity.Name))
                ConnectedUser.Ids.Add(Context.User.Identity.Name);
            string json = JsonSerializer.Serialize(ConnectedUser.Ids);

            await Clients.All.SendAsync("UserJoined", json);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (ConnectedUser.Ids.Contains(Context.User.Identity.Name))
                ConnectedUser.Ids.Remove(Context.User.Identity.Name);
            string json = JsonSerializer.Serialize(ConnectedUser.Ids);

            await Clients.All.SendAsync("UserJoined", json);
        }

    }
}
