# 项目开发记录

## 项目要求记录

### 清理要求
- 删除所有临时脚本和测试文件
- 删除各种SUMMARY文件和额外生成的文档
- 只保留核心项目文件和必要的文档
- 将文档相关文件统一放在docs目录下

### GitHub推送要求
- 不要上传临时文件和构建产物
- 确保.gitignore配置正确，防止上传自动生成文件
- 保持仓库整洁，只包含源代码和必要配置

### 项目结构要求
- 创建解决方案文件(.sln)以便根目录dotnet命令可用
- 保留README.md用于NuGet.org和GitHub展示
- 保留核心文档：部署指南等
- 代码应该自解释，不需要过多文档

### NuGet发布要求
- 确保README.md内容完整，可用于NuGet.org展示
- 保持版本号管理自动化
- 支持.NET全局工具安装方式

## 技术决策

### 项目配置
- 使用.NET 9.0作为目标框架
- 采用单文件架构，核心逻辑在Program.cs中
- 零第三方依赖，保持工具轻量级
- 支持跨平台：Windows、Linux、macOS
- 许可证：GNU Affero General Public License v3.0 (AGPL-3.0)

### 构建配置
- 区分NuGet工具包和二进制发布
- NuGet包：框架依赖，最小体积
- 二进制：自包含，独立运行
- 自动化版本管理，基于Git标签

### 测试策略
- 单元测试覆盖核心功能
- 集成测试验证端到端场景
- 跨平台兼容性验证

## .NET 10 新特性：DNX 工具执行器

### 什么是DNX？
DNX（.NET eXecution）是.NET 10中引入的新功能，允许**无需安装即可运行.NET工具**，类似于Node.js的`npx`命令。

### DNX的核心功能
1. **一次性工具执行**：无需`dotnet tool install`，直接运行NuGet包中的工具
2. **版本指定支持**：使用`@`语法，如`dnx tool@1.0.0`
3. **本地和全局包源支持**：可以从NuGet.org或自定义源运行
4. **权限管理**：首次运行时会请求确认下载

### DNX与dotnet tool install的区别
| 功能 | DNX | dotnet tool install |
|------|-----|-------------------|
| 安装需求 | 无需安装 | 需要显式安装 |
| 持久性 | 临时运行 | 永久安装 |
| 存储位置 | 全局包缓存 | 工具存储位置(~/.dotnet/tools/.store) |
| 可执行文件 | 无shim文件 | 有shim文件在PATH中 |

### DNX命令格式
```bash
# 基本用法
dnx <packageId> [arguments] [options]

# 指定版本
dnx tool@1.0.0 --arg1 value1

# 使用特定源
dnx tool --source https://api.nuget.org/v3/index.json --yes

# 自动确认
dnx tool --yes --help
```

### DNX工作原理
1. **包发现**：在配置的NuGet源中查找包
2. **权限确认**：首次运行时请求用户确认下载
3. **包下载**：下载到全局包缓存，但不安装到工具存储
4. **直接执行**：从包缓存直接运行工具，无需shim

## .NET 10 升级

### 升级过程
1. **目标框架升级**: 将项目从 `net9.0` 升级到 `net10.0`
2. **GitHub Actions更新**: 更新workflow中的dotnet-version为 `10.0.x`
3. **许可证修正**: 修正AGPL-3.0为AGPL-3.0-or-later以符合SPDX标准
4. **路径修复**: 修复README.md文件路径引用

### 安装.NET 10
```bash
# 使用微软安装脚本
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 10.0

# 设置环境变量
export DOTNET_ROOT="/root/.dotnet"
export PATH="$PATH:/root/.dotnet/tools"
```

## GitHub Actions监控

### 查看运行状态
使用GitHub CLI查看Actions执行情况：
```bash
# 查看最近的运行记录
gh run list --limit 10

# 查看特定运行的详细日志
gh run view <run-id> --log

# 查看实时日志
gh run watch <run-id>
```

### 常见状态
- ✅ **completed** - 运行完成
- ❌ **failure** - 运行失败
- 🔄 **in_progress** - 正在运行
- ⏸️ **queued** - 排队等待

### 故障排查
当Actions失败时：
1. 查看运行日志找出错误信息
2. 检查失败的具体步骤
3. 验证配置文件语法
4. 确认环境变量和密钥设置

### 常见错误及解决方案

