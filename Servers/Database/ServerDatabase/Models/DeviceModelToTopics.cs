namespace Database.ServerDatabase.Models;

public class DeviceModelToTopics
{
    public int DeviceModelToTopicsId { get; set; }
    
    public int TopicId { get; set; }
    public Topic Topic { get; set; }
    
    public int DeviceModelId { get; set; }
    public DeviceModel DeviceModel { get; set; }
}