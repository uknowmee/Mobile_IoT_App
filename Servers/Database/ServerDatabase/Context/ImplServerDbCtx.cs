using System.Data.Common;
using Database.ServerDatabase.Models;

namespace Database.ServerDatabase.Context;

public partial class ServerDbCtx
{
    public bool InitialInserts()
    {
        return MakeInserts();
    }

    public Token? GetToken(string tokenHash)
    {
        return Tokens.SingleOrDefault(tok => tok.TokenHash == tokenHash);
    }

    public Topic? GetTopic(string topicName)
    {
        return Topics.SingleOrDefault(top => top.Name == topicName);
    }

    public Topic? GetTopic(int topicId)
    {
        return Topics.SingleOrDefault(top => top.TopicId == topicId);
    }

    public User? GetUser(int userId)
    {
        return Users.SingleOrDefault(us => us.UserId == userId);
    }

    public DeviceModel? GetDeviceModel(string deviceModel)
    {
        return DeviceModels.SingleOrDefault(mod => mod.ModelName == deviceModel);
    }

    public DeviceModel? GetDeviceModel(int deviceModelId)
    {
        return DeviceModels.SingleOrDefault(mod => mod.DeviceModelId == deviceModelId);
    }

    public User? GetUser(string email)
    {
        return Users.SingleOrDefault(us => us.Email == email);
    }

    public Device? GetDevice(string deviceMac)
    {
        return Devices.SingleOrDefault(dev => dev.Mac == deviceMac);
    }

    public Device? GetDevice(string deviceMac, int userId)
    {
        return Devices.SingleOrDefault(dev => dev.Mac == deviceMac && dev.UserId == userId);
    }

    public List<Device> GetUserDevices(int userId)
    {
        return Devices.Where(dev => dev.UserId == userId).ToList();
    }

    public List<Device> GetDevices()
    {
        return Devices.ToList();
    }

    public List<string> GetAllDeviceTopicNames(int deviceId)
    {
        if (Devices.SingleOrDefault(dev => dev.DeviceId == deviceId) is not { } device)
        {
            return new List<string>();
        }

        if (GetDeviceModel(device.DeviceModelId) is not { } deviceModel)
        {
            return new List<string>();
        }

        return DeviceModelToTopics
            .Where(modToTop => modToTop.DeviceModelId == deviceModel.DeviceModelId)
            .Select(modToTop => modToTop.Topic.Name)
            .ToList();
    }

    public List<Topic> GetAllDeviceTopics(int deviceId)
    {
        if (Devices.SingleOrDefault(dev => dev.DeviceId == deviceId) is not { } device)
        {
            return new List<Topic>();
        }

        if (GetDeviceModel(device.DeviceModelId) is not { } deviceModel)
        {
            return new List<Topic>();
        }

        return DeviceModelToTopics
            .Where(modToTop => modToTop.DeviceModelId == deviceModel.DeviceModelId)
            .Select(modToTop => modToTop.Topic)
            .ToList();
    }

    public List<TopicData> GetTopicData(int deviceId, int topicId, DateTime newerThan)
    {
        return TopicDatas
            .Where(topDat =>
                topDat.DeviceId == deviceId &&
                topDat.TopicId == topicId &&
                topDat.CreatedAt >= newerThan)
            .OrderByDescending(topDat => topDat.CreatedAt)
            .Take(200)
            .OrderBy(topDat => topDat.CreatedAt)
            .ToList();
    }

    public bool DoesDeviceExist(int deviceId)
    {
        return Devices.Any(dev => dev.DeviceId == deviceId);
    }

    public Device? AddDevice(int userId, string modelName, string deviceMac)
    {
        if (Users.SingleOrDefault(us => us.UserId == userId) is not { } user)
        {
            return null;
        }

        if (DeviceModels.SingleOrDefault(md => md.ModelName == modelName) is not { } model)
        {
            return null;
        }

        Device device = new Device()
        {
            DeviceModelId = model.DeviceModelId,
            LEDState = false,
            RegistrationDate = DateTime.UtcNow,
            Mac = deviceMac,
            UserId = user.UserId
        };

        try
        {
            Devices.Add(device);
            SaveChanges();
            return device;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    public bool ChangeDeviceLedState(bool state, Device deviceToChange)
    {
        if (Devices.SingleOrDefault(dev => dev.DeviceId == deviceToChange.DeviceId) is not { } device)
        {
            return false;
        }

        try
        {
            device.LEDState = state;
            SaveChanges();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public bool RemoveToken(Token tokenToRemove)
    {
        if (Tokens.SingleOrDefault(tok => tok.TokenId == tokenToRemove.TokenId) is not { } token)
        {
            return false;
        }

        try
        {
            Tokens.Remove(token);
            SaveChanges();
            return true;
        }
        catch (DbException e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public bool IsTokenHashValid(string tokenHash)
    {
        if (Tokens.SingleOrDefault(tok => tok.TokenHash == tokenHash) is { } token)
        {
            return false;
        }

        return true;
    }

    public Token? NewSessionToken(int userId, string tokenHash)
    {
        if (Users.SingleOrDefault(us => us.UserId == userId) is not { } user)
        {
            return null;
        }

        Token newToken = new Token()
        {
            UserId = user.UserId,
            CreationDate = DateTime.UtcNow,
            TokenHash = tokenHash,
        };

        try
        {
            Tokens.Add(newToken);
            SaveChanges();
            return newToken;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    public User? CreateUser(string email, string passwordHash)
    {
        if (Users.SingleOrDefault(us => us.Email == email) is { } user)
        {
            return null;
        }

        User newUser = new User()
        {
            CreatedAt = DateTime.UtcNow,
            Email = email,
            PasswordHash = passwordHash
        };

        try
        {
            Users.Add(newUser);
            SaveChanges();
            return newUser;
        }
        catch (DbException e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    public TopicData? NewTopicData(int deviceId, int topicId, string value)
    {
        if (Devices.SingleOrDefault(dev => dev.DeviceId == deviceId) is not { } device)
        {
            return null;
        }

        if (Topics.SingleOrDefault(top => top.TopicId == topicId) is not { } topic)
        {
            return null;
        }

        TopicData newTopicData = new TopicData()
        {
            TopicId = topicId,
            DeviceId = deviceId,
            Data = value,
            CreatedAt = DateTime.UtcNow,
        };

        try
        {
            TopicDatas.Add(newTopicData);
            SaveChanges();
            return newTopicData;
        }
        catch (DbException e)
        {
            Console.WriteLine(e);
            return null;
        }
    }
}