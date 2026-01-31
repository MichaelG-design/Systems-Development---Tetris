using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public TetronimoData data;
    public Board board;
    public Vector2Int[] cells;

    public Vector2Int position;

    public bool freeze = false;

    //This will track whether my custom piece's ability has already been used or not
    //so that it can only be used once per piece placement
    public bool BigBonus = false;

    int activeCellCount = -1;

    public void Initialize(Board board, Tetronimo tetronimo)
    {

        this.board = board;

        for (int i = 0; i < board.tetronimos.Length; i++)
        {
            if (board.tetronimos[i].tetronimo == tetronimo)
            {
                this.data = board.tetronimos[i];
                break;
            }
        }

        cells = new Vector2Int[data.cells.Length];
        for (int i = 0; i < data.cells.Length; i++) cells[i] = data.cells[i];

        position = board.startPostion;

        activeCellCount = cells.Length;

    }
    private void Update()
    {
        if (board.tetrisManager.gameOver) return;

        if (freeze) return;

        board.Clear(this);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDrop();
        }
        else
        {
            // If we harddrop do not proccess any other piece motion
            if (Input.GetKeyDown(KeyCode.A))
            {
                Move(Vector2Int.left);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                Move(Vector2Int.right);
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                Move(Vector2Int.down);
            }

            // else if (Input.GetKeyDown(KeyCode.W))
            //{
            //    Move(Vector2Int.up);
            //}

            // Rotation
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Rotate(1);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                Rotate(-1);
            }
        }
        board.Set(this);

        // Debug only - "P"
        if (Input.GetKeyDown(KeyCode.P))
        {
            board.CheckBoard();
        }

        // We only check board, and spawn piece after piece has been moved
        if (freeze)
        {
            board.CheckBoard();
            board.SpawnPiece();
        }
    }

    void Rotate(int direction)
    {
        // Store the cell locations so that we can revert it
        Vector2Int[] temporaryCells = new Vector2Int[cells.Length];

        for (int i = 0; i < cells.Length; i++) temporaryCells[i] = cells[i];

        ApplyRotation(direction);

        if (!board.IsPositionValid(this, position))
        {
            if (!TryWallKicks())
            {
                RevertRotation(temporaryCells);
            }
            else
            {
                Debug.Log("Wall kick suceeded");
            }
        }
        else
        {
            Debug.Log("Valid rotation");
        }
    }

    bool TryWallKicks()
    {
        Vector2Int[] wallKickOffsets = new Vector2Int[]
        {
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.down,
            new Vector2Int(-1, -1), // Diagonal down-left
            new Vector2Int(1, -1) // Diagonal down-right
        };

        foreach (Vector2Int offset in wallKickOffsets)
        {
            if (Move(offset)) return true;
        }

        return false;
    }

    void RevertRotation(Vector2Int[] temporarycells)
    {
        for (int i = 0; i < cells.Length; i++) cells[i] = temporarycells[i];
    }

    void ApplyRotation(int direction)
    {
        Quaternion rotation = Quaternion.Euler(0, 0, 90.0f * direction);

        bool isSpecial = data.tetronimo == Tetronimo.I || data.tetronimo == Tetronimo.O;
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3 cellPosition = new Vector3(cells[i].x, cells[i].y);

            if (isSpecial)
            {
                cellPosition.x -= 0.5f;
                cellPosition.y -= 0.5f;
            }

            Vector3 result = rotation * cellPosition;

            if (isSpecial)
            {
                cells[i].x = Mathf.CeilToInt(result.x);
                cells[i].y = Mathf.CeilToInt(result.y);
            }
            else
            {
                cells[i].x = Mathf.RoundToInt(result.x);
                cells[i].y = Mathf.RoundToInt(result.y);
            }

        }
    }

    void HardDrop()
    {
        while (Move(Vector2Int.down))
        {
            // Do Nothing
        }

        freeze = true;
    }

    public bool Move(Vector2Int translation)
    {
        Vector2Int newPosition = position;
        newPosition += translation;

        bool positionValid = board.IsPositionValid(this, newPosition);
        if (positionValid) position = newPosition;

        return positionValid;
    }

    public void ReduceActiveCount()
    {
        activeCellCount -= 1;
        Debug.Log($"Tetronimo {data.tetronimo} active cell count = {activeCellCount}");
        if (activeCellCount <= 0)
        {
            Debug.Log($"Tetronimo {data.tetronimo} destroyed");
            Destroy(gameObject);
        }
    }

}