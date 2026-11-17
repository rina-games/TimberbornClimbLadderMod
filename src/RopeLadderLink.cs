using System.Collections.Generic;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.EntitySystem;
using Timberborn.Navigation;
using Timberborn.PathSystem;
using UnityEngine;

namespace Jove.ClimbLadder
{
    /// <summary>
    /// 梯子不占 Path 格，才能和已有道路叠在同一格。
    /// 建筑要算连上道路，门口外面那一格必须和门内那一格接通。
    /// 梯子放在同一层时，横向道路刚好接上这两格；放在楼下时接不到，这里补上。
    /// </summary>
    public sealed class RopeLadderLink : BaseComponent, IAwakableComponent, IInitializableEntity, IDeletableEntity, INavMeshListener, IPathConnectionEnforcer
    {
        readonly IBlockService _blocks;

        readonly PreviewBlockService _previews;

        readonly INavMeshService _navMesh;

        readonly NavMeshGroupService _groups;

        readonly INavMeshListenerEntityRegistry _listeners;

        readonly List<NavMeshEdge> _added = new List<NavMeshEdge>();

        BlockObject _block;

        bool _listening;

        public RopeLadderLink(IBlockService blocks, PreviewBlockService previews, INavMeshService navMesh, NavMeshGroupService groups, INavMeshListenerEntityRegistry listeners)
        {
            _blocks = blocks;
            _previews = previews;
            _navMesh = navMesh;
            _groups = groups;
            _listeners = listeners;
        }

        public void Awake()
        {
            _block = GetComponent<BlockObject>();
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
            LinkNearbyEntrances();
        }

        public void DeleteEntity()
        {
            if (_listening)
            {
                _listeners.UnregisterNavMeshListener(this);
                _listening = false;
            }
            for (int i = 0; i < _added.Count; i++)
            {
                _navMesh.RemoveEdge(_added[i]);
            }
            _added.Clear();
        }

        public void OnNavMeshUpdated(NavMeshUpdate navMeshUpdate)
        {
            if (navMeshUpdate.UpdatedRoads)
            {
                LinkNearbyEntrances();
            }
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

        void LinkNearbyEntrances()
        {
            if (_block == null || !_block.Positioned || _block.IsPreview)
            {
                return;
            }

            Vector3Int origin = _block.Coordinates;
            for (int dz = 0; dz <= 2; dz++)
            {
                for (int dx = -2; dx <= 2; dx++)
                {
                    for (int dy = -2; dy <= 2; dy++)
                    {
                        if (Mathf.Abs(dx) + Mathf.Abs(dy) > 3)
                        {
                            continue;
                        }
                        Vector3Int cell = origin + new Vector3Int(dx, dy, dz);
                        if (!_blocks.Contains(cell))
                        {
                            continue;
                        }
                        foreach (BlockObject blockObject in _blocks.GetObjectsAt(cell))
                        {
                            TryLink(origin, blockObject);
                        }
                    }
                }
            }
        }

        void TryLink(Vector3Int origin, BlockObject blockObject)
        {
            if (blockObject == null || blockObject.IsPreview || !blockObject.HasEntrance)
            {
                return;
            }
            PositionedEntrance entrance = blockObject.PositionedEntrance;
            Vector3Int access = entrance.Coordinates;
            Vector3Int delta = access - origin;
            if (delta.z < 0 || delta.z > 2 || Mathf.Abs(delta.x) + Mathf.Abs(delta.y) > 3)
            {
                return;
            }
            EnsureBothWays(origin, access);
            EnsureBothWays(access, entrance.DoorstepCoordinates);
        }

        void EnsureBothWays(Vector3Int start, Vector3Int end)
        {
            if (start == end)
            {
                return;
            }
            EnsureOneWay(start, end);
            EnsureOneWay(end, start);
        }

        void EnsureOneWay(Vector3Int start, Vector3Int end)
        {
            for (int i = 0; i < _added.Count; i++)
            {
                if (_added[i].Start == start && _added[i].End == end)
                {
                    return;
                }
            }
            NavMeshEdge edge = NavMeshEdge.CreateGrouped(start, end, _groups.GetDefaultGroupId(), true, 1f);
            _navMesh.AddEdge(edge);
            _added.Add(edge);
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
