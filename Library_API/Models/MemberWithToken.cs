namespace Library_API.Models
{
    public class MemberWithToken : Member
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public MemberWithToken(Member user)
        {
            this.MemberId = user.MemberId;
            this.Email = user.Email;
            this.FirstName = user.FirstName;
            this.LastName = user.LastName;
            this.DateOfBirth= user.DateOfBirth;           
            this.PhoneNumber= user.PhoneNumber;
        }
    }
}
