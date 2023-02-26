namespace Library_API.Models
{
    public partial class RefreshToken
    {
        public int TokenId { get; set; }
        public int MemberId { get; set; }
        public string Token { get; set; }
        public DateTime ExpiryDate { get; set; }

        public virtual Member Member { get; set; }
    }
}
