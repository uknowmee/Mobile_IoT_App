using Database.ServerDatabase;
using Database.ServerDatabase.Context;
using Database.ServerDatabase.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Servers.ProxyMaker.ViewModels;
using Device = Database.ServerDatabase.Models.Device;
using Token = Database.ServerDatabase.Models.Token;
using TopicData = Database.ServerDatabase.Models.TopicData;

namespace Servers.DbManagers;

public class SrvDbManager : IDbManager
{
    public bool EnsureCreated()
    {
        using (DbContext db = new ServerDbCtx())
        {
            return db.Database.EnsureCreated();
        }
    }
    
    public bool InitialInserts()
    {
        using (IServerDbCtx db = new ServerDbCtx())
        {
            return db.InitialInserts();
        }
    }
    
    public Token? GetToken(string tokenHash)
    {
        using (IServerDbCtx db = new ServerDbCtx())
        {
            return db.GetToken(tokenHash);
        }
    }

    public Topic? GetTopic(string topicName)
    {
        using (IServerDbCtx db = new ServerDbCtx())
        {
            return db.GetTopic(topicName);
        }
    }
    public Topic? GetTopic(int topicId)
    {
        using (IServerDbCtx db = new ServerDbCtx())
        {
            return db.GetTopic(topicId);
        }
    }

    public User? GetUser(int userId)
    {
        using (IServerDbCtx db = new ServerDbCtx())
        {
            return db.GetUser(userId);
        }
    }

    public User? GetUser(string email)
    {
        using (IServerDbCtx db = new ServerDbCtx())
        {
            return db.GetUser(email);
        }
    }
    public DeviceModel? GetDeviceModel(string deviceModel)
    {
        using (IServerDbCtx db = new ServerDbCtx())
        {
            return db.GetDeviceModel(deviceModel);
        }
    }

    public DeviceModel? GetDeviceModel(int deviceModelId)
    {
        using (IServerDbCtx db = new ServerDbCtx())
        {
            return db.GetDeviceModel(deviceModelId);
        }
    }
    
    public Device? GetDevice(string deviceMac)
    {
        using (IServerDbCtx db = new ServerDbCtx())
        {
            return db.GetDevice(deviceMac);
        }
    }

    public Device? GetDevice(string deviceMac, int userId)
    {
        using (IServerDbCtx db = new ServerDbCtx())
        {
            return db.GetDevice(deviceMac, userId);
        }
    }

    public List<Device> GetUserDevices(int userId)
    {
        using (IServerDbCtx db = new ServerDbCtx())
        {
            return db.GetUserDevices(userId);
        }
    }
    
    public List<Device> GetDevices()
    {
        using (IServerDbCtx db = new ServerDbCtx())
        {
            return db.GetDevices();
        }
    }

    public List<string> GetAllDeviceTopicNames(int deviceId)
    {
        using (IServerDbCtx db = new ServerDbCtx())
        {
            return db.GetAllDeviceTopicNames(deviceId);
        }
    }
    
    public List<Topic> GetAllDeviceTopics(int deviceId)
    {
        using (IServerDbCtx db = new ServerDbCtx())
        {
            return db.GetAllDeviceTopics(deviceId);
        }
    }

    public List<TopicData> GetTopicData(int deviceId, int topicId, DateTime newerThan)
    {
        using (IServerDbCtx db = new ServerDbCtx())
        {
            return db.GetTopicData(deviceId, topicId, newerThan);
        }
    }

    public bool DoesDeviceExist(int deviceId)
    {
        using (IServerDbCtx db = new ServerDbCtx())
        {
            return db.DoesDeviceExist(deviceId);
        }
    }

    public Device? AddDevice(int userId, string modelName, string deviceMac)
    {
        using (IServerDbCtx db = new ServerDbCtx())
        {
            return db.AddDevice(userId, modelName, deviceMac);
        }
    }

    public bool ChangeDeviceLedState(bool state, Device deviceToChange)
    {
        using (IServerDbCtx db = new ServerDbCtx())
        {
            return db.ChangeDeviceLedState(state, deviceToChange);
        }
    }

    public bool RemoveToken(Token tokenToRemove)
    {
        using (IServerDbCtx db = new ServerDbCtx())
        {
            return db.RemoveToken(tokenToRemove);
        }
    }

    public bool IsTokenHashValid(string tokenHash)
    {
        using (IServerDbCtx db = new ServerDbCtx())
        {
            return db.IsTokenHashValid(tokenHash);
        }
    }

    public Token? NewSessionToken(int userId, string tokenHash)
    {
        using (IServerDbCtx db = new ServerDbCtx())
        {
            return db.NewSessionToken(userId, tokenHash);
        }
    }

    public User? CreateUser(string email, string passwordHash)
    {
        using (IServerDbCtx db = new ServerDbCtx())
        {
            return db.CreateUser(email, passwordHash);
        }
    }

    public TopicData? NewTopicData(int deviceId, int topicId, string value)
    {
        using (IServerDbCtx db = new ServerDbCtx())
        {
            return db.NewTopicData(deviceId, topicId, value);
        }
    }

    public SrvDbManager()
    {
        
    }
}