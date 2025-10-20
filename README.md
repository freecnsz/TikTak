# TikTak - Professional Presentation Timer

TikTak is a modern timer application designed specifically for conferences, seminars, and professional presentations. With its intuitive interface and powerful feature set, it ensures smooth and effective time management during events.

## Key Features

### Display and Positioning
- **Full-Screen Presentation Mode** - Optimized for on-stage use with clear, large visual display
- **Custom Positioning & Smart Alignment** - Place the timer anywhere on the screen with pixel-perfect control
- **WASD Keyboard Navigation** - Move and position the timer quickly using keyboard controls
- **Multi-Screen Support & Position Memory** - Automatically restores window positions across multiple displays
- **Auto Screen Detection** - Detects screen changes and repositions intelligently
- **Transparent & Draggable UI** - Smooth, flexible window placement
- **Always-on-Top Mode** - Keeps the timer visible during presentations
- **DPI Scaling Support** - Fully compatible with high-DPI displays

### Timer Controls
- **Flexible Time Setting** - Supports 1 to 999 minutes for diverse session types
- **Quick Time Adjustment Buttons** - +1, -1, +5, -5, and Reset for fast manual tuning
- **Negative Counting Mode** - Continue counting beyond zero to track overtime
- **Global Keyboard Shortcuts** - Control the timer even when the window is unfocused

### Customization
- **5 Theme Options** - Light, Dark, Light Blue, Dark Blue, and Green
- **3 Size Levels** - Small, Medium, and Large display options
- **4 Edge Margin Settings** - Adjustable distance from screen edges
- **Multi-Language Support** - Turkish and English interface

### Notifications
- **Smart Notification System** - Automatic alerts at 5-minute and 1-minute marks
- **Desktop Notifications** - System popups for time warnings
- **System Tray Integration** - Quick show/hide and control from the tray

## Quick Start

### System Requirements
- Windows 10 or later
- .NET 9.0 Runtime

### Installation

**Option 1: Install via Winget**
```bash
winget install freecnsz.TikTak
```

**Option 2: Manual Installation**
```bash
git clone https://github.com/freecnsz/TikTak.git
cd TikTak
dotnet build
dotnet run
```

## User Guide

### Basic Usage
1. **Set Duration** - Enter the desired time in minutes
2. **Start Timer** - Click the play button or press `F5`
3. **Pause Timer** - Click the pause button or press `F5` again
4. **Reset Timer** - Click the reset button or press `F7`

### Keyboard Shortcuts
| Shortcut | Action |
|----------|--------|
| `Ctrl+Shift+T` | Show/Hide the timer |
| `F5` | Start/Pause |
| `F7` | Reset |
| `ESC` | Close and reset |

### System Tray Integration
- **Double-click** - Show or hide the window
- **Right-click** - Open menu (Settings, About, Exit)
- **Hover** - View current timer status

## Configuration

Access the settings window to customize:
- Theme selection (5 color options)
- Language preference (TR/EN)
- Notification alerts (5-minute and 1-minute warnings)
- Desktop notification popups
- Screen positioning and alignment
- Display size and edge margins

## Technical Overview

### Architecture
- **Framework** - .NET 9.0 (WPF)
- **Design Pattern** - MVVM (Model-View-ViewModel)
- **System Integration** - Windows Forms NotifyIcon
- **Data Storage** - JSON configuration

### Project Structure
```
TikTak/
├── Models/           # Data models
├── Services/         # Business logic services
├── Windows/          # User interface windows
└── Resources/        # Images and assets
```

## Support

- **Report Issues** - [GitHub Issues](https://github.com/freecnsz/TikTak/issues)
- **Request Features** - [GitHub Discussions](https://github.com/freecnsz/TikTak/discussions)

## License

Licensed under the [MIT License](LICENSE).

---

**TikTak** - Take control of time in your professional presentations.
