using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMove {

    public Tile tileStart;
    public Tile tileEnd;

    public TileMove(Tile tileStart_, Tile tileEnd_)
    {
        tileStart = tileStart_;
        tileEnd = tileEnd_;
    }
}
