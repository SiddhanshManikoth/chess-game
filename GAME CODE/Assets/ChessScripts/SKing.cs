using System.Collections;
using UnityEngine;

public class SKing : SChessman
{
    public override bool[,] chessPossibleMoves()
    {
        bool[,] r = new bool[8, 8];

        chessMove(CurrentX + 1, CurrentY, ref r); // up
        chessMove(CurrentX - 1, CurrentY, ref r); // down
        chessMove(CurrentX, CurrentY - 1, ref r); // left
        chessMove(CurrentX, CurrentY + 1, ref r); // right
        chessMove(CurrentX + 1, CurrentY - 1, ref r); // up left
        chessMove(CurrentX - 1, CurrentY - 1, ref r); // down left
        chessMove(CurrentX + 1, CurrentY + 1, ref r); // up right
        chessMove(CurrentX - 1, CurrentY + 1, ref r); // down right

        return r;
    }



}
