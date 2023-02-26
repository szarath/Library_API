using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
namespace Library_API.Models
{
    public partial class Member
    {
        public Member()
        {
            RefreshTokens = new HashSet<RefreshToken>();
        }

        public int MemberId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public virtual ICollection<RefreshToken>? RefreshTokens { get; set; }

    }
}
