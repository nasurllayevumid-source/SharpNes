// Core/PPU/Sprites.cs
using SharpNES.Core.Memory;

namespace SharpNES.Core.PPU;

public partial class PPU
{
    private void RenderSprites()
    {
        if ((_mask & 0x10) == 0) return;

        int spriteCount = 0;
        int maxSprites = 8;

        for (int i = 0; i < 0x100 && spriteCount < maxSprites; i += 4)
        {
            int y = _oam[i];
            int tileIndex = _oam[i + 1];
            byte attributes = _oam[i + 2];
            int x = _oam[i + 3];

            int spriteY = y - 1;
            if (_scanline < spriteY || _scanline >= spriteY + 8)
                continue;

            spriteCount++;

            bool flipX = (attributes & 0x40) != 0;
            bool flipY = (attributes & 0x80) != 0;
            int paletteIndex = (attributes & 0x03) * 4 + 0x10;

            int row = _scanline - spriteY;
            if (flipY) row = 7 - row;

            int tileAddr = 0x1000 * ((_ctrl & 0x10) != 0 ? 1 : 0) + tileIndex * 16 + row;
            byte lowByte = _vram[tileAddr];
            byte highByte = _vram[tileAddr + 1];

            for (int col = 0; col < 8; col++)
            {
                int pixelX = x + col;
                if (pixelX < 0 || pixelX >= 256) continue;

                int bit = flipX ? col : 7 - col;
                int color = ((highByte >> bit) & 1) << 1;
                color |= (lowByte >> bit) & 1;

                if (color == 0) continue;

                int palAddr = 0x3F00 + paletteIndex + color;
                byte pixel = _vram[palAddr];

                int index = (_scanline * 256 + pixelX) * 3;
                _framebuffer[index] = (byte)((pixel & 0x01) * 255);
                _framebuffer[index + 1] = (byte)((pixel & 0x02) * 127);
                _framebuffer[index + 2] = (byte)((pixel & 0x04) * 63);
            }
        }
    }
}