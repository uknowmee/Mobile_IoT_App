namespace Database.ServerDatabase.Models
{
    public class Token
    {
        public int TokenId { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; }
        
        public DateTime CreationDate { get; set; }
        public string TokenHash { get; set; }
        // TODO: implement session timeout functionality
    }
}
