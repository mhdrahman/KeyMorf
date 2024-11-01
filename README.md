# KeyMorf
**KeyMorf is a C# application designed to provide customizable key mapping and macro functionality on Windows which can be defined through a JSON configuration file.**

**Built for personal use.**

## Table of Contents
- [Getting Started](#getting-started)
- [Configuration](#configuration)

## Getting Started

### Prerequisites
- Windows OS
- [.NET 6.0 SDK or higher](https://dotnet.microsoft.com/download) installed

### Configuration
The `Keymap.json` file defines custom key mappings and macros for each toggleable layer. Each layer can specify:
- **Toggle Key**: The key used to enable or disable the layer.
- **Toggle Time (ms)**: Time (in milliseconds) that the toggle key must be held to activate the layer.
- **Mappings**: Key-to-key remappings within the layer.
- **Macros**: Multi-key macros triggered by specific keys within the layer.

#### Example `Keymap.json`
```json
{
  "Symbols": {
    "ToggleKey": "Z",
    "ToggleTimeMs": 100,
    "Mappings": [
      {
        "From": "Y",
        "To": "Nine",
        "Mods": [ "LShift" ]
      },
      {
        "From": "U",
        "To": "Zero",
        "Mods": [ "LShift" ]
      },
      {
        "From": "I",
        "To": "LSquareBracket",
        "Mods": [ "LShift" ]
      },
      {
        "From": "O",
        "To": "RSquareBracket",
        "Mods": [ "LShift" ]
      },
      {
        "From": "P",
        "To": "LSquareBracket"
      },
      {
        "From": "Semicolon",
        "To": "RSquareBracket"
      },
      {
        "From": "N",
        "To": "One",
        "Mods": [ "RShift" ]
      },
      {
        "From": "M",
        "To": "Equal"
      },
      {
        "From": "Comma",
        "To": "Seven",
        "Mods": [ "LShift" ]
      },
      {
        "From": "Fullstop",
        "To": "Backslash",
        "Mods": [ "RShift" ]
      },
      {
        "From": "H",
        "To": "Left"
      },
      {
        "From": "J",
        "To": "Down"
      },
      {
        "From": "K",
        "To": "Up"
      },
      {
        "From": "L",
        "To": "Right"
      }
    ],
    "Macros": [
      {
        "ToggleKey": "Up",
        "Macro": [
          { "Key": "H", "Mods": [ "LShift" ] },
          { "Key": "E" },
          { "Key": "L" },
          { "Key": "L" },
          { "Key": "O" },
          { "Key": "Comma" },
          { "Key": "Space" },
          { "Key": "W", "Mods": [ "RShift" ] },
          { "Key": "O" },
          { "Key": "R" },
          { "Key": "L" },
          { "Key": "D" },
          { "Key": "One", "Mods": [ "RShift" ] }
        ]
      }
    ]
  }
}
```
