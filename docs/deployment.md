# 部署指南

## 发布流程

### 1. 本地测试

```bash
# 运行单元测试
dotnet test

# 构建所有平台
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
dotnet publish -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true  
dotnet publish -c Release -r osx-x64 --self-contained -p:PublishSingleFile=true
```

### 2. 创建标签

```bash
# 创建新版本标签
git tag v1.0.0
git push origin v1.0.0
```

### 3. GitHub Actions 自动发布

推送标签后，GitHub Actions 会自动：
- 运行多平台测试
- 构建各平台二进制文件
- 发布到 NuGet.org
- 创建 GitHub Release 草稿
- 上传压缩包和校验和

### 4. 手动发布（备用）

如果需要手动发布：

```bash
# 创建发布目录
mkdir -p releases

# Windows
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o releases/win-x64
cd releases/win-x64 && zip -r ../../aniorg-win-x64.zip * && cd ../..

# Linux  
dotnet publish -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true -o releases/linux-x64
cd releases/linux-x64 && tar -czf ../../aniorg-linux-x64.tar.gz * && cd ../..

# macOS
dotnet publish -c Release -r osx-x64 --self-contained -p:PublishSingleFile=true -o releases/osx-x64
cd releases/osx-x64 && tar -czf ../../aniorg-osx-x64.tar.gz * && cd ../..

# 生成校验和
cd releases && sha256sum *.zip *.tar.gz > checksums.txt
```

## 安装说明

### 方式一：.NET 全局工具安装（推荐）
```bash
# 安装最新版本
dotnet tool install --global AnimeOrganizer.NET

# 安装特定版本
dotnet tool install --global AnimeOrganizer.NET --version 1.0.0

# 更新到最新版本
dotnet tool update --global AnimeOrganizer.NET
```

安装后可以直接使用：
```bash
aniorg --help
```

### 方式二：下载二进制文件

##### Windows
1. 下载 `aniorg-win-x64.zip`
2. 解压到任意目录
3. 将目录添加到 PATH 环境变量（可选）
4. 在命令行运行 `aniorg --help`

##### Linux
```bash
# 下载并解压
wget https://github.com/ModerRAS/AnimeOrganizer.NET/releases/latest/download/aniorg-linux-x64.tar.gz
tar -xzf aniorg-linux-x64.tar.gz

# 移动到系统目录（可选）
sudo mv aniorg /usr/local/bin/
sudo chmod +x /usr/local/bin/aniorg

# 运行
aniorg --help
```

#### macOS
```bash
# 下载并解压  
curl -L -o aniorg-osx-x64.tar.gz https://github.com/ModerRAS/AnimeOrganizer.NET/releases/latest/download/aniorg-osx-x64.tar.gz
tar -xzf aniorg-osx-x64.tar.gz

# 移动到系统目录（可选）
sudo mv aniorg /usr/local/bin/
sudo chmod +x /usr/local/bin/aniorg

# 运行
aniorg --help
```

## 验证安装

```bash
# 检查版本
aniorg --help

# 测试功能
mkdir -p test_source test_target
echo "test" > "test_source/[ANi] 测试 - 01 [1080P].mp4"
aniorg --source=test_source --target=test_target --dry-run
```

## 故障排除

### 权限问题
```bash
# Linux/macOS
chmod +x aniorg

# 或移动到系统目录
sudo mv aniorg /usr/local/bin/
```

### 依赖问题
工具为自包含发布，无需额外依赖。如遇到问题：
1. 检查系统架构是否匹配
2. 验证下载文件完整性（使用校验和）
3. 尝试使用 `--verbose` 参数获取详细错误信息

### 运行时错误
- **硬链接失败**: 确保源和目标在同一文件系统
- **权限被拒绝**: 检查文件和目录权限
- **文件不存在**: 验证路径是否正确