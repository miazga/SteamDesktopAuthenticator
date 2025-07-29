# Steam Desktop Authenticator - Cross-Platform Version

This is a cross-platform version of Steam Desktop Authenticator that can run on Windows, macOS, and Linux using .NET 8 and Avalonia UI.

## Changes Made

The original Windows Forms application has been converted to use:

1. **.NET 8** instead of `net8.0-windows` for cross-platform compatibility
2. **Avalonia UI** instead of Windows Forms for the user interface
3. **Cross-platform URL opening** using platform-specific commands
4. **Console-based password prompts** instead of Windows Forms dialogs

## Prerequisites

- **.NET 8 SDK** installed on your system
  - Windows: Download from https://dotnet.microsoft.com/download
  - macOS: `brew install dotnet` or download from Microsoft
  - Linux: Follow instructions at https://docs.microsoft.com/en-us/dotnet/core/install/linux

## Building the Application

### On macOS/Linux:
```bash
# Navigate to the project directory
cd "Steam Desktop Authenticator"

# Restore dependencies
dotnet restore

# Build the application
dotnet build

# Run the application
dotnet run
```

### On Windows:
```cmd
# Navigate to the project directory
cd "Steam Desktop Authenticator"

# Restore dependencies
dotnet restore

# Build the application
dotnet build

# Run the application
dotnet run
```

## Creating a Self-Contained Executable

To create a standalone executable that doesn't require .NET to be installed:

### For macOS:
```bash
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true
```

### For Linux:
```bash
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true
```

### For Windows:
```cmd
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The executable will be created in the `bin/Release/net8.0/[platform]/publish/` directory.

## Features

- **Cross-platform compatibility**: Runs on Windows, macOS, and Linux
- **Modern UI**: Uses Avalonia UI with Fluent design
- **Account management**: View, add, and remove Steam accounts
- **Steam Guard codes**: Generate authentication codes (implementation needed)
- **Trade confirmations**: Handle trade confirmations (implementation needed)
- **Encryption support**: Secure storage of account data

## Limitations

This is a basic conversion of the original application. Some features still need to be fully implemented:

1. **Steam Guard code generation**: The core authentication code generation logic
2. **Trade confirmation handling**: Web-based confirmation interface
3. **Account addition workflow**: Complete account linking process
4. **Settings interface**: Application settings management

## File Structure

- `Program.cs`: Main application entry point with Avalonia setup
- `App.axaml`: Avalonia application configuration
- `MainWindow.axaml/cs`: Main application window
- `WelcomeWindow.axaml/cs`: Welcome screen for first-time users
- `Manifest.cs`: Account data management (updated for cross-platform)

## Security Notice

⚠️ **Important**: This application is for educational purposes. Using third-party Steam authenticators can put your account at risk. Steam recommends using only the official mobile app for authentication.

## Troubleshooting

### Common Issues:

1. **Missing .NET 8 SDK**: Install the .NET 8 SDK from Microsoft
2. **Avalonia dependencies**: Run `dotnet restore` to install all required packages
3. **Permission issues**: On macOS/Linux, you may need to grant permissions for file access

### Building Issues:

If you encounter build errors, try:
```bash
dotnet clean
dotnet restore
dotnet build
```

## Contributing

This is a conversion of the original Steam Desktop Authenticator. The core Steam authentication logic remains in the `lib/SteamAuth` directory and should be preserved for functionality. 