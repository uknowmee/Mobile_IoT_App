using Database.ServerDatabase.Models;
using MQTTnet;
using MQTTnet.Client;
using Servers.DbManagers;

namespace Servers.Listeners;

public class Listener
{
    private readonly ListenersManager _listenersManager;

    private readonly Device _device;
    private readonly List<string> _listeningToTopics;
    private readonly IMqttClient _client;
    private const string MqttServerIp = "192.168.137.85";
    private const int MqttServerPort = 1883;

    private readonly SrvDbManager _srvDbManager;

    public bool IsListeningToDevice(Device device)
    {
        return device.DeviceId == _device.DeviceId;
    }

    public async Task StartListening()
    {
        await Connect_Client(_client);
        HandleDisconnect(_client, _device.DeviceId);

        foreach (string topic in _listeningToTopics)
        {
            Send_Responses(_client, topic);
        }
    }

    private void HandleDisconnect(IMqttClient mqttClient, int deviceId)
    {
        async void Action()
        {
            while (_srvDbManager.DoesDeviceExist(deviceId))
            {
                Thread.Sleep(500);
            }

            await Clean_Disconnect(mqttClient);
        }

        var task = new Task(Action);
        task.Start();
    }

    private async Task Connect_Client(IMqttClient mqttClient)
    {
        MqttClientOptions? mqttClientOptions = new MqttClientOptionsBuilder()
            .WithClientId("Csharp" + Guid.NewGuid().ToString().Substring(0, 15))
            .WithTcpServer(MqttServerIp, MqttServerPort)
            .WithCleanSession()
            .WithRequestProblemInformation(false)
            .WithTryPrivate(false)
            .Build();
        await mqttClient.ConnectAsync(mqttClientOptions);
    }

    private async Task Clean_Disconnect(IMqttClient mqttClient)
    {
        await mqttClient.DisconnectAsync(
            new MqttClientDisconnectOptionsBuilder()
                .WithReason(MqttClientDisconnectReason.NormalDisconnection)
                .Build()
        );

        _listenersManager.RemoveListener(this);
    }

    private void Send_Responses(IMqttClient mqttClient, string topic)
    {
        string exactTopicName = topic.Split("/")[^1];
        if (_srvDbManager.GetTopic(exactTopicName) is not { } exactTopic)
        {
            return;
        }

        mqttClient.ApplicationMessageReceivedAsync += delegate(MqttApplicationMessageReceivedEventArgs args)
        {
            string value = System.Text.Encoding.Default.GetString(args.ApplicationMessage.Payload);

            if (topic != args.ApplicationMessage.Topic) return Task.CompletedTask;

            if (_srvDbManager.NewTopicData(_device.DeviceId, exactTopic.TopicId, value) is not { } topicData)
            {
                // TODO: maybe do something not truly needed :)
            }

            return Task.CompletedTask;
        };

        var mqttSubscribeOptions = new MqttFactory().CreateSubscribeOptionsBuilder()
            .WithTopicFilter(
                f =>
                {
                    f.WithTopic(topic);
                }
            )
            .Build();

        mqttClient.SubscribeAsync(mqttSubscribeOptions);
    }

    public Listener(Device device, SrvDbManager srvDbManager, ListenersManager listenersManager)
    {
        _listenersManager = listenersManager;
        _srvDbManager = srvDbManager;
        _device = device;
        _listeningToTopics = new List<string>();

        try
        {
            _client = new MqttFactory().CreateMqttClient();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception("Couldn't create mqtt client");
        }

        List<string> topicNames = _srvDbManager.GetAllDeviceTopicNames(device.DeviceId);
        foreach (string topicName in topicNames)
        {
            _listeningToTopics.Add(_device.Mac + "/" + topicName);
        }
    }
}