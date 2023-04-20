using Database.ServerDatabase.Models;

namespace Database.ServerDatabase.Context;

public partial class ServerDbCtx
{
    private bool MakeInserts()
    {
        DeviceModel rangeSensor = new DeviceModel(){ ModelName = "Range Sensor"};
        DeviceModel diceModel = new DeviceModel(){ModelName = "Dice"};
        DeviceModel netGyro = new DeviceModel(){ModelName = "NetGyro"};
        DeviceModel rangeDice = new DeviceModel(){ModelName = "Range Dice"};

        Topic distance = new Topic(){Name = "distance"};
        Topic diceTopic = new Topic(){Name = "dice"};
        List<string> topicNames = new List<string>(){ "gyro x", "gyro y", "gyro z", "acc x", "acc y", "acc z", "mag x", "mag y", "mag z" };
        List<Topic> netGyroTopics = topicNames.Select(name => new Topic() { Name = name }).ToList();

        DeviceModels.Add(rangeSensor);
        DeviceModels.Add(diceModel);
        DeviceModels.Add(netGyro);
        DeviceModels.Add(rangeDice);
        
        Topics.Add(distance);
        Topics.Add(diceTopic);
        netGyroTopics.ForEach(top => Topics.Add(top));
        SaveChanges();

        DeviceModelToTopics.Add(new DeviceModelToTopics(){
            DeviceModelId = rangeSensor.DeviceModelId,
            TopicId = distance.TopicId
        });
        DeviceModelToTopics.Add(new DeviceModelToTopics()
        {
            DeviceModelId = diceModel.DeviceModelId,
            TopicId = diceTopic.TopicId
        });
        foreach (Topic netGyroTopic in netGyroTopics)
        {
            DeviceModelToTopics.Add(new DeviceModelToTopics()
            {
                DeviceModelId = netGyro.DeviceModelId,
                TopicId = netGyroTopic.TopicId
            });
        }
        DeviceModelToTopics.Add(new DeviceModelToTopics()
        {
            DeviceModelId = rangeDice.DeviceModelId,
            TopicId = distance.TopicId
        });
        DeviceModelToTopics.Add(new DeviceModelToTopics()
        {
            DeviceModelId = rangeDice.DeviceModelId,
            TopicId = diceTopic.TopicId
        });
        SaveChanges();
        
        return true;
    }
}