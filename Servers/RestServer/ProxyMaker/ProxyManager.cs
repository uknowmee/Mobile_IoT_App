using Servers.DbManagers;
using Servers.ProxyMaker.ViewModels;

namespace Servers.ProxyMaker;

public class ProxyManager
{
    private readonly SrvDbManager _srvDbManager;

    public List<Device> RestDevicesWithTopics(int userId)
    {
        return _srvDbManager.GetUserDevices(userId)
            .Select(device => new Device(device, _srvDbManager.GetDeviceModel(device.DeviceModelId) is not { } model
                ? "NOT KNOWN!!"
                : model.ModelName, _srvDbManager.GetAllDeviceTopics(device.DeviceId)))
            .ToList();
    }

    public List<TopicData> RestTopicDataFromTopicData(List<Database.ServerDatabase.Models.TopicData> topicData)
    {
        List<TopicData> retData = new List<TopicData>();
        foreach (Database.ServerDatabase.Models.TopicData data in topicData)
        {
            retData.Add(new TopicData(data,
                _srvDbManager.GetTopic(data.TopicId) is not { } topic ? "NOT KNOWN!!" : topic.Name));
        }

        return retData;
    }

    public Token RestTokenFromToken(Database.ServerDatabase.Models.Token token)
    {
        return new Token(token);
    }

    public ProxyManager(SrvDbManager srvDbManager)
    {
        _srvDbManager = srvDbManager;
    }
}