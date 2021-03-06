using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class CollectBounds
    {
        private Position2 collectPosition;
        private GameFrame gameFrame;
        public CollectBounds(Position2 position, int radius)
        {
            collectRadius = radius;
            collectPosition = position;
            gameFrame = new GameFrame();
        }

        private int collectRadius;
        private List<Position2> collectedPositions = new List<Position2>();
        public IReadOnlyCollection<Position2> CollectedPositions
        {
            get
            {
                return collectedPositions;
            }
        }
        public bool IsVisible
        {
            get
            {
                return gameFrame.IsVisible;
            }
            set
            {
                gameFrame.IsVisible = value;
            }
        }
        public void Update(int playerId)
        {
            gameFrame.Destroy();
            if (collectPosition != Position2.Null)
            {
                collectedPositions.Add(collectPosition);

                Position3 position3 = new Position3(collectPosition);

                List<Position3> positions;
                if (collectRadius == 1)
                {
                    positions = new List<Position3>();
                    positions.Add(position3); 
                    gameFrame.CreateFrame(playerId, positions);
                }
                else
                {
                    positions = position3.CreateRing(collectRadius-1);
                    gameFrame.CreateFrame(playerId, positions);

                    positions = position3.GetNeighbors(collectRadius-1);
                    foreach (Position3 position31 in positions)
                    {
                        collectedPositions.Add(position31.Pos);
                        GroundCell groundCell;
                        if (HexGrid.MainGrid.GroundCells.TryGetValue(position31.Pos, out groundCell))
                        {
                            if (groundCell.TileCounter.NumberOfCollectables > 0)
                                groundCell.SetHighlighted(true);
                        }
                    }
                }
            }
        }
        public void Destroy()
        {
            foreach (Position2 position in collectedPositions)
            {
                GroundCell groundCell;
                if (HexGrid.MainGrid.GroundCells.TryGetValue(position, out groundCell))
                {
                    groundCell.SetHighlighted(false);
                }
            }
            collectedPositions.Clear();
            gameFrame.Destroy();
        }
    }

    public class GameFrame
    {
        public GameFrame()
        {
            visible = true;
        }

        private Dictionary<Position2, GameObject> visibleFrames = new Dictionary<Position2, GameObject>();
        private List<GameObject> visibleGameObjects = new List<GameObject>();
        public void CreateTargetFrame(Position2 position)
        {
            if (!visibleFrames.ContainsKey(position))
            {
                GroundCell groundCell;
                if (HexGrid.MainGrid.GroundCells.TryGetValue(position, out groundCell))
                {
                    Vector3 vector3 = groundCell.transform.position;
                    vector3.y += 0.08f;

                    GameObject previewUnitMarker = HexGrid.MainGrid.InstantiatePrefab("GroundTargetFrame");
                    previewUnitMarker.transform.SetParent(HexGrid.MainGrid.transform, false);
                    previewUnitMarker.transform.position = vector3;

                    visibleFrames.Add(position, previewUnitMarker);
                }
            }
        }

        public void CreateBuildFrame(Position3 position)
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
        public void CreateUnitFrame(Position3 position)
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

        public void CreateFrameBorder(Position3 position, bool isCorner)
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

        public void CreateFrame(int playerId, List<Position3> positions)
        {
            GameObject lineRendererObject = new GameObject();
            visibleGameObjects.Add(lineRendererObject);

            LineRenderer lineRenderer = lineRendererObject.AddComponent<LineRenderer>();
            lineRenderer.transform.SetParent(HexGrid.MainGrid.transform, false);
            lineRenderer.material = HexGrid.MainGrid.GetMaterial("Player" + playerId);
            lineRenderer.loop = true;
            lineRenderer.startWidth = 0.15f;
            lineRenderer.endWidth = 0.15f;
            lineRenderer.receiveShadows = false;
            lineRenderer.numCornerVertices = 100;
            //lineRenderer.numCapVertices = 100;
            lineRenderer.useWorldSpace = true;

            List<Vector3> allvertices = new List<Vector3>();

            Transform lastChild = null;
            float aboveGround = 0.1f;

            GroundCell groundCell;
            if (positions.Count == 1)
            {
                Vector3 transVert;
                if (HexGrid.MainGrid.GroundCells.TryGetValue(positions[0].Pos, out groundCell))
                {
                    Transform child = groundCell.GetDisplayedGroundCell();
                    transVert = child.transform.TransformPoint(GroundCellMesh[4]);
                    transVert.y += aboveGround;
                    allvertices.Add(transVert);

                    transVert = child.transform.TransformPoint(GroundCellMesh[3]);
                    transVert.y += aboveGround;
                    allvertices.Add(transVert);
                    
                    transVert = child.transform.TransformPoint(GroundCellMesh[0]);
                    transVert.y += aboveGround;
                    allvertices.Add(transVert);
                    
                    transVert = child.transform.TransformPoint(GroundCellMesh[1]);
                    transVert.y += aboveGround;
                    allvertices.Add(transVert);
                    
                    transVert = child.transform.TransformPoint(GroundCellMesh[2]);
                    transVert.y += aboveGround;
                    allvertices.Add(transVert);

                    transVert = child.transform.TransformPoint(GroundCellMesh[5]);
                    transVert.y += aboveGround;
                    allvertices.Add(transVert);
                }
            }
            else
            {
                for (int i = 0; i < positions.Count; i++)
                {
                    Position3 position = positions[i];
                    bool isBorder = false;
                    if (i > 0)
                        isBorder = position.Direction != positions[i - 1].Direction;

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
            }
            //CreateFrameBorder(position, nextPosition.Direction != position.Direction);                

            Vector3[] sm = CommandPreview.SmoothLine(allvertices.ToArray(), 0.01f);

            lineRenderer.positionCount = sm.Length;
            for (int i = 0; i < sm.Length; i++)
            {
                lineRenderer.SetPosition(i, sm[i]);
            }
        }

        public void Destroy()
        {
            foreach (GameObject gameObject in visibleGameObjects)
            {
                HexGrid.Destroy(gameObject);
            }
            visibleGameObjects.Clear();

            foreach (GameObject gameObject in visibleFrames.Values)
            {
                HexGrid.Destroy(gameObject);
            }
            visibleFrames.Clear();
        }
    }

    public class UnitBounds
    {
        public UnitBase UnitBase { get; private set; }

        private GameFrame gameFrame;

        public UnitBounds(UnitBase unitBase)
        {
            UnitBase = unitBase;
            gameFrame = new GameFrame();
        }

        public Position2 Pos
        {
            get
            {
                return lastPosition;
            }
        }

        private Position2 lastPosition;

        private bool addBuildGrid;
        public void AddBuildGrid()
        {
            addBuildGrid = true;
        }

        public bool IsVisible
        {
            get
            {
                return gameFrame.IsVisible;
            }
            set
            {
                gameFrame.IsVisible = value;
            }
        }

        public void Update()
        {
            gameFrame.Destroy();

            bool hasEngine = UnitBase.HasEngine();
            bool reactorAdded = false;
            bool containerAdded = false;
            bool radarAdded = false;
            bool weaponAdded = false;

            lastPosition = UnitBase.CurrentPos;
            Position3 position3 = new Position3(UnitBase.CurrentPos);
            foreach (UnitBasePart unitBasePart in UnitBase.UnitBaseParts)
            {
                if (!hasEngine && unitBasePart.PartType == TileObjectType.PartReactor && !reactorAdded)
                {
                    reactorAdded = true;
                    List<Position3> positions = position3.CreateRing(unitBasePart.Range);
                    gameFrame.CreateFrame(UnitBase.PlayerId, positions);
                }
                if (!hasEngine && unitBasePart.PartType == TileObjectType.PartContainer && !containerAdded)
                {
                    containerAdded = true;
                    List<Position3> positions = position3.CreateRing(unitBasePart.Range);
                    gameFrame.CreateFrame(UnitBase.PlayerId, positions);
                }
                if (!hasEngine && unitBasePart.PartType == TileObjectType.PartRadar && !radarAdded)
                {
                    radarAdded = true;
                    List<Position3> positions = position3.CreateRing(unitBasePart.Range);
                    gameFrame.CreateFrame(UnitBase.PlayerId, positions);
                }
                if (unitBasePart.PartType == TileObjectType.PartWeapon && !weaponAdded)
                {
                    weaponAdded = true;
                    List<Position2> positions = UnitBase.GetHitablePositions();
                    if (positions != null)
                    {
                        foreach (Position2 position in positions)
                        {
                            gameFrame.CreateTargetFrame(position);
                        }
                    }
                }
            }

            if (addBuildGrid)
            {
                List<Position3> positions = position3.GetNeighbors(2);
                foreach (Position3 position in positions)
                {
                    gameFrame.CreateBuildFrame(position);
                }
                gameFrame.CreateUnitFrame(position3);
            }
        }
        public void Destroy()
        {
            gameFrame.Destroy();
        }
        
    }
}
