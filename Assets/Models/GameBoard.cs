using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard {

    // Position of bottom left edge
    public Vector2 position; 
    public Vector2 size;

    public List<Piece> necessaryBlackPieces;
    public List<Piece> necessaryWhitePieces;

    public int currentPlayer { get; set; }
    public int maxPlayers { get; private set; }
    int turnNumber = 1;

    Tile[,] tiles;
    public int width;
    public int height;

    HashSet<Piece> activePieces;
    public int[,] blackAttackersAtTile { get; private set; }
    public int[,] whiteAttackersAtTile { get; private set; }

    public enum PieceTypes { Pawn, Knight, Bishop, Rook, Queen, King }


    public GameBoard(int width_, int height_)
    {
        width = width_;
        height = height_;

        necessaryBlackPieces = new List<Piece>();
        necessaryWhitePieces = new List<Piece>();

        activePieces = new HashSet<Piece>();

        currentPlayer = 0;
        maxPlayers = 2;

        initializeTiles();
        initializePieces();
    }

    void initializeTiles()
    {
        // Initialize the tiles for the board
        tiles = new Tile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bool isWhite;
                if ((x + y) % 2 == 0)
                    isWhite = true;
                else
                    isWhite = false;

                //BoardController bc = BoardController.instance;
                //Vector2 position = bc.worldCoordAtIndex(new Vector2(x, y));
                tiles[x, y] = new Tile(this, isWhite, x, y);
            }
        }

        //Initialize attackersAtTile array
        blackAttackersAtTile = new int[width, height];
        whiteAttackersAtTile = new int[width, height];
        calculateAttackersAtTile();
    }

    //Initializes pieces and adds to the list of necessary pieces
    void initializePieces()
    {
        //Create the pieces (holding the data), but do not create the gameObjects
        //Each piece holds two possible textures to be used, either white or black

        //Create the white pawns
        for (int x = 0; x < width; x++)
        {
            createPieceAtIndex(PieceTypes.Pawn, 0, new Vector2(x, 1));
        }

        //Create black pawns
        for (int x = 0; x < width; x++)
        {
            createPieceAtIndex(PieceTypes.Pawn, 1, new Vector2(x, height-2));
        }

        //Create rooks
        createPieceAtIndex(PieceTypes.Rook, 1, new Vector2(height-1, height-1));
        createPieceAtIndex(PieceTypes.Rook, 1, new Vector2(0, height-1));
        createPieceAtIndex(PieceTypes.Rook, 0, new Vector2(7, 0));
        createPieceAtIndex(PieceTypes.Rook, 0, new Vector2(0, 0));

        //Create knights
        createPieceAtIndex(PieceTypes.Knight, 1, new Vector2(6, height-1));
        createPieceAtIndex(PieceTypes.Knight, 1, new Vector2(1, height-1));
        createPieceAtIndex(PieceTypes.Knight, 0, new Vector2(6, 0));
        createPieceAtIndex(PieceTypes.Knight, 0, new Vector2(1, 0));

        //Create bishops
        createPieceAtIndex(PieceTypes.Bishop, 1, new Vector2(5, height-1));
        createPieceAtIndex(PieceTypes.Bishop, 1, new Vector2(2, height-1));
        createPieceAtIndex(PieceTypes.Bishop, 0, new Vector2(5, 0));
        createPieceAtIndex(PieceTypes.Bishop, 0, new Vector2(2, 0));

        //Create queens
        createPieceAtIndex(PieceTypes.Queen, 1, new Vector2(3, height-1));
        createPieceAtIndex(PieceTypes.Queen, 0, new Vector2(3, 0));

        //Create kings
        Piece blackKing = createPieceAtIndex(PieceTypes.King, 1, new Vector2(4, height-1));
        Piece whiteKing = createPieceAtIndex(PieceTypes.King, 0, new Vector2(4, 0));

        necessaryWhitePieces.Add(whiteKing);
        necessaryBlackPieces.Add(blackKing);

    }

    public Piece createPieceAtIndex(PieceTypes type, int owner, Vector2 index)
    {
        Piece piece = new Piece(owner, new Vector2(0,0));
        Vector2 worldCoord = BoardController.instance.worldCoordAtIndex(index);


        string ownerName = "";
        if (owner == 0)
            ownerName = "White";
        else if (owner == 1)
            ownerName = "Black";

        Dictionary<string, Dictionary<string, Sprite>> sprites =
            GameController.instance.themeToSpriteForPiece;


        if (type == PieceTypes.Pawn)
            piece = new Pawn(owner, worldCoord, sprites[ownerName + "Pawn"]);
        if (type == PieceTypes.Knight)
            piece = new Knight(owner, worldCoord, sprites[ownerName + "Knight"]);
        if (type == PieceTypes.Bishop)
            piece = new Bishop(owner, worldCoord, sprites[ownerName + "Bishop"]);
        if (type == PieceTypes.Rook)
            piece = new Rook(owner, worldCoord, sprites[ownerName + "Rook"]);
        if (type == PieceTypes.Queen)
            piece = new Queen(owner, worldCoord, sprites[ownerName + "Queen"]);
        if (type == PieceTypes.King)
            piece = new King(owner, worldCoord, sprites[ownerName + "King"]);

        tiles[(int)index.x, (int)index.y].myPiece_ = piece;
        activePieces.Add(piece);
        return piece;
    }

    public void destroyPiece(Piece piece_data, Tile tile_data)
    {
        piece_data.Captured = true;
        activePieces.Remove(piece_data);
    }

    public Tile getTileAtIndex(Vector2 index)
    {
        if (0 <= index.x && index.x < width && 0 <= index.y && index.y < height)
            return tiles[(int)index.x, (int)index.y];
        else
        {
            return null;
        }
    }

    public void changeTurn()
    {
        currentPlayer++;

        if (currentPlayer >= maxPlayers)
            currentPlayer = 0;

        turnNumber++;

        calculateAttackersAtTile();
    }

    void calculateAttackersAtTile()
    {
        // Clear the array
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                blackAttackersAtTile[x, y] = 0;
                whiteAttackersAtTile[x, y] = 0;
            }
        }

        
        

        // Set the values in the array
        foreach (Piece piece in activePieces)
        {
            Vector2 indexOfPiece = BoardController.instance.indexAtWorldCoord(piece.Position_);
            HashSet<Vector2> attackedTilesSet = piece.returnAttackIndices(indexOfPiece);

            if (attackedTilesSet != null)
            {
                if (piece.owner_ == 0)
                {
                    foreach (Vector2 attackedTile in attackedTilesSet)
                        whiteAttackersAtTile[(int)attackedTile.x, (int)attackedTile.y] += 1;
                }
                else if (piece.owner_ == 1)
                {
                    foreach (Vector2 attackedTile in attackedTilesSet)
                        blackAttackersAtTile[(int)attackedTile.x, (int)attackedTile.y] += 1;
                }
            }
        }
    }
}
