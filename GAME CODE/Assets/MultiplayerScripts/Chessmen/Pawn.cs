using System.Collections.Generic;

public class Pawn : Chessman
{
	protected override List<Move> GetValidMoves(bool checkFriendlyKingSafety, Cell[,] board)
	{
		List<Move> validMoves = new List<Move>();

		int endY, endX;

		if (isWhite)
			endY = Y_Board + 1;

		else
			endY = Y_Board - 1;

		for (int i = -1; i <= 1; i++)
		{
			endX = X_Board + i;

			CheckMove(endY, endX, validMoves, checkFriendlyKingSafety, board);
		}

		bool initialCell = isWhite ? Y_Board == 1 : Y_Board == 6;
		if (initialCell && board[isWhite ? Y_Board + 1 : Y_Board - 1, X_Board] == Cell.Empty)
			CheckMove(isWhite ? Y_Board + 2 : Y_Board - 2, X_Board, validMoves, checkFriendlyKingSafety, board);

		return validMoves;
	}

	void CheckMove(int endY, int endX, List<Move> validMoves, bool checkFriendlyKingSafety, Cell[,] board)
	{
		if (endX < 0 || endX > 7)
			return;

		int xDelta = endX - X_Board;
		Cell curCell = board[endY, endX];
		bool validKillMove = xDelta != 0 && (curCell & Enemy) == Enemy;
		bool validForwardMove = xDelta == 0 && curCell == Cell.Empty;

		bool isValidMove = validKillMove || validForwardMove;
		if (checkFriendlyKingSafety)
			isValidMove = isValidMove && !(isWhite ? Board.Instance.WhiteKing : Board.Instance.BlackKing).WillBeKilledAfterMove(Y_Board, X_Board, endY, endX, null);

		if (isValidMove)
			validMoves.Add(new Move(endY, endX, isKill: validKillMove, isCastle: false));
	}
}
