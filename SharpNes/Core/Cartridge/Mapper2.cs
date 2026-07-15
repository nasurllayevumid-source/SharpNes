// Core/Cartridge/Mapper2.cs (UNROM)
using System;

namespace SharpNES.Core.Cartridge;

public class Mapper2 : Mapper
{
    private int _prgBank;

    public Mapper2(byte[] prg, byte[] chr) : base(prg, chr)
    {
        _prgBank = 0;
    }

    public override byte ReadPRG(ushort address)
    {
        if (address < 0x8000)
            return 0xFF;

        if (address < 0xC000)
        {
            return _prg[(_prgBank * 0x4000) + (address - 0x8000)];
        }

        return _prg[(_prg.Length - 0x4000) + (address - 0xC000)];
    }

    public override void WritePRG(ushort address, byte value)
    {
        if (address >= 0x8000 && address < 0xC000)
        {
            _prgBank = value & 0x0F;
        }
    }

    public override byte ReadCHR(ushort address)
    {
        return _chr[address & 0x1FFF];
    }

    public override void WriteCHR(ushort address, byte value)
    {
        _chr[address & 0x1FFF] = value;
    }
}