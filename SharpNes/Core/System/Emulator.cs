// Core/System/Emulator.cs
using SharpNES.Core.CPU;
using SharpNES.Core.Memory;
using SharpNES.Core.PPU;
using SharpNES.Core.APU;
using SharpNES.Core.Cartridge;
using SharpNES.Core.Input;

namespace SharpNES.Core.System;

public class Emulator
{
    private CPU _cpu;
    private Bus _bus;
    private PPU _ppu;
    private APU _apu;
    private Cartridge _cartridge;
    private Controller _controller1;
    private Controller _controller2;
    private bool _running;

    public Emulator()
    {
        _bus = new Bus();
        _cpu = new CPU(_bus);
        _ppu = new PPU(_bus);
        _apu = new APU();
        _controller1 = new Controller();
        _controller2 = new Controller();

        _bus.SetPPU(_ppu);
        _bus.SetController1(_controller1);
        _bus.SetController2(_controller2);
        _running = false;
    }

    public void LoadRom(string path)
    {
        _cartridge = new Cartridge(path);
        _bus.LoadCartridge(_cartridge);
        _cpu.Reset();
        _ppu.Reset();
        _apu.Reset();
        Console.WriteLine($"ROM loaded: {Path.GetFileName(path)}");
    }

    public void PressButton(Buttons button) => _controller1.PressButton(button);
    public void ReleaseButton(Buttons button) => _controller1.ReleaseButton(button);

    public void Run()
    {
        _running = true;
        Console.WriteLine("Starting emulation...");
        Console.WriteLine("Controls: Z=A, X=B, Enter=Start, Space=Select, Arrows=D-Pad");
        Console.WriteLine("Press ESC to stop.");
        Console.WriteLine();

        while (_running)
        {
            _cpu.Step();
            _ppu.Step();
            _apu.Step(1);

            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape) { _running = false; break; }
                HandleInput(key);
            }
        }

        Console.WriteLine("Emulation stopped.");
    }

    private void HandleInput(ConsoleKeyInfo key)
    {
        switch (key.Key)
        {
            case ConsoleKey.Z: PressButton(Buttons.A); break;
            case ConsoleKey.X: PressButton(Buttons.B); break;
            case ConsoleKey.Enter: PressButton(Buttons.Start); break;
            case ConsoleKey.Space: PressButton(Buttons.Select); break;
            case ConsoleKey.UpArrow: PressButton(Buttons.Up); break;
            case ConsoleKey.DownArrow: PressButton(Buttons.Down); break;
            case ConsoleKey.LeftArrow: PressButton(Buttons.Left); break;
            case ConsoleKey.RightArrow: PressButton(Buttons.Right); break;
        }
    }

    public byte[] GetFramebuffer() => _ppu.GetFramebuffer();
    public bool FrameReady => _ppu.FrameReady;
}