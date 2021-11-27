using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class UnitBounds
    {
        public UnitBase UnitBase { get; private set; }
        public UnitBounds(UnitBase unitBase)
        {
            UnitBase = unitBase;
            visible = true;
        }

        public Position2 Pos
        {
            get
            {
                return lastPosition;

            }
        }

        private Position2 lastPosition;
        private List<GameObject> visibleFrames = new List<GameObject>();

        private void CreateFrame(Position3 position)
        {
            if (position.Direction == Direction.NW)
                return;

            GroundCell groundCell;
            if (HexGrid.MainGrid.GroundCells.TryGetValue(position.Pos, out groundCell))
            {
                GameObject previewUnitMarker = HexGrid.MainGrid.InstantiatePrefab("GroundBuild");
                previewUnitMarker.transform.SetParent(HexGrid.MainGrid.transform, false);
                Vector3 vector3 = groundCell.transform.position;
                vector3.y += 0.08f;
                previewUnitMarker.transform.position = vector3;
                visibleFrames.Add(previewUnitMarker);
            }

        }
        private bool visible;

        internal bool IsVisible
        {
            get
            {
                return visible;
            }
            set
            {
                if (visible != value)
                {
                    visible = value;

                    foreach (GameObject gameObject in visibleFrames)
                    {
                        gameObject.SetActive(visible);
                    }
                }
            }
        }

        private bool addBuildGrid;
        public void AddBuildGrid()
        {
            addBuildGrid = true;
        }

        private int? collectRadius;
        public void AddCollectRange(int radius)
        {
            collectRadius = radius;
        }

        public void Update()
        {
            if (visibleFrames.Count > 0)
                Destroy();

            lastPosition = UnitBase.CurrentPos;
            Position3 position3 = new Position3(UnitBase.CurrentPos);
            foreach (UnitBasePart unitBasePart in UnitBase.UnitBaseParts)
            {
                if (unitBasePart.PartType == TileObjectType.PartReactor)
                {
                    List<Position3> positions = position3.CreateRing(unitBasePart.Range);
                    foreach (Position3 position in positions)
                    {
                        CreateFrame(position);
                    }
                }
                if (unitBasePart.PartType == TileObjectType.PartContainer)
                {
                    List<Position3> positions = position3.CreateRing(unitBasePart.Range);
                    foreach (Position3 position in positions)
                    {
                        CreateFrame(position);
                    }
                }
                if (unitBasePart.PartType == TileObjectType.PartWeapon)
                {
                    List<Position3> positions = position3.CreateRing(unitBasePart.Range);
                    foreach (Position3 position in positions)
                    {
                        CreateFrame(position);
                    }
                }
            }
            if (collectRadius.HasValue)
            {
                List<Position3> positions = position3.CreateRing(collectRadius.Value);
                foreach (Position3 position in positions)
                {
                    CreateFrame(position);
                }
            }
            if (addBuildGrid)
            {
                List<Position3> positions = position3.GetNeighbors(2);
                foreach (Position3 position in positions)
                {
                    CreateFrame(position);
                }
            }
        }

        public void Destroy()
        {
            foreach (GameObject gameObject in visibleFrames)
            {
                HexGrid.Destroy(gameObject);
            }
            visibleFrames.Clear();
        }
    }
}
