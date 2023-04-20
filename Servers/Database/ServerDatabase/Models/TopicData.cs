namespace Database.ServerDatabase.Models
{
    public class TopicData
    {
        public int TopicDataId { get; set; }
        
        public int TopicId { get; set; }
        public Topic Topic { get; set; }
        
        public int DeviceId { get; set; }
        public Device Device { get; set; }

        public string Data { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
