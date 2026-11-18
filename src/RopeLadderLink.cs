using System.Collections.Generic;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.EntitySystem;
using Timberborn.Navigation;
using Timberborn.PathSystem;
using Timberborn.TerrainSystem;
using UnityEngine;

namespace Jove.ClimbLadder
{
    /// <summary>
    /// 梯子不占 Path 格，才能和已有道路叠在同一格。
    /// 水平边和向下的边是单向的，不会钻进地下。
    /// 向上的边在蓝图里是双向的，梯子正上方那一格会进入路网。
    /// 门口在同一层紧挨着，或正在梯子正上方时，补一条从门口回到梯子的边。
    /// 门口在顶格的斜上一格时，只从这一列的最高一格先上到落脚点，再横跨一步进门。
    /// 不从梯子中段抄近路，也不向地下或更远的格子拉边。
    /// </summary>
    public sealed class RopeLadderLink : BaseComponent, IAwakableComponent, IInitializableEntity, IDeletableEntity, INavMeshListener, IPathConnectionEnforcer, IUpdatableComponent
    {
        readonly IBlockService _blocks;

        readonly ITerrainService _terrain;

        readonly PreviewBlockService _previews;

        readonly INavMeshService _navMesh;

        readonly NavMeshGroupService _groups;

        readonly INavMeshListenerEntityRegistry _listeners;

        readonly List<NavMeshEdge> _added = new List<NavMeshEdge>();

        readonly List<NavMeshEdge> _desired = new List<NavMeshEdge>();

        static RopeLadderLink _nudger;

        static int _pokesLeft;

        static int _pokeSerial;

        int _seenPokeSerial;

        BlockObject _block;

        bool _listening;

        public RopeLadderLink(IBlockService blocks, ITerrainService terrain, PreviewBlockService previews, INavMeshService navMesh, NavMeshGroupService groups, INavMeshListenerEntityRegistry listeners)
        {
            _blocks = blocks;
            _terrain = terrain;
            _previews = previews;
            _navMesh = navMesh;
            _groups = groups;
            _listeners = listeners;
        }

        public void Awake()
        {
            _block = GetComponent<BlockObject>();
            if (_nudger == null)
            {
                _nudger = this;
            }
        }

        public void Update()
        {
            if (_nudger == null)
            {
                _nudger = this;
            }

            if (_nudger != this || _pokesLeft <= 0)
            {
                return;
            }

            _pokesLeft--;
            _pokeSerial++;
            PokeUpwardEdge();
        }

        public void InitializeEntity()
        {
            if (_block == null)
            {
                _block = GetComponent<BlockObject>();
            }
            if (_block == null || _block.IsPreview)
            {
                return;
            }
            _listeners.RegisterNavMeshListener(this);
            _listening = true;
            SyncEntranceLinks();
            _pokesLeft = 2;
        }

        public void DeleteEntity()
        {
            if (_listening)
            {
                _listeners.UnregisterNavMeshListener(this);
                _listening = false;
            }

            if (_nudger == this)
            {
                _nudger = null;
            }
            for (int i = 0; i < _added.Count; i++)
            {
                _navMesh.RemoveEdge(_added[i]);
            }
            _added.Clear();
        }

        public void OnNavMeshUpdated(NavMeshUpdate navMeshUpdate)
        {
            if (!navMeshUpdate.UpdatedRoads)
            {
                return;
            }

            SyncEntranceLinks();
            if (_seenPokeSerial != _pokeSerial)
            {
                _seenPokeSerial = _pokeSerial;
                return;
            }

            _pokesLeft = 2;
        }

        void PokeUpwardEdge()
        {
            if (_block == null || !_block.Positioned)
            {
                return;
            }

            Vector3Int origin = _block.Coordinates;
            Vector3Int above = new Vector3Int(origin.x, origin.y, origin.z + 1);
            int group = _groups.GetDefaultGroupId();
            NavMeshEdge up = NavMeshEdge.CreateGrouped(origin, above, group, true, 1f);
            NavMeshEdge down = NavMeshEdge.CreateGrouped(above, origin, group, true, 1f);
            _navMesh.RemoveEdge(up);
            _navMesh.AddEdge(up);
            _navMesh.RemoveEdge(down);
            _navMesh.AddEdge(down);
        }

        public bool CanConnectPath(Vector3Int origin, Vector3Int target)
        {
            if (_block == null)
            {
                _block = GetComponent<BlockObject>();
            }
            if (_block == null || !_block.Positioned)
            {
                return false;
            }

            Vector3Int here = _block.Coordinates;
            Vector3Int other;
            if (origin == here)
            {
                other = target;
            }
            else if (target == here)
            {
                other = origin;
            }
            else
            {
                return false;
            }

            Vector3Int delta = other - here;
            if (delta.z != 0 || Mathf.Abs(delta.x) + Mathf.Abs(delta.y) != 1)
            {
                return false;
            }

            return IsPathTile(other) || IsLadder(other);
        }

