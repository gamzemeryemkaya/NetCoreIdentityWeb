using Microsoft.AspNetCore.Identity;
using NetCoreIdentityApp.Core.Models;
using System.Reflection;

namespace NetCoreIdentityApp.Repository.Models
{
    public class AppUser : IdentityUser
    {
        public string? City { get; set; }
        public string? Picture { get; set; }
        public DateTime? BirthDate { get; set; }
        public Gender? Gender { get; set; }
    }
}
