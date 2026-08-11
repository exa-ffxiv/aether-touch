using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buttplug.Client;
using Dalamud.Plugin.Services;

namespace AetherTouch
{
    public class ToyManager
    {
        private readonly IPluginLog log;
        private readonly ButtplugClient client = new("Aether Touch");
        private readonly NotificationManager notificationManager;

        public ToyManager(IPluginLog log, NotificationManager notificationManager)
        {
            this.log = log;
            this.notificationManager = notificationManager;
            client.DeviceAdded += (_, args) => 
                notificationManager.notifyDeviceConnected(args.Device.Name);

            client.DeviceRemoved += (_, args) =>
                notificationManager.notifyDeviceDisconnected(args.Device.Name);

            client.ServerDisconnect += (_, _) =>
                notificationManager.notifyServerDisconnected();

            client.ErrorReceived += (_, args) =>
                log.Info($"[!] Error: {args.Exception.Message}");
        }

        public void connect()
        {
            client.ConnectAsync("ws://127.0.0.1:12345");
        }

        public void disconnect()
        {
            client.DisconnectAsync();
        }

        public void playPattern(Pattern pattern)
        {
            Task.Run(async () =>
            {
                try
                {
                    foreach (var item in pattern.Steps)
                    {
                        vibeAll(item.Intensity);
                        await Task.Delay(item.Duration);
                    }
                    vibeAll(0);
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message);
                }
            });
        }

        public void vibeAll(int intensity)
        {
            var devices = client.Devices;
            if (devices.Length > 0)
            {
                foreach (var device in devices)
                {
                    device.RunOutputAsync(DeviceOutput.Vibrate.Percent(intensity / 100.0));
                }
            }
            else
            {
                log.Warning("No devices connected.");
            }
        }

        public void testVibe()
        {
            var devices = client.Devices;
            if (devices.Length > 0)
            {
                // Send commands to all devices concurrently
                var tasks = devices
                    .Where(d => d.HasOutput(Buttplug.Core.Messages.OutputType.Vibrate))
                    .Select(async device =>
                    {
                        await device.RunOutputAsync(DeviceOutput.Vibrate.Percent(0.5));
                        await Task.Delay(500);
                        await device.StopAsync();
                    });

                // Wait for all commands to complete
                Task.WhenAll(tasks);
            }
            else
            {
                Console.WriteLine("No devices connected.");
            }
        }
    }
}
