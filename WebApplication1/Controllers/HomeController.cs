﻿using Microsoft.AspNetCore.Mvc;
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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;

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
                           .Include(x => x.Users)
                           .ThenInclude(x => x.User)
                           .Where(x => x.Type == ChatType.Room)
                           .ToList();
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            ViewData["currentUser"] = _ctx.Users.FirstOrDefault(x => x.Id == userId).UserName;
            return View(chats);
        }
        public IActionResult GetRoomViewComponent(int chatId)
        {
            return ViewComponent("Room", new {chatType = ChatType.Room, chatId = chatId });
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
            chat.Users.Add(new ChatUser
            {
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            });
            _ctx.Chats.Add(chat);
            await _ctx.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        
        [HttpGet("{id}")]
        public IActionResult Chat(int id)
        {
            Chat chat = _ctx.Chats
                .Include(x => x.Users)
                .Include(x => x.Messages)
                .ThenInclude(x => x.User)
                .FirstOrDefault(x => x.Id == id);
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            ViewData["currentUser"] = _ctx.Users.FirstOrDefault(x => x.Id == userId).UserName;
            if (chat != null)
            {
                if (chat.Users.Any(x => x.UserId == User.FindFirst(ClaimTypes.NameIdentifier).Value))
                {
                    ViewBag.ChatType = chat.Type;
                    ViewBag.ChatId = chat.Id;
                    return View(chat);
                }
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> CreateMessage(int chatId, string message, IFormFile image, [FromServices] IHostingEnvironment hostingEnvironment)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (image == null)
            {
                var msg = new Message
                {
                    ChatID = chatId,
                    Text = message,
                    UserID = userId,
                    Timestamp = DateTime.Now,
                    MessageType = MessageType.Text
                };
                _ctx.Messages.Add(msg);
                await _ctx.SaveChangesAsync();
                return RedirectToAction("Chat", new { id = chatId });
            } else
            {
                string uniqueFileName = null;
                string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                using (var fs = new FileStream(Path.Combine(uploadsFolder, uniqueFileName), FileMode.Create))
                {
                    await image.CopyToAsync(fs);
                }
                var msg = new Message
                {
                    ChatID = chatId,
                    Text = message,
                    UserID = userId,
                    Timestamp = DateTime.Now,
                    MessageType = MessageType.Image
                };
                return RedirectToAction("Chat", new { id = chatId });
            }

        }
        public IActionResult Find()
        {
            var users = _ctx.Users
                        .Where(x => x.Id != User.FindFirst(ClaimTypes.NameIdentifier).Value)
                        .ToList();
            return View(users);
        }
        public IActionResult FindUserByName(string search)
        {
            var users = _ctx.Users
                        .Where(x => x.Id != User.FindFirst(ClaimTypes.NameIdentifier).Value)
                        .Where(x => x.DisplayName.Contains(search) || x.UserName.Contains(search))
                        .ToList();
            return Json(users);

        }
        public IActionResult FindRoomByName(string search)
        {
            var rooms = _ctx.Chats
                        .Where(x => x.Type == ChatType.Room && x.Name.Contains(search))
                        .ToList();
            return Json(rooms);

        }
        public async Task<IActionResult> CreatePrivateRoom(string userId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            List<ChatUser> duplicateChat = _ctx.ChatUsers.Include(x => x.Chat)
                                           .Where(x => (x.User.Id == userId || x.UserId == currentUserId) && x.Chat.Type == ChatType.Private)
                            .ToList();
            List<int> _chats = duplicateChat.GroupBy(x => x.ChatId)
                 .Where(x => x.Count() == 2)
                 .Select(x => x.Key)
                 .ToList();
            if (_chats.Count() > 0)
            {
                int existingChat = _chats.First();
                return RedirectToAction("Chat", new { id = existingChat });
            }
            var chat = new Chat
            {
                Type = ChatType.Private,
                Name = "private" + currentUserId + "_" + userId
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

        public IActionResult Private()
        {
            var chats = _ctx.Users
                       .Where(x => x.Id != User.FindFirst(ClaimTypes.NameIdentifier).Value)
                       .ToList();
            /*var chats = _ctx.Chats
                            .Include(x => x.Users)
                            .ThenInclude(x => x.User)
                            .Where(x => x.Type == ChatType.Private
                                            && x.Users
                                            .Any(y => y.UserId == User.FindFirst(ClaimTypes.NameIdentifier).Value))
                            .ToList();*/
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            ViewData["currentUser"] = _ctx.Users.FirstOrDefault(x => x.Id == userId).UserName;
            return View(chats);
        }

    }
}
