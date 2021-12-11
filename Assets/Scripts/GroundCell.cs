using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class GroundCellBorder
    {
        public List<Position2> Positions { get; set; }
        public GameObject Borderline { get; set; }

        public void Delete()
        {
            if (Borderline != null)
            {
                HexGrid.Destroy(Borderline);
                Borderline = null;
            }
        }
    }

    public class GroundCell : MonoBehaviour
    {
        public Position2 Pos { get; set; }

        public MoveUpdateStats Stats { get; set; }

        public bool ShowPheromones { get; set; }

        public bool VisibleByPlayer { get; set; }
        //public GroundCellBorder GroundCellBorder { get; set; }

        private bool visible;
        public bool Visible
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
                    UnitBase unitbase = FindUnit();
                    if (unitbase != null)
                    {
                        unitbase.IsVisible = visible;
                    }
                }
            }
        }

        public UnitBase FindUnit()
        {
            foreach (UnitBase unitbase in HexGrid.MainGrid.BaseUnits.Values)
            {
                if (unitbase.CurrentPos == Pos)
                {
                    return unitbase;
                }
            }
            return null;
        }

        internal List<UnitCommand> UnitCommands { get; private set; }
        public List<UnitBaseTileObject> GameObjects { get; private set; }

        private GameObject markerEnergy;
        private GameObject markerToHome;
        private GameObject markerToMineral;
        private GameObject markerToEnemy;

        internal float Diffuse { get; set; }
        private float targetDiffuse;

        public GroundCell()
        {
            GameObjects = new List<UnitBaseTileObject>();
            UnitCommands = new List<UnitCommand>();
            ShowPheromones = true;
            visible = true;
            targetDiffuse = 0.1f;
            Diffuse = 0.1f;
        }

        public void UpdateColor()
        {
            foreach (UnitBaseTileObject unitBaseTileObject1 in GameObjects)
            {
                if (unitBaseTileObject1.TileObject.TileObjectType == TileObjectType.Tree)
                {

                    //renderer = unitBaseTileObject1.GameObject.GetComponent<Renderer>();
                    //if (renderer != null)
                    //renderer.material.SetFloat("Darkness", Diffuse);
                }
            }

            Renderer[] rr = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in rr)
            // Set color
            //Renderer renderer = GetComponent<Renderer>();
            {

                if (renderer.materials.Length > 1)
                {
                    renderer.materials[0].SetFloat("Darkness", Diffuse);
                    renderer.materials[1].SetFloat("Darkness", Diffuse);

                    /*
                    renderer.materials[0].SetColor("_TopTint", Color.blue);
                    renderer.materials[0].SetColor("_BottomTint", Color.blue);
                    renderer.materials[0].SetColor("_RandomColorTint", Color.blue);

                    renderer.materials[1].SetColor("_TopTint", Color.blue);
                    renderer.materials[1].SetColor("_BottomTint", Color.blue);
                    renderer.materials[1].SetColor("_RandomColorTint", Color.blue);*/
                }
                else
                {
                    renderer.material.SetFloat("Darkness", Diffuse);
                }
                /*
                foreach (UnitBaseTileObject unitBaseTileObject1 in GameObjects)
                {
                    if (unitBaseTileObject1.GameObject != null)
                    {
                        //renderer = unitBaseTileObject1.GameObject.GetComponent<Renderer>();
                        //if (renderer != null)
                            //renderer.material.SetFloat("Darkness", Diffuse);
                    }
                }*/
            }
        }

        private void CreateMarker()
        {
            if (markerEnergy == null)
            {
                GameObject markerPrefab = HexGrid.MainGrid.GetResource("Marker");
                markerEnergy = Instantiate(markerPrefab, transform, false);
                markerEnergy.name = name + "-Energy";

                markerToHome = Instantiate(markerPrefab, transform, false);
                markerToHome.name = name + "-Home";
                //MeshRenderer meshRenderer = markerToHome.GetComponent<MeshRenderer>();
                //meshRenderer.material.color = new Color(0, 0, 0.6f);

                markerToMineral = Instantiate(markerPrefab, transform, false);
                markerToMineral.name = name + "-Mineral";
                //meshRenderer = markerToMineral.GetComponent<MeshRenderer>();
                //meshRenderer.material.color = new Color(0, 0.4f, 0);

                markerToEnemy = Instantiate(markerPrefab, transform, false);
                markerToEnemy.name = name + "-Mineral";
                //meshRenderer = markerToEnemy.GetComponent<MeshRenderer>();
                //meshRenderer.material.color = new Color(0.4f, 0, 0);
            }
        }

        internal void UpdatePheromones(MapPheromone mapPheromone)
        {
            if (!ShowPheromones)
                return;
            if (mapPheromone == null)
            {
                if (markerEnergy != null)
                {
                    markerEnergy.transform.position = transform.position;
                }
                if (markerToHome != null)
                {
                    markerToHome.transform.position = transform.position;
                }
                if (markerToMineral != null)
                {
                    markerToMineral.transform.position = transform.position;
                }
                if (markerToEnemy != null)
                {
                    markerToEnemy.transform.position = transform.position;
                }
            }
            else
            {
                if (markerEnergy == null)
                {
                    CreateMarker();
                }

                /*
                if (mapPheromone.IntensityContainer > 0)
                {
                    Vector3 position = transform.position;
                    position.y += 0.054f + (0.2f * mapPheromone.IntensityContainer);
                    position.x += 0.1f;
                    markerToHome.transform.position = position;
                }
                else
                {
                    Vector3 position = transform.position;
                    position.y -= 1;
                    position.x += 0.1f;
                    markerToHome.transform.position = position;
                }
                */
                /*
                
                if (mapPheromone.IntensityToMineral > 0)
                {
                    Vector3 position = transform.position;
                    position.y += 0.054f + (0.2f * mapPheromone.IntensityToMineral);

                    if (mapPheromone.IntensityToMineral == 1)
                        position.y += 0.9f;

                    position.x += 0.2f;
                    markerToMineral.transform.position = position;
                }
                else
                {
                    Vector3 position = transform.position;
                    position.y -= 1;
                    position.x += 0.2f;
                    markerToMineral.transform.position = position;
                }
                */
                /*
                if (mapPheromone.IntensityToEnemy > 0)
                {
                    Vector3 position = transform.position;
                    position.y += 0.054f + (0.2f * mapPheromone.IntensityToEnemy);
                    position.x += 0.3f;
                    markerToEnemy.transform.position = position;
                }
                else
                {
                    Vector3 position = transform.position;
                    position.y -= 1;
                    position.x += 0.3f;
                    markerToEnemy.transform.position = position;
                }*/

                
                float highestEnergy = -1;
                int highestPlayerId = 0;

                foreach (MapPheromoneItem mapPheromoneItem in mapPheromone.PheromoneItems)
                {
                    if (mapPheromoneItem.PheromoneType == Engine.Ants.PheromoneType.Energy)
                    {
                        if (mapPheromoneItem.Intensity >= highestEnergy)
                        {
                            highestEnergy = mapPheromoneItem.Intensity;
                            highestPlayerId = mapPheromoneItem.PlayerId;
                        }
                    }
                }

                if (highestEnergy > 0)
                {
                    Vector3 position = transform.position;
                    position.y += 0.054f + (0.2f * highestEnergy);
                    markerToEnemy.transform.position = position;
                    UnitBase.SetPlayerColor(highestPlayerId, markerToEnemy);
                }
                else
                {
                    Vector3 position = transform.position;
                    position.y -= 1;
                    markerToEnemy.transform.position = position;
                }
                
            }
        }



        internal void SetGroundMaterial()
        {


            /*
            for (int i = 0; i < unit.transform.childCount; i++)
            {
                GameObject child = unit.transform.GetChild(i).gameObject;
                if (!child.name.StartsWith("Mineral") && !child.name.StartsWith("Ammo"))
                    SetPlayerColor(hexGrid, playerId, child);
            }*/
            Color color = Color.black;

            //string materialName;
            if (Stats.MoveUpdateGroundStat.IsUnderwater)
            {
                if (Stats.MoveUpdateGroundStat.Height < 0.1f)
                {
                    // Beach
                    if (ColorUtility.TryParseHtmlString("#D3B396", out color))
                    {
                    }
                }
                else
                {
                    // Sea
                    //materialName = "Water";
                    if (ColorUtility.TryParseHtmlString("#278BB2", out color))
                    {
                    }
                }
            }
            else
            {
                //materialName = "Dirt";

                /*
                if (tileObject.TileObjectType == TileObjectType.Gras)
                {
                    materialName = "Grass";
                }
                else if (tileObject.TileObjectType == TileObjectType.Bush)
                {
                    materialName = "GrassDark";
                }
                else if (tileObject.TileObjectType == TileObjectType.Tree)
                {
                    materialName = "Wood";
                }
                else
                {
                    int x = 0;
                }
                */

                if (Stats.MoveUpdateGroundStat.IsHill())
                {
                    //materialName = "Hill";
                }
                else if (Stats.MoveUpdateGroundStat.IsRock())
                {
                    //materialName = "Rock";
                }
                else if (Stats.MoveUpdateGroundStat.IsSand())
                {
                    if (ColorUtility.TryParseHtmlString("#D3B396", out color))
                    {
                    }
                    //materialName = "Sand";
                }
                else if (Stats.MoveUpdateGroundStat.IsDarkSand())
                {
                    if (ColorUtility.TryParseHtmlString("#9D7C68", out color))
                    {
                    }
                    //materialName = "DarkSand";
                }
                else if (Stats.MoveUpdateGroundStat.IsDarkWood())
                {
                    //materialName = "DarkWood";
                }
                else if (Stats.MoveUpdateGroundStat.IsWood())
                {
                    if (ColorUtility.TryParseHtmlString("#45502D", out color))
                    {
                    }
                    //materialName = "Wood";
                }
                else if (Stats.MoveUpdateGroundStat.IsLightWood())
                {
                    if (ColorUtility.TryParseHtmlString("#60703C", out color))
                    {
                    }
                    //materialName = "LightWood"; 
                }
                else if (Stats.MoveUpdateGroundStat.IsGrassDark())
                {
                    if (ColorUtility.TryParseHtmlString("#6F803F", out color))
                    {
                    }
                    //materialName = "GrassDark";
                }
                else if (Stats.MoveUpdateGroundStat.IsGras())
                {
                    ColorUtility.TryParseHtmlString("#899E52", out color);

                    //materialName = "Grass";
                }
                else
                {
                    if (ColorUtility.TryParseHtmlString("#513A31", out color))
                    {
                    }

                }
            }
            if (Stats.MoveUpdateGroundStat.IsBorder)

            //if (Stats.MoveUpdateGroundStat.IsBorder)
            //if (Stats.MoveUpdateGroundStat.ZoneId > 0)
            {
                //materialName = "DarkSand";
                //color = Color.red;
            }

            if (Stats.MoveUpdateGroundStat.IsUnderwater)
            {
                ColorUtility.TryParseHtmlString("#006080", out color);
            }

            Renderer[] rr = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in rr)
            {
                renderer.material.SetColor("SurfaceColor", color);
            }
            /*
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.SetColor("SurfaceColor", color);
                if (highlightEffect != null)
                {
                    highlightEffect.innerGlowColor = color;
                }
            }*/

        }

        internal static void CreateBorderLines(List<GroundCellBorder> groundCellBorders, List<Position2> groundCellBorderChanged)
        {
            if (groundCellBorderChanged.Count == 0)
                return;

            List<Position2> visitedBorders = new List<Position2>();

            foreach (Position2 position2 in groundCellBorderChanged)
            {
                GroundCell gc = HexGrid.MainGrid.GroundCells[position2];
                if (!visitedBorders.Contains(gc.Pos))
                {
                    if (!gc.IsBorder)
                    {
                        visitedBorders.Add(gc.Pos);
                        foreach (GroundCellBorder groundCellBorder in groundCellBorders)
                        {
                            if (groundCellBorder.Positions.Contains(gc.Pos))
                            {
                                groundCellBorder.Delete();
                                groundCellBorders.Remove(groundCellBorder);
                                break;
                            }
                        }
                    }
                    else
                    {
                        List<Position2> visitedStartBorders = new List<Position2>();
                        visitedStartBorders.Add(position2);

                        bool startGcFound = false;
                        while (!startGcFound)
                        {
                            startGcFound = true;

                            // Find start border tile
                            Position3 position3 = new Position3(gc.Pos);
                            for (int i = position3.Neighbors.Count - 1; i >= 0; i--)
                            {
                                Position3 n = position3.Neighbors[i];

                                GroundCell neighbor;
                                if (!HexGrid.MainGrid.GroundCells.TryGetValue(n.Pos, out neighbor))
                                    continue;
                                if (!visitedStartBorders.Contains(neighbor.Pos) &&
                                    neighbor.IsBorder && neighbor.Stats.MoveUpdateGroundStat.Owner == gc.Stats.MoveUpdateGroundStat.Owner)
                                {
                                    if (!visitedBorders.Contains(neighbor.Pos))
                                    {
                                        visitedStartBorders.Add(neighbor.Pos);
                                        startGcFound = false;
                                        gc = neighbor;
                                    }
                                    break;
                                }
                            }
                        }
                        GroundCellBorder groundCellBorder = gc.CreateBorderLine(visitedBorders);
                        if (groundCellBorder != null)
                        {
                            groundCellBorders.Add(groundCellBorder);
                        }
                    }
                }
            }
        }

        internal Transform GetDisplayedGroundCell()
        {
            LODGroup lodGroup = GetComponent<LODGroup>();
            if (lodGroup != null)
            {
                Transform lodTransform = lodGroup.transform;
                foreach (Transform child in lodTransform)
                {
                    if (!child.name.StartsWith("HexCell"))
                        continue;
                    return child;
                }
            }

            return null;
        }

        void UpdateArrow(LineRenderer cachedLineRenderer)
        {
            float PercentHead = 0.4f;
            Vector3 ArrowOrigin = Vector3.back;
            Vector3 ArrowTarget = Vector3.back;

            if (cachedLineRenderer == null)
                cachedLineRenderer = this.GetComponent<LineRenderer>();
            cachedLineRenderer.widthCurve = new AnimationCurve(
                new Keyframe(0, 0.4f)
                , new Keyframe(0.999f - PercentHead, 0.4f)  // neck of arrow
                , new Keyframe(1 - PercentHead, 1f)  // max width of arrow head
                , new Keyframe(1, 0f));  // tip of arrow
            cachedLineRenderer.SetPositions(new Vector3[] {
              ArrowOrigin
              , Vector3.Lerp(ArrowOrigin, ArrowTarget, 0.999f - PercentHead)
              , Vector3.Lerp(ArrowOrigin, ArrowTarget, 1 - PercentHead)
              , ArrowTarget });
        }

        void DrawQuadraticBezierCurve(LineRenderer lineRenderer, Vector3 point0, Vector3 point1, Vector3 point2)
        {
            lineRenderer.positionCount = 200;
            float t = 0f;
            Vector3 B = new Vector3(0, 0, 0);
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                B = (1 - t) * (1 - t) * point0 + 2 * (1 - t) * t * point1 + t * t * point2;
                lineRenderer.SetPosition(i, B);
                t += (1 / (float)lineRenderer.positionCount);
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
                    /*
                    groundCellMesh.Add(new Vector3(0.86f, 0, 0.5f)); // 0
                    groundCellMesh.Add(new Vector3(0.86f, 0, -0.5f)); // 1
                    groundCellMesh.Add(new Vector3(0, 0, -1)); // 2
                    groundCellMesh.Add(new Vector3(0, 0, 1)); // 3
                    groundCellMesh.Add(new Vector3(-0.86f, 0, 0.5f)); // 4
                    groundCellMesh.Add(new Vector3(-0.86f, 0, -0.5f)); // 5
                    */
                    /* Smaller*/
                    groundCellMesh.Add(new Vector3(0.81f, 0, 0.45f)); // 0
                    groundCellMesh.Add(new Vector3(0.81f, 0, -0.45f)); // 1
                    groundCellMesh.Add(new Vector3(0, 0, -1)); // 2
                    groundCellMesh.Add(new Vector3(0, 0, 1)); // 3
                    groundCellMesh.Add(new Vector3(-0.81f, 0, 0.45f)); // 4
                    groundCellMesh.Add(new Vector3(-0.81f, 0, -0.45f)); // 5

                }
                return groundCellMesh;
            }
        }

        internal GroundCellBorder CreateBorderLine(List<Position2> visitedBorders)
        {
            GroundCellBorder groundCellBorder = null;
            if (Stats.MoveUpdateGroundStat.Owner == 0)
            {
                return null;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("CreateBorderLine at " + Pos.ToString());

            List<Vector3> allvertices = new List<Vector3>();
            List<Position2> positions = new List<Position2>();

            CreateBorderLine(positions, allvertices, visitedBorders, sb);

            if (allvertices.Count > 1)
            {
                groundCellBorder = new GroundCellBorder();

                GameObject lineRendererObject = new GameObject();
                lineRendererObject.name = "Borderline";

                LineRenderer lineRenderer = lineRendererObject.AddComponent<LineRenderer>();
                lineRenderer.transform.SetParent(HexGrid.MainGrid.transform, false);
                lineRenderer.material = HexGrid.MainGrid.GetMaterial("Player" + Stats.MoveUpdateGroundStat.Owner);
                //lineRenderer.material = HexGrid.MainGrid.GetMaterial("Shield");
                //lineRenderer.loop = true;
                //lineRenderer.startColor = Color.yellow;
                //lineRenderer.endColor = Color.yellow;
                //lineRenderer.alignment = LineAlignment.View;
                lineRenderer.allowOcclusionWhenDynamic = true;

                lineRenderer.startWidth = 0.25f;
                lineRenderer.endWidth = 0.25f;
                lineRenderer.receiveShadows = false;
                lineRenderer.numCornerVertices = 10;
                lineRenderer.useWorldSpace = true;

                groundCellBorder.Positions = positions;
                groundCellBorder.Borderline = lineRendererObject;

                lineRenderer.positionCount = allvertices.Count;
                for (int i = 0; i < allvertices.Count; i++)
                {
                    lineRenderer.SetPosition(i, allvertices[i]);
                }

                Debug.Log(sb.ToString());
            }
            return groundCellBorder;
        }

        internal void CreateBorderLine(List<Position2> positions, List<Vector3> allvertices, List<Position2> visitedBorders, StringBuilder sb)
        {
            Vector3 transVert;
            float aboveGround = 0.08f;

            //Transform groundCellObject = GetDisplayedGroundCell();
            visitedBorders.Add(Pos);

            bool updateNE = false;
            bool updateN = false;
            bool updateNW = false;
            bool updateSE = false;
            bool updateS = false;
            bool updateSW = false;

            GroundCell neighborBorder = null;

            Position3 position3 = new Position3(Pos);
            foreach (Position3 n in position3.Neighbors)
            {
                GroundCell neighbor;
                if (!HexGrid.MainGrid.GroundCells.TryGetValue(n.Pos, out neighbor))
                    continue;
                if (neighbor.IsBorder && neighbor.Stats.MoveUpdateGroundStat.Owner == Stats.MoveUpdateGroundStat.Owner &&
                    /*neighbor.GroundCellBorder == null && */ neighborBorder == null)
                {
                    if (!visitedBorders.Contains(neighbor.Pos))
                        neighborBorder = neighbor;
                }

                if (neighbor.Stats.MoveUpdateGroundStat.Owner != Stats.MoveUpdateGroundStat.Owner)
                {
                    /*
                    if (Pos.X == 64 && Pos.Y == 80)
                    {
                        Renderer[] rr = neighbor.GetComponentsInChildren<Renderer>();
                        foreach (Renderer renderer in rr)
                        {
                            renderer.material.SetColor("SurfaceColor", Color.cyan);
                        }
                    }*/
                    if (n.Direction == Direction.N) updateN = true;
                    if (n.Direction == Direction.NE) updateNE = true;
                    if (n.Direction == Direction.NW) updateNW = true;
                    if (n.Direction == Direction.S) updateS = true;
                    if (n.Direction == Direction.SE) updateSE = true;
                    if (n.Direction == Direction.SW) updateSW = true;
                }
            }

            positions.Add(Pos);

            // Middle
            transVert = transform.position;
            transVert.y += aboveGround;
            allvertices.Add(transVert);


            /*
            if (updateN && updateNW)
            {
                // other direction
                if (updateNW)
                {
                    transVert = groundCellObject.TransformPoint(GroundCellMesh[4]);
                    transVert.y += aboveGround;
                    allvertices.Add(transVert);

                    transVert = groundCellObject.TransformPoint(GroundCellMesh[3]);
                    transVert.y += aboveGround;
                    allvertices.Add(transVert);
                }
                if (updateSW)
                {
                    transVert = groundCellObject.TransformPoint(GroundCellMesh[5]);
                    transVert.y += aboveGround;
                    allvertices.Add(transVert);

                    transVert = groundCellObject.TransformPoint(GroundCellMesh[4]);
                    transVert.y += aboveGround;
                    allvertices.Add(transVert);
                }
                if (updateS)
                {
                    transVert = groundCellObject.TransformPoint(GroundCellMesh[2]);
                    transVert.y += aboveGround;
                    allvertices.Add(transVert);

                    transVert = groundCellObject.TransformPoint(GroundCellMesh[5]);
                    transVert.y += aboveGround;
                    allvertices.Add(transVert);
                }
                if (updateSE)
                {
                    transVert = groundCellObject.TransformPoint(GroundCellMesh[1]);
                    transVert.y += aboveGround;
                    allvertices.Add(transVert);

                    transVert = groundCellObject.TransformPoint(GroundCellMesh[2]);
                    transVert.y += aboveGround;
                    allvertices.Add(transVert);
                }
                if (updateNE)
                {
                    transVert = groundCellObject.TransformPoint(GroundCellMesh[0]);
                    transVert.y += aboveGround;
                    allvertices.Add(transVert);

                    transVert = groundCellObject.TransformPoint(GroundCellMesh[1]);
                    transVert.y += aboveGround;
                    allvertices.Add(transVert);
                }
                if (updateN)
                {
                    transVert = groundCellObject.TransformPoint(GroundCellMesh[3]);
                    transVert.y += aboveGround;
                    allvertices.Add(transVert);

                    transVert = groundCellObject.TransformPoint(GroundCellMesh[0]);
                    transVert.y += aboveGround;
                    allvertices.Add(transVert);
                }
            }
            else
            {
                if (updateN)
                {
                    transVert = groundCellObject.TransformPoint(GroundCellMesh[3]);
                    transVert.y += aboveGround;
                    allvertices.Add(transVert);

                    transVert = groundCellObject.TransformPoint(GroundCellMesh[0]);
                    transVert.y += aboveGround;
                    allvertices.Add(transVert);
                }
                if (updateNE)
                {
                    transVert = groundCellObject.TransformPoint(GroundCellMesh[0]);
                    transVert.y += aboveGround;
                    allvertices.Add(transVert);

                    transVert = groundCellObject.TransformPoint(GroundCellMesh[1]);
                    transVert.y += aboveGround;
                    allvertices.Add(transVert);
                }

                if (updateSE)
                {
                    transVert = groundCellObject.TransformPoint(GroundCellMesh[1]);
                    transVert.y += aboveGround;
                    allvertices.Add(transVert);

                    transVert = groundCellObject.TransformPoint(GroundCellMesh[2]);
                    transVert.y += aboveGround;
                    allvertices.Add(transVert);
                }

                if (updateS)
                {
                    transVert = groundCellObject.TransformPoint(GroundCellMesh[2]);
                    transVert.y += aboveGround;
                    allvertices.Add(transVert);

                    transVert = groundCellObject.TransformPoint(GroundCellMesh[5]);
                    transVert.y += aboveGround;
                    allvertices.Add(transVert);
                }
                if (updateSW)
                {
                    transVert = groundCellObject.TransformPoint(GroundCellMesh[5]);
                    transVert.y += aboveGround;
                    allvertices.Add(transVert);

                    transVert = groundCellObject.TransformPoint(GroundCellMesh[4]);
                    transVert.y += aboveGround;
                    allvertices.Add(transVert);
                }

                if (updateNW)
                {
                    transVert = groundCellObject.TransformPoint(GroundCellMesh[4]);
                    transVert.y += aboveGround;
                    allvertices.Add(transVert);

                    transVert = groundCellObject.TransformPoint(GroundCellMesh[3]);
                    transVert.y += aboveGround;
                    allvertices.Add(transVert);
                }
            }
            */
            if (neighborBorder != null)
            {
                sb.AppendLine("Neighbor : " + neighborBorder.Pos.ToString());
                neighborBorder.CreateBorderLine(positions, allvertices, visitedBorders, sb);
            }
        }

        public bool IsBorder
        {
            get
            {
                //return !(Stats.MoveUpdateGroundStat.Owner == 0 || !Stats.MoveUpdateGroundStat.IsBorder || Stats.MoveUpdateGroundStat.IsUnderwater);
                return Stats.MoveUpdateGroundStat.IsBorder;
            }
        }

        internal Position2 UpdateGround(MoveUpdateStats moveUpdateStats)
        {
            bool wasBorder = IsBorder;

            Stats = moveUpdateStats;
            UpdateCache();

            Vector3 vector3 = transform.localPosition;
            vector3.y = Stats.MoveUpdateGroundStat.Height + 0.3f;
            transform.localPosition = vector3;

            CreateDestructables(false);

            Position2 borderCell = Position2.Null;

            if (wasBorder != IsBorder)
            {
                // Changed border, need update
                borderCell = Pos;
            }
            return borderCell;
        }

        internal GroundCell GetNeighbor(Direction direction)
        {
            Position3 cubePosition = new Position3(Pos);
            Position3 n = cubePosition.GetNeighbor(direction);

            GroundCell neighbor;
            HexGrid.MainGrid.GroundCells.TryGetValue(n.Pos, out neighbor);
            return neighbor;
        }

        internal TileObject FindTileObject(List<TileObject> tileObjects, TileObjectType tileObjectType)
        {
            foreach (TileObject tileObject in tileObjects)
            {
                if (tileObject.TileObjectType == tileObjectType)
                {
                    return tileObject;
                }
            }
            return null;
        }

        internal UnitBaseTileObject AddDestructable(List<UnitBaseTileObject> destroyedTileObjects, TileObject tileObject)
        {
            foreach (UnitBaseTileObject exisitingDestructable in destroyedTileObjects)
            {
                if (exisitingDestructable.TileObject.TileObjectType == tileObject.TileObjectType)
                {
                    destroyedTileObjects.Remove(exisitingDestructable);
                    return null;
                }
            }
            GameObject destructable;

            destructable = HexGrid.MainGrid.CreateDestructable(transform, tileObject);
            if (destructable != null)
            {
                destructable.transform.Rotate(Vector3.up, Random.Range(0, 360));
            }
            UnitBaseTileObject unitBaseTileObject = new UnitBaseTileObject();
            unitBaseTileObject.GameObject = destructable;
            unitBaseTileObject.TileObject = tileObject.Copy();

            GameObjects.Add(unitBaseTileObject);
            return unitBaseTileObject;
        }

        internal void AddDestructableItems(int itemsinLargeMineral, List<UnitBaseTileObject> destroyedTileObjects, TileObjectType tileObjectType, TileObjectKind tileObjectKind)
        {
            bool found = false;
            foreach (UnitBaseTileObject exisitingDestructable in destroyedTileObjects)
            {
                if (exisitingDestructable.TileObject.TileObjectType == tileObjectType &&
                    exisitingDestructable.TileObject.TileObjectKind == tileObjectKind)
                {
                    destroyedTileObjects.Remove(exisitingDestructable);
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                GameObject destructable;
                TileObject largeMineral = new TileObject(tileObjectType, Direction.C);
                largeMineral.TileObjectKind = tileObjectKind;

                destructable = HexGrid.MainGrid.CreateDestructable(transform, largeMineral);
                if (destructable != null)
                {
                    destructable.transform.Rotate(Vector3.up, Random.Range(0, 360));
                    destructable.name = largeMineral.TileObjectType.ToString();
                }
                UnitBaseTileObject unitBaseTileObject = new UnitBaseTileObject();
                unitBaseTileObject.GameObject = destructable;
                unitBaseTileObject.TileObject = largeMineral;

                GameObjects.Add(unitBaseTileObject);
            }
        }

        internal void CreateDestructables(bool init)
        {
            SetGroundMaterial();
            //return;

            List<UnitBaseTileObject> addedTileObjects = new List<UnitBaseTileObject>();
            List<UnitBaseTileObject> destroyedTileObjects = new List<UnitBaseTileObject>();
            destroyedTileObjects.AddRange(GameObjects);

            int countMinerals = 0;
            foreach (TileObject tileObject in Stats.MoveUpdateGroundStat.TileObjects)
            {
                if (tileObject.TileObjectType == TileObjectType.Mineral)
                {
                    countMinerals++;
                }
                else if (tileObject.Direction == Direction.C && tileObject.TileObjectType != TileObjectType.Mineral)
                {

                }
                else
                {
                    UnitBaseTileObject addedObject = AddDestructable(destroyedTileObjects, tileObject);
                    if (addedObject != null)
                        addedTileObjects.Add(addedObject);
                }
            }

            if (countMinerals > 0)
            {
                List<TileObject> tileObjects = new List<TileObject>();
                foreach (TileObject tileObject1 in Stats.MoveUpdateGroundStat.TileObjects)
                {
                    if (tileObject1.TileObjectType == TileObjectType.Mineral)
                        tileObjects.Add(tileObject1);
                }
                int itemsinLargeMineral = 5;
                int itemsinBlockingMineral = Tile.BlockPathItemCount;
                while (tileObjects.Count > 0)
                {
                    if (tileObjects.Count > itemsinBlockingMineral)
                    {
                        AddDestructableItems(itemsinBlockingMineral, destroyedTileObjects, TileObjectType.Mineral, TileObjectKind.Block);

                        for (int i = 0; i < itemsinBlockingMineral; i++)
                            tileObjects.RemoveAt(0);
                        countMinerals -= itemsinBlockingMineral;
                    }
                    else if (tileObjects.Count > itemsinLargeMineral)
                    {
                        AddDestructableItems(itemsinLargeMineral, destroyedTileObjects, TileObjectType.Mineral, TileObjectKind.Many);

                        for (int i = 0; i < itemsinLargeMineral; i++)
                            tileObjects.RemoveAt(0);
                        countMinerals -= itemsinLargeMineral;
                    }
                    else
                    {
                        TileObject tileObject = FindTileObject(tileObjects, TileObjectType.Mineral);
                        if (tileObject != null)
                        {
                            tileObjects.Remove(tileObject);
                            AddDestructable(destroyedTileObjects, tileObject);
                        }
                        countMinerals--;
                    }
                }
            }

            if (init)
            {
                UpdateColor();
            }
            else
            {
                if (Visible)
                {
                    if (VisibleByPlayer)
                        targetDiffuse = 0.8f;
                    else
                        targetDiffuse = 0.3f;

                }
                else
                {
                    targetDiffuse = 0.1f;

                }
                if (targetDiffuse < (Diffuse + 0.03f) || targetDiffuse > (Diffuse - 0.03f))
                {
                    StartCoroutine(UpdateColorLerp());
                }
                //    UpdateColor();
            }
            if (visible)
            {
                foreach (UnitBaseTileObject destructable in destroyedTileObjects)
                {
                    if (destructable.GameObject != null)
                    {
                        if (destructable.TileObject.TileObjectType == TileObjectType.Mineral)
                        {
                            Destroy(destructable.GameObject);
                        }
                        else
                        {
                            StartCoroutine(FadeOutDestructable(destructable.GameObject, destructable.GameObject.transform.position.y - 0.1f));
                            GameObjects.Remove(destructable);
                        }
                    }
                }
                foreach (UnitBaseTileObject destructable in addedTileObjects)
                {
                    break;
                    if (destructable.GameObject != null)
                    {
                        float amount = 0.0001f;
                        float sink = 0.1f;

                        if (destructable.TileObject.TileObjectType == TileObjectType.Gras)
                        {
                            sink = 0.1f;
                            amount = 0.0001f;
                        }
                        else if (destructable.TileObject.TileObjectType == TileObjectType.TreeTrunk)
                        {
                            amount = 0;
                        }
                        else
                        {
                            sink = 1f;
                            amount = 0.01f;
                        }

                        // Raise
                        if (amount != 0)
                        {
                            Vector3 vector3 = destructable.GameObject.transform.position;
                            float raiseTo = vector3.y;

                            vector3.y -= sink;
                            destructable.GameObject.transform.position = vector3;

                            StartCoroutine(FadeInDestructable(destructable.GameObject, raiseTo, amount));
                        }
                    }
                }
            }
        }

        private IEnumerator UpdateColorLerp()
        {
            while (targetDiffuse < (Diffuse - 0.03f) || targetDiffuse > (Diffuse + 0.03f))
            {
                Diffuse = Mathf.Lerp(Diffuse, targetDiffuse, 0.03f);
                UpdateColor();
                yield return null;
            }
            yield break;
        }
        private IEnumerator FadeInDestructable(GameObject gameObject, float raiseTo, float amount)
        {
            while (gameObject.transform.position.y < raiseTo)
            {
                if (gameObject == null)
                    yield break;
                Vector3 pos = gameObject.transform.position;
                pos.y += amount;
                gameObject.transform.position = pos;
                yield return null;
            }
            HexGrid.Destroy(gameObject);
            yield break;
        }
        private IEnumerator FadeOutDestructable(GameObject gameObject, float sinkTo)
        {
            while (gameObject.transform.position.y > sinkTo)
            {
                if (gameObject == null)
                    yield break;
                Vector3 pos = gameObject.transform.position;
                pos.y -= 0.0001f;
                gameObject.transform.position = pos;
                yield return null;
            }
            HexGrid.Destroy(gameObject);
            yield break;
        }

        public bool IsHighlighted { get; private set; }
        internal void SetHighlighted(bool isHighlighted)
        {
            if (IsHighlighted != isHighlighted)
            {
                IsHighlighted = isHighlighted;

            }
        }

        private List<CommandPreview> cellGameCommands = new List<CommandPreview>();

        public bool ClearCommands()
        {
            List<CommandPreview> deletedCommands = new List<CommandPreview>();
            foreach (CommandPreview cellGameCommand in cellGameCommands)
            {
                if (cellGameCommand.Touched == false)
                {
                    cellGameCommand.Delete();
                    deletedCommands.Add(cellGameCommand);
                }
            }
            foreach (CommandPreview deletedCellGameCommand in deletedCommands)
            {
                cellGameCommands.Remove(deletedCellGameCommand);
                HexGrid.MainGrid.CommandPreviews.Remove(deletedCellGameCommand);
            }
            return cellGameCommands.Count == 0;
        }

        public void UntouchCommands()
        {
            foreach (CommandPreview cellGameCommand in cellGameCommands)
            {
                cellGameCommand.Touched = false;
            }
        }

        public CommandPreview RemoveGameCommand(MapGameCommand gameCommand)
        {
            foreach (CommandPreview checkCellGameCommand in cellGameCommands)
            {
                if (checkCellGameCommand.GameCommand.TargetPosition == gameCommand.TargetPosition &&
                    //checkCellGameCommand.GameCommand.GameCommandType == gameCommand.GameCommandType && Attack -> Move
                    checkCellGameCommand.GameCommand.PlayerId == gameCommand.PlayerId)
                {
                    cellGameCommands.Remove(checkCellGameCommand);
                    return checkCellGameCommand;
                }
            }
            return null;
        }

        private bool cacheUpdated;
        private void UpdateCache()
        {
            cacheUpdated = true;
            mineralCache = 0;
            numberOfCollectablesCache = 0;

            canBuild = true;


            if (Stats.MoveUpdateGroundStat.IsUnderwater)
                canBuild = false;

            canMove = canBuild;
            foreach (TileObject tileObject in Stats.MoveUpdateGroundStat.TileObjects)
            {
                if (tileObject.TileObjectType == TileObjectType.Mineral)
                    mineralCache++;

                if (TileObject.IsTileObjectTypeObstacle(tileObject.TileObjectType))
                {
                    canBuild = false;
                    canMove = false;
                    break;
                }
                if (TileObject.IsTileObjectTypeCollectable(tileObject.TileObjectType))
                {
                    numberOfCollectablesCache++;
                    if (tileObject.TileObjectType != TileObjectType.Mineral)
                        canMove = false;
                }
            }
            if (mineralCache >= 20)
            {
                canBuild = false;
            }
        }

        private int mineralCache;
        private int numberOfCollectablesCache;
        private bool canBuild;
        private bool canMove;
        public int NumberOfCollectables
        {
            get
            {
                if (!cacheUpdated)
                {
                    UpdateCache();
                }
                return numberOfCollectablesCache;
            }
        }
        public int Minerals
        {
            get
            {
                if (!cacheUpdated)
                {
                    UpdateCache();
                }
                return mineralCache;
            }
        }
        public void UpdateMoveCommand(CommandPreview commandPreview)
        {
            cellGameCommands.Add(commandPreview);
        }

        public CommandPreview UpdateCommands(MapGameCommand gameCommand, CommandPreview commandPreview)
        {
            CommandPreview cellGameCommand = null;

            if (gameCommand.GameCommandType == GameCommandType.Cancel ||
                gameCommand.GameCommandType == GameCommandType.Move)
            {
                foreach (CommandPreview checkCellGameCommand in cellGameCommands)
                {
                    if (checkCellGameCommand.GameCommand.TargetPosition == gameCommand.TargetPosition &&
                        checkCellGameCommand.GameCommand.PlayerId == gameCommand.PlayerId)
                    {
                        checkCellGameCommand.Touched = false;
                    }
                }
                return null;
            }
            else
            {
                foreach (CommandPreview checkCellGameCommand in cellGameCommands)
                {
                    if (checkCellGameCommand.GameCommand.TargetPosition == gameCommand.TargetPosition &&
                        checkCellGameCommand.GameCommand.PlayerId == gameCommand.PlayerId &&
                        checkCellGameCommand.GameCommand.GameCommandType == gameCommand.GameCommandType)
                    {
                        cellGameCommand = checkCellGameCommand;
                        cellGameCommand.Touched = true;

                        if (HexGrid.MainGrid.MoveCounter <= checkCellGameCommand.ValidAfterThisMoveNr)
                        {
                            // commands returned may contain an old version
                        }
                        else
                        {
                            if (cellGameCommand.UpdateCommandPreview(gameCommand))
                            {
                                cellGameCommand.SetPosition(this);
                            }
                        }
                        break;
                    }
                }
            }
            if (cellGameCommand == null)
            {
                if (commandPreview == null)
                {
                    cellGameCommand = new CommandPreview();

                    cellGameCommand.CreateCommandPreview(gameCommand);
                    cellGameCommand.SetActive(false);
                    cellGameCommand.SetPosition(this);
                    cellGameCommand.Touched = true;
                    HexGrid.MainGrid.CommandPreviews.Add(cellGameCommand);
                }
                else
                {
                    cellGameCommand = commandPreview;
                }

                if (cellGameCommand.Command != null)
                {
                    MeshRenderer meshRenderer = cellGameCommand.Command.GetComponent<MeshRenderer>();
                    if (meshRenderer != null)
                    {
                        if (HexMapEditor.IsPlaying)
                        {
                            if (meshRenderer.materials.Length == 1)
                            {
                                meshRenderer.material.SetColor("Color_main", UnitBase.GetPlayerColor(gameCommand.PlayerId));
                            }
                        }
                    }
                }
                cellGameCommands.Add(cellGameCommand);
            }
            return cellGameCommand;
        }
    }

    public class CellGameCommandx
    {
        public UnitBase GhostUnit { get; set; }
        public MapGameCommand GameCommand { get; set; }
        public GameObject Command { get; set; }
        public bool Touched { get; set; }
    }

}