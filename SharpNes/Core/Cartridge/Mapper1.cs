// Core/Cartridge/Mapper1.cs (MMC1)
using System;

namespace SharpNES.Core.Cartridge;

public class Mapper1 : Mapper
{
    private byte _shiftRegister;
    private byte _shiftCount;
    private byte[] _registers = new byte[4];
    private int _prgBankMode;
    private int _chrBankMode;

    private int _prgBank1;
    private int _prgBank2;
    private int _chrBank1;
    private int _chrBank2;

    public Mapper1(byte[] prg, byte[] chr) : base(prg, chr)
    {
        _shiftRegister = 0;
        _shiftCount = 0;
        _registers = new byte[4];
        _prgBankMode = 0;
        _chrBankMode = 0;
        _prgBank1 = 0;
        _prgBank2 = 0;
        _chrBank1 = 0;
        _chrBank2 = 0;
    }

    public override byte ReadPRG(ushort address)
    {
        if (address < 0x8000)
            return 0xFF;

        if (address < 0xC000)
        {
            int bank = _prgBank1;
            if (_prgBankMode == 0)
                bank = 0;
            return _prg[(bank * 0x4000) + (address - 0x8000)];
        }

        int bank2 = _prgBank2;
        if (_prgBankMode < 2)
            bank2 = 0x0F;
        return _prg[(bank2 * 0x4000) + (address - 0xC000)];
    }

    public override void WritePRG(ushort address, byte value)
    {
        if ((value & 0x80) != 0)
        {
            _shiftRegister = 0;
            _shiftCount = 0;
            return;
        }

        _shiftRegister >>= 1;
        _shiftRegister |= (byte)((value & 0x01) << 4);
        _shiftCount++;

        if (_shiftCount < 5)
            return;

        int reg = (address >> 13) & 0x03;
        _registers[reg] = _shiftRegister;

        _shiftRegister = 0;
        _shiftCount = 0;

        UpdateBanks();
    }

    private void UpdateBanks()
    {
        _prgBankMode = (_registers[0] >> 2) & 0x03;
        _chrBankMode = (_registers[0] >> 4) & 0x01;

        _prgBank1 = _registers[3] & 0x0F;
        _prgBank2 = _registers[3] & 0x0F;

        _chrBank1 = _registers[1] & 0x1F;
        _chrBank2 = _registers[2] & 0x1F;
    }

    public override byte ReadCHR(ushort address)
    {
        if (_chrBankMode == 0)
        {
            int bank = _chrBank1 >> 1;
            return _chr[(bank * 0x2000) + (address & 0x1FFF)];
        }

        if (address < 0x1000)
        {
            return _chr[(_chrBank1 * 0x1000) + address];
        }

        return _chr[(_chrBank2 * 0x1000) + (address - 0x1000)];
    }

    public override void WriteCHR(ushort address, byte value)
    {
        if (_chrBankMode == 0)
        {
            int bank = _chrBank1 >> 1;
            _chr[(bank * 0x2000) + (address & 0x1FFF)] = value;
            return;
        }

        if (address < 0x1000)
        {
            _chr[(_chrBank1 * 0x1000) + address] = value;
        }
        else
        {
            _chr[(_chrBank2 * 0x1000) + (address - 0x1000)] = value;
        }
    }
}