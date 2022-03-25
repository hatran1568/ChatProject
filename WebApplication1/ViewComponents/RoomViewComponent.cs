﻿using ChatProject.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ChatProject.ViewComponents
{
    public class RoomViewComponent : ViewComponent
    {
        private AppDbContext _ctx;
        public RoomViewComponent(AppDbContext ctx)
        {
            _ctx = ctx;
        }
        public IViewComponentResult Invoke()
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var chat = _ctx.ChatUsers
                .Include(x => x.Chat)
                .Where(x => x.UserId == userId)
                .Select(x => x.Chat)
                .ToList();
            return View(chat);
        }
    }
}
