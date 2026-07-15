# SharpNES

**SharpNES** is a lightweight, cross-platform Nintendo Entertainment System (NES) emulator written in C#. It accurately emulates the 6502 CPU, PPU graphics, APU audio, and supports multiple common mappers (NROM, MMC1, MMC3, UNROM, CNROM).

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![License](https://img.shields.io/badge/license-MIT-green)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey)

---

## 📌 Table of Contents

- [Features](#features)
- [System Requirements](#system-requirements)
- [Installation & Build](#installation--build)
- [Usage](#usage)
- [Controls](#controls)
- [Supported Mappers](#supported-mappers)
- [Project Structure](#project-structure)
- [Performance](#performance)
- [Roadmap](#roadmap)
- [License](#license)

---

## ✨ Features

- **Full 6502 CPU emulation** — all 56 instructions with correct cycle timing
- **PPU (Picture Processing Unit)** — background rendering, sprite support, palette management
- **APU (Audio Processing Unit)** — 5-channel sound (Pulse 1/2, Triangle, Noise, DMC)
- **Mapper support** — NROM (0), MMC1 (1), UNROM (2), CNROM (3), MMC3 (4)
- **Controller input** — keyboard mapping for two players
- **OAM DMA** — correct sprite memory transfer
- **Clean, modular architecture** — easy to extend and maintain

---

## 💻 System Requirements

| Component | Minimum |
|-----------|---------|
| **OS** | Windows 10/11, Linux, macOS |
| **.NET** | .NET 8.0 SDK or higher |
| **RAM** | 128 MB |
| **Storage** | ~20 MB |
| **CPU** | Any modern x64/ARM64 processor |

---

## 🛠️ Installation & Build

Build
bash

dotnet build -c Release

Run
bash

dotnet run -- roms/game.nes

Publish as Standalone EXE
bash

dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./release

🎮 Usage
Command Line
bash

dotnet run -- path/to/rom.nes

Example
bash

dotnet run -- roms/super_mario_bros.nes

ROM Directory

Place your .nes ROM files in the roms/ directory.
🎯 Controls
Player 1
Keyboard Key	NES Button
Z	A
X	B
Enter	Start
Space	Select
Arrow Keys	D-Pad
Player 2
Keyboard Key	NES Button
I	A
O	B
P	Start
L	Select
WASD	D-Pad
🔌 Supported Mappers
Mapper	ID	Games
NROM	0	Super Mario Bros., Donkey Kong, Pac-Man
MMC1	1	The Legend of Zelda, Metroid, Tetris
UNROM	2	Castlevania, Contra, Mega Man
CNROM	3	Arkanoid, Duck Hunt
MMC3	4	Super Mario Bros. 3, Kirby's Adventure, Megaman 3-6
📁 Project Structure
text

SharpNES/
├── SharpNES.csproj
├── Program.cs
├── Core/
│   ├── CPU/
│   │   ├── CPU.cs
│   │   └── Registers.cs
│   ├── PPU/
│   │   └── PPU.cs
│   ├── APU/
│   │   └── APU.cs
│   ├── Memory/
│   │   └── Bus.cs
│   ├── Cartridge/
│   │   ├── Cartridge.cs
│   │   ├── Mapper.cs
│   │   ├── Mapper0.cs
│   │   ├── Mapper1.cs
│   │   ├── Mapper2.cs
│   │   ├── Mapper3.cs
│   │   └── Mapper4.cs
│   ├── Input/
│   │   └── Controller.cs
│   └── System/
│       └── Emulator.cs
├── roms/
│   └── (your .nes ROMs)
└── README.md

⚡ Performance

SharpNES uses a pure software interpreter (no JIT). Performance is excellent for all NES games on any modern hardware.
Game	FPS
Super Mario Bros.	60
The Legend of Zelda	60
Metroid	60
Castlevania	60
Contra	60
🗺️ Roadmap
Version	Features
v0.1	✅ CPU, PPU, APU, Mapper 0/1/2
v0.2	✅ MMC3 support, sprite rendering
v0.3	🔜 Save/Load States
v1.0	🔜 GUI (Avalonia), all mappers
v1.1	🔜 Netplay
📄 License

This project is licensed under the MIT License — see the LICENSE file for details.
text

MIT License

Copyright (c) 2026 [Your Name]

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

🙏 Acknowledgments

    NESDev Wiki — for CPU, PPU, and mapper documentation

    6502.org — for 6502 instruction reference

    Open-source NES emulator community

⭐ Support the Project
