using Database.ServerDatabase.Models;
using Servers.DbManagers;

namespace Servers.ProxyMaker.ViewModels;

public class Device
{
    public int DeviceId { get; set; }
    public string Model { get; set; }
    public DateTime RegistrationDate { get; set; }
    public string Mac { get; set; }
    public bool LEDState { get; set; }
    public List<Topic> Topics { get; set; }

    public Device(Database.ServerDatabase.Models.Device device, string modelName, List<Topic> topics)
    {
        Model = modelName;
        RegistrationDate = device.RegistrationDate;
        Mac = device.Mac;
        LEDState = device.LEDState;
        DeviceId = device.DeviceId;
        Topics = topics;
    }
}