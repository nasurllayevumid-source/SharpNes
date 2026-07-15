// Program.cs
using SharpNES.Core.System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("╔══════════════════════════════════╗");
        Console.WriteLine("║     SharpNES Emulator v0.1      ║");
        Console.WriteLine("╚══════════════════════════════════╝");
        Console.WriteLine();

        if (args.Length == 0)
        {
            Console.WriteLine("Usage: dotnet run -- <rom.nes>");
            Console.WriteLine("Example: dotnet run -- roms/super_mario.nes");
            return;
        }

        string romPath = args[0];

        if (!File.Exists(romPath))
        {
            Console.WriteLine($"ROM not found: {romPath}");
            return;
        }

        var emu = new Emulator();
        emu.LoadRom(romPath);
        emu.Run();
    }
}