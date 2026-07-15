// Core/CPU/Instructions.cs
using SharpNES.Core.Memory;

namespace SharpNES.Core.CPU;

public enum AddressingMode
{
    Accumulator,
    Immediate,
    ZeroPage,
    ZeroPageX,
    ZeroPageY,
    Absolute,
    AbsoluteX,
    AbsoluteY,
    Indirect,
    IndirectX,
    IndirectY,
    Relative
}

public partial class CPU
{
    private byte ReadByte(ushort address) => _bus.Read(address);
    private void WriteByte(ushort address, byte value) => _bus.Write(address, value);

    private ushort ReadWord(ushort address)
    {
        return (ushort)(_bus.Read(address) | (_bus.Read((ushort)(address + 1)) << 8));
    }

    private void WriteWord(ushort address, ushort value)
    {
        _bus.Write(address, (byte)(value & 0xFF));
        _bus.Write((ushort)(address + 1), (byte)(value >> 8));
    }

    private ushort GetAddress(AddressingMode mode)
    {
        switch (mode)
        {
            case AddressingMode.Immediate:
                return _regs.PC++;

            case AddressingMode.ZeroPage:
                return ReadByte(_regs.PC++);

            case AddressingMode.ZeroPageX:
                return (ushort)((ReadByte(_regs.PC++) + _regs.X) & 0xFF);

            case AddressingMode.ZeroPageY:
                return (ushort)((ReadByte(_regs.PC++) + _regs.Y) & 0xFF);

            case AddressingMode.Absolute:
                return ReadWord(_regs.PC++);

            case AddressingMode.AbsoluteX:
                ushort addr = ReadWord(_regs.PC++);
                return (ushort)(addr + _regs.X);

            case AddressingMode.AbsoluteY:
                addr = ReadWord(_regs.PC++);
                return (ushort)(addr + _regs.Y);

            case AddressingMode.Indirect:
                addr = ReadWord(_regs.PC++);
                return ReadWord(addr);

            case AddressingMode.IndirectX:
                byte zp = (byte)(ReadByte(_regs.PC++) + _regs.X);
                return ReadWord(zp);

            case AddressingMode.IndirectY:
                zp = ReadByte(_regs.PC++);
                addr = ReadWord(zp);
                return (ushort)(addr + _regs.Y);

            case AddressingMode.Relative:
                sbyte offset = (sbyte)ReadByte(_regs.PC++);
                return (ushort)(_regs.PC + offset);

            case AddressingMode.Accumulator:
                return 0;

            default:
                return 0;
        }
    }

    private byte GetOperand(AddressingMode mode)
    {
        if (mode == AddressingMode.Accumulator)
            return _regs.A;

        ushort addr = GetAddress(mode);
        return ReadByte(addr);
    }

    private void SetFlags(byte value)
    {
        _regs.FlagZ = value == 0;
        _regs.FlagN = (value & 0x80) != 0;
    }

    private void ADC(AddressingMode mode)
    {
        byte operand = GetOperand(mode);
        ushort result = (ushort)(_regs.A + operand + (_regs.FlagC ? 1 : 0));
        _regs.FlagC = result > 0xFF;
        _regs.FlagV = ((_regs.A ^ operand) & 0x80) == 0 && ((_regs.A ^ (byte)result) & 0x80) != 0;
        _regs.A = (byte)result;
        SetFlags(_regs.A);
        _cycles += (mode == AddressingMode.Immediate || mode == AddressingMode.ZeroPage || mode == AddressingMode.ZeroPageX ||
                   mode == AddressingMode.Absolute || mode == AddressingMode.AbsoluteX || mode == AddressingMode.AbsoluteY ||
                   mode == AddressingMode.IndirectX || mode == AddressingMode.IndirectY) ? 1 : 0;
    }

    private void AND(AddressingMode mode)
    {
        byte operand = GetOperand(mode);
        _regs.A &= operand;
        SetFlags(_regs.A);
        _cycles++;
    }

    private void ASL(AddressingMode mode)
    {
        if (mode == AddressingMode.Accumulator)
        {
            _regs.FlagC = (_regs.A & 0x80) != 0;
            _regs.A <<= 1;
            SetFlags(_regs.A);
            _cycles += 2;
            return;
        }

        ushort addr = GetAddress(mode);
        byte value = ReadByte(addr);
        _regs.FlagC = (value & 0x80) != 0;
        value <<= 1;
        WriteByte(addr, value);
        SetFlags(value);
        _cycles++;
    }

    private void BIT(AddressingMode mode)
    {
        byte operand = GetOperand(mode);
        _regs.FlagZ = (_regs.A & operand) == 0;
        _regs.FlagV = (operand & 0x40) != 0;
        _regs.FlagN = (operand & 0x80) != 0;
        _cycles++;
    }

