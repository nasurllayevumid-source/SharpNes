// Core/Cartridge/Cartridge.cs
using System.IO;

namespace SharpNES.Core.Cartridge;

public class Cartridge
{
    private byte[] _prg, _chr;
    private Mapper _mapper;
    private string _path;

    public Cartridge(string path)
    {
        _path = path;
        byte[] data = File.ReadAllBytes(path);
        if (data.Length < 16) throw new Exception("Invalid NES ROM");

        int mapperId = ((data[6] >> 4) & 0x0F) | (data[7] & 0xF0);
        int prgBanks = data[4];
        int chrBanks = data[5];

        _prg = new byte[prgBanks * 0x4000];
        _chr = new byte[chrBanks * 0x2000];
        Array.Copy(data, 16, _prg, 0, _prg.Length);
        if (chrBanks > 0) Array.Copy(data, 16 + _prg.Length, _chr, 0, _chr.Length);
        else _chr = new byte[0x2000];

        _mapper = mapperId switch
        {
            0 => new Mapper0(_prg, _chr),
            1 => new Mapper1(_prg, _chr),
            2 => new Mapper2(_prg, _chr),
            3 => new Mapper3(_prg, _chr),
            4 => new Mapper4(_prg, _chr),
            _ => new Mapper0(_prg, _chr)
        };
        Console.WriteLine($"Mapper: {mapperId}");
    }

    public byte ReadPRG(ushort address) => _mapper.ReadPRG(address);
    public void WritePRG(ushort address, byte value) => _mapper.WritePRG(address, value);
    public byte ReadCHR(ushort address) => _mapper.ReadCHR(address);
    public void WriteCHR(ushort address, byte value) => _mapper.WriteCHR(address, value);
}