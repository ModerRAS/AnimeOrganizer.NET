#!/usr/bin/env pwsh
# 端到端测试脚本 - Windows PowerShell

Write-Host "=== AnimeOrganizer.NET 端到端测试 ===" -ForegroundColor Green

# 设置测试目录
$testDir = Join-Path $env:TEMP "aniorg-e2e-test-$(Get-Random)"
$sourceDir = Join-Path $testDir "source"
$targetDir = Join-Path $testDir "target"

# 创建测试目录
New-Item -ItemType Directory -Path $testDir -Force | Out-Null
New-Item -ItemType Directory -Path $sourceDir -Force | Out-Null
New-Item -ItemType Directory -Path $targetDir -Force | Out-Null

# 创建测试文件
$testFiles = @(
    "[ANi] 间谍过家家 - 01 [1080P].mp4",
    "[ANi] 间谍过家家 - 02 [1080P].mp4", 
    "[SubsPlease] 进击的巨人 - 01 [1080p].mkv",
    "[EMBER] 鬼灭之刃 - 01 [1080p][Multiple Subtitle].avi"
)

foreach ($file in $testFiles) {
    $filePath = Join-Path $sourceDir $file
    "测试内容 for $file" | Out-File -FilePath $filePath -Encoding UTF8
}

Write-Host "✓ 测试文件创建完成" -ForegroundColor Green

# 获取项目根目录
$projectRoot = Split-Path -Parent $PSScriptRoot
$projectRoot = Split-Path -Parent $projectRoot

# 构建项目
Write-Host "=== 构建项目 ===" -ForegroundColor Yellow
$buildResult = dotnet build --configuration Release $projectRoot
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ 构建失败" -ForegroundColor Red
    exit 1
}
Write-Host "✓ 构建成功" -ForegroundColor Green

# 测试1: DNX 模式 (dotnet run)
Write-Host "=== 测试 DNX 模式 ===" -ForegroundColor Yellow
$dnxResult = dotnet run --project "$projectRoot/src/AnimeOrganizer.csproj" -- --source="$sourceDir" --target="$targetDir" --mode=copy
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ DNX 模式测试通过" -ForegroundColor Green
} else {
    Write-Host "✗ DNX 模式测试失败" -ForegroundColor Red
}

# 验证文件
$expectedFiles = @(
    "间谍过家家/01 [1080P].mp4",
    "间谍过家家/02 [1080P].mp4",
    "进击的巨人/01 [1080p].mkv", 
    "鬼灭之刃/01 [1080p][Multiple Subtitle].avi"
)

$allFilesExist = $true
foreach ($expectedFile in $expectedFiles) {
    $fullPath = Join-Path $targetDir $expectedFile
    if (Test-Path $fullPath) {
        Write-Host "✓ 找到文件: $expectedFile" -ForegroundColor Green
    } else {
        Write-Host "✗ 缺失文件: $expectedFile" -ForegroundColor Red
        $allFilesExist = $false
    }
}

# 测试2: 命令行参数
Write-Host "=== 测试命令行参数 ===" -ForegroundColor Yellow

# 测试帮助
$helpResult = dotnet run --project "$projectRoot/src/AnimeOrganizer.csproj" -- --help
if ($helpResult -like "*用法:*") {
    Write-Host "✓ 帮助命令测试通过" -ForegroundColor Green
} else {
    Write-Host "✗ 帮助命令测试失败" -ForegroundColor Red
}

# 测试版本
$versionResult = dotnet run --project "$projectRoot/src/AnimeOrganizer.csproj" -- --version
if ($versionResult -like "*AnimeOrganizer.NET v*") {
    Write-Host "✓ 版本命令测试通过" -ForegroundColor Green
} else {
    Write-Host "✗ 版本命令测试失败" -ForegroundColor Red
}

# 测试3: 全局工具模式 (如果可能)
Write-Host "=== 测试全局工具模式 ===" -ForegroundColor Yellow

# 尝试打包为工具
$packResult = dotnet pack "$projectRoot/src/AnimeOrganizer.csproj" --configuration Release --output "$testDir/nupkg" -p:PackAsTool=true
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ 工具包创建成功" -ForegroundColor Green
    
    # 尝试本地安装工具进行测试
    $localToolResult = dotnet tool install --local --add-source "$testDir/nupkg" AnimeOrganizer.NET
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ 工具本地安装成功" -ForegroundColor Green
        
        # 运行工具
        $toolRunResult = dotnet aniorg --source="$sourceDir" --target="$targetDir" --mode=copy
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ 工具运行测试通过" -ForegroundColor Green
        } else {
            Write-Host "✗ 工具运行测试失败" -ForegroundColor Red
        }
        
        # 卸载工具
        dotnet tool uninstall --local AnimeOrganizer.NET | Out-Null
    } else {
        Write-Host "⚠ 工具本地安装失败 (非关键错误)" -ForegroundColor Yellow
    }
} else {
    Write-Host "⚠ 工具包创建失败 (非关键错误)" -ForegroundColor Yellow
}

# 测试4: 二进制模式
Write-Host "=== 测试二进制模式 ===" -ForegroundColor Yellow

# 发布单文件
$publishResult = dotnet publish "$projectRoot/src/AnimeOrganizer.csproj" --configuration Release --runtime win-x64 --self-contained true --output "$testDir/publish"
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ 单文件发布成功" -ForegroundColor Green
    
    $binaryPath = Join-Path "$testDir/publish" "aniorg.exe"
    if (Test-Path $binaryPath) {
        Write-Host "✓ 二进制文件存在" -ForegroundColor Green
        
        # 清理目标目录
        Remove-Item -Recurse -Force $targetDir
        New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
        
        # 运行二进制文件
        $binaryResult = & $binaryPath --source="$sourceDir" --target="$targetDir" --mode=copy
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ 二进制文件运行测试通过" -ForegroundColor Green
        } else {
            Write-Host "✗ 二进制文件运行测试失败" -ForegroundColor Red
        }
    } else {
        Write-Host "✗ 二进制文件不存在" -ForegroundColor Red
    }
} else {
    Write-Host "✗ 单文件发布失败" -ForegroundColor Red
}

# 清理
Write-Host "=== 清理测试文件 ===" -ForegroundColor Yellow
Remove-Item -Recurse -Force $testDir
Write-Host "✓ 测试清理完成" -ForegroundColor Green

Write-Host "=== 端到端测试完成 ===" -ForegroundColor Green