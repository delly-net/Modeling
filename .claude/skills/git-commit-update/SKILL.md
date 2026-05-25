---
name: git-commit-update
description: 自动暂存更改、生成 git 提交消息并提交推送
effort: low
user-invocable: true
disable-model-invocation: true
agent: Explore
context: fork
allowed-tools: Read, Write, Bash
---

# Git 自动提交
# 命令：/git-commit-update

# ====================== 核心逻辑 ======================
# 1. 升级版本号：yyyy.MM.修订号，取最大修订号+1，年份月份使用当前时间
# 2. 自动提交代码
# 3. 中文返回结果
# ======================================================

echo "=== 开始执行 Git 自动提交任务 ==="

# 1. 获取当前年份、月份
CURRENT_YEAR=$(date +%Y)
CURRENT_MONTH=$(date +%m)

# 2. 查找所有 .csproj 文件
CSPROJ_FILES=$(find . -name "*.csproj" -type f)
if [ -z "$CSPROJ_FILES" ]; then
  echo "❌ 错误：未找到任何 .csproj 文件"
  exit 1
fi

# 3. 提取最大修订号
MAX_REVISION=0
for file in $CSPROJ_FILES; do
  VERSION=$(grep -oP '<Version>\K[^<]+' "$file" 2>/dev/null | head -1)
  if [ -n "$VERSION" ]; then
    IFS='.' read -r y m r <<< "$VERSION"
    if [[ $r =~ ^[0-9]+$ ]] && (( r > MAX_REVISION )); then
      MAX_REVISION=$r
    fi
  fi
done

# 4. 生成新版本号
NEW_REVISION=$((MAX_REVISION + 1))
NEW_VERSION="${CURRENT_YEAR}.${CURRENT_MONTH}.${NEW_REVISION}"

# 5. 批量更新所有 .csproj 版本号
for file in $CSPROJ_FILES; do
  sed -i "s/<Version>.*<\/Version>/<Version>${NEW_VERSION}<\/Version>/g" "$file"
done

echo "✅ 版本号更新完成：${NEW_VERSION}"

# 6. 执行 Git 提交
echo "📤 正在提交代码..."
COMMIT_RESULT=$(/eazy-git-commit)

# 7. 输出最终结果
echo -e "\n=== 执行完成 ==="
echo "新版本号：${NEW_VERSION}"
echo -e "提交结果：\n${COMMIT_RESULT}"
echo "✅ 自动提交任务已全部结束"