namespace SharpNES.Core.CPU;

public class Registers
{
    public byte A;
    public byte X;
    public byte Y;
    public byte SP;
    public ushort PC;
    public byte Status;

    public bool FlagN
    {
        get => (Status & 0x80) != 0;
        set => Status = (byte)(value ? Status | 0x80 : Status & ~0x80);
    }

    public bool FlagV
    {
        get => (Status & 0x40) != 0;
        set => Status = (byte)(value ? Status | 0x40 : Status & ~0x40);
    }

    public bool FlagB
    {
        get => (Status & 0x10) != 0;
        set => Status = (byte)(value ? Status | 0x10 : Status & ~0x10);
    }

    public bool FlagD
    {
        get => (Status & 0x08) != 0;
        set => Status = (byte)(value ? Status | 0x08 : Status & ~0x08);
    }

    public bool FlagI
    {
        get => (Status & 0x04) != 0;
        set => Status = (byte)(value ? Status | 0x04 : Status & ~0x04);
    }

    public bool FlagZ
    {
        get => (Status & 0x02) != 0;
        set => Status = (byte)(value ? Status | 0x02 : Status & ~0x02);
    }

    public bool FlagC
    {
        get => (Status & 0x01) != 0;
        set => Status = (byte)(value ? Status | 0x01 : Status & ~0x01);
    }

    public void Reset()
    {
        A = 0;
        X = 0;
        Y = 0;
        SP = 0xFD;
        PC = 0xFFFC;
        Status = 0x24;
    }
}