using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class GroundCell : MonoBehaviour
    {
        public ulong Pos { get; set; }

        public MoveUpdateStats Stats { get; set; }

        public bool ShowPheromones { get; set; }

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

                    foreach (UnitBase unitbase in HexGrid.MainGrid.BaseUnits.Values)
                    {
                        if (unitbase.CurrentPos == Pos)
                        {
                            unitbase.IsVisible = visible;
                        }
                    }

                    gameObject.SetActive(visible);
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

        public GroundCell()
        {
            GameObjects = new List<UnitBaseTileObject>();

            UnitCommands = new List<UnitCommand>();
            ShowPheromones = true;
            visible = true;
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
                MeshRenderer meshRenderer = markerToHome.GetComponent<MeshRenderer>();
                meshRenderer.material.color = new Color(0, 0, 0.6f);

                markerToMineral = Instantiate(markerPrefab, transform, false);
                markerToMineral.name = name + "-Mineral";
                meshRenderer = markerToMineral.GetComponent<MeshRenderer>();
                meshRenderer.material.color = new Color(0, 0.4f, 0);

                markerToEnemy = Instantiate(markerPrefab, transform, false);
                markerToEnemy.name = name + "-Mineral";
                meshRenderer = markerToEnemy.GetComponent<MeshRenderer>();
                meshRenderer.material.color = new Color(0.4f, 0, 0);
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
                if (mapPheromone.IntensityToWork > 0)
                {
                    Vector3 position = transform.position;
                    position.y += 0.054f + (0.2f * mapPheromone.IntensityToWork);
                    position.x += 0.1f;
                    markerToHome.transform.position = position;
                }
                else
                {
                    Vector3 position = transform.position;
                    position.y -= 1;
                    position.x += 0.1f;
                    markerToHome.transform.position = position;
                }*/

                
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

                /*
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
                    //UnitBase.SetPlayerColor(HexGrid, highestPlayerId, markerToEnemy);
                }
                else
                {
                    Vector3 position = transform.position;
                    position.y -= 1;
                    markerToEnemy.transform.position = position;
                }
                */
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
                //materialName = "Water";
                if (ColorUtility.TryParseHtmlString("#278BB2", out color))
                {
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
                    //materialName = "Grass";
                }
                else
                {
                    if (ColorUtility.TryParseHtmlString("#513A31", out color))
                    {
                    }

                }
            }
            if (Stats.MoveUpdateGroundStat.IsOpenTile)
            //if (Stats.MoveUpdateGroundStat.ZoneId > 0)
            {
                //materialName = "DarkSand";
            }
            Renderer renderer = GetComponent<Renderer>();
            renderer.material.SetColor("SurfaceColor", color);
        }

        internal void UpdateGround()
        {
            if (Stats.MoveUpdateGroundStat.Owner == 0 || !Stats.MoveUpdateGroundStat.IsBorder || Stats.MoveUpdateGroundStat.IsUnderwater)
            {
                if (markerEnergy != null)
                {
                    Vector3 position = transform.position;
                    position.y -= 2;
                    markerEnergy.transform.position = position;
                }
            }
            else
            {
                if (markerEnergy == null)
                {
                    CreateMarker();
                }
                float highestEnergy = 1;

                Vector3 position = transform.position;
                position.y += 0.054f + (0.2f * highestEnergy);
                markerEnergy.transform.position = position;
                UnitBase.SetPlayerColor(Stats.MoveUpdateGroundStat.Owner, markerEnergy);
            }

            Vector3 vector3 = transform.localPosition;
            vector3.y = Stats.MoveUpdateGroundStat.Height + 0.3f;
            transform.localPosition = vector3;

            CreateDestructables();
            
        }

        internal GroundCell GetNeighbor(Direction direction)
        {
            CubePosition cubePosition = new CubePosition(Pos);
            CubePosition n = cubePosition.GetNeighbor(direction);

            GroundCell neighbor;
            HexGrid.MainGrid.GroundCells.TryGetValue(n.Pos, out neighbor);
            return neighbor;
        }

        internal void CreateDestructables()
        {
            SetGroundMaterial();
            
            List<UnitBaseTileObject> allTileObjects = new List<UnitBaseTileObject>();
            allTileObjects.AddRange(GameObjects);

            foreach (TileObject tileObject in Stats.MoveUpdateGroundStat.TileObjects)
            {
                if (tileObject.Direction == Direction.C && tileObject.TileObjectType != TileObjectType.Mineral)
                {

                }
                else
                {
                    bool found = false;
                    foreach (UnitBaseTileObject destructable in allTileObjects)
                    {
                        if (destructable.TileObject.TileObjectType == tileObject.TileObjectType)
                        {
                            found = true;
                            allTileObjects.Remove(destructable);
                            break;
                        }
                    }
                    if (!found)
                    {

                        GameObject destructable;

                        destructable = HexGrid.MainGrid.CreateDestructable(transform, tileObject);
                        if (destructable != null)
                        {
                            destructable.transform.Rotate(Vector3.up, Random.Range(0, 360));
                            destructable.name = tileObject.TileObjectType.ToString();
                        }
                        UnitBaseTileObject unitBaseTileObject = new UnitBaseTileObject();
                        unitBaseTileObject.GameObject = destructable;
                        unitBaseTileObject.TileObject = tileObject.Copy();

                        GameObjects.Add(unitBaseTileObject);
                    }
                }
            }

            // Set color
            Renderer renderer = GetComponent<Renderer>();
            if (Stats.MoveUpdateGroundStat.Owner != 0)
                renderer.material.SetFloat("Darkness", 0.8f);
            else
                renderer.material.SetFloat("Darkness", 0.2f);
            foreach (UnitBaseTileObject unitBaseTileObject1 in GameObjects)
            {
                if (unitBaseTileObject1.GameObject != null)
                {
                    renderer = unitBaseTileObject1.GameObject.GetComponent<Renderer>();

                    if (Stats.MoveUpdateGroundStat.Owner != 0)
                        renderer.material.SetFloat("Darkness", 0.8f);
                    else
                        renderer.material.SetFloat("Darkness", 0.2f);
                }
            }

            if (visible)
            {
                foreach (UnitBaseTileObject destructable in allTileObjects)
                {
                    StartCoroutine(FadeOutDestructable(destructable.GameObject, destructable.GameObject.transform.position.y - 0.1f));
                    //HexGrid.Destroy(destructable.GameObject);
                    GameObjects.Remove(destructable);
                }
            }
        }

        private IEnumerator FadeOutDestructable(GameObject gameObject, float sinkTo)
        {
            while (gameObject.transform.position.y > sinkTo)
            {
                Vector3 pos = gameObject.transform.position;
                pos.y -= 0.0001f;
                gameObject.transform.position = pos;
                yield return null;
            }
            HexGrid.Destroy(gameObject);
        }

        private Light selectionLight;

        public bool IsSelected { get; private set; }
        internal void SetSelected(bool selected)
        {
            if (IsSelected != selected)
            {
                IsSelected = selected;

                if (IsSelected)
                {
                    selectionLight = HexGrid.MainGrid.CreateSelectionLight(gameObject);
                }
                else
                {
                    Destroy(selectionLight);
                }
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
                        cellGameCommand.GameCommand = gameCommand;
                        cellGameCommand.Touched = true;
                        break;
                    }
                }
            }
            if (cellGameCommand == null)
            {
                if (commandPreview == null)
                {
                    cellGameCommand = new CommandPreview();
                    cellGameCommand.GameCommand = gameCommand;
                    cellGameCommand.CreateCommandPreview(this);
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