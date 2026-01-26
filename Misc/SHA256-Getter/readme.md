# 🎮 Sims 4 SHA256 Getter

<div align="center">

![Python](https://img.shields.io/badge/Python-3.8+-3776AB?style=for-the-badge&logo=python&logoColor=white)
![CustomTkinter](https://img.shields.io/badge/CustomTkinter-GUI-00D9FF?style=for-the-badge)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)
![Platform](https://img.shields.io/badge/Platform-Windows-blue?style=for-the-badge&logo=windows)

**A powerful file integrity verification tool for The Sims 4**

Generate SHA-256 checksums for your game files and export them to organized reports

[Features](#-features) • [Installation](#-installation) • [Usage](#-usage) • [Output](#-output) • [Contributing](#-contributing)

</div>

---

## 📖 Overview

**Sims 4 Integrity Auditor** is a desktop application designed to help Sims 4 players and modders verify the integrity of their game installation. It scans critical game folders, calculates SHA-256 hashes for each file, and generates both human-readable text reports and a comprehensive JSON export for automated verification.

### Why Use This Tool?

- 🔍 **Verify Game Files**: Ensure your Sims 4 installation hasn't been corrupted or modified
- 🛡️ **Mod Safety**: Create snapshots before and after installing mods
- 📊 **Documentation**: Generate detailed reports of your game state
- 🔄 **Compare Installations**: Use JSON exports to compare different game versions
- 🎯 **DLC Detection**: Automatically identifies expansion packs, game packs, and stuff packs

---

## ✨ Features

### 🚀 Core Functionality

- **Smart Folder Detection**: Automatically scans base game folders and all DLC content (EP*, GP*, SP*, FP* prefixes)
- **SHA-256 Hashing**: Uses industry-standard cryptographic hashing for reliable file verification
- **Dual Output Format**:
  - Individual `.txt` files for each folder (human-readable)
  - Master `master_hashes.json` file (machine-readable)
- **Multithreaded Processing**: Runs hashing in a separate thread to keep UI responsive
- **Modern Dark UI**: Built with CustomTkinter for a sleek, modern appearance

### 📁 Scanned Folders

The tool automatically processes these critical game directories:

| Folder Type | Description |
|-------------|-------------|
| `__Installer` | Installation files |
| `Data` | Core game data |
| `Delta` | Update patches |
| `Game` | Main game binaries and assets |
| `Support` | Support files |
| `EP*` | Expansion Packs |
| `GP*` | Game Packs |
| `SP*` | Stuff Packs |
| `FP*` | Feature Packs |

---

## 🔧 Installation

### Prerequisites

- Python 3.8 or higher
- Windows OS (default path configured for Steam installation)

### Step 1: Clone the Repository

```bash
git clone https://github.com/yourusername/sims4-integrity-auditor.git
cd sims4-integrity-auditor
```

### Step 2: Install Dependencies

```bash
pip install customtkinter
```

### Step 3: Run the Application

```bash
python sims4_hasher.py
```

---

## 🎯 Usage

### Quick Start

1. **Launch the Application**
   ```bash
   python sims4_hasher.py
   ```

2. **Set Game Path** (Optional)
   - Default path: `C:\Program Files (x86)\Steam\steamapps\common\The Sims 4`
   - Click "Buscar" (Browse) to select a different installation directory

3. **Generate Reports**
   - Click the **"GENERAR REPORTES Y JSON"** button
   - Watch the progress in the log window

4. **Access Results**
   - Find your reports in the newly created `Sims4_Integrity_YYYYMMDD_HHMMSS` folder

### Custom Installation Paths

The tool supports various installation locations:

- **Steam**: `C:\Program Files (x86)\Steam\steamapps\common\The Sims 4`
- **Origin/EA App**: `C:\Program Files\EA Games\The Sims 4`
- **Custom**: Use the browse button to select any directory

---

## 📤 Output

### Folder Structure

```
Sims4_Integrity_20250126_143052/
├── Hashes___Installer.txt
├── Hashes_Data.txt
├── Hashes_Delta.txt
├── Hashes_Game.txt
├── Hashes_Support.txt
├── Hashes_EP01.txt (if installed)
├── Hashes_GP03.txt (if installed)
├── ...
└── master_hashes.json
```

### Text File Format

```
TS4 FOLDER: Game

Game/Bin/TS4_x64.exe | a3f8d9e2b1c4f6a8d9e2b1c4f6a8d9e2b1c4f6a8d9e2b1c4f6a8d9e2b1c4f6a8
Game/Bin/TS4.exe | b4e9d0f3c2d5e7b9e0f3c2d5e7b9e0f3c2d5e7b9e0f3c2d5e7b9e0f3c2d5e7b9
...
```

### JSON Format

```json
{
    "info": {
        "game": "The Sims 4",
        "date": "20250126_143052",
        "base_path": "C:\\Program Files (x86)\\Steam\\steamapps\\common\\The Sims 4"
    },
    "files": {
        "Game/Bin/TS4_x64.exe": "a3f8d9e2b1c4f6a8...",
        "Game/Bin/TS4.exe": "b4e9d0f3c2d5e7b9...",
        ...
    }
}
```

---

## 🔐 Use Cases

### 1. Pre-Mod Installation Snapshot
```bash
# Before installing mods
python sims4_hasher.py
# Save the output folder as "baseline"
```

### 2. Verify Game Integrity After Update
```bash
# After a game update
python sims4_hasher.py
# Compare JSON with previous snapshot
```

### 3. Troubleshooting Corrupted Files
- Generate a fresh hash report
- Compare with a known-good backup
- Identify modified or corrupted files

---

## 🛠️ Technical Details

### Hashing Algorithm

- **Algorithm**: SHA-256 (256-bit Secure Hash Algorithm)
- **Block Size**: 8192 bytes (8 KB) for optimal performance
- **Format**: Hexadecimal digest (64 characters)

### Performance

- Processes files in 8 KB chunks for memory efficiency
- Multithreaded architecture prevents UI freezing
- Typical scan time: 5-15 minutes (depending on installation size)

---

## 🤝 Contributing

Contributions are welcome! Here's how you can help:

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/AmazingFeature`)
3. **Commit** your changes (`git commit -m 'Add some AmazingFeature'`)
4. **Push** to the branch (`git push origin feature/AmazingFeature`)
5. **Open** a Pull Request

### Ideas for Contributions

- [ ] Add support for Origin/EA App default paths
- [ ] Implement hash comparison between two JSON files
- [ ] Add progress bar for individual folder scanning
- [ ] Create CLI version for automation
- [ ] Add support for custom folder selection
- [ ] Implement file exclusion filters

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## ⚠️ Disclaimer

This tool is provided as-is for educational and verification purposes. It does not modify any game files. Always back up your game saves before troubleshooting file integrity issues.

**Not affiliated with Electronic Arts or Maxis.**

---

## 📧 Contact

Have questions or suggestions? Feel free to open an issue!

---

<div align="center">

**Made with ❤️ for The Sims 4 Community**

⭐ If this tool helped you, consider giving it a star!

</div>
