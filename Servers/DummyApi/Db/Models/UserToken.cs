namespace DummyApi
{
    public class UserToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime CreationDate { get; set; }
        
        // TODO: implement session timeout functionality

        public string TokenHash { get; set; }
    }
}
