// Core/Memory/Bus.cs
using SharpNES.Core.CPU;
using SharpNES.Core.PPU;
using SharpNES.Core.Cartridge;
using SharpNES.Core.Input;

namespace SharpNES.Core.Memory;

public class Bus
{
    private byte[] _ram = new byte[0x0800];
    private Cartridge _cartridge;
    private PPU _ppu;
    private Controller _controller1, _controller2;

    public void SetPPU(PPU ppu) => _ppu = ppu;
    public void SetController1(Controller c) => _controller1 = c;
    public void SetController2(Controller c) => _controller2 = c;
    public void LoadCartridge(Cartridge c) => _cartridge = c;

    public byte Read(ushort address)
    {
        if (address < 0x2000) return _ram[address & 0x07FF];
        if (address < 0x4000) return _ppu.ReadRegister((ushort)(0x2000 + (address & 0x0007)));
        if (address == 0x4016) return _controller1.Read();
        if (address == 0x4017) return _controller2.Read();
        if (address >= 0x4020) return _cartridge.ReadPRG(address);
        return 0xFF;
    }

    public void Write(ushort address, byte value)
    {
        if (address < 0x2000) { _ram[address & 0x07FF] = value; return; }
        if (address < 0x4000) { _ppu.WriteRegister((ushort)(0x2000 + (address & 0x0007)), value); return; }
        if (address == 0x4014) { ushort baseAddr = (ushort)(value << 8); for (int i = 0; i < 0x0100; i++) _ppu.WriteRegister(0x2004, Read((ushort)(baseAddr + i))); return; }
        if (address == 0x4016) { _controller1.SetStrobe(value); _controller2.SetStrobe(value); return; }
        if (address >= 0x4020) { _cartridge.WritePRG(address, value); return; }
    }
}