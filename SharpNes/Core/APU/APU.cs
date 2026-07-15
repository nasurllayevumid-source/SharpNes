// Core/APU/APU.cs
using System;

namespace SharpNES.Core.APU;

public class APU
{
    private PulseChannel _pulse1;
    private PulseChannel _pulse2;
    private TriangleChannel _triangle;
    private NoiseChannel _noise;
    private DMCChannel _dmc;

    private int _cycleCounter;
    private int _frameCounter;

    public APU()
    {
        _pulse1 = new PulseChannel();
        _pulse2 = new PulseChannel();
        _triangle = new TriangleChannel();
        _noise = new NoiseChannel();
        _dmc = new DMCChannel();
        Reset();
    }

    public void Reset()
    {
        _pulse1.Reset();
        _pulse2.Reset();
        _triangle.Reset();
        _noise.Reset();
        _dmc.Reset();
        _cycleCounter = 0;
        _frameCounter = 0;
    }

    public void Step(int cycles)
    {
        _cycleCounter += cycles;

        while (_cycleCounter >= 1)
        {
            _cycleCounter--;
            _pulse1.Step();
            _pulse2.Step();
            _triangle.Step();
            _noise.Step();
            _dmc.Step();

            _frameCounter++;
            if (_frameCounter >= 14915)
            {
                _frameCounter = 0;
            }
        }
    }

    public byte ReadRegister(ushort address)
    {
        switch (address)
        {
            case 0x4015:
                return (byte)((_pulse1.Enabled ? 0x01 : 0) | (_pulse2.Enabled ? 0x02 : 0) |
                              (_triangle.Enabled ? 0x04 : 0) | (_noise.Enabled ? 0x08 : 0) |
                              (_dmc.Enabled ? 0x10 : 0));
            default:
                return 0xFF;
        }
    }

    public void WriteRegister(ushort address, byte value)
    {
        if (address >= 0x4000 && address < 0x4004)
            _pulse1.WriteRegister(address, value);
        else if (address >= 0x4004 && address < 0x4008)
            _pulse2.WriteRegister(address, value);
        else if (address >= 0x4008 && address < 0x400C)
            _triangle.WriteRegister(address, value);
        else if (address >= 0x400C && address < 0x4010)
            _noise.WriteRegister(address, value);
        else if (address >= 0x4010 && address < 0x4014)
            _dmc.WriteRegister(address, value);
        else if (address == 0x4015)
        {
            _pulse1.Enabled = (value & 0x01) != 0;
            _pulse2.Enabled = (value & 0x02) != 0;
            _triangle.Enabled = (value & 0x04) != 0;
            _noise.Enabled = (value & 0x08) != 0;
            _dmc.Enabled = (value & 0x10) != 0;
        }
    }
}

public class PulseChannel
{
    private int _duty;
    private int _dutyPosition;
    private int _lengthCounter;
    private int _envelopeCounter;
    private int _envelopeStart;
    private int _envelopeVolume;
    private int _envelopeReset;
    private int _sweepPeriod;
    private int _sweepShift;
    private int _timer;
    private int _period;
    private bool _enabled;
    private bool _constantVolume;
    private int _volume;

    private readonly int[] _dutyPatterns = { 0b00000001, 0b10000001, 0b10000111, 0b01111110 };

    public bool Enabled { get; set; }

    public PulseChannel()
    {
        Reset();
    }

    public void Reset()
    {
        _duty = 0;
        _dutyPosition = 0;
        _lengthCounter = 0;
        _envelopeCounter = 0;
        _envelopeStart = 0;
        _envelopeVolume = 0;
        _envelopeReset = 0;
        _sweepPeriod = 0;
        _sweepShift = 0;
        _timer = 0;
        _period = 0;
        _enabled = false;
        _constantVolume = false;
        _volume = 0;
    }

    public void WriteRegister(ushort address, byte value)
    {
        switch (address)
        {
            case 0x4000:
                _duty = (value >> 6) & 0x03;
                _constantVolume = (value & 0x10) != 0;
                _envelopeStart = value & 0x0F;
                _envelopeReset = _envelopeStart;
                _volume = _constantVolume ? _envelopeStart : 0;
                break;
            case 0x4001:
                _sweepPeriod = (value >> 4) & 0x07;
                _sweepShift = value & 0x07;
                break;
            case 0x4002:
                _period = (_period & 0x0700) | value;
                break;
            case 0x4003:
                _period = (_period & 0x00FF) | ((value & 0x07) << 8);
                _lengthCounter = 64 - (value >> 3);
                break;
        }
    }

