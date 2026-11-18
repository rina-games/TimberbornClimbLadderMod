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

# 游戏若订阅了创意工坊里的同一份模组，会加载工坊目录而不是上面的本地目录。
WORKSHOP_ROOT="$HOME/Library/Application Support/Steam/steamapps/workshop/content/1062090"
if [ -d "$WORKSHOP_ROOT" ]; then
  while IFS= read -r manifest; do
    id="$(python3 -c 'import json,sys; print(json.load(open(sys.argv[1])).get("Id",""))' "$manifest")"
    if [ "$id" = "Jove.ClimbLadder" ]; then
      workshop="$(dirname "$manifest")"
      rsync -a --delete "$REPO/Buildings/" "$workshop/Buildings/"
      rsync -a --delete "$REPO/Blueprints/" "$workshop/Blueprints/"
      rsync -a --delete "$REPO/Localizations/" "$workshop/Localizations/"
      rsync -a --delete "$REPO/Sprites/" "$workshop/Sprites/"
      mkdir -p "$workshop/Scripts"
      if [ -f "$REPO/Scripts/Jove.ClimbLadder.dll" ]; then
        cp "$REPO/Scripts/Jove.ClimbLadder.dll" "$workshop/Scripts/Jove.ClimbLadder.dll"
      fi
      cp "$REPO/manifest.json" "$workshop/manifest.json"
      if [ -f "$REPO/thumbnail.png" ]; then
        cp "$REPO/thumbnail.png" "$workshop/thumbnail.png"
      fi
      echo "deployed -> $workshop"
    fi
  done < <(find "$WORKSHOP_ROOT" -name manifest.json -print)
fi
