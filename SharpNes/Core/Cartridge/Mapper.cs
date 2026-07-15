namespace SharpNES.Core.Cartridge;

public abstract class Mapper
{
    protected byte[] _prg;
    protected byte[] _chr;

    public Mapper(byte[] prg, byte[] chr)
    {
        _prg = prg;
        _chr = chr;
    }

    public abstract byte ReadPRG(ushort address);
    public abstract void WritePRG(ushort address, byte value);
    public abstract byte ReadCHR(ushort address);
    public abstract void WriteCHR(ushort address, byte value);
}