using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buttplug.Client;

namespace AetherTouch
{
    public class ToyClient
    {
        private ButtplugClient client = new ButtplugClient("Aether Touch");

        public ToyClient()
        {
            client.DeviceAdded += (_, args) =>
                Console.WriteLine($"[+] Device connected: {args.Device.Name}");

            client.DeviceRemoved += (_, args) =>
                Console.WriteLine($"[-] Device disconnected: {args.Device.Name}");

            client.ServerDisconnect += (_, _) =>
                Console.WriteLine("[!] Server connection lost!");

            client.ErrorReceived += (_, args) =>
                Console.WriteLine($"[!] Error: {args.Exception.Message}");
        }

        public void connect()
        {
            client.ConnectAsync("ws://127.0.0.1:12345");
        }

        public void disconnect()
        {
            client.DisconnectAsync();
        }

        public void vibe()
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
