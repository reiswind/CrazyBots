﻿using Assets.Scripts;
using Engine.Interface;
using System;
//using Engine.Master;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour {

	public HexCell cellPrefab;
	public Text cellLabelPrefab;
	public Engine1 Engine1;
	public Container1 Container1;

	public int gridWidth = 20;
	public int gridHeight = 20;
	public float GameSpeed = 0.8f;

	//private float gap = 0.0f;
	//private float hexWidth = 1.732f;
	//private float hexHeight = 2.0f;

	//Vector3 startPos;
	private Canvas gridCanvas;

	public Dictionary<Position, HexCell> GroundCells { get; private set; }
	public Dictionary<string, UnitFrame> Units { get; private set; }

	// Shard with backgound tread
	private IGameController game;
	private bool windowClosed;
	private List<Move> newMoves;
	public EventWaitHandle WaitForTurn = new EventWaitHandle(false, EventResetMode.AutoReset);
	public EventWaitHandle WaitForDraw = new EventWaitHandle(false, EventResetMode.AutoReset);
	private Thread computeMoves = null;

	public void CreateGame(GameModel gameModel)
	{
		// (int)DateTime.Now.Ticks; -1789305431
		newMoves = new List<Move>();
		game = gameModel.CreateGame(); // -1789305431);

		GroundCells = new Dictionary<Position, HexCell>();
		Units = new Dictionary<string, UnitFrame>();

		// Render ground
		for (int y = 0; y < game.Map.MapHeight; y++)
		{
			for (int x = 0; x < game.Map.MapWidth; x++)
			{
				Position pos = new Position(x, y);
				Tile t = game.Map.GetTile(pos);
				HexCell hexCell = CreateCell(t);

				GroundCells.Add(pos, hexCell);
			}
		}

		// Start game engine
		computeMoves = new Thread(new ThreadStart(ComputeMove));
		computeMoves.Start();
		WaitForDraw.Set();
	}

	public void ComputeMove()
	{
		try
		{
			// Prerender some ground
			//ClientMap.RenderBackgound(Game, allImages, 0, 0, Game.Map.MapWidth, Game.Map.MapHeight, true, gridSize);

			while (!windowClosed)
			{
				long iTicks = DateTime.Now.Ticks;
				DateTime tStart = DateTime.Now;

				while (!windowClosed)
				{
					if (!WaitForDraw.WaitOne(10))
					{
						// Animation not ready
						Thread.Sleep(10);
						continue;
					}
					break;
				}

				int id = 0;

				//lock (allImages)
				{
					Move nextMove = new Move();
					nextMove.MoveType = MoveType.None;

					List<Move> current = game.ProcessMove(id, nextMove);
					if (newMoves.Count > 0)
					{
						//throw new Exception("Should not happen");
						// Heppens during shutdown, ignore
						windowClosed = true;
					}
					else
					{
						newMoves.Clear();
						newMoves.AddRange(current);
					}
					//ProcessAreas(allImages);
					//ProcessCurrentMoves(newMoves, allImages);
					//lastMapInfo = Game.Map.GetMapInfo();
				}
				// New move is ready, continue with next move
				WaitForTurn.Set();
			}
		}
		catch (Exception err)
		{
			throw new Exception("Game move wrecked " + err.Message);
		}
	}

	void Awake () 
	{
		if (GameSpeed == 0)
			GameSpeed = 0.5f;

		gridCanvas = GetComponentInChildren<Canvas>();

		GameModel gameModel = new GameModel();
		gameModel.MapHeight = gridWidth;
		gameModel.MapWidth = gridHeight;

		if (gridWidth > 10)
		{
			gameModel.Players = new List<PlayerModel>();

			PlayerModel p = new PlayerModel();
			p.ControlLevel = 1;
			p.Id = 1;
			p.Name = "WebPLayer";
			p.StartPosition = new Position(10, 10);
			gameModel.Players.Add(p);
		}
		CreateGame(gameModel);

		InvokeRepeating("invoke", 1f, GameSpeed);
	}

    private void OnDestroy()
    {
		windowClosed = true;
    }

    void Start()
	{
		//CalcStartPos();

		//hexWidth += hexWidth * gap;
		//hexHeight += hexHeight * gap;
	}

	void invoke()
	{
		if (WaitForTurn.WaitOne(10))
		{
			foreach (UnitFrame unitFrame in Units.Values)
            {
				if (unitFrame.FinalDestination != null)
				{
					unitFrame.JumpToTarget(unitFrame.FinalDestination);
					unitFrame.FinalDestination = null;
				}
				unitFrame.NextMove = null;
			}

			foreach (Move move in newMoves)
			{
				if (move.MoveType == MoveType.Add)
				{
					CreateUnit(move);
				}
				else if (move.MoveType == MoveType.Move || move.MoveType == MoveType.UpdateStats)
				{
					UnitFrame unit = Units[move.UnitId];
					unit.NextMove = move;
				}
			}
			newMoves.Clear();
			WaitForDraw.Set();
		}
	}

	void CreateUnit(Move move)
	{
		UnitFrame unit = new UnitFrame();

		unit.HexGrid = this;
		unit.NextMove = move;

		/*
		Position pos = move.Positions[move.Positions.Count- 1];
		//unit.transform.SetParent(transform, false);
		HexCell targetCell = GroundCells[pos];
		Vector3 unitPos3 = targetCell.transform.localPosition;
		unitPos3.y -= 1;
		unit.transform.position = unitPos3;*/

		unit.Assemble();

		Units.Add(move.UnitId, unit);




		/*
		Text label = Instantiate<Text>(cellLabelPrefab);
		label.rectTransform.SetParent(gridCanvas.transform, false);
		label.rectTransform.anchoredPosition = new Vector2(unitPos3.x, unitPos3.z);
		label.text = "\r\n" + move.UnitId;*/
	}

	/*
	public void ColorCell (Vector3 position, Color color) {
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		int index = coordinates.X + coordinates.Z * width + coordinates.Z / 2;
		HexCell cell = cells[index];
		cell.color = color;
		hexMesh.Triangulate(cells);
	}*/

	/*
	void CalcStartPos()
	{
		float offset = 0;
		if (gridHeight / 2 % 2 != 0)
			offset = hexWidth / 2;

		float x = -hexWidth * (gridWidth / 2) - offset;
		float z = hexHeight * 0.75f * (gridHeight / 2);

		startPos = new Vector3(0, 0, 0);
	}*/

	private Vector3 CalcWorldPos(Vector2 gridPos)
	{
		float gridSizeX = 1.50f;
		float gridSizeY = 1.75f;
		float halfGridSize = 0.86f;

		float x = gridPos.x;
		float y = gridPos.y;

		if (x % 2 != 0 && y % 2 != 0)
		{
			return new Vector3((x * gridSizeX), 0, -(y * gridSizeY) - halfGridSize);
		}
		else if (x % 2 == 0 && y % 2 != 0)
		{
			return new Vector3((x * gridSizeX), 0, -(y * gridSizeY));
		}
		else if (x % 2 != 0 && y % 2 == 0)
		{
			return new Vector3((x * gridSizeX), 0, -(y * gridSizeY) - halfGridSize);
		}
		return new Vector3((x * gridSizeX), 0,  -y * gridSizeY);
	}

	private HexCell CreateCell(Tile t)
	{
		int x = t.Pos.X;
		int y = t.Pos.Y;

		HexCell cell = Instantiate<HexCell>(cellPrefab);
		cell.transform.SetParent(transform, false);

		Vector2 gridPos = new Vector2(x, y);
		Vector3 gridPos3 = CalcWorldPos(gridPos);

		cell.X = t.Pos.X;
		cell.Z = t.Pos.Y;

		double height = t.Height;
		float tileY;

		string materialName;
		if (height > 0.27 && height < 0.33)
		{
			materialName = "Materials/Sand";
			tileY = 0.3f;
		}
		else if (height >= 0.26 && height <= 0.32)
		{
			materialName = "Materials/DarkSand";
			tileY = 0.4f;
		}
		else if (height > 0.48 && height < 0.52)
		{
			materialName = "Materials/DarkWood";
			tileY = 0.9f;
		}
		else if (height > 0.47 && height < 0.53)
		{
			materialName = "Materials/Wood";
			tileY = 0.8f;
		}
		else if (height >= 0.46 && height <= 0.54)
		{
			materialName = "Materials/LightWood";
			tileY = 0.7f;
		}
		else if (height >= 0.45 && height <= 0.55)
		{
			materialName = "Materials/GrassDark";
			tileY = 0.6f;
		}
		else
		{
			materialName = "Materials/Grass";
			tileY = 0.5f;
		}

		gridPos3.y = tileY / 2;
		cell.transform.localPosition = gridPos3;

		//
		Material material = Resources.Load<Material>(materialName);

		MeshRenderer meshRenderer = cell.GetComponent<MeshRenderer>();
		meshRenderer.material = material;

		/*

				Text label = Instantiate<Text>(cellLabelPrefab);
				label.rectTransform.SetParent(gridCanvas.transform, false);
				label.rectTransform.anchoredPosition = new Vector2(gridPos3.x, gridPos3.z);
				label.text = t.Pos.X.ToString() + "," + t.Pos.Y;
		*/
		return cell;
	}
}