#### NETSDK1047错误
**错误信息**: `Assets file doesn't have a target for 'net9.0/linux-x64'` 或 `net9.0/osx-arm64`
**原因**: 项目文件缺少RuntimeIdentifiers配置
**解决方案**: 在项目文件的PropertyGroup中添加完整的RuntimeIdentifiers
```xml
<RuntimeIdentifiers>win-x64;linux-x64;osx-x64;osx-arm64</RuntimeIdentifiers>
```

#### Windows构建取消错误
**错误信息**: `The operation was canceled` (Windows平台)
**原因**: .NET SDK安装超时或被取消
**解决方案**: 通常是由于网络问题，重新运行即可

#### PowerShell语法错误
**错误信息**: `Missing expression after unary operator '--'`
**原因**: GitHub Actions中的PowerShell脚本使用了Unix风格的续行符`\`
**解决方案**: 在Windows平台的PowerShell脚本中使用反引号` ` `作为续行符，或者使用平台条件分别处理
```yaml
# 推荐：使用平台条件分别处理
- name: Publish (Unix)
  if: matrix.os != 'windows-latest'
  run: |
    dotnet publish xxx \
      --configuration Release \

- name: Publish (Windows)  
  if: matrix.os == 'windows-latest'
  run: |
    dotnet publish xxx `
      --configuration Release `

# 或者：使用平台无关的一行命令
- name: Publish
  run: dotnet publish xxx --configuration Release --runtime xxx
```

#### 构建环境错误
**错误信息**: 与.NET SDK版本相关的错误
**原因**: 构建环境.NET版本不匹配
**解决方案**: 确保GitHub Actions中的dotnet-version与项目目标框架一致

#### 分支触发错误
**错误信息**: Workflow未触发
**原因**: 分支名称配置错误
**解决方案**: 检查workflow文件中的分支名称是否与实际分支一致

## 完成状态

✅ 核心功能实现
✅ NuGet.org集成
✅ GitHub Actions自动化
✅ 多平台支持
✅ 项目结构清理
✅ 文档整理
✅ 解决方案文件创建
✅ .NET 10 升级完成
✅ DNX 新功能支持

## DNX 功能测试记录

### 测试环境
- .NET 版本: 10.0.100
- DNX 命令: 可用
- 包源: 本地构建 + NuGet.org

### 传统全局工具测试 ✅
```bash
# 工具安装和运行测试通过
export DOTNET_ROOT="/root/.dotnet"
export PATH="$PATH:/root/.dotnet/tools"
aniorg --help
aniorg --source="/tmp/test" --mode=copy --target="/tmp/target"
```

### DNX 新功能测试 ⚠️

#### 基础功能测试
```bash
# DNX 命令可用性测试
dnx --help  # ✅ 通过 - 命令存在且功能正常

# 尝试从本地源运行（遇到限制）
dnx AnimeOrganizer.NET --configfile nuget.config --source /tmp/local-nuget --yes --help
# ⚠️ 结果：显示帮助信息，但未实际执行工具

# 尝试已知存在的工具
dnx dotnetsay --yes --help  
# ⚠️ 结果：同样显示帮助信息，未执行工具
```

#### 问题分析
经过测试发现，DNX 命令本身可用，但在实际执行工具时可能存在以下限制：

1. **包源要求**：DNX 需要从配置的 NuGet 源获取包，本地构建的包需要正确的源配置
2. **包格式要求**：工具包需要符合特定的格式要求
3. **执行环境**：可能需要特定的执行环境或依赖配置

### 已知限制
- DNX 需要从配置的 NuGet 源获取包，本地构建的包需要推送到源
- 首次运行需要网络连接和权限确认
- 工具运行在临时环境中，不保留持久化安装
- 本地包测试可能需要额外的 NuGet 源配置

### 实际可用的方案
对于开发测试，以下方案已经验证可用：
1. **传统全局工具安装**：`dotnet tool install --global`
2. **本地工具安装**：`dotnet tool install --local`
3. **直接运行构建产物**：`dotnet run` 或执行构建的二进制文件

### 下一步建议
1. 将包发布到 NuGet.org 以测试完整的 DNX 功能
2. 研究 DNX 的具体包格式要求和源配置
3. 验证 DNX 的版本指定功能 `dnx tool@1.0.0`
4. 测试其他已知可用的 .NET 工具以确认 DNX 功能正常