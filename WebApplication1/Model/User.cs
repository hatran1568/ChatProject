using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatProject.Model
{
    public class User : IdentityUser
    {
        public ICollection<ChatUser> Chats { get; set; }

    }
}
