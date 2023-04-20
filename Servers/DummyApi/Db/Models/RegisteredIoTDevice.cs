namespace DummyApi
{
    public class RegisteredIoTDevice
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string UniqueName { get; set; }
        public string Mac { get; set; }
        public bool LEDState { get; set; }
    }
}