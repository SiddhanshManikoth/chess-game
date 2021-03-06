using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;
using UnityEngine.SceneManagement;

// Acts as a GameManager.
// Responsible for ending the game, detecting who is currently moving.
public class Board : NetworkBehaviour
{
	#region Singleton
	public static Board Instance { get; private set; }
	void Awake()
	{
		Instance = this;
	}
	void OnDestroy()
	{
		Instance = null;
	}
	#endregion

	// For spawning when Pawn reaches the other side of the board.
	public GameObject WhiteQueenPrefab { get; private set; }
	public GameObject BlackQueenPrefab { get; private set; }

	public King WhiteKing { get; private set; }
	public King BlackKing { get; private set; }

	// All Chessmen, including Kings.
	public List<Chessman> WhiteChessmen { get; private set; }
	public List<Chessman> BlackChessmen { get; private set; }

	// Represents chessboard.
	Cell[,] cells;
	public Cell[,] GetCells()
	{
		return cells;
	}

	
	/// <summary>
	/// Which player's turn?
	/// </summary>
	[SyncVar]
	bool _whiteMoves = true;

	public bool WhiteMoves
	{
		get
		{
			return _whiteMoves;
		}
	}

	// Some player has moved it's time for other player to move.
	[Server]
	public void SwapPlayer()
	{
		_whiteMoves = !_whiteMoves;
	}


	[SyncVar]
	GameEnd _gameEnd = GameEnd.None;

	public bool GameIsRunning()
	{
		return _gameEnd == GameEnd.None;
	}

	[Server]
	public void EndGame(bool? whiteIsWinner)
	{
		Debug.Log("EndGame().");

		if (!whiteIsWinner.HasValue)
			_gameEnd = GameEnd.Draw;

		else if (whiteIsWinner.Value)
			_gameEnd = GameEnd.ServerWin;

		else
			_gameEnd = GameEnd.ClientWin;

		RpcDisplayWinLoseUI(_gameEnd);
	}

	[ClientRpc]
	void RpcDisplayWinLoseUI(GameEnd end)
	{
		GetComponent<UI>().DisplayEndGameUI(end);
	}

	
	/// <summary>
	/// The physical size of the Cell in the world (in units).
	/// Both width and height of the Cell.
	/// </summary>
	const int CELL_SIZE = 9;

	public int GetWorldPos(int boardCoordinate) { return boardCoordinate * CELL_SIZE; }

	public int GetBoardPos(int worldCoordinate) { return worldCoordinate / CELL_SIZE; }


	public void AddQueenToList(Queen queen, bool isWhite)
	{
		(isWhite ? WhiteChessmen : BlackChessmen).Add(queen);
	}

	public void RemoveChessmanFromList(Chessman toRemove)
	{
		if ((toRemove.isWhite ? WhiteChessmen : BlackChessmen) != null)
			(toRemove.isWhite ? WhiteChessmen : BlackChessmen).Remove(toRemove);
	}

	// Casts ray and gets component.
	public TComponent GetComponentInChessman<TComponent>(int z_Board, int x_Board)
	{
		RaycastHit hit;
		Ray ray = new Ray(new Vector3(GetWorldPos(x_Board), 30f, GetWorldPos(z_Board)), Vector3.down);

		if (Physics.Raycast(ray, out hit, 30f, LayerMask.GetMask("Chessmen")))
			return hit.collider.GetComponent<TComponent>();

		Debug.LogWarning("GetChessmanByBoardIndex:: Collider not found. Position: (" + z_Board + " " + x_Board + ")");
		return default(TComponent);
	}

	// Can opponent put his figure in here?
	public bool CellIsInDanger(int y, int x, bool enemy_IsWhite)
	{
		return enemy_IsWhite ? WhiteChessmen.Any(whiteChessman => whiteChessman.CanKillCell(y, x))
			: BlackChessmen.Any(blackChessman => blackChessman.CanKillCell(y, x));
	}

