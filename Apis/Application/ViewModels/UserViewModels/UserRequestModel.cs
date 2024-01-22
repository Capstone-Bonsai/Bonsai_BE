using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.UserViewModels
{
    public class UserRequestModel
    {
        public string Email { get; set; }
        public string Fullname { get; set; }
        public string? AvatarUrl { get; set; }
        public string PhoneNumber { get; set; }

    }
}
