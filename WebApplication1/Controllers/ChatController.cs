using ChatProject.Hubs;
using ChatProject.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatProject.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class ChatController : Controller
    {
        private IHubContext<ChatHub> _chat;

        public ChatController(IHubContext<ChatHub> chat)
        {
            _chat = chat;
        }
        [HttpPost("[action]/{connectionId}/{roomId}")]
        public async Task<IActionResult> JoinRoom(string connectionId, string roomId, [FromServices] UserManager<User> _userManager)
        {
            
            await _chat.Groups.AddToGroupAsync(connectionId, roomId);
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            string userJson = JsonSerializer.Serialize(user);
            await _chat.Clients.Groups(roomId).SendAsync("UserAdded", userJson);

            return Ok();
        }
        [HttpPost("[action]/{connectionId}/{roomId}")]
        public async Task<IActionResult> LeaveRoom(string connectionId, string roomId, [FromServices] UserManager<User> _userManager)
        {
            await _chat.Groups.RemoveFromGroupAsync(connectionId, roomId);
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            string userJson = JsonSerializer.Serialize(user);
            await _chat.Clients.Groups(roomId).SendAsync("UserRemoved", userJson);
            return Ok();
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> SendMessage(
            string message, 
            int chatId,
            string roomId
            ,[FromServices] AppDbContext ctx,
            [FromServices] UserManager<User> _userManager)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            var msg = new Message
            {
                ChatID = chatId,
                Text = message,
                UserID = userId,
                Timestamp = DateTime.Now
            };
            var date = msg.Timestamp.ToString("dd/MM/yyyy hh:mm:ss");
            ctx.Messages.Add(msg);
            await ctx.SaveChangesAsync();
            await _chat.Clients.Groups(roomId)
                .SendAsync("ReceiveMessage", msg, user, date);
            return Ok();
        }
    }
}
