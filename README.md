# Supercell.ArxanUnprotector.UI - Fork with Graphical User Interface (GUI)

This is a fork of the [Supercell.ArxanUnprotector](https://github.com/Mimi8298/Supercell.ArxanUnprotector) tool to remove and re-protect Arxan protection from Supercell's games on Android (arm/arm64) and iOS (arm64).

This fork adds a modern, dark-themed **Graphical User Interface (GUI)** and supports saving the output files to a custom folder.

Fork repository URL: https://github.com/FilesProjects/Supercell.ArxanUnprotector.UI

## Requirements
* .NET 7 (https://dotnet.microsoft.com/download/dotnet/7.0)
* Desktop environment (Windows, Linux, or macOS) to run the GUI.

## GUI Usage
Launch the application without any arguments to open the graphical user interface.
* **Select Action**: Decrypt Strings, Encrypt Strings, Update Checksums, or Verify Checksums.
* **Browse Files**: Select your original, decrypted, or modified files easily.
* **Drag and Drop**: Drag files directly onto the path text boxes.
* **Save Folder**: Specify a custom folder to save your output files. If left empty, files are saved in the default location.
* **Execution Log**: Check progress, success messages, and details in the terminal-like logs at the bottom.

## CLI Usage
If run with arguments, the application falls back to the original console execution mode:

```
Usage: Supercell.ArxanUnprotector -a <action> -i <original> -m <modified> [-o <output>]
Actions:
    verify-crc - Verify checksums in modified file
    update-crc - Update checksums in modified file and save to output file
    decrypt - Decrypt strings in original file and save to output file
    encrypt - Encrypt strings in modified file and save to output file
```

### CLI Examples
* ```Supercell.ArxanUnprotector -a verify-crc -i liboriginal.so -m libmodified.so```: Verify if the checksums in libmodified.so are correct
* ```Supercell.ArxanUnprotector -a update-crc -i liboriginal.so -m libmodified.so -o libmodified.so```: Update the checksums in libmodified.so
* ```Supercell.ArxanUnprotector -a decrypt -i liboriginal.so -o liboriginal.so.decrypted```: Decrypt strings in liboriginal.so and save to liboriginal.so.decrypted
* ```Supercell.ArxanUnprotector -a encrypt -i liboriginal.so -m liboriginal.so.decrypted -o liboriginal.so.encrypted```: Encrypt strings in liboriginal.so.decrypted and save to liboriginal.so.encrypted. Note that you need to update the checksums after encrypting the strings.

## Precompiled Binaries
You can compile precompiled binaries for Windows, Linux, and macOS. They will be generated in the project root folder inside the following directories:
* `Windows/` - Standalone build for Windows (contains `Supercell.ArxanUnprotector.exe` and `capstone.dll`).
* `lINUX/` - Standalone build for Linux (contains `Supercell.ArxanUnprotector` and `libcapstone.so`).
* `MacOS/` - Standalone build for macOS (contains `Supercell.ArxanUnprotector` and `libcapstone.dylib`).

## Building
To publish the standalone builds yourself, run the following commands from the root directory:
* **Windows**: `dotnet publish Supercell.ArxanUnprotector/Supercell.ArxanUnprotector.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o Windows`
* **Linux**: `dotnet publish Supercell.ArxanUnprotector/Supercell.ArxanUnprotector.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o lINUX`
* **macOS**: `dotnet publish Supercell.ArxanUnprotector/Supercell.ArxanUnprotector.csproj -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -o MacOS`

## Contact
You can contact the original author on Discord: ```mimi8297```.
