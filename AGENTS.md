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