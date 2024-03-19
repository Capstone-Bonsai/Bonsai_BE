using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.UserViewModels
{
    public class GardenerViewModel
    {
        public string Id { get; set; }
        public string Fullname { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsRegister { get; set; } = false;
        public string Role { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsLockout { get; set; } = false;
        [NotMapped]
        public int CurrentService { get; set; } = 0;
    }
}
