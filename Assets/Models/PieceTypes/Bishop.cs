using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bishop : Piece {

    public Bishop(int owner, Vector2 position, Dictionary<string, Sprite> spriteForTheme) : base(owner, position)
    {
        this.spriteForTheme = spriteForTheme;
    }

    public override HashSet<Vector2> returnPossibleMoveIndices(Vector2 myIndex, HashSet<IndexMove> otherMoves)
    {
        return returnMoveIndicesFromAttackIndices(myIndex);
    }

    public override HashSet<Vector2> returnAttackIndices(Vector2 myIndex)
    {
        return returnAttackIndices_Diagonal(myIndex);
    }
}
