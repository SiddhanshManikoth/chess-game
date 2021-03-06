using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;

// This will be spawned as Player Object in network.
public class Player : NetworkBehaviour
{
	// For performance and conveniency.
	Board board;

	Chessman selectedChessman;
	NetworkIdentity selectedChessman_NetworkIdentity;

	void Start()
	{
		board = Board.Instance;

		if (isLocalPlayer)
		{
			board.Init();

			FindObjectOfType<PlayerCamera>().SetDefaultPos(pointToWhite: isServer == PlayerColorHandler.Instance.ServerPlaysAsWhite);
		}
	}

	void OnDisable()
	{
		board.HideHighlighters();
	}

	void Update()
	{
		if (!isLocalPlayer || (isServer == PlayerColorHandler.Instance.ServerPlaysAsWhite) != board.WhiteMoves)
			return;

		CheckSelection();
		CheckAction();
	}

	void CheckSelection()
	{
		if (!Input.GetMouseButtonDown(0))
			return;

		RaycastHit hit;
		if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, LayerMask.GetMask("Chessmen")))
		{
			Chessman chessmanComponent = hit.collider.GetComponent<Chessman>();

			if (chessmanComponent == null)
				Debug.LogError("CheckSelection:: ChessmanComponent is null");

			else if (selectedChessman == chessmanComponent)
			{
				selectedChessman = null;
				selectedChessman_NetworkIdentity = null;

				board.HideHighlighters();
			}

			else if ((isServer == PlayerColorHandler.Instance.ServerPlaysAsWhite) == chessmanComponent.isWhite)
			{
				board.HideHighlighters();

				selectedChessman = chessmanComponent;
				selectedChessman_NetworkIdentity = hit.collider.GetComponent<NetworkIdentity>();

				board.DisplayHighlighters(chessmanComponent.GetValidMoves());
			}
		}
	}

	// Detects if player clicks on Highlighter.
	void CheckAction()
	{
		if (!Input.GetMouseButtonDown(0))
			return;

		RaycastHit hit;
		if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, LayerMask.GetMask("Highlighters", "Chessmen")))
		{
			// This also could be a chessman, so it fixes the bug.
			if (hit.collider.CompareTag("Move Highlighter"))
				MakeMove((int)hit.transform.position.z, (int)hit.transform.position.x);
		}
	}

	// Moves selectedChessman.
	void MakeMove(int toZ_World, int toX_World)
	{
		CmdMoveFigure(selectedChessman_NetworkIdentity, selectedChessman.Y_Board, selectedChessman.X_Board, toZ_World, toX_World);

		selectedChessman = null;
		selectedChessman_NetworkIdentity = null;

		board.HideHighlighters();
	}

	// TODO: Check if it was correct client.
	[Command]
	void CmdMoveFigure(NetworkIdentity identity, int fromZ_Board, int fromX_Board, int toZ_World, int toX_World)
	{
		if (NetworkServer.connections.Count != 2 || !board.GameIsRunning())
			return;

		Chessman identityChessman = identity.GetComponent<Chessman>();

		int boardToX = board.GetBoardPos(toX_World),
			boardToZ = board.GetBoardPos(toZ_World);

		List<Move> allValidMoves = identityChessman.GetValidMoves();
		int validMoveIndex = allValidMoves.FindIndex(move => move.x == boardToX && move.z == boardToZ);

		if (validMoveIndex == -1)
		{
			Debug.LogError("NOT LEGIT MOVE");
			return;
		}

		King kingComponent = identityChessman as King;
		bool isKing = kingComponent != null;
		bool pawnGotToEnd = false;

		if (allValidMoves[validMoveIndex].isKill)
		{
			Debug.Log("Entered is kill");

			NetworkIdentity toKill = board.GetComponentInChessman<NetworkIdentity>(allValidMoves[validMoveIndex].z, allValidMoves[validMoveIndex].x);
			NetworkServer.Destroy(NetworkServer.FindLocalObject(toKill.netId));
		}

		if (identityChessman is Pawn)
		{
			pawnGotToEnd = identityChessman.isWhite ? board.GetBoardPos(toZ_World) == 7 : board.GetBoardPos(toZ_World) == 0;
			if (pawnGotToEnd)
				ServerTurnPawnIntoQueen(fromZ_Board, fromX_Board, toZ_World, toX_World, identityChessman);
		}

		else if (allValidMoves[validMoveIndex].isCastle)
			SetRookPositionInCastle(toX_World, kingComponent);

		board.SwapPlayer();

		if (!pawnGotToEnd)
			RpcMoveFigure(identity, fromZ_Board, fromX_Board, toZ_World, toX_World, identityChessman.isWhite, isKing);
	}

	[Server]
	void IsCheckOrEndGame(Chessman referencedChessman)
	{
		bool friendsHaveValidMoves = referencedChessman.EnemyKing.FriendsHaveValidMoves();
		if (referencedChessman.ThreatForEnemyKing(board.GetCells()))
		{
			if (!friendsHaveValidMoves)
			{
				board.EndGame(referencedChessman.isWhite);
			}
			// TODO: Visuals.
			else
				Debug.Log("Check.");
		}
		// Draw: No valid moves and no Check.
		else if (!friendsHaveValidMoves)
		{
			Debug.Log("Draw");
			board.EndGame(null);
		}
	}

	[Server]
	void SetRookPositionInCastle(int toX_World, King kingComponent)
	{
		int xDelta = board.GetBoardPos(toX_World) - kingComponent.X_Board;
		int row = kingComponent.isWhite ? 0 : 7;

		int rookOldX = xDelta == -3 ? 0 : 7;
		int rookNewX = xDelta == -3 ? 2 : 5;

		Chessman rook = board.GetComponentInChessman<Chessman>(row, rookOldX);
		RpcMoveFigure(rook.GetComponent<NetworkIdentity>(), row, rookOldX, board.GetWorldPos(row), board.GetWorldPos(rookNewX), row == 0, false);
	}

	// Called when Pawn reaches end.
	[Server]
	void ServerTurnPawnIntoQueen(int fromZ_Board, int fromX_Board, int toZ_World, int toX_World, Chessman identityChessman)
	{
		Debug.Log("TurnPawnIntoQueen();");

		GameObject queen = Instantiate(identityChessman.isWhite ? board.WhiteQueenPrefab : board.BlackQueenPrefab,
			new Vector3(toX_World, 0, toZ_World), Quaternion.identity);
		NetworkServer.Spawn(queen);

		NetworkIdentity pawnToKill = board.GetComponentInChessman<NetworkIdentity>(fromZ_Board, fromX_Board);
		NetworkServer.Destroy(NetworkServer.FindLocalObject(pawnToKill.netId));

		RpcSetCell(fromZ_Board, fromX_Board, board.GetBoardPos(toZ_World), board.GetBoardPos(toX_World), queen.GetComponent<Queen>().isWhite, false);
	}

	[ClientRpc]
	void RpcMoveFigure(NetworkIdentity identity, int fromZ_Board, int fromX_Board, int toZ_World, int toX_World, bool isWhite, bool isKing)
	{
		board.SetCell(fromZ_Board, fromX_Board, board.GetBoardPos(toZ_World), board.GetBoardPos(toX_World), isWhite, isKing);
		identity.transform.position = new Vector3(toX_World, 0, toZ_World);
		identity.GetComponent<Chessman>().OnMove(toZ_World, toX_World);

		if (isServer)
			IsCheckOrEndGame(identity.GetComponent<Chessman>());
	}

	[ClientRpc]
	void RpcSetCell(int fromZ_Board, int fromX_Board, int toZ_Board, int toX_Board, bool isWhite, bool isKing)
	{
		board.SetCell(fromZ_Board, fromX_Board, toZ_Board, toX_Board, isWhite, isKing);
	}
}