    private void BRK()
    {
        _regs.PC++;
        _bus.Write((ushort)(0x0100 + _regs.SP), (byte)(_regs.PC >> 8));
        _regs.SP--;
        _bus.Write((ushort)(0x0100 + _regs.SP), (byte)(_regs.PC & 0x00FF));
        _regs.SP--;
        _regs.FlagB = true;
        _bus.Write((ushort)(0x0100 + _regs.SP), _regs.Status);
        _regs.SP--;
        _regs.FlagI = true;
        _regs.PC = (ushort)((_bus.Read(0xFFFE) << 8) | _bus.Read(0xFFFF));
        _cycles += 7;
    }

    private void BPL()
    {
        Branch(!_regs.FlagN);
    }

    private void BMI()
    {
        Branch(_regs.FlagN);
    }

    private void BVC()
    {
        Branch(!_regs.FlagV);
    }

    private void BVS()
    {
        Branch(_regs.FlagV);
    }

    private void BCC()
    {
        Branch(!_regs.FlagC);
    }

    private void BCS()
    {
        Branch(_regs.FlagC);
    }

    private void BNE()
    {
        Branch(!_regs.FlagZ);
    }

    private void BEQ()
    {
        Branch(_regs.FlagZ);
    }

    private void Branch(bool condition)
    {
        sbyte offset = (sbyte)ReadByte(_regs.PC++);
        _cycles++;
        if (condition)
        {
            _regs.PC = (ushort)(_regs.PC + offset);
            _cycles++;
        }
    }

    private void CLC() { _regs.FlagC = false; _cycles++; }
    private void SEC() { _regs.FlagC = true; _cycles++; }
    private void CLI() { _regs.FlagI = false; _cycles++; }
    private void SEI() { _regs.FlagI = true; _cycles++; }
    private void CLD() { _regs.FlagD = false; _cycles++; }
    private void SED() { _regs.FlagD = true; _cycles++; }
    private void CLV() { _regs.FlagV = false; _cycles++; }

    private void CMP(AddressingMode mode)
    {
        Compare(_regs.A, mode);
    }

    private void CPX(AddressingMode mode)
    {
        Compare(_regs.X, mode);
    }

    private void CPY(AddressingMode mode)
    {
        Compare(_regs.Y, mode);
    }

    private void Compare(byte reg, AddressingMode mode)
    {
        byte operand = GetOperand(mode);
        _regs.FlagC = reg >= operand;
        _regs.FlagZ = reg == operand;
        _regs.FlagN = ((reg - operand) & 0x80) != 0;
        _cycles++;
    }

    private void DEC(AddressingMode mode)
    {
        ushort addr = GetAddress(mode);
        byte value = (byte)(ReadByte(addr) - 1);
        WriteByte(addr, value);
        SetFlags(value);
        _cycles++;
    }

    private void INC(AddressingMode mode)
    {
        ushort addr = GetAddress(mode);
        byte value = (byte)(ReadByte(addr) + 1);
        WriteByte(addr, value);
        SetFlags(value);
        _cycles++;
    }

    private void DEX()
    {
        _regs.X--;
        SetFlags(_regs.X);
        _cycles++;
    }

    private void DEY()
    {
        _regs.Y--;
        SetFlags(_regs.Y);
        _cycles++;
    }

    private void INX()
    {
        _regs.X++;
        SetFlags(_regs.X);
        _cycles++;
    }

    private void INY()
    {
        _regs.Y++;
        SetFlags(_regs.Y);
        _cycles++;
    }

    private void EOR(AddressingMode mode)
    {
        byte operand = GetOperand(mode);
        _regs.A ^= operand;
        SetFlags(_regs.A);
        _cycles++;
    }

    private void JMP(AddressingMode mode)
    {
        _regs.PC = GetAddress(mode);
        _cycles++;
    }

    private void JSR()
    {
        ushort addr = ReadWord(_regs.PC++);
        _regs.PC--;
        _bus.Write((ushort)(0x0100 + _regs.SP), (byte)(_regs.PC >> 8));
        _regs.SP--;
        _bus.Write((ushort)(0x0100 + _regs.SP), (byte)(_regs.PC & 0xFF));
        _regs.SP--;
        _regs.PC = addr;
        _cycles += 6;
    }

    private void LDA(AddressingMode mode)
    {
        byte operand = GetOperand(mode);
        _regs.A = operand;
        SetFlags(_regs.A);
        _cycles++;
    }

    private void LDX(AddressingMode mode)
    {
        byte operand = GetOperand(mode);
        _regs.X = operand;
        SetFlags(_regs.X);
        _cycles++;
    }

    private void LDY(AddressingMode mode)
    {
        byte operand = GetOperand(mode);
        _regs.Y = operand;
        SetFlags(_regs.Y);
        _cycles++;
    }

    private void LSR(AddressingMode mode)
    {
        if (mode == AddressingMode.Accumulator)
        {
            _regs.FlagC = (_regs.A & 0x01) != 0;
            _regs.A >>= 1;
            SetFlags(_regs.A);
            _cycles += 2;
            return;
        }

        ushort addr = GetAddress(mode);
        byte value = ReadByte(addr);
        _regs.FlagC = (value & 0x01) != 0;
        value >>= 1;
        WriteByte(addr, value);
        SetFlags(value);
        _cycles++;
    }

