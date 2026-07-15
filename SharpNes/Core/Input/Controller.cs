namespace SharpNES.Core.Input;

public class Controller
{
    private byte _state;
    private byte _strobe;

    public Controller()
    {
        Reset();
    }

    public void Reset()
    {
        _state = 0;
        _strobe = 0;
    }

    public void PressButton(Buttons button)
    {
        _state |= (byte)button;
    }

    public void ReleaseButton(Buttons button)
    {
        _state &= (byte)~button;
    }

    public void SetStrobe(byte value)
    {
        _strobe = value;
    }

    public byte Read()
    {
        if (_strobe == 0)
        {
            byte result = (byte)(_state & 0x01);
            _state >>= 1;
            return result;
        }
        else
        {
            return (byte)(_state & 0x01);
        }
    }

    public byte GetState()
    {
        return _state;
    }
}

[Flags]
public enum Buttons
{
    A = 0x01,
    B = 0x02,
    Select = 0x04,
    Start = 0x08,
    Up = 0x10,
    Down = 0x20,
    Left = 0x40,
    Right = 0x80
}