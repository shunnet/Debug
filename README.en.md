<h1 align="center">🔧 Snet.Iot.Debug</h1>

<p align="center">
  <img width="120" height="120" src="https://api.snet.cn/pic/nuget.png" alt="Snet Logo"/>
</p>

<p align="center">
  <b>Open source · Free · Multi-protocol · Industrial IoT debugging & diagnostics tool</b>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10.0-blue?logo=dotnet"/>
  <img src="https://img.shields.io/badge/platform-Windows-success?logo=windows"/>
  <img src="https://img.shields.io/badge/license-MIT-green"/>
  <img src="https://img.shields.io/github/stars/shunnet/Debug?style=social"/>
</p>

<p align="center">
  <a href="https://snet.cn"><b>🌐 Website</b></a> ·
  <a href="https://github.com/shunnet/Debug"><b>📦 GitHub</b></a> ·
  <a href="https://github.com/shunnet/Debug/releases"><b>📥 Download</b></a> ·
  <a href="https://github.com/shunnet/Daq"><b>🔌 Data Collector Daq</b></a>
</p>

<p align="center">
  English | 📖 <a href="README.md"><b>简体中文</b></a>
</p>

## ✨ Introduction

**Snet.Iot.Debug** is a multi-protocol debugging and diagnostics tool built on the **Snet.cn industrial communication library**, designed for industrial device communication debugging and protocol testing. It complements the data collector [Snet.Iot.Daq](https://github.com/shunnet/Daq) — Daq handles bulk collection, while Debug focuses on **interactive single-device / single-protocol debugging**.

```
┌──────────────────────────────────────────┐
│   Snet.Iot.Debug (WPF desktop app)        │  ← UI: MVVM + modern interface
├──────────────────────────────────────────┤
│   Snet.Windows.Controls / Snet.Core       │  ← Framework: controls / MVVM / log editor
├──────────────────────────────────────────┤
│   Snet.cn industrial comm. library (40+)  │  ← Protocol: PLC / MQ / Comm / OPC UA
└──────────────────────────────────────────┘
```

## 🚀 Feature Overview

| Module | Description |
|--------|-------------|
| 🔌 **DAQ Protocol Debugging** | 40+ Snet protocol packages (30+ PLC drivers), read/write, subscribe, data type & encoding config |
| 📨 **MQ Messaging** | MQTT / Netty / RabbitMQ / Kafka / NetMQ clients + MQTT Broker / WebSocket / Netty servers |
| 🔗 **Communication** | TCP / WebSocket / UDP (unicast / broadcast / multicast) / Serial clients + multi-client servers |
| 📡 **OPC UA** | Built-in OPC UA Server (node CRUD / import-export) + tree node browser (paging / subscription / JSON export) |
| 📊 **Real-time Charts** | ScottPlot 5.x multi-line charts with crosshair, legend interaction, multi-format export |
| ⚡ **Performance Testing** | Protocol read/write performance tests via Snet.PerformanceTesting |
| 🎬 **Media Tools** | Video → GIF (FFmpeg) · SVG Path → WPF XAML conversion |
| 🌐 **i18n / 🌓 Themes** | Chinese / English switching · dark / light themes, charts follow the skin |
| 📑 **Multi-Tab** | Tabbed document UI with right-click menu (close / close others / close all) |
| 🔔 **LED Status** | Device connection status LEDs (solid / blinking / color change) |
| 🛡️ **Global Exception Handling** | Three-layer capture (Task / UI / non-UI threads) with logging |

## 📡 Core Features

### 🔌 DAQ Protocol Debugging

- ✅ Address read/write with 20+ data types (Byte / String / Double / Float / Bool / Int / Short / Long, incl. arrays)
- ✅ Encoding config (ANSI / Unicode / UTF-8 / Hex)
- ✅ Data subscribe / unsubscribe, real-time logs (info / data / interaction panels)
- ✅ Dynamic property editing via parameter panel

### 📨 MQ Messaging

| Clients | Servers |
|---------|---------|
| MQTT Client | MQTT Broker |
| Netty Client | MQTT WebSocket Broker |
| RabbitMQ / Kafka / NetMQ | Netty Service |

- ✅ Topic publish / subscribe / unsubscribe, real-time message logs, parameter panel

### 🔗 Communication

| Clients | Servers |
|---------|---------|
| TCP / WebSocket Client | TCP / WebSocket Service |
| UDP Unicast / Broadcast / Multicast | UDP Service |
| Serial | |

- ✅ ASCII / Hex dual-format send & receive, Send & Wait mode
- ✅ Server multi-client management (list + unicast / broadcast), connection event tracking

### 📊 Real-time Charts

- ✅ Multi-line DataLogger (max 10 lines to prevent degradation)
- ✅ Crosshair, dark / light skin auto-switch, Chinese font auto-detection
- ✅ Right-click menu (adjust / reset / save / copy / remove), detached legend interaction
- ✅ Multi-format export (PNG / JPEG / BMP / WebP / SVG), configurable refresh interval

### 🌳 OPC UA Node Browser

- ✅ Recursive tree loading with paging (handles huge node sets), auto-load on scroll
- ✅ Node details table (name / address / value / type / access level / description)
- ✅ Batch subscribe / unsubscribe, JSON structure export, skin-following icons

## 📦 Installation & Usage

### 📋 Requirements

| Component | Requirement |
|-----------|-------------|
| 🖥️ **OS** | Windows 10 / 11 (x64) |
| 🔧 **.NET Runtime** | .NET 10.0 Desktop Runtime |
| 🛠️ **Build Tools** | Visual Studio 2022+ (to compile) |
| 🎬 **FFmpeg** | Required for GIF conversion (default path `lib/ffmpeg/ffmpeg.exe`, configurable in settings) |
| 💾 **Disk Space** | ≥ 500 MB |

### 📥 1️⃣ Clone

```bash
git clone https://github.com/shunnet/Debug.git
cd Debug
```

### 🔨 2️⃣ Build

Open `Debug.slnx` with **Visual Studio 2022** or later, then build Debug or Release.

### ▶️ 3️⃣ Run

Launch `Snet.Iot.Debug.exe` from the output directory.

> 💡 **No build required?** Download the pre-built ZIP from [GitHub Releases](https://github.com/shunnet/Debug/releases) and run it directly.

## 🖥️ Screenshots

<p align="center">
  <img src="images/1.png" width="900"/>
  <img src="images/2.png" width="900"/>
  <img src="images/3.png" width="900"/>
  <img src="images/4.png" width="900"/>
  <img src="images/5.png" width="900"/>
  <img src="images/6.png" width="900"/>
  <img src="images/7.png" width="900"/>
</p>

## 🗂️ Project Structure

```
Snet.Iot.Debug/
├── App.xaml / MainWindow.xaml        # Entry · global exception handling · DI · navigation
├── TabDeviceControl.xaml             # Tab container + right-click menu management
├── Language.resx / .en.resx          # Chinese / English resources
├── behaviors/                        # TabItem right-click select behavior
├── chart/                            # ScottPlot charts (data models / operations / legend popup)
├── handler/                          # Control finder · FFmpeg GIF · OPC UA paging
├── model/                            # OPC UA node models · tab item model · paged result
├── resources/icons.xaml              # Custom icon resources
├── template/                         # MQ server ViewModel base template
├── view/                             # 12 feature pages (Daq / Mq / Comm / OPC UA / SVG / GIF / About…)
└── viewModel/                        # 11 page ViewModels
```

## 📚 Resources & Community

| Channel | Link |
|---------|------|
| 🌐 **Website** | [snet.cn](https://snet.cn) |
| 📦 **NuGet Package** | [Snet.Core](https://www.nuget.org/packages/Snet.Core) |
| 🔌 **Data Collector Daq** | [github.com/shunnet/Daq](https://github.com/shunnet/Daq) |
| 🐛 **Issues** | [GitHub Issues](https://github.com/shunnet/Debug/issues) — bug reports & feature requests |
| 💬 **QQ Group** | [Join](https://qm.qq.com/q/gPjrD9wGty) — technical community |
| ⭐ **Star** | If this project helps you, please give it a Star ❤️ |

## 🙏 Acknowledgements

- [Snet.cn](https://snet.cn) — Industrial communication library
- [Snet.Windows.Controls](https://github.com/shunnet/WpfMUI) — WPF controls (incl. log editor highlighting)
- [ScottPlot](https://scottplot.net) — Scientific charting library
- [WPF UI](https://github.com/lepoco/wpfui) — Modern UI components
- [Material Design In XAML](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit) — Themes & controls
- [FFmpeg](https://ffmpeg.org) — Video conversion

## 📜 License

![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)

This project is licensed under the **MIT** License — free to use, modify and distribute.

📄 See the [LICENSE](LICENSE) file for the full terms.

> ⚠️ The software is provided "as is", without warranty of any kind.

## 📈 Star History

<a href="https://www.star-history.com/?repos=shunnet%2FDebug&type=date&legend=bottom-right">
 <picture>
   <source media="(prefers-color-scheme: dark)" srcset="https://api.star-history.com/chart?repos=shunnet/Debug&type=date&theme=dark&legend=bottom-right&sealed_token=kU93MuJXyoYfwuhgp01iZFRkdxu_dYO4cZ4R1-p5-nKUKrV2nktH5HS4LkrGQS3PB0ArdiIIm3Q-NzduUGCAu6STsdDbjorxl942Dks2clVxxO0vKqKBAQ"/>
   <source media="(prefers-color-scheme: light)" srcset="https://api.star-history.com/chart?repos=shunnet/Debug&type=date&legend=bottom-right&sealed_token=kU93MuJXyoYfwuhgp01iZFRkdxu_dYO4cZ4R1-p5-nKUKrV2nktH5HS4LkrGQS3PB0ArdiIIm3Q-NzduUGCAu6STsdDbjorxl942Dks2clVxxO0vKqKBAQ"/>
   <img alt="Star History Chart" src="https://api.star-history.com/chart?repos=shunnet/Debug&type=date&legend=bottom-right&sealed_token=kU93MuJXyoYfwuhgp01iZFRkdxu_dYO4cZ4R1-p5-nKUKrV2nktH5HS4LkrGQS3PB0ArdiIIm3Q-NzduUGCAu6STsdDbjorxl942Dks2clVxxO0vKqKBAQ"/>
 </picture>
</a>
