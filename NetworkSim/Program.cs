global using System;
global using System.Numerics;
global using System.Collections.Generic;
using Raylib_cs;

namespace NetworkSim;

internal static class Program
{
    [System.STAThread]
    public static void Main()
    {
        Raylib.InitWindow(800, 480, "Hello World");
        Raylib.SetTargetFPS(60);

        var world = World.Instance;
        world.TimeScale = 0.01f;

        var nodes = NetworkFactory.CreateLocalNetwork(world, "192.168.1.1", "255.255.255.0", 7)
            .ToList();

        // set up timer to send random data from some host
        Timer timer = new Timer();
        timer.Timeout += () =>
        {
            if (new System.Random().Next(50) > 2)
            {
                return;
            }

            // pick a random host to send data
            var host = nodes
                .Where(n => n is NetworkLayer.NetworkHost)
                .Select(n => n as NetworkLayer.NetworkHost!)
                .Skip(new System.Random().Next(nodes.Count))
                .FirstOrDefault();

            var recipient = nodes
                .Where(n => n is NetworkLayer.NetworkHost)
                .Where(n => n != host)
                .Select(n => n as NetworkLayer.NetworkHost!)
                .Skip(new System.Random().Next(nodes.Count))
                .FirstOrDefault();

            if (host is null || recipient is null)
            {
                return;
            }

            var datagram = new NetworkLayer.Datagram
            {
                SourceIp = host.Interface.IpAddress,
                DestinationIp = recipient.Interface.IpAddress,
            };

            host.SendDatagram(datagram);
        };

        world.AddEntity(timer);
        timer.IsRepeating = true;
        timer.Start(world.TimeScale * 0.05f);

        while (!Raylib.WindowShouldClose())
        {
            float delta = Raylib.GetFrameTime();
            world.Update(delta);

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.White);
            world.Draw();
            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}
