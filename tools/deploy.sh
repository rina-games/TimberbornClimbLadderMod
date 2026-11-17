#!/usr/bin/env bash
# 把仓库里的 Buildings / Blueprints / Localizations / Scripts / manifest 同步到游戏的 mods 目录。
# 用法： tools/deploy.sh
set -euo pipefail

REPO="$(cd "$(dirname "$0")/.." && pwd)"
DEST="${MOD_DEST:-$HOME/Documents/Timberborn/Mods/ClimbLadder}"

mkdir -p "$DEST"
# 建筑蓝图必须在 mod 根目录的 Buildings/ 下（放进 Blueprints/ 会 Blueprint not found）
rsync -a --delete "$REPO/Buildings/" "$DEST/Buildings/"
rsync -a --delete "$REPO/Blueprints/" "$DEST/Blueprints/"
rsync -a --delete "$REPO/Localizations/" "$DEST/Localizations/"
rsync -a --delete "$REPO/Sprites/" "$DEST/Sprites/"
mkdir -p "$DEST/Scripts"
if [ -f "$REPO/Scripts/Jove.ClimbLadder.dll" ]; then
  cp "$REPO/Scripts/Jove.ClimbLadder.dll" "$DEST/Scripts/Jove.ClimbLadder.dll"
fi
cp "$REPO/manifest.json" "$DEST/manifest.json"
if [ -f "$REPO/thumbnail.png" ]; then
  cp "$REPO/thumbnail.png" "$DEST/thumbnail.png"
fi

python3 "$REPO/tools/check-localization.py"

echo "deployed -> $DEST"
find "$DEST" -type f | sort
