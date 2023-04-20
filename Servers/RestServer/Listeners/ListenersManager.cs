using Database.ServerDatabase.Models;
using Servers.DbManagers;

namespace Servers.Listeners;

public class ListenersManager
{
    private readonly SrvDbManager _srvDbManager;
    private readonly List<Listener> _listeners;

    private void AddListeners()
    {
        foreach (Device device in _srvDbManager.GetDevices())
        {
            try
            {
                AddListenerToDevice(device);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public async void AddListenerToDevice(Device device)
    {
        if (_listeners.Any(list => list.IsListeningToDevice(device)) ||
            !_srvDbManager.DoesDeviceExist(device.DeviceId))
        {
            return;
        }

        try
        {
            Listener listener = new Listener(device, _srvDbManager, this);
            await listener.StartListening();
            _listeners.Add(listener);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public void RemoveListener(Listener eventListener)
    {
        _listeners.Remove(eventListener);
    }

    private void Process()
    {
        while (true)
        {
            try
            {
                AddListeners();
                Thread.Sleep(10_000);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public ListenersManager(SrvDbManager dbManager)
    {
        _srvDbManager = dbManager;
        _listeners = new List<Listener>();
        new Task(Process).Start();
    }
}