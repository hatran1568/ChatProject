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

namespace ChatProject.Controllers
{
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
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var msg = new Message
            {
                ChatID = chatId,
                Text = message,
                UserID = userId,
                Timestamp = DateTime.Now
            };
            _ctx.Messages.Add(msg);
            await _ctx.SaveChangesAsync();
            return RedirectToAction("Chat", new { id = chatId });
        }
    }
}
