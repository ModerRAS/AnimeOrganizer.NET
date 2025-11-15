# AnimeOrganizer.NET

轻量级、跨平台命令行工具，专为动漫收藏者批量整理视频文件，支持硬链接模式实现零额外空间占用。

## 🚀 功能特性

- **智能解析**: 自动识别 `[发布组] 动漫名 - 集数 [标签].ext` 格式
- **灵活整理**: 重构为 `动漫名/集数 [标签].ext` 结构
- **多种模式**: 支持移动、复制、硬链接三种操作模式
- **跨平台**: 支持 Windows、Linux、macOS
- **零依赖**: 单文件部署，无需外部配置
- **高性能**: 单线程处理，1000个文件扫描+解析<5秒

## 📥 下载安装

### 方式一：.NET 全局工具安装（推荐）
```bash
# 安装最新版本
dotnet tool install --global AnimeOrganizer.NET

# 安装特定版本
dotnet tool install --global AnimeOrganizer.NET --version 1.0.0

# 更新到最新版本  
dotnet tool update --global AnimeOrganizer.NET
```

### 方式二：下载二进制文件
从 [GitHub Releases](https://github.com/ModerRAS/AnimeOrganizer.NET/releases) 下载对应平台的二进制文件：

- **Windows**: `aniorg-win-x64.zip`
- **Linux**: `aniorg-linux-x64.tar.gz`
- **macOS**: `aniorg-osx-x64.tar.gz`

下载后解压即可使用，无需安装。

## 🎯 快速开始

### 基本用法

```bash
# 移动模式（默认）
aniorg --source="/path/to/downloads"

# 硬链接模式（推荐，零额外空间）
aniorg --source="/path/to/downloads" --mode=link --target="/path/to/anime"

# 复制模式
aniorg --source="/path/to/downloads" --mode=copy --target="/path/to/anime"
```

### 预览模式

在实际操作前先预览变更：

```bash
aniorg --source="/path/to/downloads" --dry-run --verbose
```

## 📋 参数说明

| 参数 | 类型 | 必填 | 默认值 | 说明 |
|------|------|------|--------|------|
| `--source` | string | ✅ | - | 源目录路径 |
| `--target` | string | ❌ | source | 目标根目录 |
| `--mode` | enum | ❌ | move | 操作模式：move/copy/link |
| `--dry-run` | bool | ❌ | false | 仅预览不执行 |
| `--include-ext` | string | ❌ | mp4,mkv,avi,mov,wmv,flv,rmvb | 处理的扩展名 |
| `--verbose` | bool | ❌ | false | 显示详细日志 |
| `--help` | bool | ❌ | false | 显示帮助 |

## 🎨 文件命名格式

### 支持的源文件名格式

```
[发布组] 动漫名称（可含季度） - 集数 [标签信息].扩展名
```

示例：
- `[ANi] 妖怪旅館營業中 貳 - 07 [1080P][Baha][WEB-DL][AAC AVC][CHT].mp4`
- `[SubsPlease] 间谍过家家 - 12 [1080p].mkv`
- `[EMBER] 进击的巨人 The Final Season - 01 [1080p][Multiple Subtitle].avi`

### 目标文件结构

```
动漫名称（含季度）/
├── 01 [标签信息].扩展名
├── 02 [标签信息].扩展名
└── ...
```

示例：
```
妖怪旅館營業中 貳/
├── 07 [1080P][Baha][WEB-DL][AAC AVC][CHT].mp4
├── 08 [1080P][Baha][WEB-DL][AAC AVC][CHT].mp4
└── ...
```

## 🔗 硬链接说明

硬链接是推荐的整理方式，具有以下优势：

- **零额外空间**: 不占用额外磁盘空间
- **快速操作**: 几乎瞬间完成
- **文件同步**: 源文件和目标文件内容完全同步

### 使用条件

1. **同一文件系统**: 源文件和目标必须在同一分区/NAS卷
2. **文件系统支持**: ext4、NTFS、APFS 等均支持
3. **权限要求**: 需要对源和目标目录有写入权限

### 跨设备错误

如果源文件和目标不在同一文件系统，会显示错误：
```
硬链接失败：源文件和目标必须在同一文件系统
```

此时可选择：
- 将目标目录改为与源文件同一文件系统
- 使用复制模式 (`--mode=copy`)
- 使用移动模式 (`--mode=move`)

## 💡 使用示例

### 基本整理
```bash
# 整理下载目录
aniorg --source="D:\Downloads\Anime"

# 整理到指定目录
aniorg --source="/home/user/Downloads" --target="/media/anime"
```

### 硬链接模式
```bash
# Windows
aniorg --source="D:\Downloads" --mode=link --target="E:\Anime"

# Linux/macOS
aniorg --source="/home/user/Downloads" --mode=link --target="/mnt/anime"
```

### 预览和调试
```bash
# 预览变更
aniorg --source="/path/to/downloads" --dry-run

# 显示详细日志
aniorg --source="/path/to/downloads" --verbose

# 指定文件类型
aniorg --source="/path/to/downloads" --include-ext="mp4,mkv"
```

### 递归处理
默认递归处理所有子目录：
```bash
# 处理downloads及其所有子目录
aniorg --source="/path/to/downloads"
```

## 🛠️ 构建开发

### 环境要求

- .NET 9.0 SDK
- Git

### 本地构建

```bash
# 克隆仓库
git clone https://github.com/ModerRAS/AnimeOrganizer.NET.git
cd AnimeOrganizer.NET

# 还原依赖
dotnet restore

# 构建项目
dotnet build --configuration Release

# 运行测试
dotnet test

# 构建NuGet包
dotnet pack src/AnimeOrganizer.csproj --configuration Release -p:PackAsTool=true

# 发布单文件
dotnet publish src/AnimeOrganizer.csproj \
  --configuration Release \
  --runtime win-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:PackAsTool=false
```

### 多平台构建

```bash
# Windows
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true

# Linux
dotnet publish -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true

# macOS
dotnet publish -c Release -r osx-x64 --self-contained -p:PublishSingleFile=true
```

## 🧪 测试

项目包含完整的单元测试：

```bash
# 运行所有测试
dotnet test

# 运行指定测试
dotnet test --filter "FullyQualifiedName~FileParserTests"

# 生成测试报告
dotnet test --logger "trx;LogFileName=test-results.trx"
```

## 📈 性能

- **扫描速度**: 1000个文件扫描+解析 < 5秒
- **内存使用**: < 50MB（1000个文件处理）
- **CPU使用**: 单线程，低CPU占用

## 🔧 故障排除

### 常见问题

**Q: 硬链接失败，提示"必须在同一文件系统"**
A: 源文件和目标目录必须在同一分区/NAS卷，检查文件系统挂载点。

**Q: 权限被拒绝**
A: 确保对源文件和目标目录有读写权限。

**Q: 文件未被识别**
A: 检查文件名格式是否符合 `[发布组] 动漫名 - 集数 [标签].ext` 格式。

**Q: 如何处理特殊字符？**
A: 工具会自动处理文件名中的特殊字符，无需额外配置。

### 调试模式

使用 `--verbose` 参数获取详细日志：
```bash
aniorg --source="/path/to/downloads" --verbose
```

## 📄 许可证

GNU Affero General Public License v3.0 (AGPL-3.0) - 详见 [LICENSE](LICENSE) 文件

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

## 📞 联系方式

- GitHub: [@ModerRAS](https://github.com/ModerRAS)
- 项目仓库: [AnimeOrganizer.NET](https://github.com/ModerRAS/AnimeOrganizer.NET)

---

**Made with ❤️ for anime collectors**