        void SyncEntranceLinks()
        {
            _desired.Clear();
            if (_block != null && _block.Positioned && !_block.IsPreview)
            {
                CollectUpwardLink(_block.Coordinates);
                CollectEntranceLinks(_block.Coordinates);
            }

            for (int i = _added.Count - 1; i >= 0; i--)
            {
                if (!ContainsEdge(_desired, _added[i]))
                {
                    _navMesh.RemoveEdge(_added[i]);
                    _added.RemoveAt(i);
                }
            }

            for (int i = 0; i < _desired.Count; i++)
            {
                if (!ContainsEdge(_added, _desired[i]))
                {
                    _navMesh.AddEdge(_desired[i]);
                    _added.Add(_desired[i]);
                }
            }
        }

        void CollectEntranceLinks(Vector3Int origin)
        {
            for (int dz = -2; dz <= 2; dz++)
            {
                for (int dx = -2; dx <= 2; dx++)
                {
                    for (int dy = -2; dy <= 2; dy++)
                    {
                        Vector3Int cell = origin + new Vector3Int(dx, dy, dz);
                        if (!_blocks.Contains(cell))
                        {
                            continue;
                        }
                        foreach (BlockObject blockObject in _blocks.GetObjectsAt(cell))
                        {
                            TryCollectEntrance(origin, blockObject);
                        }
                    }
                }
            }
        }

        void TryCollectEntrance(Vector3Int origin, BlockObject blockObject)
        {
            if (blockObject == null || blockObject.IsPreview || !blockObject.HasEntrance)
            {
                return;
            }

            Vector3Int access = blockObject.PositionedEntrance.Coordinates;
            Vector3Int delta = access - origin;
            int horizontal = Mathf.Abs(delta.x) + Mathf.Abs(delta.y);
            if (delta.z < 0 || delta.z > 1 || horizontal > 1)
            {
                return;
            }

            if (horizontal == 1 && delta.z == 0)
            {
                // 梯子还没盖完时，蓝图边不在正式路网上，两个方向都要自己补。
                AddDesired(origin, access);
                AddDesired(access, origin);
                return;
            }

            if (horizontal == 0 && delta.z == 1)
            {
                LinkUp(origin);
                return;
            }

            if (horizontal != 1 || delta.z != 1 || HasLadderAbove(origin))
            {
                return;
            }

            // 顶格斜上方的门口：先爬到梯子正上方的落脚点，再横跨一步。中段梯子不抄这条路。
            Vector3Int landing = new Vector3Int(origin.x, origin.y, origin.z + 1);
            if (!CanStand(landing))
            {
                return;
            }

            AddDesired(origin, landing);
            AddDesired(landing, origin);
            AddDesired(landing, access);
            AddDesired(access, landing);
        }

        void CollectUpwardLink(Vector3Int origin)
        {
            Vector3Int above = new Vector3Int(origin.x, origin.y, origin.z + 1);
            if (!_blocks.Contains(above) || _terrain.Underground(above))
            {
                return;
            }

            // 工地状态的梯子不会把蓝图边放进正式路网。这里直接把正上方接上，
            // 三层楼只盖了两层梯时，顶楼门口就在这一格。
            LinkUp(origin);
        }

        void LinkUp(Vector3Int origin)
        {
            Vector3Int above = new Vector3Int(origin.x, origin.y, origin.z + 1);
            AddDesired(origin, above);
            AddDesired(above, origin);
            if (_loggedUp.Add(origin))
            {
                Debug.Log("[ClimbLadder] 向上接通 " + origin + " -> " + above);
            }
        }

        static readonly HashSet<Vector3Int> _loggedUp = new HashSet<Vector3Int>();

        void AddDesired(Vector3Int start, Vector3Int end)
        {
            if (start == end)
            {
                return;
            }

            NavMeshEdge edge = NavMeshEdge.CreateGrouped(start, end, _groups.GetDefaultGroupId(), true, 1f);
            if (!ContainsEdge(_desired, edge))
            {
                _desired.Add(edge);
            }
        }

        bool HasLadderAbove(Vector3Int origin)
        {
            Vector3Int above = new Vector3Int(origin.x, origin.y, origin.z + 1);
            if (!_blocks.Contains(above))
            {
                return false;
            }

            return _blocks.GetFirstObjectWithComponentAt<RopeLadderLink>(above) != null;
        }

        bool CanStand(Vector3Int cell)
        {
            if (!_blocks.Contains(cell) || _terrain.Underground(cell))
            {
                return false;
            }

            foreach (BlockObject blockObject in _blocks.GetObjectsAt(cell))
            {
                if (blockObject == null || blockObject.IsPreview)
                {
                    continue;
                }

                if (blockObject.GetComponent<RopeLadderLink>() != null)
                {
                    continue;
                }

                if (_blocks.GetPathObjectAt(cell) == blockObject)
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        static bool ContainsEdge(List<NavMeshEdge> edges, NavMeshEdge edge)
        {
            for (int i = 0; i < edges.Count; i++)
            {
                if (edges[i] == edge)
                {
                    return true;
                }
            }
            return false;
        }

        bool IsPathTile(Vector3Int cell)
        {
            BlockObject path = _blocks.GetPathObjectAt(cell);
            if (path != null)
            {
                return true;
            }
            return _previews.GetPathObjectAt(cell) != null;
        }

        bool IsLadder(Vector3Int cell)
        {
            if (_blocks.GetFirstObjectWithComponentAt<RopeLadderLink>(cell) != null)
            {
                return true;
            }
            return _previews.GetFirstObjectWithComponentAt<RopeLadderLink>(cell) != null;
        }
    }
}
