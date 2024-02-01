using Microsoft.AspNetCore.Http;

namespace Application.ViewModels.UserViewModels
{
    public class UserRequestModel
    {
        public string Username { get; set; }
        public string Fullname { get; set; }
        public IFormFile? Avatar { get; set; }
        public string PhoneNumber { get; set; }
    }
}
