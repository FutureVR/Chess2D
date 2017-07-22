using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rook : Piece {

    public Rook(int owner, Vector2 position, Dictionary<string, Sprite> spriteForTheme) : base(owner, position)
    {
        isCastleable = true;
        this.spriteForTheme = spriteForTheme;
    }

    public override HashSet<Vector2> returnPossibleMoveIndices(Vector2 myIndex, HashSet<IndexMove> otherMoves)
    {
        return returnMoveIndicesFromAttackIndices(myIndex);
    }

    public override HashSet<Vector2> returnAttackIndices(Vector2 myIndex)
    {
        return returnAttackIndices_Straight(myIndex);
    }
}
