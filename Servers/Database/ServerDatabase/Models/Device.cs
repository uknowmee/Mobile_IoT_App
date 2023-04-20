namespace Database.ServerDatabase.Models
{
    public class Device
    {
        public int DeviceId { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; }

        public int DeviceModelId { get; set; }
        public DeviceModel DeviceModel { get; set; }
        
        public DateTime RegistrationDate { get; set; }
        public string Mac { get; set; }
        public bool LEDState { get; set; }
    }
}