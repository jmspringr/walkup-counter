# Walk-Up Counter

A simple WinForms desktop app for tracking walk-up IT requests (or anything else you want to count daily).

## Features

- **One-click counting** - Big button to add requests
- **Keyboard shortcuts** - Press `Space` or `+` to add, `-` to undo
- **Daily totals** - Automatically resets each day
- **History** - Shows recent days' counts
- **System tray** - Minimizes to tray, runs in background
- **Always on top** - Stays visible while you work
- **Persistent data** - Saves to `walkup_data.json` next to the exe

## Build & Run

### Prerequisites
- .NET 6.0 SDK or later (download from https://dotnet.microsoft.com/download)

### Build from Command Line

```bash
# Navigate to the project folder
cd WalkUpCounter

# Build release version
dotnet build -c Release

# Run it
dotnet run

# Or find the exe at:
# bin/Release/net6.0-windows/WalkUpCounter.exe
```

### Publish as Standalone EXE (no .NET install required on target machine)

```bash
# Self-contained single-file exe
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# Find it at:
# bin/Release/net6.0-windows/win-x64/publish/WalkUpCounter.exe
```

### Open in Visual Studio

Just double-click `WalkUpCounter.sln` and press F5 to run.

## Usage

1. Run `WalkUpCounter.exe`
2. Click **+ Add Request** (or press Space) each time someone walks up
3. Close the window to minimize to system tray
4. Double-click tray icon to reopen
5. Right-click tray icon for quick actions

## Data Storage

Counts are saved to `walkup_data.json` in the same folder as the exe. The file looks like:

```json
{
  "Days": {
    "2024-01-15": 12,
    "2024-01-16": 8
  }
}
```

You can edit this file manually if needed, or copy it to preserve history.
