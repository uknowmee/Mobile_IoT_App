﻿namespace DummyApi
{
    public class User
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
    }
}
