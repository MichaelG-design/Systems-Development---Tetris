using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

//Added V for my custom piece
public enum Tetronimo { I, O, T, J, L, S, Z, V }

[Serializable]
public struct TetronimoData
{
    public Tetronimo tetronimo;
    public Vector2Int[] cells;
    public Tile tile;
}