using Timberborn.BlueprintSystem;

namespace Jove.ClimbLadder
{
    /// <summary>
    /// 标记这是一格攀爬梯。寻路和删除都走游戏自己的道路建筑，不另做工具。
    /// </summary>
    public record RopeLadderSpec : ComponentSpec;
}
