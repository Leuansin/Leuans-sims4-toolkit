# 🌍 Sims 4 String Relocator

<div align="center">

![Python](https://img.shields.io/badge/Python-3.8+-3776AB?style=for-the-badge&logo=python&logoColor=white)
![CustomTkinter](https://img.shields.io/badge/CustomTkinter-GUI-00D9FF?style=for-the-badge)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)
![Platform](https://img.shields.io/badge/Platform-Windows-blue?style=for-the-badge&logo=windows)

**Extract and organize language files from The Sims 4**

Automatically detect and relocate all language string packages to separate folders

[Features](#-features) • [Installation](#-installation) • [Usage](#-usage) • [Output](#-output) • [Contributing](#-contributing)

</div>

---

## 📖 Overview

**Sims 4 String Relocator** is a specialized tool for The Sims 4 modders and translators that automatically extracts and organizes language string files (`.package` files) from your game installation. It intelligently scans both base game and DLC content, separating strings by language into organized folder structures.

### Why Use This Tool?

- 🌐 **Translation Work**: Easy access to all language strings for translation projects
- 🔧 **Mod Development**: Quick extraction of string files for custom content creation
- 📦 **Organization**: Automatically sorts files by language and maintains folder hierarchy
- 🚀 **Automation**: One-click extraction of all languages at once
- 🎨 **Bilingual Interface**: Switch between English and Spanish on the fly

---

## ✨ Features

### 🚀 Core Functionality

- **Auto-Detection**: Automatically identifies all installed languages in your game
- **Smart Scanning**: Recursively searches through `Data/Client` and `Delta` folders
- **Organized Output**: Creates separate folders for each language with preserved directory structure
- **DLC Support**: Extracts strings from all expansion packs, game packs, and stuff packs
- **Bilingual UI**: Full interface available in English and Spanish
- **One-Click Extraction**: Simple, automated workflow
- **Auto-Open Results**: Automatically opens the output folder when complete

### 🎨 Modern Interface

- **Dark Theme**: Easy on the eyes with a sleek dark mode interface
- **Real-Time Logging**: Watch the extraction process in the integrated console
- **Customizable Paths**: Set your own game installation and output directories
- **Responsive Design**: Clean, modern UI built with CustomTkinter

---

## 🔧 Installation

### Prerequisites

- Python 3.8 or higher
- Windows OS (Linux/Mac compatible with minor modifications)

### Step 1: Clone the Repository

```bash
git clone https://github.com/yourusername/sims4-string-relocator.git
cd sims4-string-relocator
```

### Step 2: Install Dependencies

```bash
pip install customtkinter
```

### Step 3: Run the Application

```bash
python sims4_strings_relocator.py
```

---

## 🎯 Usage

### Quick Start

1. **Launch the Application**
   ```bash
   python sims4_strings_relocator.py
   ```

2. **Configure Paths**
   - **Game Installation Path**: Set to your Sims 4 installation folder
     - Default: `D:\SteamLibrary\steamapps\common\The Sims 4`
     - Common paths:
       - Steam: `C:\Program Files (x86)\Steam\steamapps\common\The Sims 4`
       - Origin/EA: `C:\Program Files\EA Games\The Sims 4`
   
   - **Output Destination**: Where extracted files will be saved
     - Default: `Desktop/Strings`

3. **Start Extraction**
   - Click **"INICIAR EXTRACCIÓN"** (Spanish) or **"START EXTRACTION"** (English)
   - Monitor progress in the console log
   - Output folder opens automatically when complete

### Language Toggle

Switch between English and Spanish using the **EN/ES** toggle in the top-right corner.

| Language | Button Text |
|----------|-------------|
| English | START EXTRACTION |
| Spanish | INICIAR EXTRACCIÓN |

---

## 📤 Output

### Folder Structure

The tool creates an organized hierarchy preserving the original game structure:

```
Strings/
├── ENG_US/
│   ├── Data/
│   │   └── Client/
│   │       └── Strings_ENG_US.package
│   └── Delta/
│       ├── EP01/
│       │   └── Strings_ENG_US.package
│       ├── GP03/
│       │   └── Strings_ENG_US.package
│       └── ...
├── SPA_EA/
│   ├── Data/
│   │   └── Client/
│   │       └── Strings_SPA_EA.package
│   └── Delta/
│       └── ...
├── FRE_FR/
│   └── ...
└── ...
```

### Detected Languages

The tool automatically detects all installed language packages. Common languages include:

| Code | Language |
|------|----------|
| `ENG_US` | English (US) |
| `SPA_EA` | Spanish |
| `FRE_FR` | French |
| `GER_DE` | German |
| `ITA_IT` | Italian |
| `JPN_JP` | Japanese |
| `KOR_KR` | Korean |
| `POL_PL` | Polish |
| `POR_BR` | Portuguese (Brazil) |
| `RUS_RU` | Russian |
| `CHI_CN` | Chinese (Simplified) |
| `CHI_TW` | Chinese (Traditional) |

---

## 🛠️ How It Works

### Detection Process

1. **Scans** `Data/Client` for `Strings_*.package` files
2. **Identifies** all language codes from filenames
3. **Searches** recursively through:
   - `Data/Client` - Base game strings
   - `Delta` - All DLC string files (EP*, GP*, SP*, FP*)
4. **Copies** files while preserving directory structure
5. **Organizes** by language into separate folders

### File Naming Convention

The Sims 4 uses this naming pattern for language files:
```
Strings_{LANGUAGE_CODE}.package
```

Example: `Strings_ENG_US.package`, `Strings_SPA_EA.package`

---

## 🎨 Use Cases

### 1. Translation Projects
```
Extract all Spanish strings → Modify translations → Repackage
```

### 2. Mod Development
```
Extract base game strings → Reference for custom content → Create compatible mods
```

### 3. Language Comparison
```
Extract multiple languages → Compare translations → Quality assurance
```

### 4. Backup & Archive
```
Extract before game update → Archive language versions → Restore if needed
```

---

## 🖥️ Technical Details

### File Operations

- **Method**: Uses Python's `shutil.copy2` to preserve file metadata
- **Structure Preservation**: Maintains relative paths from game directory
- **Overwrite Safety**: Creates new directories as needed without overwriting existing files
- **Cross-Platform**: Compatible with Windows, Linux, and macOS (with Path library)

### Performance

- **Speed**: Processes files as fast as your disk I/O allows
- **Memory Efficient**: Copies files one at a time
- **Non-Destructive**: Original game files remain untouched
- **Typical Time**: 30 seconds - 2 minutes (depending on DLC count)

---

## 🌍 Supported Languages

The application interface supports:

- 🇬🇧 **English** (EN)
- 🇪🇸 **Spanish** (ES)

The tool can extract **any language** that The Sims 4 supports, regardless of the UI language selected.

---

## 🤝 Contributing

Contributions are welcome! Here's how you can help:

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/AmazingFeature`)
3. **Commit** your changes (`git commit -m 'Add some AmazingFeature'`)
4. **Push** to the branch (`git push origin feature/AmazingFeature`)
5. **Open** a Pull Request

### Ideas for Contributions

- [ ] Add more UI languages (French, German, etc.)
- [ ] Implement file comparison between language versions
- [ ] Add progress bar for extraction process
- [ ] Create package merger (combine multiple string files)
- [ ] Add string search and filter functionality
- [ ] Implement diff viewer for translation changes
- [ ] Add batch processing for multiple game installations
- [ ] Create CLI version for automation scripts

---

## 🔧 Troubleshooting

### Issue: "No languages detected"

**Solution**: Verify your game path points to the root Sims 4 directory (should contain `Data` and `Delta` folders)

### Issue: Permission denied

**Solution**: Run as administrator or choose an output folder where you have write permissions

### Issue: Missing DLC strings

**Solution**: Ensure all DLC is properly installed and the Delta folder contains the respective expansion pack folders

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## ⚠️ Disclaimer

This tool is provided as-is for educational and development purposes. It does not modify any game files - it only reads and copies them. Always back up your work before making modifications to extracted files.

**Not affiliated with Electronic Arts or Maxis.**

---

## 📧 Contact

Have questions, suggestions, or found a bug? Feel free to open an issue!

---

## 🙏 Acknowledgments

- Built with [CustomTkinter](https://github.com/TomSchimansky/CustomTkinter) for the modern UI
- Inspired by the amazing Sims 4 modding community
- Thanks to all translators who keep The Sims 4 accessible worldwide

---

<div align="center">

**Made with ❤️ for The Sims 4 Modding Community**

⭐ If this tool helps your workflow, consider giving it a star!

[Report Bug](../../issues) · [Request Feature](../../issues) · [Discussions](../../discussions)

</div>
