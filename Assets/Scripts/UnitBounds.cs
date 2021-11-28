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

        private void CreateBuildFrame(Position3 position)
        {
            GroundCell groundCell;
            if (HexGrid.MainGrid.GroundCells.TryGetValue(position.Pos, out groundCell))
            {
                Vector3 vector3 = groundCell.transform.position;
                vector3.y += 0.08f;

                GameObject previewUnitMarker = HexGrid.MainGrid.InstantiatePrefab("GroundBuildFrame");
                previewUnitMarker.transform.SetParent(HexGrid.MainGrid.transform, false);
                previewUnitMarker.transform.position = vector3;

                
                visibleFrames.Add(previewUnitMarker);
            }
        }
        private void CreateUnitFrame(Position3 position)
        {
            GroundCell groundCell;
            if (HexGrid.MainGrid.GroundCells.TryGetValue(position.Pos, out groundCell))
            {
                Vector3 vector3 = groundCell.transform.position;
                vector3.y += 0.08f;

                GameObject previewUnitMarker = HexGrid.MainGrid.InstantiatePrefab("GroundFrame");
                previewUnitMarker.transform.SetParent(HexGrid.MainGrid.transform, false);
                previewUnitMarker.transform.position = vector3;


                visibleFrames.Add(previewUnitMarker);
            }
        }


        private void CreateFrameBorder(Position3 position, bool isCorner)
        {
            //if (!isCorner)
            //    return;
            //if (position.Direction != Direction.SW)
            //    return;
            //if (position.Direction != Direction.NE && position.Direction != Direction.N)
            //    return;
            GroundCell groundCell;
            if (HexGrid.MainGrid.GroundCells.TryGetValue(position.Pos, out groundCell))
            {
                Vector3 vector3 = groundCell.transform.position;
                vector3.y += 0.18f;

                GameObject previewUnitMarker1 = HexGrid.MainGrid.InstantiatePrefab("GroundFramePart");
                previewUnitMarker1.transform.SetParent(HexGrid.MainGrid.transform, false);
                previewUnitMarker1.transform.position = vector3;

                GameObject previewUnitMarker2 = HexGrid.MainGrid.InstantiatePrefab("GroundFramePart");
                previewUnitMarker2.transform.SetParent(HexGrid.MainGrid.transform, false);
                previewUnitMarker2.transform.position = vector3;

                GameObject previewUnitMarker3 = null;

                if (isCorner)
                {
                    previewUnitMarker3 = HexGrid.MainGrid.InstantiatePrefab("GroundFramePart");
                    previewUnitMarker3.transform.SetParent(HexGrid.MainGrid.transform, false);
                    previewUnitMarker3.transform.position = vector3;
                }

                if (position.Direction == Direction.N)
                {
                    previewUnitMarker1.transform.rotation = Quaternion.AngleAxis(-90, Vector3.up);
                    previewUnitMarker2.transform.rotation = Quaternion.AngleAxis(-30, Vector3.up);

                    if (isCorner && previewUnitMarker3 != null)
                        previewUnitMarker3.transform.rotation = Quaternion.AngleAxis(30, Vector3.up);
                }
                
                if (position.Direction == Direction.NE)
                {
                    previewUnitMarker1.transform.rotation = Quaternion.AngleAxis(-30, Vector3.up);
                    previewUnitMarker2.transform.rotation = Quaternion.AngleAxis(30, Vector3.up);

                    if (isCorner && previewUnitMarker3 != null)
                        previewUnitMarker3.transform.rotation = Quaternion.AngleAxis(90, Vector3.up);
                }
                if (position.Direction == Direction.NW)
                {
                    previewUnitMarker1.transform.rotation = Quaternion.AngleAxis(210, Vector3.up);
                    previewUnitMarker2.transform.rotation = Quaternion.AngleAxis(270, Vector3.up);

                    if (isCorner && previewUnitMarker3 != null)
                        previewUnitMarker3.transform.rotation = Quaternion.AngleAxis(330, Vector3.up);
                }
                if (position.Direction == Direction.S)
                {
                    previewUnitMarker1.transform.rotation = Quaternion.AngleAxis(90, Vector3.up);
                    previewUnitMarker2.transform.rotation = Quaternion.AngleAxis(150, Vector3.up);

                    if (isCorner && previewUnitMarker3 != null)
                        previewUnitMarker3.transform.rotation = Quaternion.AngleAxis(210, Vector3.up);
                }
                if (position.Direction == Direction.SE)
                {
                    previewUnitMarker1.transform.rotation = Quaternion.AngleAxis(30, Vector3.up);
                    previewUnitMarker2.transform.rotation = Quaternion.AngleAxis(90, Vector3.up);

                    if (isCorner && previewUnitMarker3 != null)
                        previewUnitMarker3.transform.rotation = Quaternion.AngleAxis(150, Vector3.up);
                }
                if (position.Direction == Direction.SW)
                {
                    previewUnitMarker1.transform.rotation = Quaternion.AngleAxis(150, Vector3.up);
                    previewUnitMarker2.transform.rotation = Quaternion.AngleAxis(210, Vector3.up);

                    if (isCorner && previewUnitMarker3 != null)
                        previewUnitMarker3.transform.rotation = Quaternion.AngleAxis(270, Vector3.up);
                }
                visibleFrames.Add(previewUnitMarker1);
                visibleFrames.Add(previewUnitMarker2);
                if (previewUnitMarker3 != null)
                    visibleFrames.Add(previewUnitMarker3);
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

        private void CreateFrame(List<Position3> positions)
        {
            Direction lastDirection = Direction.C;
            //foreach (Position3 position in positions)
            for (int i = 0; i < positions.Count; i++)
            {
                Position3 position = positions[i];
                Position3 nextPosition;
                if (i < positions.Count - 1)
                    nextPosition = positions[i + 1];
                else
                    nextPosition = positions[0];

                CreateFrameBorder(position, nextPosition.Direction != position.Direction);
                lastDirection = position.Direction;
            }
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
                    CreateFrame(positions);
                }
                if (unitBasePart.PartType == TileObjectType.PartContainer)
                {
                    List<Position3> positions = position3.CreateRing(unitBasePart.Range);
                    CreateFrame(positions);
                }
                if (unitBasePart.PartType == TileObjectType.PartWeapon)
                {
                    List<Position3> positions = position3.CreateRing(unitBasePart.Range);
                    CreateFrame(positions);
                }
            }
            if (collectRadius.HasValue)
            {
                List<Position3> positions = position3.CreateRing(collectRadius.Value);
                CreateFrame(positions);
            }
            if (addBuildGrid)
            {
                List<Position3> positions = position3.GetNeighbors(2);
                foreach (Position3 position in positions)
                {
                    CreateBuildFrame(position);
                }
            }
            CreateUnitFrame(position3);
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