	// When the game starts.
	public void Init()
	{
		Init_Cells();
		Init_Chessmen();

		WhiteQueenPrefab = Resources.Load("White Queen", typeof(GameObject)) as GameObject;
		BlackQueenPrefab = Resources.Load("Black Queen", typeof(GameObject)) as GameObject;
	}

	// Finds all the chessmen on the board and caches them into WhiteChessmen, BlackChessmen.
	// Also finds kings and caches them.
	void Init_Chessmen()
	{
		Chessman[] chessmen = FindObjectsOfType<Chessman>();

		WhiteChessmen = new List<Chessman>();
		BlackChessmen = new List<Chessman>();

		for (int i = 0; i < chessmen.Length; i++)
		{
			if (chessmen[i].isWhite)
				WhiteChessmen.Add(chessmen[i]);
			else
				BlackChessmen.Add(chessmen[i]);
		}

		King[] kings = FindObjectsOfType<King>();

		for (int i = 0; i < 2; i++)
		{
			if (kings[i].isWhite)
				WhiteKing = kings[i];
			else
				BlackKing = kings[i];
		}
	}

	void Init_Cells()
	{
		cells = new Cell[8, 8];

		if (SceneManager.GetActiveScene().name == "Game")
		{
			Debug.Log("Game");
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 8; j++)
					GetCells()[i < 2 ? i : i + 4, j] = i < 2 ? Cell.WhiteFigure : Cell.BlackFigure;
			}

			GetCells()[0, 4] |= Cell.King;
			GetCells()[7, 4] |= Cell.King;
		}
		// FOR_TEST Scene.
		else
		{
			Debug.Log("FOR_TEST");
			CellUtils.UpdateCells();
		}
	}

	public void SetCellEmpty(int z, int x)
	{
		cells[z, x] = Cell.Empty;
	}

	public void SetCell(int fromZ, int fromX, int toZ, int toX, bool isWhite, bool isKing)
	{
		GetCells()[fromZ, fromX] = Cell.Empty;
		GetCells()[toZ, toX] = (isWhite ? Cell.WhiteFigure : Cell.BlackFigure);

		if (isKing)
			GetCells()[toZ, toX] |= Cell.King;
	}

	// Displays red, blue and green boxes whenever the player clicks on chessman.
	public void DisplayHighlighters(List<Move> possibleMoves)
	{
		if (possibleMoves.Count() == 0)
		{
			Debug.Log("No Possible moves");
			return;
		}

		foreach (var p in possibleMoves)
		{
			int objectIndex;
			float height_Y = 1;

			if (p.isCastle)
				objectIndex = 2;

			else if (p.isKill)
			{
				Chessman chessman = GetComponentInChessman<Chessman>(p.z, p.x);

				//Getting the height of Red Cube above the figure to kill.
				//My figures will be replaced anyway, this should be fixed.
				switch (chessman.tag)
				{
					case "Pawn": height_Y = 11; break;
					case "Rook": height_Y = 10; break;
					case "Knight": height_Y = 13; break;
					case "Bishop": height_Y = 14; break;
					case "Queen": height_Y = 13.5f; break;
					case "King":
						Debug.LogError("DisplayValidMoves:: Entered King tag in switch chessman.tag");
						break;
					default:
						Debug.LogError("DisplayValidMoves:: Wrong tag");
						break;
				}

				objectIndex = 1;
			}

			else
				objectIndex = 0;

			//Sets the highlighter at correct place
			GameObject highlighter = ObjectPooler.Instance.GetPooledObject(objectIndex);
			Vector3 newPos = new Vector3(GetWorldPos(p.x), height_Y, GetWorldPos(p.z));
			highlighter.transform.position = newPos;
			highlighter.SetActive(true);
		}
	}

	public void HideHighlighters()
	{
		for (int i = 0; i < 3; i++)
		{
			foreach (GameObject pooled in ObjectPooler.Instance.GetAllPooledObjects(i))
			{
				if (pooled.activeInHierarchy)
					pooled.SetActive(false);
			}
		}
	}
}
