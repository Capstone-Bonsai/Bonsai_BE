
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ApplicationUser: IdentityUser
    {
        public string Fullname { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsRegister { get; set; } = false;

        public virtual Customer? Customer { get; set; }
        public virtual Gardener? Gardener { get; set; }
        public virtual Manager? Manager { get; set; }
        public virtual Staff? Staff { get; set; }


    }
}
