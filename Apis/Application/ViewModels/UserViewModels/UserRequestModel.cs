using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace Application.ViewModels.UserViewModels
{
    public class UserRequestModel
    {
        public string Username { get; set; }
        public string Fullname { get; set; }
        [NotMapped]
        public IFormFile? Avatar { get; set; }
        public string PhoneNumber { get; set; }
    }
}
