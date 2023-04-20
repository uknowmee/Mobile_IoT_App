using Servers.DbManagers;

namespace Servers.ProxyMaker.ViewModels;

public class TopicData
{
    public int TopicDataId { get; set; }
        
    public string TopicName { get; set; }
        
    public string Data { get; set; }
    public DateTime CreatedAt { get; set; }

    public TopicData(Database.ServerDatabase.Models.TopicData topicData, string topicName)
    {
        TopicDataId = topicData.TopicDataId;
        TopicName = topicName;
        Data = topicData.Data;
        CreatedAt = topicData.CreatedAt;
    }
}