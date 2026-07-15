// Core/Cartridge/Mapper4.cs (MMC3)
using System;

namespace SharpNES.Core.Cartridge;

public class Mapper4 : Mapper
{
    private byte[] _registers = new byte[8];
    private int _prgBankMode;
    private int _chrBankMode;
    private int _prgBank1, _prgBank2, _prgBank3, _prgBank4;
    private int _chrBank1, _chrBank2, _chrBank3, _chrBank4, _chrBank5, _chrBank6;

    public Mapper4(byte[] prg, byte[] chr) : base(prg, chr)
    {
        _prgBankMode = 0;
        _chrBankMode = 0;
        _prgBank1 = 0;
        _prgBank2 = 0;
        _prgBank3 = 0;
        _prgBank4 = 0;
        _chrBank1 = 0;
        _chrBank2 = 0;
        _chrBank3 = 0;
        _chrBank4 = 0;
        _chrBank5 = 0;
        _chrBank6 = 0;
        UpdateBanks();
    }

    public override byte ReadPRG(ushort address)
    {
        if (address < 0x8000)
            return 0xFF;

        if (address < 0xA000)
        {
            int bank = _prgBankMode == 0 ? _prgBank2 : _prgBank1;
            return _prg[(bank * 0x2000) + (address - 0x8000)];
        }

        if (address < 0xC000)
        {
            int bank = _prgBankMode == 0 ? _prgBank3 : _prgBank3;
            return _prg[(bank * 0x2000) + (address - 0xA000)];
        }

        if (address < 0xE000)
        {
            int bank = _prgBankMode == 0 ? _prgBank4 : _prgBank2;
            return _prg[(bank * 0x2000) + (address - 0xC000)];
        }

        return _prg[(_prg.Length - 0x2000) + (address - 0xE000)];
    }

    public override void WritePRG(ushort address, byte value)
    {
        if (address < 0x8000)
            return;

        if (address < 0xA000)
        {
            _registers[address & 0x07] = value;
            UpdateBanks();
            return;
        }

        if (address < 0xC000)
        {
            _prgBankMode = value & 0x40;
            _chrBankMode = value & 0x80;
            UpdateBanks();
            return;
        }

        if (address < 0xE000)
        {
            // IRQ counter
            return;
        }

        // IRQ disable
    }

    private void UpdateBanks()
    {
        _prgBank1 = _registers[6] & 0x3F;
        _prgBank2 = _registers[7] & 0x3F;
        _prgBank3 = 0x3E;
        _prgBank4 = 0x3F;

        _chrBank1 = _registers[0] & 0xFE;
        _chrBank2 = _registers[1] & 0xFE;
        _chrBank3 = _registers[2] & 0xFE;
        _chrBank4 = _registers[3] & 0xFE;
        _chrBank5 = _registers[4] & 0xFE;
        _chrBank6 = _registers[5] & 0xFE;
    }

    public override byte ReadCHR(ushort address)
    {
        if (address < 0x0400)
            return _chr[(_chrBank1 * 0x0400) + address];
        if (address < 0x0800)
            return _chr[(_chrBank2 * 0x0400) + (address - 0x0400)];
        if (address < 0x0C00)
            return _chr[(_chrBank3 * 0x0400) + (address - 0x0800)];
        if (address < 0x1000)
            return _chr[(_chrBank4 * 0x0400) + (address - 0x0C00)];
        if (address < 0x1400)
            return _chr[(_chrBank5 * 0x0400) + (address - 0x1000)];
        if (address < 0x1800)
            return _chr[(_chrBank6 * 0x0400) + (address - 0x1400)];
        return _chr[((_chrBank5 | 1) * 0x0400) + (address - 0x1800)];
    }

    public override void WriteCHR(ushort address, byte value)
    {
        if (address < 0x0400)
            _chr[(_chrBank1 * 0x0400) + address] = value;
        else if (address < 0x0800)
            _chr[(_chrBank2 * 0x0400) + (address - 0x0400)] = value;
        else if (address < 0x0C00)
            _chr[(_chrBank3 * 0x0400) + (address - 0x0800)] = value;
        else if (address < 0x1000)
            _chr[(_chrBank4 * 0x0400) + (address - 0x0C00)] = value;
        else if (address < 0x1400)
            _chr[(_chrBank5 * 0x0400) + (address - 0x1000)] = value;
        else if (address < 0x1800)
            _chr[(_chrBank6 * 0x0400) + (address - 0x1400)] = value;
        else
            _chr[((_chrBank5 | 1) * 0x0400) + (address - 0x1800)] = value;
    }
}