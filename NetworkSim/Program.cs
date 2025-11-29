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
        world.TimeScale = 0.001f;

        var routerA = new NetworkLayer.IpTablesRouter(1);
        routerA.Interfaces[0].LinkNode = new LinkLayer.LinkEndpoint("A");
        //routerA.Interfaces[0].IpAddress
        routerA.RoutingTable.Entries.Add(new(0, 0, 0));

        var routerB = new NetworkLayer.IpTablesRouter(2);
        routerB.Interfaces[0].LinkNode = new LinkLayer.LinkEndpoint("B");
        routerB.Interfaces[0].IpAddress = 2;
        routerB.Interfaces[1].LinkNode = new LinkLayer.LinkEndpoint("B-C");
        routerB.Interfaces[1].IpAddress = 8;
        routerB.RoutingTable.Entries.Add(new(3, uint.MaxValue, 1));
        routerB.RoutingTable.Entries.Add(new(0, 0, 0));

        var routerC = new NetworkLayer.IpTablesRouter(1);
        routerC.Interfaces[0].LinkNode = new LinkLayer.LinkEndpoint("C");
        routerC.Interfaces[0].IpAddress = 3;
        routerC.RoutingTable.Entries.Add(new(0, 0, 0));

        world.AddEntity(routerA);
        world.AddEntity(routerB);
        world.AddEntity(routerC);

        routerA.Interfaces[0].LinkNode!.LinkWith(routerB.Interfaces[0].LinkNode!);
        routerB.Interfaces[1].LinkNode!.LinkWith(routerC.Interfaces[0].LinkNode!);

        var datagram = new NetworkLayer.Datagram
        {
            SourceIp = 1,
            DestinationIp = 3,
        };

        routerA.SendDatagram(datagram, routerA.Interfaces[0]);

        routerA.Position = new Vector2(200, 240);
        routerB.Position = new Vector2(600, 240);
        routerC.Position = new Vector2(700, 280);

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
