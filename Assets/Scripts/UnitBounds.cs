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
        //private List<GameObject> visibleFrames = new List<GameObject>();
        private Dictionary<Position2, GameObject> visibleFrames = new Dictionary<Position2, GameObject>();
        private void CreateTargetFrame(Position3 position)
        {
            if (!visibleFrames.ContainsKey(position.Pos))
            {
                GroundCell groundCell;
                if (HexGrid.MainGrid.GroundCells.TryGetValue(position.Pos, out groundCell))
                {
                    Vector3 vector3 = groundCell.transform.position;
                    vector3.y += 0.08f;

                    GameObject previewUnitMarker = HexGrid.MainGrid.InstantiatePrefab("GroundTargetFrame");
                    previewUnitMarker.transform.SetParent(HexGrid.MainGrid.transform, false);
                    previewUnitMarker.transform.position = vector3;

                    visibleFrames.Add(position.Pos, previewUnitMarker);
                }
            }
        }

        private void CreateBuildFrame(Position3 position)
        {
            if (!visibleFrames.ContainsKey(position.Pos))
            {
                GroundCell groundCell;
                if (HexGrid.MainGrid.GroundCells.TryGetValue(position.Pos, out groundCell))
                {
                    Vector3 vector3 = groundCell.transform.position;
                    vector3.y += 0.08f;

                    GameObject previewUnitMarker = HexGrid.MainGrid.InstantiatePrefab("GroundBuildFrame");
                    previewUnitMarker.transform.SetParent(HexGrid.MainGrid.transform, false);
                    previewUnitMarker.transform.position = vector3;

                    visibleFrames.Add(position.Pos, previewUnitMarker);
                }
            }
        }
        private void CreateUnitFrame(Position3 position)
        {
            if (!visibleFrames.ContainsKey(position.Pos))
            {
                GroundCell groundCell;
                if (HexGrid.MainGrid.GroundCells.TryGetValue(position.Pos, out groundCell))
                {
                    Vector3 vector3 = groundCell.transform.position;
                    vector3.y += 0.08f;

                    GameObject previewUnitMarker = HexGrid.MainGrid.InstantiatePrefab("GroundFrame");
                    previewUnitMarker.transform.SetParent(HexGrid.MainGrid.transform, false);
                    previewUnitMarker.transform.position = vector3;

                    visibleFrames.Add(position.Pos, previewUnitMarker);
                }
            }
        }


        private void CreateFrameBorder(Position3 position, bool isCorner)
        {
            if (visibleFrames.ContainsKey(position.Pos))
                return;

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

                GameObject previewUnitMarker = new GameObject();
                previewUnitMarker.transform.SetParent(HexGrid.MainGrid.transform, false);
                previewUnitMarker.transform.position = vector3;

                GameObject previewUnitMarker1 = HexGrid.MainGrid.InstantiatePrefab("GroundFramePart");
                previewUnitMarker1.transform.SetParent(previewUnitMarker.transform, false);
                //previewUnitMarker1.transform.position = vector3;

                GameObject previewUnitMarker2 = HexGrid.MainGrid.InstantiatePrefab("GroundFramePart");
                previewUnitMarker2.transform.SetParent(previewUnitMarker.transform, false);
                //previewUnitMarker2.transform.SetParent(HexGrid.MainGrid.transform, false);
                //previewUnitMarker2.transform.position = vector3;

                GameObject previewUnitMarker3 = null;

                if (isCorner)
                {
                    previewUnitMarker3 = HexGrid.MainGrid.InstantiatePrefab("GroundFramePart");
                    //previewUnitMarker3.transform.SetParent(HexGrid.MainGrid.transform, false);
                    previewUnitMarker3.transform.SetParent(previewUnitMarker.transform, false);
                    //previewUnitMarker3.transform.position = vector3;
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

                
                //visibleFrames.Add(previewUnitMarker1);
                //visibleFrames.Add(previewUnitMarker2);
                //if (previewUnitMarker3 != null)
                //    visibleFrames.Add(previewUnitMarker3);

                
                    visibleFrames.Add(position.Pos, previewUnitMarker);
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

                    foreach (GameObject gameObject in visibleFrames.Values)
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

        private static List<Vector3> groundCellMesh;

        public static List<Vector3> GroundCellMesh
        {
            get
            {
                if (groundCellMesh == null)
                {
                    groundCellMesh = new List<Vector3>();
                    groundCellMesh.Add(new Vector3(0.86f, 0, 0.5f)); // 0
                    groundCellMesh.Add(new Vector3(0.86f, 0, -0.5f)); // 1
                    groundCellMesh.Add(new Vector3(0, 0, -1)); // 2
                    groundCellMesh.Add(new Vector3(0, 0, 1)); // 3
                    groundCellMesh.Add(new Vector3(-0.86f, 0, 0.5f)); // 4
                    groundCellMesh.Add(new Vector3(-0.86f, 0, -0.5f)); // 5

                    /* Smaller
                    meshvertices.Add(new Vector3(0.6f, 0, 0.4f)); // 0
                    meshvertices.Add(new Vector3(0.6f, 0, -0.4f)); // 1
                    meshvertices.Add(new Vector3(0, 0, -0.8f)); // 2
                    meshvertices.Add(new Vector3(0, 0, 0.8f)); // 3
                    meshvertices.Add(new Vector3(-0.6f, 0, 0.4f)); // 4
                    meshvertices.Add(new Vector3(-0.6f, 0, -0.4f)); // 5
                    */
                }
                return groundCellMesh;
            }
        }

        private void CreateFrame(List<Position3> positions)
        {
            GameObject lineRendererObject = new GameObject();
            visibleFrames.Add(positions[0].Pos, lineRendererObject);

            LineRenderer lineRenderer = lineRendererObject.AddComponent<LineRenderer>();
            lineRenderer.transform.SetParent(HexGrid.MainGrid.transform, false);
            lineRenderer.material = HexGrid.MainGrid.GetMaterial("test");
            lineRenderer.loop = true;

            //lineRenderer.startColor = Color.yellow;
            //lineRenderer.endColor = Color.yellow;

            lineRenderer.startWidth = 0.15f;
            lineRenderer.endWidth = 0.15f;
            //lineRenderer.widthCurve = new AnimationCurve();

            lineRenderer.receiveShadows = false;

            lineRenderer.numCornerVertices = 100;
            //lineRenderer.numCapVertices = 100;
            lineRenderer.useWorldSpace = true;

            List<Vector3> allvertices = new List<Vector3>();


            Transform lastChild = null;
            float aboveGround = 0.1f;

            for (int i = 0; i < positions.Count; i++)
            {
                Position3 position = positions[i];
                bool isBorder = false;
                if (i > 0)
                    isBorder = position.Direction != positions[i - 1].Direction;

                GroundCell groundCell;
                if (HexGrid.MainGrid.GroundCells.TryGetValue(position.Pos, out groundCell))
                {
                    Transform child = groundCell.GetDisplayedGroundCell();

                    var renderer = child.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        Vector3 transVert;
                        if (position.Direction == Direction.NE)
                        {
                            transVert = child.transform.TransformPoint(GroundCellMesh[4]);
                            transVert.y += aboveGround;
                            allvertices.Add(transVert);

                            transVert = child.transform.TransformPoint(GroundCellMesh[3]);
                            transVert.y += aboveGround;
                            allvertices.Add(transVert);

                            transVert = child.transform.TransformPoint(GroundCellMesh[0]);
                            transVert.y += aboveGround;
                            allvertices.Add(transVert);
                        }
                        
                        if (position.Direction == Direction.SE)
                        {
                            transVert = child.transform.TransformPoint(GroundCellMesh[3]);
                            transVert.y += aboveGround;
                            allvertices.Add(transVert);

                            transVert = child.transform.TransformPoint(GroundCellMesh[0]);
                            transVert.y += aboveGround;
                            allvertices.Add(transVert);
                        }
                        if (position.Direction == Direction.S)
                        {
                            if (isBorder)
                            {
                                transVert = lastChild.transform.TransformPoint(GroundCellMesh[1]);
                                transVert.y += aboveGround;
                                allvertices.Add(transVert);

                                transVert = lastChild.transform.TransformPoint(GroundCellMesh[2]);
                                transVert.y += aboveGround;
                                allvertices.Add(transVert);
                            }
                            transVert = child.transform.TransformPoint(GroundCellMesh[1]);
                            transVert.y += aboveGround;
                            allvertices.Add(transVert);
                            transVert = child.transform.TransformPoint(GroundCellMesh[2]);
                            transVert.y += aboveGround;
                            allvertices.Add(transVert);
                        }
                        if (position.Direction == Direction.SW)
                        {
                            transVert = child.transform.TransformPoint(GroundCellMesh[1]);
                            transVert.y += aboveGround;
                            allvertices.Add(transVert);
                            transVert = child.transform.TransformPoint(GroundCellMesh[2]);
                            transVert.y += aboveGround;
                            allvertices.Add(transVert);
                        }
                        if (position.Direction == Direction.NW)
                        {
                            if (isBorder)
                            {
                                transVert = lastChild.transform.TransformPoint(GroundCellMesh[5]);
                                transVert.y += aboveGround;
                                allvertices.Add(transVert);

                                transVert = lastChild.transform.TransformPoint(GroundCellMesh[4]);
                                transVert.y += aboveGround;
                                allvertices.Add(transVert);
                            }

                            transVert = child.transform.TransformPoint(GroundCellMesh[5]);
                            transVert.y += aboveGround;
                            allvertices.Add(transVert);
                            transVert = child.transform.TransformPoint(GroundCellMesh[4]);
                            transVert.y += aboveGround;
                            allvertices.Add(transVert);
                        }
                        if (position.Direction == Direction.N)
                        {
                            if (isBorder)
                            {
                                transVert = lastChild.transform.TransformPoint(GroundCellMesh[4]);
                                transVert.y += aboveGround;
                                allvertices.Add(transVert);

                                transVert = lastChild.transform.TransformPoint(GroundCellMesh[3]);
                                transVert.y += aboveGround;
                                allvertices.Add(transVert);
                            }

                            transVert = child.transform.TransformPoint(GroundCellMesh[4]);
                            transVert.y += aboveGround;
                            allvertices.Add(transVert);

                            transVert = child.transform.TransformPoint(GroundCellMesh[3]);
                            transVert.y += aboveGround;
                            allvertices.Add(transVert);
                        }
                        
                    }
                    lastChild = child;
                }
            }

            //CreateFrameBorder(position, nextPosition.Direction != position.Direction);                
        
            lineRenderer.positionCount = allvertices.Count;
            for (int i = 0; i < allvertices.Count; i++)
            {
                lineRenderer.SetPosition(i, allvertices[i]);
            }
        }

        public void Update()
        {
            if (visibleFrames.Count > 0)
                Destroy();

            bool hasEngine = UnitBase.HasEngine();

            lastPosition = UnitBase.CurrentPos;
            Position3 position3 = new Position3(UnitBase.CurrentPos);
            foreach (UnitBasePart unitBasePart in UnitBase.UnitBaseParts)
            {
                if (!hasEngine && unitBasePart.PartType == TileObjectType.PartReactor)
                {
                    List<Position3> positions = position3.CreateRing(unitBasePart.Range);
                    CreateFrame(positions);
                }
                if (!hasEngine && unitBasePart.PartType == TileObjectType.PartContainer)
                {
                    List<Position3> positions = position3.CreateRing(unitBasePart.Range);
                    CreateFrame(positions);
                }
                if (unitBasePart.PartType == TileObjectType.PartWeapon)
                {
                    if (UnitBase.HasEngine())
                    {
                        List<Position3> positions = new List<Position3>();
                        Position3 canFireAt = position3;

                        for (int i = 0; i < unitBasePart.Range; i++)
                        {
                            canFireAt = canFireAt.GetNeighbor(UnitBase.Direction);
                            positions.Add(canFireAt);
                        }

                        canFireAt = positions[positions.Count - 2];

                        Position3 canFireLeft = canFireAt.GetNeighbor(Tile.TurnLeft(canFireAt.Direction));
                        positions.Add(canFireLeft);

                        positions.AddRange(canFireAt.DrawLine(canFireLeft));

                        Position3 canFireRight = canFireAt.GetNeighbor(Tile.TurnRight(canFireAt.Direction));
                        positions.Add(canFireRight);

                        foreach (Position3 position in positions)
                        {
                            CreateTargetFrame(position);
                        }
                    }
                    else
                    {
                        List<Position3> positions = position3.CreateRing(unitBasePart.Range);
                        foreach (Position3 position in positions)
                        {
                            CreateTargetFrame(position);
                        }
                    }
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
                CreateUnitFrame(position3);
            }
            
        }

        public void Destroy()
        {
            foreach (GameObject gameObject in visibleFrames.Values)
            {
                HexGrid.Destroy(gameObject);
            }
            visibleFrames.Clear();
        }
    }
}
