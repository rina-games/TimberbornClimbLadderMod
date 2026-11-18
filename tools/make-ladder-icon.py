#!/usr/bin/env python3
"""生成攀爬梯的建筑图标 Sprites/Rope/Ladder.png。

画风对齐原版 Timberborn 图标（从游戏 resources.assets 里导出的 PathIcon /
StairsIcon / PlatformIcon 量出来的规则）：

- 画布 112→128 px，单色线稿，颜色 #BBA16C（原版 187,161,108）
- 线宽约为画布的 3.2%（原版 3~4 px / 112）
- 线条圆头圆角，笔画连续
- 线条下方一层纯黑柔和投影：模糊 ~4 px、峰值 alpha ~150、略偏下

用法：
    python3 tools/make-ladder-icon.py            # 写到 Sprites/Rope/Ladder.png
    python3 tools/make-ladder-icon.py --preview  # 额外输出候选到 .preview/
"""

from __future__ import annotations

import argparse
import os

from PIL import Image, ImageDraw, ImageFilter

SIZE = 128
SS = 4  # 超采样倍数
LINE = (187, 161, 108)
SHADOW_ALPHA = 0.72
SHADOW_BLUR = 2.8  # 128 px 空间
SHADOW_OFFSET = (1.2, 2.6)
SCALE = 0.86  # 图形相对画布的缩放（1.0 = 撑满，原版图标约 0.8）


def _round_line(draw: ImageDraw.ImageDraw, points, width: float, color) -> None:
    """画一条带圆头圆角的折线（Pillow 的 joint/端帽在超采样下不够圆，这里补端点圆）。"""
    pts = [(x * SS, y * SS) for x, y in points]
    w = width * SS
    draw.line(pts, fill=color, width=int(round(w)), joint="curve")
    r = w / 2
    for x, y in (pts[0], pts[-1]):
        draw.ellipse([x - r, y - r, x + r, y + r], fill=color)


def _canvas() -> Image.Image:
    im = Image.new("RGBA", (SIZE * SS, SIZE * SS), (0, 0, 0, 0))
    return im


def draw_ladder(variant: str = "f") -> Image.Image:
    """在超采样画布上画线稿，返回 alpha 蒙版（L 模式）。"""
    im = _canvas()
    d = ImageDraw.Draw(im)

    if variant == "a":
        # 正视攀爬梯：两根竖轨 + 四根横档，横档比竖轨细一点
        rail_l, rail_r = 46.0, 82.0
        top, bottom = 14.0, 114.0
        rail_w = 4.6
        rung_w = 3.8
        rungs = [34.0, 54.5, 75.0, 95.5]
        _round_line(d, [(rail_l, top), (rail_l, bottom)], rail_w, LINE)
        _round_line(d, [(rail_r, top), (rail_r, bottom)], rail_w, LINE)
        for y in rungs:
            _round_line(d, [(rail_l - 1.0, y), (rail_r + 1.0, y)], rung_w, LINE)

    elif variant == "b":
        # 实机框架：四角立柱 + 顶梁 + 密横档（更接近游戏里的 1×1 木框）
        rail_l, rail_r = 44.0, 84.0
        top, bottom = 16.0, 112.0
        rail_w = 4.4
        rung_w = 3.2
        _round_line(d, [(rail_l, top), (rail_l, bottom)], rail_w, LINE)
        _round_line(d, [(rail_r, top), (rail_r, bottom)], rail_w, LINE)
        _round_line(d, [(rail_l - 3.0, top), (rail_r + 3.0, top)], rail_w + 0.4, LINE)
        for i in range(6):
            y = 36.0 + i * 13.6
            _round_line(d, [(rail_l, y), (rail_r, y)], rung_w, LINE)

    elif variant == "c":
        # 侧视剖面：一根竖轨 + 水平踏板（呼应原版 StairsIcon 的剖面语言）
        rail_x = 64.0
        top, bottom = 16.0, 112.0
        rail_w = 4.6
        peg_w = 4.2
        _round_line(d, [(rail_x, top), (rail_x, bottom)], rail_w, LINE)
        for i in range(4):
            y = 32.0 + i * 22.0
            _round_line(d, [(rail_x - 9.0, y), (rail_x + 18.0, y)], peg_w, LINE)

    elif variant == "d":
        # 方形木框梯：宽度接近 1×1 格，四角立柱 + 顶梁 + 四根外凸横档（贴近实机建筑）
        rail_l, rail_r = 34.0, 94.0
        top, bottom = 16.0, 114.0
        rail_w = 5.2
        rung_w = 4.4
        _round_line(d, [(rail_l, top), (rail_l, bottom)], rail_w, LINE)
        _round_line(d, [(rail_r, top), (rail_r, bottom)], rail_w, LINE)
        _round_line(d, [(rail_l - 4.0, top), (rail_r + 4.0, top)], rail_w, LINE)
        for i in range(4):
            y = 38.0 + i * 22.0
            _round_line(d, [(rail_l - 3.5, y), (rail_r + 3.5, y)], rung_w, LINE)

    elif variant == "e":
        # 窄直梯加粗版：竖轨 5.6、横档 4.6，四档等距
        rail_l, rail_r = 42.0, 86.0
        top, bottom = 14.0, 114.0
        rail_w = 5.6
        rung_w = 4.6
        _round_line(d, [(rail_l, top), (rail_l, bottom)], rail_w, LINE)
        _round_line(d, [(rail_r, top), (rail_r, bottom)], rail_w, LINE)
        for i in range(4):
            y = 33.0 + i * 21.0
            _round_line(d, [(rail_l - 2.0, y), (rail_r + 2.0, y)], rung_w, LINE)

    elif variant == "f":
        # 方形木框梯·疏版：顶梁 + 三根横档，间隙更大，缩到 40px 仍清晰
        rail_l, rail_r = 34.0, 94.0
        top, bottom = 16.0, 114.0
        rail_w = 5.4
        rung_w = 4.8
        _round_line(d, [(rail_l, top), (rail_l, bottom)], rail_w, LINE)
        _round_line(d, [(rail_r, top), (rail_r, bottom)], rail_w, LINE)
        _round_line(d, [(rail_l - 4.0, top), (rail_r + 4.0, top)], rail_w, LINE)
        for i in range(3):
            y = 46.0 + i * 28.0
            _round_line(d, [(rail_l - 3.5, y), (rail_r + 3.5, y)], rung_w, LINE)

    else:
        raise SystemExit(f"unknown variant {variant}")

    return im.getchannel("A")


