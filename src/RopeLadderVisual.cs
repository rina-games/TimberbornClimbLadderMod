using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.Coordinates;
using Timberborn.EntitySystem;
using UnityEngine;

namespace Jove.ClimbLadder
{
    /// <summary>
    /// 一格攀爬梯的外观。
    /// </summary>
    public sealed class RopeLadderVisual : BaseComponent, IInitializableEntity, IFinishedStateListener, IUpdatableComponent
    {
        const float RopeLength = 1f;

        const float PegLength = 0.4f;

        readonly IBlockService _blocks;

        BlockObject _block;

        Transform _verticalRope;

        Transform _horizontalRope;

        Transform _sidePeg;

        Transform _downPeg;

        bool _fitted;

        int _frame;

        public RopeLadderVisual(IBlockService blocks)
        {
            _blocks = blocks;
        }

        public void InitializeEntity()
        {
            _fitted = false;
        }

        public void OnEnterFinishedState()
        {
            _fitted = false;
        }

        public void OnExitFinishedState()
        {
        }

        public void Update()
        {
            if (_block == null)
            {
                _block = GetComponent<BlockObject>();
            }
            if (_block == null || _block.IsUnfinished)
            {
                return;
            }
            if (_verticalRope == null)
            {
                _verticalRope = Find("VerticalRope");
                _horizontalRope = Find("HorizontalRope");
                _sidePeg = Find("SidePeg");
                _downPeg = Find("DownPeg");
            }
            if (_verticalRope == null)
            {
                return;
            }
            if (!_fitted)
            {
                _fitted = TryFit();
            }
            if (_fitted && (_frame++ % 15) == 0)
            {
                ApplyConnections();
            }
        }

        Transform Find(string name)
        {
            Transform[] transforms = Transform.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i].name == name)
                {
                    return transforms[i];
                }
            }
            return null;
        }

        bool TryFit()
        {
            if (_horizontalRope == null || _sidePeg == null || _downPeg == null)
            {
                return false;
            }
            if (_verticalRope.GetComponentInChildren<Renderer>(true) == null || _sidePeg.GetComponentInChildren<Renderer>(true) == null)
            {
                return false;
            }
            KeepPillars(_sidePeg);
            KeepPillars(_downPeg);
            Place(_verticalRope, Vector3.up, RopeLength, new Vector3(0.5f, 0.5f, 0.5f));
            Place(_horizontalRope, Transform.forward, RopeLength, new Vector3(0.5f, 0.65f, 0.5f));
            Place(_sidePeg, Transform.right, PegLength, new Vector3(0.92f, 0.72f, 0.5f));
            Place(_downPeg, Vector3.down, PegLength, new Vector3(0.5f, -0.05f, 0.5f));
            ApplyConnections();
            return true;
        }

        static void KeepPillars(Transform root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            int pillars = 0;
            for (int i = 0; i < renderers.Length; i++)
            {
                Vector3 size = renderers[i].bounds.size;
                float height = size.y;
                float width = Mathf.Max(size.x, size.z);
                bool pillar = height > width * 1.5f && height > 0.15f;
                renderers[i].enabled = pillar;
                if (pillar)
                {
                    pillars++;
                }
            }
            if (pillars > 1)
            {
                bool kept = false;
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (!renderers[i].enabled)
                    {
                        continue;
                    }
                    if (kept)
                    {
                        renderers[i].enabled = false;
                    }
                    kept = true;
                }
            }
        }

        void Place(Transform piece, Vector3 worldDirection, float length, Vector3 localCenter)
        {
            piece.gameObject.SetActive(true);
            piece.localScale = Vector3.one;
            piece.localRotation = Quaternion.identity;
            AlignLongestAxis(piece, worldDirection);
            Bounds bounds = BoundsOf(piece);
            float along = Mathf.Abs(Vector3.Dot(bounds.size, worldDirection.normalized));
            if (along > 0.001f)
            {
                piece.localScale = Vector3.one * (length / along);
                bounds = BoundsOf(piece);
            }
            piece.position += Transform.TransformPoint(localCenter) - bounds.center;
        }

        static void AlignLongestAxis(Transform piece, Vector3 worldDirection)
        {
            Bounds bounds = BoundsOf(piece);
            Vector3 size = bounds.size;
            Vector3 localAxis = Vector3.up;
            float longest = size.y;
            if (size.x > longest)
            {
                longest = size.x;
                localAxis = Vector3.right;
            }
            if (size.z > longest)
            {
                localAxis = Vector3.forward;
            }
            Vector3 worldAxis = piece.TransformDirection(localAxis);
            piece.rotation = Quaternion.FromToRotation(worldAxis, worldDirection) * piece.rotation;
        }

        static Bounds BoundsOf(Transform piece)
        {
            Renderer[] renderers = piece.GetComponentsInChildren<Renderer>(true);
            Bounds bounds = new Bounds(piece.position, Vector3.zero);
            bool any = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (!renderers[i].enabled)
                {
                    continue;
                }
                if (!any)
                {
                    bounds = renderers[i].bounds;
                    any = true;
                }
                else
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }
            return bounds;
        }

        void ApplyConnections()
        {
            bool stacked = HasLadder(new Vector3Int(0, 0, 1)) || HasLadder(new Vector3Int(0, 0, -1));
            bool flat = HasLadder(_block.Orientation.Transform(new Vector3Int(1, 0, 0)))
                || HasLadder(_block.Orientation.Transform(new Vector3Int(-1, 0, 0)))
                || HasLadder(_block.Orientation.Transform(new Vector3Int(0, 1, 0)))
                || HasLadder(_block.Orientation.Transform(new Vector3Int(0, -1, 0)));
            if (!stacked && !flat)
            {
                stacked = true;
            }
            SetActive(_verticalRope, stacked);
            SetActive(_sidePeg, stacked);
            SetActive(_horizontalRope, flat);
            SetActive(_downPeg, flat);
        }

        bool HasLadder(Vector3Int offset)
        {
            Vector3Int cell = _block.Coordinates + offset;
            if (!_blocks.Contains(cell))
            {
                return false;
            }
            foreach (BlockObject block in _blocks.GetObjectsAt(cell))
            {
                if (block != null && block != _block && !block.IsPreview && block.GetComponent<RopeLadderVisual>() != null)
                {
                    return true;
                }
            }
            return false;
        }

        static void SetActive(Transform piece, bool active)
        {
            if (piece != null && piece.gameObject.activeSelf != active)
            {
                piece.gameObject.SetActive(active);
            }
        }
    }
}
