namespace Database.ServerDatabase.Models
{
    public class User
    {
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
    }
}
