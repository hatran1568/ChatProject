using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ChatProject.Models;
using ChatProject.Model;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ChatProject.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private AppDbContext _ctx;

        public IActionResult Index()
        {
            var chats = _ctx.Chats
            .Include(c => c.Users)
            .ToList();
            return View(chats);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public HomeController(AppDbContext ctx) => _ctx = ctx;
        [HttpPost]
        public async Task<IActionResult> CreateRoom(string name)
        {
            var chat = new Chat
            {
                Name = name,
                Type = ChatType.Room
            };
            _ctx.Chats.Add(chat);
            await _ctx.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        [HttpGet("{id}")]
        public IActionResult Chat(int id)
        {
            var chat = _ctx.Chats
                .Include(x => x.Messages)
                .ThenInclude(x => x.User)
                .FirstOrDefault(x => x.Id == id);
            return View(chat);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(int chatId, string message)
        {
            var msg = new Message
            {
                ChatID = chatId,
                Text = message,
                UserID = "5f324953-2d71-403e-b83e-87f1c0303f5e",
                Timestamp = DateTime.Now
            };
            _ctx.Messages.Add(msg);
            await _ctx.SaveChangesAsync();
            return RedirectToAction("Chat", new { id = chatId });
        }
        public IActionResult Find()
        {
            var users = _ctx.Users
                        .Where(x => x.Id != User.FindFirst(ClaimTypes.NameIdentifier).Value)
                        .ToList();
            return View(users);
        }
        public IActionResult FindByName(string search)
        {
            var users = _ctx.Users
                        .Where(x => x.Id != User.FindFirst(ClaimTypes.NameIdentifier).Value)
                        .Where(x => x.DisplayName.Contains(search) || x.UserName.Contains(search))
                        .ToList();
            return Json(users);

        }
        public async Task<IActionResult> CreatePrivateRoom(string userId)
        {
            var chat = new Chat
            {
                Type = ChatType.Private
            };
            chat.Users.Add(new ChatUser
            {
                UserId = userId
            });
            chat.Users.Add(new ChatUser
            {
                UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value
            });
            _ctx.Chats.Add(chat);
            await _ctx.SaveChangesAsync();
            return RedirectToAction("Chat", new { id = chat.Id });
        }
    }
}
