using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Queen : Piece {

    public Queen(int owner, Vector2 position, Dictionary<string, Sprite> spriteForTheme) : base(owner, position)
    {
        this.spriteForTheme = spriteForTheme;
    }

    public override HashSet<Vector2> returnPossibleMoveIndices(Vector2 myIndex, HashSet<IndexMove> otherMoves)
    {
        return returnMoveIndicesFromAttackIndices(myIndex);
    }

    public override HashSet<Vector2> returnAttackIndices(Vector2 myIndex)
    {
        HashSet<Vector2> diagonalMoveIndices = returnAttackIndices_Diagonal(myIndex);
        HashSet<Vector2> straightMoveIndices = returnAttackIndices_Straight(myIndex);
        diagonalMoveIndices.UnionWith(straightMoveIndices);
        return diagonalMoveIndices;
    }
}
