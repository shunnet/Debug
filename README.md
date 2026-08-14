<h1 align="center">🔧 Snet.Iot.Debug</h1>

<p align="center">
  <img width="120" height="120" src="https://api.snet.cn/pic/nuget.png" alt="Snet Logo"/>
</p>

<p align="center">
  <b>开源 · 免费 · 多协议 · 工业物联网调试诊断工具</b>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10.0-blue?logo=dotnet"/>
  <img src="https://img.shields.io/badge/platform-Windows-success?logo=windows"/>
  <img src="https://img.shields.io/badge/license-MIT-green"/>
  <img src="https://img.shields.io/github/stars/shunnet/Debug?style=social"/>
</p>

<p align="center">
  <a href="https://snet.cn"><b>🌐 官方网站</b></a> ·
  <a href="https://github.com/shunnet/Debug"><b>📦 GitHub</b></a> ·
  <a href="https://github.com/shunnet/Debug/releases"><b>📥 下载</b></a> ·
  <a href="https://github.com/shunnet/Daq"><b>🔌 数采工具 Daq</b></a>
</p>

<p align="center">
  📖 <a href="README.en.md"><b>English</b></a> | 简体中文
</p>

## ✨ 项目简介

**Snet.Iot.Debug** 是基于 **Snet.cn 工业通信库** 开发的多协议调试诊断工具，专为工业设备通信调试与协议测试场景设计。与数采工具 [Snet.Iot.Daq](https://github.com/shunnet/Daq) 互补——Daq 负责批量采集，Debug 聚焦**单设备 / 单协议的交互式调试**。

```
┌──────────────────────────────────────────┐
│   Snet.Iot.Debug (WPF 桌面应用)            │  ← UI 层：MVVM + 现代化界面
├──────────────────────────────────────────┤
│   Snet.Windows.Controls / Snet.Core       │  ← 框架层：控件 / MVVM / 日志编辑
├──────────────────────────────────────────┤
│   Snet.cn 工业通信库 (40+ 协议包)         │  ← 协议层：PLC / MQ / 通信 / OPC UA
└──────────────────────────────────────────┘
```

## 🚀 功能总览

| 模块 | 说明 |
|------|------|
| 🔌 **DAQ 协议调试** | 40+ Snet 协议包（30+ PLC 驱动），支持读写 / 订阅 / 数据类型切换 / 编码配置 |
| 📨 **MQ 消息调试** | MQTT / Netty / RabbitMQ / Kafka / NetMQ 客户端 + MQTT Broker / WebSocket / Netty 服务端 |
| 🔗 **通信调试** | TCP / WebSocket / UDP（单播 / 广播 / 组播）/ 串口 客户端 + 服务端多客户端管理 |
| 📡 **OPC UA** | 内置 OPC UA Server（节点增删 / 导入导出）+ 树形节点浏览器（分页 / 订阅 / JSON 导出） |
| 📊 **实时图表** | ScottPlot 5.x 多曲线实时曲线，十字准线、图例交互、多格式导出 |
| ⚡ **性能测试** | 基于 Snet.PerformanceTesting 的协议读写性能测试 |
| 🎬 **媒体工具** | 视频转 GIF（FFmpeg）· SVG Path → WPF XAML 转换 |
| 🌐 **多语言 / 🌓 主题** | 中英文动态切换 · 暗色 / 亮色主题，图表跟随变色 |
| 📑 **多标签页** | Tab 文档界面，右键菜单（关闭 / 关闭其他 / 全部关闭） |
| 🔔 **LED 状态指示** | 设备连接状态 LED（常亮 / 闪烁 / 颜色变化） |
| 🛡️ **全局异常捕获** | Task / UI / 非 UI 三层异常捕获与日志记录 |

## 📡 核心特性

### 🔌 DAQ 协议调试

- ✅ 地址读写，支持 Byte / String / Double / Float / Bool / Int / Short / Long 等 20+ 数据类型（含数组）
- ✅ 编码格式配置（ANSI / Unicode / UTF-8 / Hex）
- ✅ 数据订阅 / 取消订阅，实时数据日志（信息 / 数据 / 交互三面板）
- ✅ 参数面板动态属性编辑

### 📨 MQ 消息调试

| 客户端 | 服务端 |
|--------|--------|
| MQTT Client | MQTT Broker |
| Netty Client | MQTT WebSocket Broker |
| RabbitMQ / Kafka / NetMQ | Netty Service |

- ✅ 主题发布 / 订阅 / 取消订阅，实时消息日志，参数配置面板

### 🔗 通信调试

| 客户端 | 服务端 |
|--------|--------|
| TCP / WebSocket Client | TCP / WebSocket Service |
| UDP Unicast / Broadcast / Multicast | UDP Service |
| Serial 串口 | |

- ✅ ASCII / Hex 双格式收发，发送等待（Send & Wait）模式
- ✅ 服务端多客户端管理（列表 + 单发 / 群发），连接事件追踪

### 📊 实时图表

- ✅ 多曲线 DataLogger（最多 10 条，防止性能下降）
- ✅ 十字准线、暗 / 亮皮肤自动切换、中文字体自动检测
- ✅ 右键菜单（调整 / 重置 / 保存 / 复制 / 移除）、图例分离交互
- ✅ 多格式导出（PNG / JPEG / BMP / WebP / SVG）、可配置刷新间隔

### 🌳 OPC UA 节点浏览

- ✅ 树形节点递归加载 + 分页（防止大节点量卡顿），滚动自动加载
- ✅ 节点详情表格（名称 / 地址 / 值 / 类型 / 访问级别 / 描述）
- ✅ 批量订阅 / 取消订阅，节点结构 JSON 导出，图标跟随皮肤

## 📦 安装与使用

### 📋 环境要求

| 组件 | 要求 |
|------|------|
| 🖥️ **操作系统** | Windows 10 / 11 (x64) |
| 🔧 **.NET 运行时** | .NET 10.0 Desktop Runtime |
| 🛠️ **开发工具** | Visual Studio 2022+（编译需要） |
| 🎬 **FFmpeg** | GIF 转换功能需要 FFmpeg 可执行文件（默认查找 `lib/ffmpeg/ffmpeg.exe`，可在设置页配置路径） |
| 💾 **磁盘空间** | ≥ 500 MB |

### 📥 1️⃣ 克隆仓库

```bash
git clone https://github.com/shunnet/Debug.git
cd Debug
```

### 🔨 2️⃣ 编译项目

使用 **Visual Studio 2022** 或更高版本打开 `Debug.slnx`，选择 Debug 或 Release 构建。

### ▶️ 3️⃣ 运行程序

构建完成后运行输出目录中的 `Snet.Iot.Debug.exe`。

> 💡 **无需编译？** 前往 [GitHub Releases](https://github.com/shunnet/Debug/releases) 下载预编译 ZIP 包，解压即用。

## 🖥️ 界面展示

<p align="center">
  <img src="images/1.png" width="900"/>
  <img src="images/2.png" width="900"/>
  <img src="images/3.png" width="900"/>
  <img src="images/4.png" width="900"/>
  <img src="images/5.png" width="900"/>
  <img src="images/6.png" width="900"/>
  <img src="images/7.png" width="900"/>
</p>

## 🗂️ 项目结构

```
Snet.Iot.Debug/
├── App.xaml / MainWindow.xaml        # 入口 · 全局异常捕获 · 依赖注入 · 导航
├── TabDeviceControl.xaml             # Tab 多文档容器 + 右键菜单管理
├── Language.resx / .en.resx          # 中英文多语言资源
├── behaviors/                        # TabItem 右键选中附加行为
├── chart/                            # ScottPlot 图表（数据模型 / 操作 / 图例弹窗）
├── handler/                          # 控件查找 · FFmpeg GIF 转换 · OPC UA 分页
├── model/                            # OPC UA 节点模型 · Tab 项模型 · 分页结果
├── resources/icons.xaml              # 自定义图标资源
├── template/                         # MQ 服务端 ViewModel 基类模板
├── view/                             # 12 个功能页面（Daq / Mq / 通信 / OPC UA / SVG / GIF / About…）
└── viewModel/                        # 11 个页面 ViewModel
```

## 📚 资源与社区

| 渠道 | 链接 |
|------|------|
| 🌐 **官方网站** | [snet.cn](https://snet.cn) |
| 📦 **NuGet 包** | [Snet.Core](https://www.nuget.org/packages/Snet.Core) |
| 🔌 **数采工具 Daq** | [github.com/shunnet/Daq](https://github.com/shunnet/Daq) |
| 🐛 **Issues** | [GitHub Issues](https://github.com/shunnet/Debug/issues) — 反馈 Bug 或功能建议 |
| 💬 **QQ 群** | [点击加群](https://qm.qq.com/q/gPjrD9wGty) — 技术交流与问答 |
| ⭐ **Star** | 如果这个项目对你有帮助，请点亮 Star 支持我们 ❤️ |

## 🙏 致谢

- [Snet.cn](https://snet.cn) — 工业通信库
- [Snet.Windows.Controls](https://github.com/shunnet/WpfMUI) — WPF 控件库（含日志编辑高亮）
- [ScottPlot](https://scottplot.net) — 科学图表库
- [WPF UI](https://github.com/lepoco/wpfui) — 现代化 UI 组件
- [Material Design In XAML](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit) — 主题与控件
- [FFmpeg](https://ffmpeg.org) — 视频转换

## 📜 License

![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)

本项目基于 **MIT** 开源协议 —— 自由使用、修改、分发。

📄 完整条款请阅读 [LICENSE](LICENSE) 文件。

> ⚠️ 软件按「原样」提供，作者不对使用后果承担责任。

## 📈 Star History

<a href="https://www.star-history.com/?repos=shunnet%2FDebug&type=date&legend=bottom-right">
 <picture>
   <source media="(prefers-color-scheme: dark)" srcset="https://api.star-history.com/chart?repos=shunnet/Debug&type=date&theme=dark&legend=bottom-right&sealed_token=kU93MuJXyoYfwuhgp01iZFRkdxu_dYO4cZ4R1-p5-nKUKrV2nktH5HS4LkrGQS3PB0ArdiIIm3Q-NzduUGCAu6STsdDbjorxl942Dks2clVxxO0vKqKBAQ"/>
   <source media="(prefers-color-scheme: light)" srcset="https://api.star-history.com/chart?repos=shunnet/Debug&type=date&legend=bottom-right&sealed_token=kU93MuJXyoYfwuhgp01iZFRkdxu_dYO4cZ4R1-p5-nKUKrV2nktH5HS4LkrGQS3PB0ArdiIIm3Q-NzduUGCAu6STsdDbjorxl942Dks2clVxxO0vKqKBAQ"/>
   <img alt="Star History Chart" src="https://api.star-history.com/chart?repos=shunnet/Debug&type=date&legend=bottom-right&sealed_token=kU93MuJXyoYfwuhgp01iZFRkdxu_dYO4cZ4R1-p5-nKUKrV2nktH5HS4LkrGQS3PB0ArdiIIm3Q-NzduUGCAu6STsdDbjorxl942Dks2clVxxO0vKqKBAQ"/>
 </picture>
</a>
