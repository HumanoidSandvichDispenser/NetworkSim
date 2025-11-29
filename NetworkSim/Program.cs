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
        var timer = new Timer();
        world.AddEntity(timer);
        world.TimeScale = (float)Math.Pow(2, -11);
        timer.Start(world.TimeScale);
        timer.IsRepeating = true;

        var switchA = new LinkLayer.Switch("00:00:00:00:00:01");
        var switchB = new LinkLayer.Switch("00:00:00:00:00:02");
        var switchC = new LinkLayer.Switch("00:00:00:00:00:03");
        var switchD = new LinkLayer.Switch("00:00:00:00:00:04");

        timer.Timeout += () =>
        {
            var switches = new List<LinkLayer.Switch>
            {
                switchA,
                switchB,
                switchC,
                switchD,
            };

            // pick random source and destination
            var rnd = new Random();
            var source = switches[rnd.Next(switches.Count)];
            LinkLayer.Switch dest;
            do
            {
                dest = switches[rnd.Next(switches.Count)];
            }
            while (source == dest);

            var frame = new LinkLayer.Frame
            {
                SourceMac = source.MacAddress,
                DestinationMac = dest.MacAddress,
            };

            world.AddEntity(frame);
            source.SendFrame(frame);
        };

        world.AddEntity(switchA);
        world.AddEntity(switchB);
        world.AddEntity(switchC);
        world.AddEntity(switchD);

        switchA.LinkWith(switchB);
        switchB.LinkWith(switchC);
        switchB.LinkWith(switchD);

        switchA.Position = new Vector2(200, 250);
        switchB.Position = new Vector2(400, 250);
        switchC.Position = new Vector2(500, 200);
        switchD.Position = new Vector2(500, 300);

        var frame1 = new LinkLayer.Frame
        {
            SourceMac = switchA.MacAddress,
            DestinationMac = switchD.MacAddress,
        };

        var frame2 = new LinkLayer.Frame
        {
            SourceMac = switchA.MacAddress,
            DestinationMac = switchC.MacAddress,
        };

        world.AddEntity(frame1);
        world.AddEntity(frame2);

        switchA.SendFrame(frame1);
        switchA.SendFrame(frame2);

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
