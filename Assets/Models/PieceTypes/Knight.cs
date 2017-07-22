using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : Piece {

    public Knight(int owner, Vector2 position, Dictionary<string, Sprite> spriteForTheme) : base(owner, position)
    {
        this.spriteForTheme = spriteForTheme;
    }

    //bool checkAndAddMovementAtTile(Vector2 currentIndex, HashSet<Vector2> possibleMoves, GameBoard gameBoard)
    public override HashSet<Vector2> returnPossibleMoveIndices(Vector2 myIndex, HashSet<IndexMove> otherMoves)
    {
        otherMoves.Add(new IndexMove(new Vector2(0, 0), new Vector2(0, 0)));
        //Debug.Log(otherMoves.Count);

        return returnMoveIndicesFromAttackIndices(myIndex);
    }

    public override HashSet<Vector2> returnAttackIndices(Vector2 myIndex)
    {
        HashSet<Vector2> possibleMoveIndices = new HashSet<Vector2>();
        GameBoard gameBoard = BoardController.instance.gameBoard_;

        // Add all possible displacements from original position
        // to the set allIndexDisplacements
        HashSet<Vector2> allIndexDisplacements = new HashSet<Vector2>();

        allIndexDisplacements.Add(new Vector2(2, 1));
        allIndexDisplacements.Add(new Vector2(1, 2));
        allIndexDisplacements.Add(new Vector2(1, -2));
        allIndexDisplacements.Add(new Vector2(2, -1));
        allIndexDisplacements.Add(new Vector2(-2, 1));
        allIndexDisplacements.Add(new Vector2(-1, 2));
        allIndexDisplacements.Add(new Vector2(-1, -2));
        allIndexDisplacements.Add(new Vector2(-2, -1));


        // Iterate through each displacement, adding it to possibleMoveIndices
        foreach (Vector2 displacement in allIndexDisplacements)
            checkAndAddMovementAtTile(myIndex + displacement, possibleMoveIndices, gameBoard, true, true);

        return possibleMoveIndices;
    }
}
