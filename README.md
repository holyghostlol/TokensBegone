# TokensBegone

> *Removes challenge tokens from your LoL profile. Because minimalism.*

![Windows](https://img.shields.io/badge/Windows-0078D6?style=flat-square&logo=windows&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)

## Download

**[Download TokensBegone.exe](https://github.com/holyghostlol/TokensBegone/releases/latest)**

Tiny (~170 KB). Instant startup. Requires .NET 8 Runtime (Windows prompts to install if missing).

## Usage

1. Open League Client & log in
2. Run TokensBegone
3. Click **"Remove Tokens"**
4. Done

## FAQ

**Is this bannable?** No. Uses Riot's official LCU API.

**Can I add tokens back?** Yes, through the challenges menu in-game.

**Windows SmartScreen warning?** Click "More info" then "Run anyway".

## Build from Source

If you prefer not to download the exe, you can build it yourself:

```bash
git clone https://github.com/holyghostlol/TokensBegone.git
cd TokensBegone
dotnet publish -c Release -o publish
```

The exe will be in the `publish` folder. Requires [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

## Legal

Not endorsed by Riot Games. Riot Games and LoL are trademarks of Riot Games, Inc.

---

<p align="center">Star if useful!</p>
