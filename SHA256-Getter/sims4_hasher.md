# 🎮 Sims 4 SHA256 Getter

A simple easy peasy script in python, focused in generate SHA256 checksums for the official Sims 4 game files, giving future reliable file integrity verifications and repair capabilities.

## 📋 Overview

The Sims 4 Integrity Auditor scans your game installation directory and generates comprehensive hash reports for all `.package` files from DLCs and the game's root folder. This tool is part of the upcoming **Toolkit v1.4.0** and provides a robust solution for verifying game file integrity.

## ✨ Features

- **Automated Scanning**: Recursively scans all relevant game directories
- **SHA256 Hashing**: Generates cryptographic checksums for file verification
- **Organized Reports**: Creates separate hash files for each game folder
- **User-Friendly Interface**: Modern dark-themed GUI built with CustomTkinter
- **Multi-threaded**: Non-blocking UI during the scanning process
- **Smart Folder Detection**: Automatically identifies DLC folders (EP, GP, SP, FP prefixes)

## 📦 Requirements

- Python 3.7+
- CustomTkinter library

## 🚀 Installation

1. Install the required dependency:

```bash
pip install customtkinter
```

2. Run the script:

```bash
python sims4_hasher.py
```

## 🎯 Target Folders

The tool automatically processes the following directories:

### Core Folders
- `__Installer`
- `Data`
- `Delta`
- `Game`
- `Support`

### DLC Folders (Auto-detected)
- **EP** (Expansion Packs)
- **GP** (Game Packs)
- **SP** (Stuff Packs)
- **FP** (Feature Packs)

## 💻 Usage

1. **Launch the Application**: Run the Python script to open the GUI

2. **Set Game Path**: 
   - Default path: `C:\Program Files (x86)\Steam\steamapps\common\The Sims 4`
   - Click "Browse" to select a different installation directory

3. **Start Scanning**: Click the "INICIAR ESCANEO" button to begin

4. **View Progress**: Monitor the scanning process in the real-time log console

5. **Access Reports**: Hash files are saved in a timestamped folder:
   ```
   Sims4_Hashes_YYYYMMDD_HHMMSS/
   ├── Hashes___Installer.txt
   ├── Hashes_Data.txt
   ├── Hashes_EP01.txt
   ├── Hashes_GP01.txt
   └── ...
   ```

## 📄 Report Format

Each hash report contains:

```
REPORTE DE HASHES - CARPETA: [FolderName]
ORIGEN: [FullPath]
------------------------------------------------------------

[DIR RAIZ]
  - filename.package | SHA256: [hash]

[SUBDIR] \subfolder
  - filename.package | SHA256: [hash]

TOTAL ARCHIVOS EN [FolderName]: [count]
```

## 🔧 Use Cases

- **File Integrity Verification**: Confirm game files haven't been corrupted
- **Repair Detection**: Identify modified or missing files
- **Mod Management**: Verify original game files before modding
- **Troubleshooting**: Diagnose game issues related to file corruption

## 🛠️ Upcoming Features

This tool will be integrated into **Toolkit v1.4.0** with enhanced repair features, scheduled for release on GitHub.

## ⚠️ Notes

- Scanning large game installations may take several minutes
- Ensure sufficient disk space for hash report files
- The application runs in a separate thread to maintain UI responsiveness

## 📝 License

This tool is part of the Sims 4 Toolkit project and will be released under the project's license.

## 🤝 Contributing

Contributions, issues, and feature requests are welcomed!
---

**Made with ❤️ by Leuan - for The Sims 4 Community**
