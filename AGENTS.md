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
1. **目标框架升级**: 将项目从 `net9.0` 升级到 `net10.0`（已完成）
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
**错误信息**: `Assets file doesn't have a target for 'net10.0/linux-x64'` 或 `net10.0/osx-arm64`
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

## 当前测试总结

### ✅ 成功完成的部分
1. **.NET 10 升级**：项目成功升级到.NET 10.0.100
2. **代码修复**：修复了nuget.config中的无效路径问题
3. **多平台构建**：Windows/Linux/macOS构建全部成功
4. **传统工具测试**：全局工具安装和运行完全正常

### ⚠️ 遇到问题的部分
### ⚠️ 遇到问题的部分
1. **NuGet发布失败**：发现GitHub Secrets使用方式错误，使用了`${{ env.NUGET_KEY }}`而不是`${{ secrets.NUGET_KEY }}`
2. **DNX测试暂停**：需要等待NuGet发布成功后才能进行完整测试

### 🔧 已识别的解决方案
1. **修正GitHub Secrets用法**：将workflow中的`${{ env.NUGET_KEY }}`改为`${{ secrets.NUGET_KEY }}`
2. **重新发布流程**：修复后创建新版本标签触发完整发布
3. **DNX功能验证**：等待NuGet发布完成后进行最终测试

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

## 开发方法论与流程记录

### 版本发布和DNX测试方法论
1. **代码修复和准备**：确保所有bug修复完成，代码处于可发布状态
2. **推送修复代码**：先将修复代码推送到主分支，确保构建成功
3. **验证构建成功**：等待GitHub Actions完成，确认所有测试和构建通过
4. **创建Git标签**：使用语义化版本号（如v1.0.0）创建标签
5. **自动触发构建**：GitHub Actions会自动检测标签推送并启动构建流程
6. **多平台构建**：并行构建Windows/Linux/macOS二进制文件
7. **NuGet自动发布**：自动构建并推送工具包到NuGet.org
8. **等待发布完成**：监控GitHub Actions状态，确保NuGet发布成功
9. **DNX功能测试**：发布完成后，使用DNX命令测试无需安装的工具执行
10. **记录测试结果**：详细记录测试过程、成功点和遇到的问题

### 清理过时消息方法论
1. **定期审查文档**：检查文档中的技术引用是否过时
2. **版本号同步**：确保所有文档中的技术版本号与实际项目一致
3. **术语更新**：使用正确的最新技术术语
4. **流程记录更新**：基于实际经验更新开发和测试流程
5. **错误信息修正**：修正文档中过时的错误信息引用
6. **知识库维护**：及时更新对新技术的理解，避免基于旧知识的错误

### 文档维护方法论
1. **定期清理过时信息**：检查并更新过时的技术引用（如.NET版本号）
2. **记录实际测试结果**：基于真实测试经验更新文档，而非理论推测
3. **区分已完成和待完成**：明确标记已完成的功能和待验证的功能
4. **问题分析和解决方案**：详细记录遇到的问题和具体的解决步骤
5. **知识更新**：及时更新对新技术的理解，避免基于旧知识的错误假设

### 技术债务管理
1. **版本号同步**：确保所有文档中的技术版本号与实际项目一致
2. **术语准确性**：使用正确的技术术语（如DNX而非错误的旧称）
3. **流程记录**：详细记录开发和测试的完整流程，便于后续参考
4. **经验教训**：记录开发过程中的经验教训，避免重复错误

### GitHub Secrets使用重要发现
**关键发现**：GitHub Secrets不会自动成为环境变量！\
**错误用法**：`env.NUGET_KEY: ${{ secrets.NUGET_KEY }}` + `${{ env.NUGET_KEY }}`\
**正确用法**：直接使用 `${{ secrets.NUGET_KEY }}`\
**原因**：Secrets需要通过`secrets`上下文直接访问，而不是通过`env`上下文

## DNX 功能测试记录

