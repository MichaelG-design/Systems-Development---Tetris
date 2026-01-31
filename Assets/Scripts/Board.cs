using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{


    public Piece piecePrefab;
    Piece activePiece;
    public Tilemap tilemap;
    public TetrisManager tetrisManager;

    public TetronimoData[] tetronimos;
    public Vector2Int boardSize;
    public Vector2Int startPostion;

    public float dropInterval = 0.5f;

    float dropTime = 0.0f;

    //Sets up the inspector to hold the bonus text gameObject
    public GameObject BonusText;

    // Maps tilemap postion to a piece gameObject
    Dictionary<Vector3Int, Piece> pieces = new Dictionary<Vector3Int, Piece>();

    int left
    {
        get { return -boardSize.x / 2; }
    }

    int right
    {
        get { return boardSize.x / 2; }
    }

    int top
    {
        get { return boardSize.y / 2; }
    }

    int bottom
    {
        get { return -boardSize.y / 2; }
    }

    private void Update()
    {
        if (tetrisManager.gameOver) return;

        dropTime += Time.deltaTime;

        if (dropTime >= dropInterval)
        {
            dropTime = 0.0f;

            Clear(activePiece);
            bool moveResult = activePiece.Move(Vector2Int.down);
            Set(activePiece);

            if (!moveResult)
            {
                activePiece.freeze = true;

                CheckBoard();
                SpawnPiece();
            }
        }
    }

    public void SpawnPiece()
    {
        activePiece = Instantiate(piecePrefab);

        Tetronimo t = (Tetronimo)Random.Range(0, tetronimos.Length);

        activePiece.Initialize(this, t);

        CheckEndGame();

        Set(activePiece);
    }

    void CheckEndGame()
    {
        if (!IsPositionValid(activePiece, activePiece.position))
        {
            //If there is not a valid position for the newly placed piece, game over
            tetrisManager.SetGameOver(true);
        }
    }

    public void UpdateGameOver()
    {
        if (!tetrisManager.gameOver)
        {
            ResetBoard();
        }
    }

    void ResetBoard()
    {
        Piece[] foundPieces = FindObjectsByType<Piece>(FindObjectsSortMode.None);

        foreach (Piece piece in foundPieces) Destroy(piece.gameObject);

        activePiece = null;

        tilemap.ClearAllTiles();

        pieces.Clear();

        SpawnPiece();
    }

    void SetTile(Vector3Int cellPosition, Piece piece)
    {
        if (piece == null)
        {
            tilemap.SetTile(cellPosition, null);

            pieces.Remove(cellPosition);
        }
        else
        {
            tilemap.SetTile(cellPosition, piece.data.tile);

            // This line is craeting a association between the cell position and the piece gameObject
            pieces[cellPosition] = piece;
        }
    }

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + piece.position);
            SetTile(cellPosition, piece);
        }
    }
    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + piece.position);
            SetTile(cellPosition, null);
        }
    }
    public bool IsPositionValid(Piece piece, Vector2Int position)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + position);


            if (cellPosition.x < left || cellPosition.x >= right ||

              cellPosition.y < bottom || cellPosition.y >= top) return false;

            if (tilemap.HasTile(cellPosition)) return false;
        }
        return true;
    }

    public void CheckBoard()
    {
        //checks if line is full and clears it
        List<int> destroyedLines = new List<int>();
        for (int y = bottom; y < top; y++)
        {
            if (IsLineFull(y))
            {
                DestroyLine(y);
                destroyedLines.Add(y);
            }
        }

        int rowsShiftedDown = 0;
        foreach (int y in destroyedLines)
        {
            ShiftRowsDown(y - rowsShiftedDown);
            rowsShiftedDown++;
        }


        int score = tetrisManager.CalculateScore(destroyedLines.Count);
        tetrisManager.ChangeScore(score);

        //The "Big Bonus" ability for the V Piece:
        //If the player manages to clear 2 or more rows at the same time, using the V piece,
        //they get a VERY considerate bonus to the total score
        if (activePiece != null && activePiece.data.tetronimo == Tetronimo.V &&
                !activePiece.BigBonus && destroyedLines.Count >= 2)
        {
            //Flat bonus rewarded when conditions are met with the V piece
            int bonusScore = 1000;
            tetrisManager.ChangeScore(bonusScore);

            //Show the bonus text for 2 seconds
            StartCoroutine(ShowBonusText(2f));

            //Big Bonus will only work once per V piece
            activePiece.BigBonus = true;
        }
    }

    void ShiftRowsDown(int clearedRow)
    {
        for (int y = clearedRow + 1; y < top; y++)
        {
            for (int x = left; x < right; x++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y);

                if (pieces.ContainsKey(cellPosition))
                {
                    // Store the piece temporarily
                    Piece currentPiece = pieces[cellPosition];

                    // Clear the position it is in
                    SetTile(cellPosition, null);

                    // Move the tile down
                    cellPosition.y -= 1;
                    SetTile(cellPosition, currentPiece);
                }
            }
        }
    }

    bool IsLineFull(int y)
    {
        for (int x = left; x < right; x++)
        {
            Vector3Int cellPosition = new Vector3Int(x, y);
            if (!tilemap.HasTile(cellPosition)) return false;
        }

        return true;
    }
    void DestroyLine(int y)
    {
        for (int x = left; x < right; x++)
        {
            Vector3Int cellPosition = new Vector3Int(x, y);

            // Clean up gameObjects
            if (pieces.ContainsKey(cellPosition))
            {
                Piece piece = pieces[cellPosition];

                piece.ReduceActiveCount();

                SetTile(cellPosition, null);
            }
        }
    }

    //Show "+Big Bonus!" text for 2 seconds then hide the object
    IEnumerator ShowBonusText(float duration)
    {
        if (BonusText == null)
            yield break;

        BonusText.SetActive(true);
        yield return new WaitForSeconds(duration);
        BonusText.SetActive(false);
    }
}