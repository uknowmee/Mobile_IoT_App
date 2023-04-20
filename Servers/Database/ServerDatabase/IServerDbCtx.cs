using Database.ServerDatabase.Models;

namespace Database.ServerDatabase;

public interface IServerDbCtx : IDisposable
{
    public bool InitialInserts();
    public Token? GetToken(string tokenHash);
    public Topic? GetTopic(string topicName);
    public Topic? GetTopic(int topicId);
    public User? GetUser(int userId);
    public DeviceModel? GetDeviceModel(string deviceModel);
    public DeviceModel? GetDeviceModel(int deviceModelId);
    public User? GetUser(string email);
    public Device? GetDevice(string deviceMac);
    public Device? GetDevice(string deviceMac, int userId);
    public List<Device> GetUserDevices(int userId);
    public List<Device> GetDevices();
    public List<string> GetAllDeviceTopicNames(int deviceId);
    public List<Topic> GetAllDeviceTopics(int deviceId);
    public List<TopicData> GetTopicData(int deviceId, int topicId, DateTime newerThan);
    public bool DoesDeviceExist(int deviceId);
    public Device? AddDevice(int userId, string modelName, string deviceMac);
    public bool ChangeDeviceLedState(bool state, Device deviceToChange);
    public bool RemoveToken(Token tokenToRemove);
    public bool IsTokenHashValid(string tokenHash);
    public Token? NewSessionToken(int userId, string tokenHash);
    public User? CreateUser(string email, string passwordHash);
    public TopicData? NewTopicData(int deviceId, int topicId, string value);
}