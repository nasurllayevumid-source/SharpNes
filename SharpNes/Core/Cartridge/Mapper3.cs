// Core/Cartridge/Mapper3.cs (CNROM)
using System;

namespace SharpNES.Core.Cartridge;

public class Mapper3 : Mapper
{
    private int _chrBank;

    public Mapper3(byte[] prg, byte[] chr) : base(prg, chr)
    {
        _chrBank = 0;
    }

    public override byte ReadPRG(ushort address)
    {
        if (address < 0x8000)
            return 0xFF;

        int index = (address - 0x8000) & (_prg.Length == 0x4000 ? 0x3FFF : 0x7FFF);
        return _prg[index];
    }

    public override void WritePRG(ushort address, byte value)
    {
        if (address >= 0x8000)
        {
            _chrBank = value & 0x03;
        }
    }

    public override byte ReadCHR(ushort address)
    {
        return _chr[(_chrBank * 0x2000) + (address & 0x1FFF)];
    }

    public override void WriteCHR(ushort address, byte value)
    {
        _chr[(_chrBank * 0x2000) + (address & 0x1FFF)] = value;
    }
}