def fit_scale(mask: Image.Image, scale: float) -> Image.Image:
    """把线稿蒙版绕画布中心等比缩放，等比缩小时线宽也一起变细。"""
    if abs(scale - 1.0) < 1e-6:
        return mask
    side = int(round(mask.width * scale))
    small = mask.resize((side, side), getattr(Image, "Resampling", Image).LANCZOS)  # type: ignore[attr-defined]
    out = Image.new("L", mask.size, 0)
    out.paste(small, ((mask.width - side) // 2, (mask.height - side) // 2))
    return out


def compose(mask: Image.Image, scale: float = SCALE) -> Image.Image:
    """把线稿蒙版合成为「柔和黑投影 + 金色线条」，并降采样到目标尺寸。

    线条颜色在降采样前保持恒定（只对蒙版做缩放），避免重采样让线色变亮；
    投影同样是「模糊蒙版 → 缩放 → 压 alpha」，峰值接近原版图标的 ~140。
    """
    resample = getattr(Image, "Resampling", Image).LANCZOS  # type: ignore[attr-defined]
    target = (SIZE, SIZE)
    mask = fit_scale(mask, scale)

    # 投影：蒙版模糊后压 alpha，在超采样空间里偏移，再降采样
    blur = mask.filter(ImageFilter.GaussianBlur(SHADOW_BLUR * SS))
    blur = blur.point(lambda v: int(v * SHADOW_ALPHA))
    dx, dy = (int(round(SHADOW_OFFSET[0] * SS)), int(round(SHADOW_OFFSET[1] * SS)))
    shadow_mask = Image.new("L", mask.size, 0)
    shadow_mask.paste(blur, (dx, dy))
    shadow_mask = shadow_mask.resize(target, resample)
    shadow = Image.new("RGBA", target, (0, 0, 0, 255))
    shadow.putalpha(shadow_mask)

    # 线条：固定线色 + 缩放后的蒙版
    line_mask = mask.resize(target, resample)
    line = Image.new("RGBA", target, LINE + (255,))
    line.putalpha(line_mask)

    return Image.alpha_composite(shadow, line)


def main() -> None:
    ap = argparse.ArgumentParser()
    ap.add_argument("--preview", action="store_true", help="输出候选到 .preview/")
    ap.add_argument("--out", default=None)
    ap.add_argument("--scale", type=float, default=SCALE, help="图形相对画布的缩放")
    args = ap.parse_args()

    root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    out = args.out or os.path.join(root, "Sprites", "Rope", "Ladder.png")

    icon = compose(draw_ladder("f"), args.scale)
    icon.save(out)
    print("wrote", out, icon.size)

    if args.preview:
        pdir = os.path.join(root, ".preview")
        os.makedirs(pdir, exist_ok=True)
        for v in ("a", "b", "c", "d", "e", "f"):
            p = os.path.join(pdir, f"ladder-{v}.png")
            compose(draw_ladder(v), args.scale).save(p)
            print("preview", p)


if __name__ == "__main__":
    main()
