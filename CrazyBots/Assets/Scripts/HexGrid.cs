using Assets.Scripts;
using Engine.Interface;
//using Engine.Master;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour {

	public Color defaultColor = Color.white;

	public HexCell cellPrefab;
	public Text cellLabelPrefab;
	public UnitFrame unitFramePrefab;

	public int gridWidth = 20;
	public int gridHeight = 20;
	float hexWidth = 1.732f;
	float hexHeight = 2.0f;
	public float gap = 0.0f;

	Vector3 startPos;

	Canvas gridCanvas;
	//HexMesh hexMesh;

    Engine.Master.Game game;

	void Awake () {

		GameModel gameModel = new GameModel();
		gameModel.MapHeight = gridWidth;
		gameModel.MapWidth = gridHeight;

		gameModel.Players = new List<PlayerModel>();

		PlayerModel p = new PlayerModel();
		p.ControlLevel = 1;
		p.Id = 1;
		p.Name = "WebPLayer";
		p.StartPosition = new Position(10, 10);
		gameModel.Players.Add(p);

		game = new Engine.Master.Game(gameModel, 1991245194);

		gridCanvas = GetComponentInChildren<Canvas>();
		//hexMesh = GetComponentInChildren<HexMesh>();

		/*
		Tile t1 = game.Map.GetTile(new Position(0, 0));
		CreateCell(t1);

		Tile t2 = game.Map.GetTile(new Position(1, 0));
		CreateCell(t2);*/

		
		for (int y = 0; y < game.Map.MapHeight; y++) 
		{
			for (int x = 0; x < game.Map.MapWidth; x++) 
			{
				Tile t = game.Map.GetTile(new Position(x, y));
				CreateCell(t);
			}
		}

		InvokeRepeating("invoke", 1, 1F);
	}

	void invoke()
	{
		Move nextMove = new Move();
		nextMove.MoveType = MoveType.None;

		List<Move> newMoves = game.ProcessMove(0, nextMove);
		foreach (Move move in newMoves)
        {
			if (move.MoveType == MoveType.Add)
            {
					CreateUnit(move);

			}
        }
	}

	void Start () {
		CalcStartPos();

		hexWidth += hexWidth * gap;
		hexHeight += hexHeight * gap;
	}

	void CreateUnit(Move move)
	{
		Position pos = move.Positions[move.Positions.Count- 1];
		HexCoordinates hexPos = HexCoordinates.FromOffsetCoordinates(pos.X, pos.Y);
		int x = hexPos.X;
		int z = hexPos.Z;

		Vector3 position;
		position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
		position.y = 0f;
		position.z = z * (HexMetrics.outerRadius * 1.5f);

		//position.x = 12;
		position.y = 0.5f;
		//position.z = 24;


		UnitFrame unit = Instantiate<UnitFrame>(unitFramePrefab);

		unit.transform.SetParent(transform, false);

		Vector2 gridPos = new Vector2(pos.X, pos.Y);
		Vector3 unitPos3 = CalcWorldPos(gridPos);
		unitPos3.y = 0.5f;
		unit.transform.localPosition = unitPos3;
		//cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

		//unit.transform.localPosition = position;

		unit.X = pos.X;
		unit.Z = pos.Y;


		//unit.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);


		/*
		HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
		cell.transform.SetParent(transform, false);
		cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

		if (t.Height > 0.05f)

			cell.color = Color.green;
		else
			cell.color = defaultColor;
		*/
		//Text label = Instantiate<Text>(cellLabelPrefab);
		//label.rectTransform.SetParent(gridCanvas.transform, false);
		//label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
		//label.text = cell.coordinates.ToStringOnSeparateLines();
		//label.text = t.Pos.X + ", " + t.Pos.Y;

		Text label = Instantiate<Text>(cellLabelPrefab);
		//label.rectTransform.SetParent(cell.transform, false);
		label.rectTransform.SetParent(gridCanvas.transform, false);
		label.rectTransform.anchoredPosition = new Vector2(unitPos3.x, unitPos3.z);
		//label.text = cell.coordinates.ToStringOnSeparateLines();
		label.text = move.UnitId;
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

	public static Position GetPixelPos(int x, int y, int topx, int topy, int gridSize)
	{
		int HalfGridSize = gridSize / 2;
		int gapX = topx * gridSize;
		int gapY = topy * gridSize;

		if (x % 2 != 0 && y % 2 != 0)
		{
			return new Position((x * gridSize) - gapX, (y * gridSize) + HalfGridSize - gapY);
		}
		else if (x % 2 == 0 && y % 2 != 0)
		{
			return new Position((x * gridSize) - gapX, (y * gridSize) - gapY);
		}
		else if (x % 2 != 0 && y % 2 == 0)
		{
			return new Position((x * gridSize) - gapX, (y * gridSize) + HalfGridSize - gapY);
		}
		return new Position((x * gridSize) - gapX, y * gridSize - gapY);
	}

	void CalcStartPos()
	{
		float offset = 0;
		if (gridHeight / 2 % 2 != 0)
			offset = hexWidth / 2;

		float x = -hexWidth * (gridWidth / 2) - offset;
		float z = hexHeight * 0.75f * (gridHeight / 2);

		startPos = new Vector3(0, 0, 0);
	}

	Vector3 CalcWorldPos(Vector2 gridPos)
	{
		float offset = 0;
		if (gridPos.y % 2 != 0)
			offset = hexWidth / 2;

		float x = startPos.x + gridPos.x * hexWidth + offset;
		float z = startPos.z - gridPos.y * hexHeight * 0.75f;

		return new Vector3(x, 0, z);
	}


	void CreateCell (Tile t) {

		int x = t.Pos.X;
		int y = t.Pos.Y;

		Vector3 position;
		position.x = x; // (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
		position.y = (float)t.Height;
		position.z = y; // z * (HexMetrics.outerRadius * 1.5f);

		if (x % 2 != 0 && y % 2 != 0)
		{
			//return new Position((x * gridSize) - gapX, (y * gridSize) + HalfGridSize - gapY);
			//return;
		}
		else if (x % 2 == 0 && y % 2 != 0)
		{
			//return new Position((x * gridSize) - gapX, (y * gridSize) - gapY);
			//return;
		}
		else if (x % 2 != 0 && y % 2 == 0)
		{
			//return new Position((x * gridSize) - gapX, (y * gridSize) + HalfGridSize - gapY);
			//return;
		}
		else
		{
			//return new Position((x * gridSize) - gapX, y * gridSize - gapY);
			
		}


		HexCell cell = Instantiate<HexCell>(cellPrefab);
		cell.transform.SetParent(transform, false);
		//cell.transform.localPosition = position;

		Vector2 gridPos = new Vector2(x, y);
		Vector3 gridPos3 = CalcWorldPos(gridPos);
		gridPos3.y = (float)t.Height;

		cell.transform.localPosition = gridPos3;
		//cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
		cell.X = t.Pos.X;
		cell.Z = t.Pos.Y;

		/*
		if (t != null && t.Height > 0.05f)

			cell.color = Color.green;
		else
			cell.color = defaultColor;
		*/
		
		
		Text label = Instantiate<Text>(cellLabelPrefab);
		//label.rectTransform.SetParent(cell.transform, false);
		label.rectTransform.SetParent(gridCanvas.transform, false);
		label.rectTransform.anchoredPosition = new Vector2(gridPos3.x, gridPos3.z);
		//label.text = cell.coordinates.ToStringOnSeparateLines();
		label.text = t.Pos.X.ToString() + "," + t.Pos.Y;
		//label.text = position.x + ", " + position.y + ", " + position.z;
		
	}
}