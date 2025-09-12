using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;
using System.Collections;
using NUnit.Framework;

public class Grid : MonoBehaviour
{
    // Types of pieces
    public enum PieceType { EARTH,GRASS,WATER,SUN };

    // Pair each type with its prefab
    [System.Serializable]
    public struct PiecePrefab
    {
        public PieceType type;     // piece type
        public GameObject prefab;  // prefab to spawn
    };

    // Board size and cell size
    public int xDim = 8;
    public int yDim = 8;
    public float cellSize = 1f;
    public float fillTime;
    private GamePiece pressedPiece;
    private GamePiece enteredPiece;
    

    // Prefabs for pieces and background
    public PiecePrefab[] piecePrefabs;
    public GameObject backgroundPrefab;
    private PieceType[] availableTypes;

    // Dictionary: type → prefab
    private Dictionary<PieceType, GameObject> piecePrefabDict;
    // 2D array for placed pieces
    private GamePiece[,] pieces;

    void Start()
    {
        // Build dictionary from piecePrefabs
        piecePrefabDict = new Dictionary<PieceType, GameObject>();
        for (int i = 0; i < piecePrefabs.Length; i++)
        {
            PiecePrefab e = piecePrefabs[i];
            if (e.prefab != null && !piecePrefabDict.ContainsKey(e.type))
            {
                piecePrefabDict.Add(e.type, e.prefab);
            }
        }
        availableTypes = new PieceType[piecePrefabDict.Count];
        piecePrefabDict.Keys.CopyTo(availableTypes, 0);

        RectTransform boardRT = (RectTransform)transform;
        boardRT.sizeDelta = new Vector2(xDim * cellSize, yDim * cellSize);
        boardRT.pivot = new Vector2(0.5f, 0.5f);

        // Create background tiles
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                GameObject background = Instantiate(backgroundPrefab);
                background.name = $"BG({x},{y})";
                background.transform.SetParent(transform, false);
                RectTransform rectTransform = background.GetComponent<RectTransform>();
                if (rectTransform) { rectTransform.sizeDelta = new Vector2(cellSize, cellSize); rectTransform.anchoredPosition = GetUIPos(x, y); }
                var img = background.GetComponent<UnityEngine.UI.Image>();
                if (img) img.raycastTarget = false;

            }
        }

        // Create random pieces on the board
        pieces = new GamePiece[xDim, yDim];
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                PieceType type = availableTypes[UnityEngine.Random.Range(0, availableTypes.Length)];
                SpawnNewPiece(x, y, type);
            }
        }
        StartCoroutine(Fill());
    }
    private PieceType GetRandomType()
{
    if (availableTypes == null || availableTypes.Length == 0)
        return PieceType.EARTH;

    int index = UnityEngine.Random.Range(0, availableTypes.Length);
    return availableTypes[index];
}
    public IEnumerator Fill()
    {
        while (FillStep())
        {
            yield return new WaitForSeconds(fillTime);

        }

    }
    public bool FillStep()
    {
        bool movedPiece = false;
        for (int y = yDim - 2; y >= 0; y--)
        {
            for (int x = 0; x < xDim; x++)
            {
                GamePiece piece = pieces[x, y];
                if (piece != null && piece.IsMoveable())
                {
                    GamePiece pieceBelow = pieces[x, y + 1];
                    if (pieceBelow == null)
                    {
                        piece.MoveableComponent.Move(x, y + 1);
                        pieces[x, y + 1] = piece;
                        pieces[x, y] = null;
                        movedPiece = true;

                    }

                }
            }
        }
            for (int x = 0; x < xDim; x++)
            {
                if (pieces[x, 0] == null)
                {
                    
                    PieceType type = GetRandomType();
                    GameObject newPiece = Instantiate(piecePrefabDict[type]);
                    newPiece.transform.SetParent(transform, false);
                    RectTransform rectTransform = newPiece.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.sizeDelta = new Vector2(cellSize, cellSize);
                        rectTransform.anchoredPosition = GetUIPos(x, -1);
                    }
                    GamePiece gamePiece = newPiece.GetComponent<GamePiece>();
                    if (gamePiece == null) gamePiece = newPiece.AddComponent<GamePiece>();
                    gamePiece.Init(x, -1, this, type);
                    gamePiece.MoveableComponent.Move(x, 0);
                    pieces[x, 0] = gamePiece;
                    movedPiece = true;

                }

            }

        return movedPiece;
    }

    public Vector2 GetUIPos(int x, int y)
    {
        return new Vector2(
                       (x * cellSize) - (cellSize * xDim / 2) + (cellSize / 2),
                       (y * cellSize) - (cellSize * yDim / 2) + (cellSize / 2)

                   );
    }
    public GamePiece SpawnNewPiece(int x, int y, PieceType type)
    {
        GameObject gameObject = Instantiate(piecePrefabDict[type]);
        gameObject.name = $"Piece({x},{y})-{type}";
        gameObject.transform.SetParent(transform, false); // UI
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        if (rectTransform!=null) { rectTransform.sizeDelta = new Vector2(cellSize, cellSize); rectTransform.anchoredPosition = GetUIPos(x, y); }
        GamePiece gamePiece = gameObject.GetComponent<GamePiece>();
        if (gamePiece == null) gamePiece = gameObject.AddComponent<GamePiece>();
        gamePiece.Init(x, y, this, type);

        pieces[x, y] = gamePiece;
        return gamePiece;
    }
    public bool IsAdjacent(GamePiece piece1, GamePiece piece2)
    {
        return (piece1.X == piece2.X && (int)Mathf.Abs(piece1.Y - piece2.Y) == 1) ||
               (piece1.Y == piece2.Y && (int)Mathf.Abs(piece1.X - piece2.X) == 1);
    }
    public void SwapPieces(GamePiece piece1, GamePiece piece2)
    {
        if (!piece1.IsMoveable() || !piece2.IsMoveable()) return;
        if (!IsAdjacent(piece1, piece2)) return;
        if (piece1.IsMoveable() && piece2.IsMoveable())
        {
            // Swap pieces in array
            pieces[piece1.X, piece1.Y] = piece2;
            pieces[piece2.X, piece2.Y] = piece1;

            // Swap their coordinates
            int piece1X = piece1.X;
            int piece1Y = piece1.Y;
            piece1.MoveableComponent.Move(piece2.X, piece2.Y);
            piece2.MoveableComponent.Move(piece1X, piece1Y);
        }   
    }
    public void PressPiece(GamePiece piece)
    {
        pressedPiece = piece;
    }
    public void EnterPiece(GamePiece piece)
    {
        if (pressedPiece != null && piece != pressedPiece)
            enteredPiece = piece;
    }
    public void ReleasePiece()
    {
        if (pressedPiece != null && enteredPiece != null && IsAdjacent(pressedPiece, enteredPiece))
        {
            SwapPieces(pressedPiece, enteredPiece);
        }
        pressedPiece = null;
        enteredPiece = null;
    }
    

    public enum TargetMode { None, Row, Column, Cell3x3, AllOfType }
    private TargetMode pendingMode = TargetMode.None;
    public void EnterTargetMode(TargetMode mode){
        pendingMode = mode;
    }
    public void OnCellClicked(int x, int y, PieceType t)
    {
        if (pendingMode == TargetMode.None) return;
        switch (pendingMode)
        {
            case TargetMode.Row:
                ClearRow(y);
                break;
            case TargetMode.Column:
                ClearColumn(x);
                break;
            case TargetMode.Cell3x3:
                Bomb3x3(x, y);
                break;
            case TargetMode.AllOfType:
                ClearAllOfType(t);
                break;
        }
        pendingMode = TargetMode.None;
    }
    public void ClearRow(int y) {
        if (y < 0 || y >= yDim) return; // Invalid row
        for (int x = 0; x < xDim; x++)
        {
            if (pieces[x, y] != null)
            {
                Destroy(pieces[x, y]);
                pieces[x, y] = null;
            }
        }
    }
    public void ClearColumn(int x)
    {
        if (x < 0 || x >= xDim) return;
        for (int y = 0; y < yDim; y++)
        {
            if (pieces[x, y] != null)
            {
                Destroy(pieces[x, y]);
                pieces[x, y] = null;
            }
        }
    }

    public void ClearAllOfType(PieceType type) {
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (pieces[x, y] != null && pieces[x, y].CompareTag(type.ToString()))
                {
                    Destroy(pieces[x, y]);
                    pieces[x, y] = null;
                }
            }
        }
    }
    public void Bomb3x3(int centerX, int centerY) {
        
    }
    
        
        
}

    
