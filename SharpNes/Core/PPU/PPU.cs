// Core/PPU/PPU.cs
using SharpNES.Core.Memory;

namespace SharpNES.Core.PPU;

public class PPU
{
    private Bus _bus;
    private byte _ctrl, _mask, _status, _oamAddr, _scroll, _addr, _data;
    private ushort _vramAddr, _tramAddr;
    private byte _fineX;
    private byte[] _vram = new byte[0x2000];
    private byte[] _oam = new byte[0x0100];
    private byte[] _palette = new byte[0x0020];
    private byte[] _framebuffer = new byte[256 * 240 * 3];
    private int _scanline, _cycle;
    private bool _frameReady;

    public PPU(Bus bus) { _bus = bus; Reset(); }

    public void Reset()
    {
        _ctrl = 0; _mask = 0; _status = 0; _oamAddr = 0; _scroll = 0; _addr = 0; _data = 0;
        _vramAddr = 0; _tramAddr = 0; _fineX = 0;
        _scanline = 0; _cycle = 0; _frameReady = false;
        Array.Clear(_vram, 0, _vram.Length);
        Array.Clear(_oam, 0, _oam.Length);
        Array.Clear(_palette, 0, _palette.Length);
        Array.Clear(_framebuffer, 0, _framebuffer.Length);
    }

    public byte ReadRegister(ushort address)
    {
        switch (address & 0x0007)
        {
            case 0x02:
                byte status = _status;
                _status &= 0x7F;
                _addr = 0; _scroll = 0;
                return status;
            case 0x04: return _oam[_oamAddr];
            case 0x07: return ReadData();
            default: return 0xFF;
        }
    }

    public void WriteRegister(ushort address, byte value)
    {
        switch (address & 0x0007)
        {
            case 0x00: _ctrl = value; _tramAddr = (ushort)((_tramAddr & 0xF3FF) | ((value & 0x03) << 10)); break;
            case 0x01: _mask = value; break;
            case 0x03: _oamAddr = value; break;
            case 0x04: _oam[_oamAddr] = value; _oamAddr++; break;
            case 0x05:
                if (_addr == 0) { _fineX = (byte)(value & 0x07); _tramAddr = (ushort)((_tramAddr & 0xFFE0) | (value >> 3)); }
                else { _tramAddr = (ushort)((_tramAddr & 0x8C1F) | ((value & 0x07) << 12) | ((value & 0xF8) << 2)); }
                _addr ^= 1; break;
            case 0x06:
                if (_addr == 0) _tramAddr = (ushort)((_tramAddr & 0x80FF) | ((value & 0x3F) << 8));
                else { _tramAddr = (ushort)((_tramAddr & 0xFF00) | value); _vramAddr = _tramAddr; }
                _addr ^= 1; break;
            case 0x07: WriteData(value); break;
        }
    }

    private byte ReadData()
    {
        byte data = _vram[_vramAddr];
        _vramAddr += (byte)((_ctrl & 0x04) != 0 ? 32 : 1);
        return data;
    }

    private void WriteData(byte value)
    {
        _vram[_vramAddr] = value;
        _vramAddr += (byte)((_ctrl & 0x04) != 0 ? 32 : 1);
    }

    public void Step()
    {
        _cycle++;
        if (_cycle > 340) { _cycle = 0; _scanline++; }
        if (_scanline > 261) { _scanline = 0; _frameReady = true; _status |= 0x80; }
        if (_scanline < 240 && _cycle > 0 && _cycle <= 256) RenderPixel();
    }

    private void RenderPixel()
    {
        int x = _cycle - 1, y = _scanline;
        if (x >= 256 || y >= 240) return;

        int tileX = x / 8, tileY = y / 8;
        int offsetX = x % 8, offsetY = y % 8;
        int baseNameTable = (_ctrl & 0x01) != 0 ? 0x1000 : 0x0000;
        int nametableAddr = baseNameTable + tileY * 32 + tileX;
        byte tileIndex = _vram[nametableAddr];
        int tileAddr = 0x1000 * ((_ctrl & 0x10) != 0 ? 1 : 0) + tileIndex * 16 + offsetY;

        byte lowByte = _vram[tileAddr];
        byte highByte = _vram[tileAddr + 1];
        int bit = 7 - offsetX;
        int color = ((highByte >> bit) & 1) << 1;
        color |= (lowByte >> bit) & 1;
        int paletteAddr = 0x3F00 + color;
        byte pixel = _vram[paletteAddr];

        int index = (y * 256 + x) * 3;
        _framebuffer[index] = (byte)((pixel & 0x01) * 255);
        _framebuffer[index + 1] = (byte)((pixel & 0x02) * 127);
        _framebuffer[index + 2] = (byte)((pixel & 0x04) * 63);

        RenderSprites(x, y);
    }

    private void RenderSprites(int x, int y)
    {
        if ((_mask & 0x10) == 0) return;
        int spriteCount = 0, maxSprites = 8;

        for (int i = 0; i < 0x100 && spriteCount < maxSprites; i += 4)
        {
            int spriteY = _oam[i] - 1;
            int tileIndex = _oam[i + 1];
            byte attributes = _oam[i + 2];
            int spriteX = _oam[i + 3];

            if (y < spriteY || y >= spriteY + 8 || x < spriteX || x >= spriteX + 8) continue;
            spriteCount++;

            bool flipX = (attributes & 0x40) != 0, flipY = (attributes & 0x80) != 0;
            int paletteIndex = (attributes & 0x03) * 4 + 0x10;
            int row = y - spriteY;
            if (flipY) row = 7 - row;

            int tileAddr = 0x1000 * ((_ctrl & 0x10) != 0 ? 1 : 0) + tileIndex * 16 + row;
            byte lowByte = _vram[tileAddr];
            byte highByte = _vram[tileAddr + 1];

            int col = x - spriteX;
            int bit = flipX ? col : 7 - col;
            int color = ((highByte >> bit) & 1) << 1;
            color |= (lowByte >> bit) & 1;
            if (color == 0) continue;

            int palAddr = 0x3F00 + paletteIndex + color;
            byte pixel = _vram[palAddr];
            int index = (y * 256 + x) * 3;
            _framebuffer[index] = (byte)((pixel & 0x01) * 255);
            _framebuffer[index + 1] = (byte)((pixel & 0x02) * 127);
            _framebuffer[index + 2] = (byte)((pixel & 0x04) * 63);
        }
    }

    public byte[] GetFramebuffer() => _framebuffer;
    public bool FrameReady => _frameReady;
}