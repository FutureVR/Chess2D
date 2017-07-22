using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : Piece {

    public King(int owner, Vector2 position, Dictionary<string, Sprite> spriteForTheme) : base(owner, position)
    {
        this.spriteForTheme = spriteForTheme;
    }

    public override HashSet<Vector2> returnPossibleMoveIndices(Vector2 myIndex, HashSet<IndexMove> otherMoves)
    {
        HashSet<Vector2> possibleMoveIndices = new HashSet<Vector2>();
        GameBoard gameBoard = BoardController.instance.gameBoard_;

        // Add all possible displacements from original position
        // to the set allIndexDisplacements
        HashSet<Vector2> allIndexDisplacements = new HashSet<Vector2>();

        // Set all possible displacements
        int[,] whiteAttackersAtTile =  BoardController.instance.gameBoard_.whiteAttackersAtTile;
        int[,] blackAttackersAtTile = BoardController.instance.gameBoard_.blackAttackersAtTile;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x != 0 || y != 0)
                {
                    // Check if the tile is being attacked
                    if (x + (int)myIndex.x >= 0 && x + (int)myIndex.x < gameBoard.width &&
                        0 <= y + (int)myIndex.y && y + (int)myIndex.y < gameBoard.height)
                    {
                        if ( (blackAttackersAtTile[x + (int)myIndex.x, y + (int)myIndex.y] == 0  &&  owner_ == 0) ||
                             (whiteAttackersAtTile[x + (int)myIndex.x, y + (int)myIndex.y] == 0 && owner_ == 1))
                        {
                            Vector2 displacement = new Vector2(x, y);
                            allIndexDisplacements.Add(displacement);
                        }
                    }
                }
            }
        }

        // Use displacements to get index positions
        foreach (Vector2 displacement in allIndexDisplacements)
            checkAndAddMovementAtTile(myIndex + displacement, possibleMoveIndices, gameBoard, true, false);

        return possibleMoveIndices;
    }

    public override HashSet<Vector2> returnAttackIndices(Vector2 myIndex)
    {
        HashSet<IndexMove> dummy = new HashSet<IndexMove>();
        return returnPossibleMoveIndices(myIndex, dummy);
    }
}