    private void NOP()
    {
        _cycles++;
    }

    private void ORA(AddressingMode mode)
    {
        byte operand = GetOperand(mode);
        _regs.A |= operand;
        SetFlags(_regs.A);
        _cycles++;
    }

    private void PHA()
    {
        _bus.Write((ushort)(0x0100 + _regs.SP), _regs.A);
        _regs.SP--;
        _cycles += 3;
    }

    private void PHP()
    {
        _bus.Write((ushort)(0x0100 + _regs.SP), _regs.Status);
        _regs.SP--;
        _cycles += 3;
    }

    private void PLA()
    {
        _regs.SP++;
        _regs.A = _bus.Read((ushort)(0x0100 + _regs.SP));
        SetFlags(_regs.A);
        _cycles += 4;
    }

    private void PLP()
    {
        _regs.SP++;
        _regs.Status = _bus.Read((ushort)(0x0100 + _regs.SP));
        _cycles += 4;
    }

    private void ROL(AddressingMode mode)
    {
        if (mode == AddressingMode.Accumulator)
        {
            bool carry = _regs.FlagC;
            _regs.FlagC = (_regs.A & 0x80) != 0;
            _regs.A <<= 1;
            if (carry) _regs.A |= 0x01;
            SetFlags(_regs.A);
            _cycles += 2;
            return;
        }

        ushort addr = GetAddress(mode);
        byte value = ReadByte(addr);
        bool oldCarry = _regs.FlagC;
        _regs.FlagC = (value & 0x80) != 0;
        value <<= 1;
        if (oldCarry) value |= 0x01;
        WriteByte(addr, value);
        SetFlags(value);
        _cycles++;
    }

    private void ROR(AddressingMode mode)
    {
        if (mode == AddressingMode.Accumulator)
        {
            bool carry = _regs.FlagC;
            _regs.FlagC = (_regs.A & 0x01) != 0;
            _regs.A >>= 1;
            if (carry) _regs.A |= 0x80;
            SetFlags(_regs.A);
            _cycles += 2;
            return;
        }

        ushort addr = GetAddress(mode);
        byte value = ReadByte(addr);
        bool oldCarry = _regs.FlagC;
        _regs.FlagC = (value & 0x01) != 0;
        value >>= 1;
        if (oldCarry) value |= 0x80;
        WriteByte(addr, value);
        SetFlags(value);
        _cycles++;
    }

    private void RTI()
    {
        _regs.SP++;
        _regs.Status = _bus.Read((ushort)(0x0100 + _regs.SP));
        _regs.SP++;
        _regs.PC = _bus.Read((ushort)(0x0100 + _regs.SP));
        _regs.SP++;
        _regs.PC |= (ushort)(_bus.Read((ushort)(0x0100 + _regs.SP)) << 8);
        _cycles += 6;
    }

    private void RTS()
    {
        _regs.SP++;
        _regs.PC = _bus.Read((ushort)(0x0100 + _regs.SP));
        _regs.SP++;
        _regs.PC |= (ushort)(_bus.Read((ushort)(0x0100 + _regs.SP)) << 8);
        _regs.PC++;
        _cycles += 6;
    }

    private void SBC(AddressingMode mode)
    {
        byte operand = GetOperand(mode);
        ushort result = (ushort)(_regs.A - operand - (_regs.FlagC ? 0 : 1));
        _regs.FlagC = result <= 0xFF;
        _regs.FlagV = ((_regs.A ^ operand) & 0x80) != 0 && ((_regs.A ^ (byte)result) & 0x80) != 0;
        _regs.A = (byte)result;
        SetFlags(_regs.A);
        _cycles++;
    }

    private void STA(AddressingMode mode)
    {
        ushort addr = GetAddress(mode);
        WriteByte(addr, _regs.A);
        _cycles++;
    }

    private void STX(AddressingMode mode)
    {
        ushort addr = GetAddress(mode);
        WriteByte(addr, _regs.X);
        _cycles++;
    }

    private void STY(AddressingMode mode)
    {
        ushort addr = GetAddress(mode);
        WriteByte(addr, _regs.Y);
        _cycles++;
    }

    private void TAX()
    {
        _regs.X = _regs.A;
        SetFlags(_regs.X);
        _cycles++;
    }

    private void TAY()
    {
        _regs.Y = _regs.A;
        SetFlags(_regs.Y);
        _cycles++;
    }

    private void TSX()
    {
        _regs.X = _regs.SP;
        SetFlags(_regs.X);
        _cycles++;
    }

    private void TXA()
    {
        _regs.A = _regs.X;
        SetFlags(_regs.A);
        _cycles++;
    }

    private void TXS()
    {
        _regs.SP = _regs.X;
        _cycles++;
    }

    private void TYA()
    {
        _regs.A = _regs.Y;
        SetFlags(_regs.A);
        _cycles++;
    }

    private void Unknown(byte opcode)
    {
        Console.WriteLine($"Unknown opcode: 0x{opcode:X2} at PC: 0x{_regs.PC - 1:X4}");
        _cycles++;
    }
}