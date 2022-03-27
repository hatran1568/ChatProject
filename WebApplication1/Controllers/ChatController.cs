using ChatProject.Hubs;
using ChatProject.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
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
        [HttpGet("[action]")]
        public async Task<IActionResult> JoinRoom(int id, [FromServices] AppDbContext _ctx, [FromServices] UserManager<User> _userManager)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            Chat chats = _ctx.Chats
                .Include(x => x.Users)
                .FirstOrDefault(x => x.Id == id);
            if (chats.Users.FirstOrDefault(x => x.UserId == User.FindFirst(ClaimTypes.NameIdentifier).Value) != null)
            {
                return RedirectToAction("Chat","Home", new { id = id });
            }
            var chatUser = new ChatUser
            {
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                ChatId = id
            };
            _ctx.ChatUsers.Add(chatUser);
            var msg = new Message
            {
                ChatID = id,
                Text = user.DisplayName + " has joined the group.",
                Timestamp = DateTime.Now,
                MessageType = MessageType.Notification
            };
            _ctx.Messages.Add(msg);
            await _ctx.SaveChangesAsync();
            await _chat.Clients.Groups(id.ToString()).SendAsync("NewUserJoined", msg.Text);
            return RedirectToAction("Chat","Home", new { id = id });
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
        public async Task<IActionResult> LeaveRoom(int id, [FromServices] AppDbContext _ctx, [FromServices] UserManager<User> _userManager)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            Chat chats = _ctx.Chats
                .Include(x => x.Users)
                .FirstOrDefault(x => x.Id == id);
                var msg = new Message
                {
                    ChatID = id,
                    Text = user.DisplayName + " has left the group.",
                    Timestamp = DateTime.Now,
                    MessageType = MessageType.Notification
                };
            _ctx.Messages.Add(msg);
            var chatUser = _ctx.ChatUsers.Where(x => (x.UserId == User.FindFirst(ClaimTypes.NameIdentifier).Value && x.ChatId == id)).FirstOrDefault();
                _ctx.ChatUsers.Remove(chatUser);
                await _ctx.SaveChangesAsync();
                await _chat.Clients.Groups(id.ToString()).SendAsync("UserLeft", msg.Text);
            return RedirectToAction("Index", "Home");
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> SendMessage(
            string message, 
            int chatId,
            string roomId, IFormFile image, [FromServices] IHostingEnvironment hostingEnvironment
            , [FromServices] AppDbContext ctx,
            [FromServices] UserManager<User> _userManager)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (image != null)
            {
                string uniqueFileName = null;
                string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                using (var fs = new FileStream(Path.Combine(uploadsFolder, uniqueFileName), FileMode.Create))
                {
                    await image.CopyToAsync(fs);
                }
                var imagemsg = new Message
                {
                    ChatID = chatId,
                    Text = uniqueFileName,
                    UserID = userId,
                    Timestamp = DateTime.Now,
                    MessageType = MessageType.Image
                };
                var imagemsgDate = imagemsg.Timestamp.ToString("dd/MM/yyyy hh:mm:ss");
                ctx.Messages.Add(imagemsg);
                await ctx.SaveChangesAsync();
                await _chat.Clients.Groups(roomId)
                    .SendAsync("ReceiveMessage", imagemsg, user, imagemsgDate);
            }
            if (message == null)
            {
                return Ok();
            }
            if (message.Trim().Length > 0)
            {
                
                var msg = new Message
                {
                    ChatID = chatId,
                    Text = message,
                    UserID = userId,
                    Timestamp = DateTime.Now,
                    MessageType = MessageType.Text
                };
                var date = msg.Timestamp.ToString("dd/MM/yyyy hh:mm:ss");
                ctx.Messages.Add(msg);
                await ctx.SaveChangesAsync();
                await _chat.Clients.Groups(roomId)
                    .SendAsync("ReceiveMessage", msg, user, date);
            }
            
            
            
                return Ok();
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Rename(
            string newName,
            int chatId,
            string roomId
            , [FromServices] AppDbContext ctx,
            [FromServices] UserManager<User> _userManager)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            var chat = ctx.Chats.FirstOrDefault(x => x.Id == chatId);
            chat.Name = newName;
            var msg = new Message
            {
                ChatID = chatId,
                Text = user.DisplayName + " named the group " + newName + ".",
                Timestamp = DateTime.Now, 
                MessageType = MessageType.Notification
            };
            var date = msg.Timestamp.ToString("dd/MM/yyyy hh:mm:ss");
            ctx.Messages.Add(msg);
            await ctx.SaveChangesAsync();
            await _chat.Clients.Groups(roomId)
                .SendAsync("Rename", msg.Text, newName, chatId);
            return Ok();
        }
    }
}
