namespace SharpNES.Core.Cartridge;

public class Mapper0 : Mapper
{
    public Mapper0(byte[] prg, byte[] chr) : base(prg, chr) { }

    public override byte ReadPRG(ushort address)
    {
        if (address < 0x8000)
            return 0xFF;

        int index = (address - 0x8000) & (_prg.Length == 0x4000 ? 0x3FFF : 0x7FFF);
        return _prg[index];
    }

    public override void WritePRG(ushort address, byte value) { }

    public override byte ReadCHR(ushort address)
    {
        return _chr[address & 0x1FFF];
    }

    public override void WriteCHR(ushort address, byte value)
    {
        _chr[address & 0x1FFF] = value;
    }
}