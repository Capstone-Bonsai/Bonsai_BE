
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string Fullname { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsRegister { get; set; } = false;
       

        [JsonIgnore]
        public virtual Customer? Customer { get; set; }
        [JsonIgnore]
        public virtual Gardener? Gardener { get; set; }
        public virtual Manager? Manager { get; set; }
        public virtual Staff? Staff { get; set; }
        public IList<Notification> Notifications { get; set; }
    }
}
