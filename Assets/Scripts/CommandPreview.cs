using Engine.Ants;
using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class CommandPreview
    {
        public CommandPreview()
        {
            GameCommand = new MapGameCommand();
            GameCommand.PlayerId = 1;
            GameCommand.TargetPosition = Position.Null;
            GameCommand.DeleteWhenFinished = false;
        }
        
        public MapGameCommand GameCommand { get; set; }
        public Command Command { get; set; }
        public bool Touched { get; set; }
        private GameObject previewGameCommand;

        public void CreateCommandForBuild(BlueprintCommand blueprintCommand)
        {
            GameCommand.BlueprintCommand = blueprintCommand;
            GameCommand.GameCommandType = blueprintCommand.GameCommandType;
            CreateCommandPreview();
        }

        public void Delete()
        {
            if (previewGameCommand != null)
            {                
                Command?.SetSelected(false);

                HexGrid.Destroy(previewGameCommand);
                previewGameCommand = null;
            }
        }

        private bool CanBuildAt(GroundCell groundCell)
        {
            if (groundCell != null)
            {
                UnitBase unitBase = groundCell.FindUnit();
                if (unitBase != null)
                {
                    if (!unitBase.HasEngine())
                        return false;
                }

                MoveUpdateGroundStat stats = groundCell.Stats.MoveUpdateGroundStat;
                if (stats.IsUnderwater)
                    return false;

                int mins = 0;
                foreach (TileObject tileObject in stats.TileObjects)
                {
                    if (tileObject.TileObjectType == TileObjectType.Mineral)
                    {
                        mins++;
                    }
                    else if (TileObject.IsTileObjectTypeCollectable(tileObject.TileObjectType))
                        return false;
                    else if (TileObject.IsTileObjectTypeObstacle(tileObject.TileObjectType))
                        return false;
                }

                if (mins >= 20)
                {
                    return false;
                }
                return true;
            }
            return true;
        }

        private bool CanCommandAt(GroundCell groundCell)
        {
            if (groundCell != null)
            {
                foreach (TileObject tileObject in groundCell.Stats.MoveUpdateGroundStat.TileObjects)
                {
                    if (TileObject.IsTileObjectTypeObstacle(tileObject.TileObjectType))
                        return false;
                }
            }
            return true;
        }
        public void CreateAtPosition(GroundCell groundCell)
        {
            if (previewGameCommand == null)
                CreateCommandLogo();
            UpdatePositions(groundCell);
        }

        public void SetPosition(GroundCell groundCell)
        {
            if (groundCell == null)
            {
                if (previewGameCommand != null)
                    previewGameCommand.SetActive(false);
                GameCommand.TargetPosition = Position.Null;
            }
            else
            {
                bool positionok;
                if (GameCommand.GameCommandType == GameCommandType.Build)
                    positionok = CanBuildAt(groundCell);
                else
                    positionok = CanCommandAt(groundCell);
                if (positionok)
                {
                    GameCommand.TargetPosition = groundCell.Pos;
                    UpdatePositions(groundCell);
                    previewGameCommand.SetActive(true);
                }
                else
                {
                    if (previewGameCommand != null)
                        previewGameCommand.SetActive(false);
                    GameCommand.TargetPosition = Position.Null;
                }
            }
        }

        public bool CanExecute()
        {
            return GameCommand.TargetPosition != Position.Null;
        }

        private static float aboveGround = 2.09f;

        private void UpdatePositions(GroundCell groundCell)
        {
            previewGameCommand.transform.SetParent(groundCell.transform, false);

            Vector3 unitPos3 = groundCell.transform.position;
            unitPos3.y += aboveGround;
            previewGameCommand.transform.position = unitPos3;
        }

        private void CreateCommandLogo()
        {
            string layout = "UIBuild";

            if (GameCommand.BlueprintCommand != null &&
                !string.IsNullOrEmpty(GameCommand.BlueprintCommand.Layout))
                layout = GameCommand.BlueprintCommand.Layout;

            previewGameCommand = HexGrid.Instantiate(HexGrid.MainGrid.GetResource(layout));

            Command = previewGameCommand.GetComponent<Command>();
            Command.CommandPreview = this;
        }
        public  bool IsSelected { get; private set; }
        public void SetSelected(bool value)
        {
            if (value == false)
            {
                int x = 0;
            }
            previewGameCommand.SetActive(value);
        }

        public void CreateCommandPreview()
        {
            CreateCommandLogo();
            Command.IsPreview = true;
            Command.SetSelected(true);

            foreach (BlueprintCommandItem blueprintCommandItem in GameCommand.BlueprintCommand.Units)
            {
                Blueprint blueprint = HexGrid.MainGrid.game.Blueprints.FindBlueprint(blueprintCommandItem.BlueprintName);
                UnitBase previewUnit = HexGrid.MainGrid.CreateTempUnit(blueprint);
                previewUnit.DectivateUnit();
                previewUnit.transform.SetParent(previewGameCommand.transform, false);

                Vector3 unitPos3 = previewGameCommand.transform.position;
                unitPos3.y -= aboveGround;
                previewUnit.transform.position = unitPos3;

                Vector3 scaleChange;
                scaleChange = new Vector3(0.01f, 0.01f, 0.01f);

                previewUnit.gameObject.transform.localScale += scaleChange;

                previewUnit.gameObject.SetActive(true);
            }
        }
    }
}
