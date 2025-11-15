#!/bin/bash
# 端到端测试脚本 - Unix/Linux/macOS

set -e  # 遇到错误立即退出

echo "=== AnimeOrganizer.NET 端到端测试 ==="

# 设置测试目录
TEST_DIR="/tmp/aniorg-e2e-test-$$"
SOURCE_DIR="$TEST_DIR/source"
TARGET_DIR="$TEST_DIR/target"

# 创建测试目录
mkdir -p "$SOURCE_DIR"
mkdir -p "$TARGET_DIR"

# 创建测试文件
cat > "$SOURCE_DIR/[ANi] 间谍过家家 - 01 [1080P].mp4" << 'EOF'
测试内容 for [ANi] 间谍过家家 - 01 [1080P].mp4
EOF

cat > "$SOURCE_DIR/[ANi] 间谍过家家 - 02 [1080P].mp4" << 'EOF'
测试内容 for [ANi] 间谍过家家 - 02 [1080P].mp4
EOF

cat > "$SOURCE_DIR/[SubsPlease] 进击的巨人 - 01 [1080p].mkv" << 'EOF'
测试内容 for [SubsPlease] 进击的巨人 - 01 [1080p].mkv
EOF

cat > "$SOURCE_DIR/[EMBER] 鬼灭之刃 - 01 [1080p][Multiple Subtitle].avi" << 'EOF'
测试内容 for [EMBER] 鬼灭之刃 - 01 [1080p][Multiple Subtitle].avi
EOF

echo "✓ 测试文件创建完成"

# 获取项目根目录
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

# 构建项目
echo "=== 构建项目 ==="
# 使用环境变量中的dotnet命令
if dotnet build --configuration Release "$PROJECT_ROOT"; then
    echo "✓ 构建成功"
else
    echo "✗ 构建失败"
    exit 1
fi

# 测试1: DNX 模式 (dotnet run)
echo "=== 测试 DNX 模式 ==="
if dotnet run --project "$PROJECT_ROOT/src/AnimeOrganizer.csproj" -- --source="$SOURCE_DIR" --target="$TARGET_DIR" --mode=copy; then
    echo "✓ DNX 模式测试通过"
else
    echo "✗ DNX 模式测试失败"
fi

# 验证文件
EXPECTED_FILES=(
    "间谍过家家/01 [1080P].mp4"
    "间谍过家家/02 [1080P].mp4"
    "进击的巨人/01 [1080p].mkv"
    "鬼灭之刃/01 [1080p][Multiple Subtitle].avi"
)

ALL_FILES_EXIST=true
for expected_file in "${EXPECTED_FILES[@]}"; do
    full_path="$TARGET_DIR/$expected_file"
    if [[ -f "$full_path" ]]; then
        echo "✓ 找到文件: $expected_file"
    else
        echo "✗ 缺失文件: $expected_file"
        ALL_FILES_EXIST=false
    fi
done

# 测试2: 命令行参数
echo "=== 测试命令行参数 ==="

# 测试帮助
HELP_RESULT=$(dotnet run --project "$PROJECT_ROOT/src/AnimeOrganizer.csproj" -- --help)
if echo "$HELP_RESULT" | grep -q "用法:"; then
    echo "✓ 帮助命令测试通过"
else
    echo "✗ 帮助命令测试失败"
fi

# 测试版本
VERSION_RESULT=$(dotnet run --project "$PROJECT_ROOT/src/AnimeOrganizer.csproj" -- --version)
if echo "$VERSION_RESULT" | grep -q "AnimeOrganizer.NET v"; then
    echo "✓ 版本命令测试通过"
else
    echo "✗ 版本命令测试失败"
fi

# 测试3: 全局工具模式 (如果可能)
echo "=== 测试全局工具模式 ==="

# 尝试打包为工具
if dotnet pack "$PROJECT_ROOT/src/AnimeOrganizer.csproj" --configuration Release --output "$TEST_DIR/nupkg" -p:PackAsTool=true; then
    echo "✓ 工具包创建成功"
    
    # 尝试本地安装工具进行测试
    if dotnet tool install --local --add-source "$TEST_DIR/nupkg" AnimeOrganizer.NET 2>/dev/null; then
        echo "✓ 工具本地安装成功"
        
        # 运行工具
        if dotnet aniorg --source="$SOURCE_DIR" --target="$TARGET_DIR" --mode=copy; then
            echo "✓ 工具运行测试通过"
        else
            echo "✗ 工具运行测试失败"
        fi
        
        # 卸载工具
        dotnet tool uninstall --local AnimeOrganizer.NET >/dev/null 2>&1
    else
        echo "⚠ 工具本地安装失败 (非关键错误)"
    fi
else
    echo "⚠ 工具包创建失败 (非关键错误)"
fi

# 测试4: 二进制模式
echo "=== 测试二进制模式 ==="

# 检测操作系统
OS=$(uname -s)
case "$OS" in
    Linux*)     RID="linux-x64";;
    Darwin*)    RID="osx-x64";;
    *)          echo "不支持的操作系统: $OS"; exit 1;;
esac

# 发布单文件
if dotnet publish "$PROJECT_ROOT/src/AnimeOrganizer.csproj" --configuration Release --runtime "$RID" --self-contained true --output "$TEST_DIR/publish"; then
    echo "✓ 单文件发布成功"
    
    BINARY_PATH="$TEST_DIR/publish/aniorg"
    if [[ -f "$BINARY_PATH" ]]; then
        echo "✓ 二进制文件存在"
        
        # 清理目标目录
        rm -rf "$TARGET_DIR"
        mkdir -p "$TARGET_DIR"
        
        # 运行二进制文件
        if "$BINARY_PATH" --source="$SOURCE_DIR" --target="$TARGET_DIR" --mode=copy; then
            echo "✓ 二进制文件运行测试通过"
        else
            echo "✗ 二进制文件运行测试失败"
        fi
    else
        echo "✗ 二进制文件不存在"
    fi
else
    echo "✗ 单文件发布失败"
fi

# 清理
echo "=== 清理测试文件 ==="
rm -rf "$TEST_DIR"
echo "✓ 测试清理完成"

echo "=== 端到端测试完成 ==="