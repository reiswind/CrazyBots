using Engine.Control;
using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Ants
{
    internal class Ant
    {
        public Ant(ControlAnt control)
        {
            Control = control;
        }

        public Ant(ControlAnt control, PlayerUnit playerUnit)
        {
            PlayerUnit = playerUnit;
            Control = control;

            Energy = MaxEnergy;
        }

        static public float HomeTrailDepositRate = 0.5f;
        static public float FoodTrailDepositRate = 0.2f;
        static public float MaxEnergy = 200;
        public float Energy { get; set; }

        static public float MaxFoodIntensity = 100;
        public float FoodIntensity { get; set; }

        //public bool HoldPosition { get; set; }
        public int MoveAttempts { get; set; }
        public int StuckCounter { get; set; }
        public bool Alive { get; set; }
        public PlayerUnit PlayerUnit { get; set; }
        public ControlAnt Control { get; set; }

        public int PheromoneDepositEnergy { get; set; }
        public int PheromoneDepositNeedMinerals { get; set; }
        public int PheromoneDepositNeedMineralsLevel { get; set; }


        public int PheromoneWaypointAttack { get; set; }

        public List<Position> FollowThisRoute { get; set; }
        public virtual bool Move(Player player, List<Move> moves)
        {
            return true;
        }

        internal GameCommand CurrentGameCommand;

        internal void HandleGameCommands(Player player)
        {
            bool hasChanged = false;
            if (PlayerUnit.Unit.GameCommands != null && PlayerUnit.Unit.GameCommands.Count > 0)
            {
                /*
                foreach (GameCommand gameCommand in PlayerUnit.Unit.GameCommands)
                {                    
                    if (gameCommand.Append)
                        Debug.Log("XXMove to " + gameCommand.TargetPosition.X + "," + gameCommand.TargetPosition.Y + " SHIFT");
                    else
                        Debug.Log("XXMove to " + gameCommand.TargetPosition.X + "," + gameCommand.TargetPosition.Y);
                }*/
                if (CurrentGameCommand != null &&
                    CurrentGameCommand.TargetPosition == PlayerUnit.Unit.GameCommands[0].TargetPosition)
                {
                    // Continue with gamecommand
                    if (CurrentGameCommand.TargetPosition == PlayerUnit.Unit.Pos)
                    {

                        // Reached
                        CurrentGameCommand = null;
                        PlayerUnit.Unit.GameCommands.RemoveAt(0);
                    }
                    else
                    {
                        GameCommand newCommand = null;
                        
                        foreach (GameCommand gameCommand in PlayerUnit.Unit.GameCommands)
                        {
                            if (gameCommand == CurrentGameCommand)
                                continue;

                            if (gameCommand.Append == false)
                                newCommand = gameCommand;
                        }
                        
                        if (newCommand == null)
                        {
                        }
                        else
                        {
                            while (newCommand != PlayerUnit.Unit.GameCommands[0])
                                PlayerUnit.Unit.GameCommands.RemoveAt(0);
                            CurrentGameCommand = null;
                            FollowThisRoute = null;
                            hasChanged = true;
                        }

                    }
                }
                else
                {
                    CurrentGameCommand = null;
                    FollowThisRoute = null;
                    hasChanged = true;
                }
                if (CurrentGameCommand == null)
                {                    
                    // Select next gamecommand
                    foreach (GameCommand gameCommand in PlayerUnit.Unit.GameCommands)
                    {
                        if (CurrentGameCommand == null)
                            CurrentGameCommand = gameCommand;
                        if (gameCommand.Append == false)
                            CurrentGameCommand = gameCommand;
                    }
                }
                if (CurrentGameCommand != null)
                {
                    if (CurrentGameCommand.GameCommandType == GameCommandType.Minerals)
                    {
                        if (hasChanged)
                        {
                            int x = player.Game.Pheromones.DropPheromones(player, CurrentGameCommand.TargetPosition, 5, PheromoneType.Mineral, 0.5f, false);
                        }
                    }
                    if (CurrentGameCommand.GameCommandType == GameCommandType.Attack)
                    {
                        if (hasChanged)
                        {
                            if (PheromoneWaypointAttack != 0)
                            {
                                Control.RemoveEnemyFound(player, PheromoneWaypointAttack);
                                PheromoneWaypointAttack = 0;
                            }
                            PheromoneWaypointAttack = Control.EnemyFound(player, CurrentGameCommand.TargetPosition);
                        }
                    }

                    if (CurrentGameCommand.GameCommandType == GameCommandType.Move ||
                        CurrentGameCommand.GameCommandType == GameCommandType.AttackMove)
                    {
                        if (CurrentGameCommand != null && FollowThisRoute == null)
                        {
                            List<Position> positions = player.Game.FindPath(PlayerUnit.Unit.Pos, CurrentGameCommand.TargetPosition, PlayerUnit.Unit);
                            if (positions != null && positions.Count > 1)
                            {
                                FollowThisRoute = new List<Position>();
                                for (int i = 1; i < positions.Count; i++)
                                {
                                    FollowThisRoute.Add(positions[i]);
                                }
                            }
                        }
                    }
                }
            }
        }

    }

}