    public void Step()
    {
        if (!Enabled) return;

        _timer--;
        if (_timer <= 0)
        {
            _timer = _period + 1;
            _dutyPosition = (_dutyPosition + 1) & 0x07;
        }

        if (_envelopeReset > 0)
        {
            _envelopeCounter++;
            if (_envelopeCounter >= _envelopeReset)
            {
                _envelopeCounter = 0;
                if (_envelopeVolume > 0)
                    _envelopeVolume--;
            }
        }

        if (!_constantVolume)
            _volume = _envelopeVolume;
    }

    public int GetSample()
    {
        if (!Enabled || _lengthCounter == 0 || _volume == 0)
            return 0;

        int bit = (_dutyPatterns[_duty] >> _dutyPosition) & 1;
        return bit * _volume;
    }
}

public class TriangleChannel
{
    private int _lengthCounter;
    private int _timer;
    private int _period;
    private int _step;
    private bool _enabled;

    public bool Enabled { get; set; }

    public TriangleChannel()
    {
        Reset();
    }

    public void Reset()
    {
        _lengthCounter = 0;
        _timer = 0;
        _period = 0;
        _step = 0;
        _enabled = false;
    }

    public void WriteRegister(ushort address, byte value)
    {
        switch (address)
        {
            case 0x4008:
                _enabled = (value & 0x80) != 0;
                _lengthCounter = 32 - (value & 0x1F);
                break;
            case 0x400A:
                _period = (_period & 0x0700) | value;
                break;
            case 0x400B:
                _period = (_period & 0x00FF) | ((value & 0x07) << 8);
                _step = 0;
                break;
        }
    }

    public void Step()
    {
        if (!Enabled || _lengthCounter == 0 || !_enabled) return;

        _timer--;
        if (_timer <= 0)
        {
            _timer = _period + 1;
            _step = (_step + 1) & 0x1F;
        }
    }

    public int GetSample()
    {
        if (!Enabled || _lengthCounter == 0 || !_enabled)
            return 0;

        if (_step < 0x10)
            return _step * 2;
        else
            return (0x1F - _step) * 2;
    }
}

public class NoiseChannel
{
    private int _lengthCounter;
    private int _timer;
    private int _period;
    private int _envelopeCounter;
    private int _envelopeStart;
    private int _envelopeVolume;
    private int _envelopeReset;
    private bool _constantVolume;
    private int _volume;
    private ushort _shiftRegister;
    private int _mode;

    public bool Enabled { get; set; }

    public NoiseChannel()
    {
        Reset();
    }

    public void Reset()
    {
        _lengthCounter = 0;
        _timer = 0;
        _period = 0;
        _envelopeCounter = 0;
        _envelopeStart = 0;
        _envelopeVolume = 0;
        _envelopeReset = 0;
        _constantVolume = false;
        _volume = 0;
        _shiftRegister = 0x7FFF;
        _mode = 0;
    }

    public void WriteRegister(ushort address, byte value)
    {
        switch (address)
        {
            case 0x400C:
                _constantVolume = (value & 0x10) != 0;
                _envelopeStart = value & 0x0F;
                _envelopeReset = _envelopeStart;
                _volume = _constantVolume ? _envelopeStart : 0;
                break;
            case 0x400E:
                _mode = (value & 0x80) != 0 ? 1 : 0;
                _period = value & 0x0F;
                break;
            case 0x400F:
                _lengthCounter = 64 - (value >> 3);
                _shiftRegister = 0x7FFF;
                break;
        }
    }

    public void Step()
    {
        if (!Enabled) return;

        _timer--;
        if (_timer <= 0)
        {
            _timer = _period + 1;
            int bit = (_shiftRegister & 0x01) ^ ((_shiftRegister >> 1) & 0x01);
            _shiftRegister >>= 1;
            _shiftRegister |= (ushort)(bit << 14);
            if (_mode == 1)
            {
                _shiftRegister |= (ushort)(bit << 6);
                _shiftRegister &= 0xFFBF;
            }
        }

        if (_envelopeReset > 0)
        {
            _envelopeCounter++;
            if (_envelopeCounter >= _envelopeReset)
            {
                _envelopeCounter = 0;
                if (_envelopeVolume > 0)
                    _envelopeVolume--;
            }
        }

        if (!_constantVolume)
            _volume = _envelopeVolume;
    }

    public int GetSample()
    {
        if (!Enabled || _lengthCounter == 0 || _volume == 0)
            return 0;

        return ((_shiftRegister & 0x01) != 0) ? 0 : _volume;
    }
}

public class DMCChannel
{
    public bool Enabled { get; set; }

    public DMCChannel()
    {
        Reset();
    }

    public void Reset() { }

    public void WriteRegister(ushort address, byte value) { }
    public void Step() { }
    public int GetSample() { return 0; }
}