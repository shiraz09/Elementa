using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;

public class Grid : MonoBehaviour
{
    // Types of pieces
    public enum PieceType { EARTH,EMPTY };
    public enum elemType{EARTH,GRASS,WATER,SUN}

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

    // Prefabs for pieces and background
    public PiecePrefab[] piecePrefabs;
    public GameObject backgroundPrefab;

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
            if (!piecePrefabDict.ContainsKey(piecePrefabs[i].type) && piecePrefabs[i].prefab != null)
            {
                piecePrefabDict.Add(piecePrefabs[i].type, piecePrefabs[i].prefab);
            }
        }

        // Create background tiles
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                GameObject background = Instantiate(
                    backgroundPrefab,
                   Vector3.zero,
                    Quaternion.identity
                );
                background.transform.parent = transform;
                background.GetComponent<RectTransform>().anchoredPosition = GetScreenPosition(x, y);
            }
        }

        // Create random pieces on the board
        pieces = new GamePiece[xDim, yDim];
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                // Create piece
                SpawnnewPiece(x, y, PieceType.EMPTY);
            }
        }
        Fill();
    }
    public void Fill()
    {
        while (FillStep()){}
    }
    public bool FillStep()
    {
        bool movedPiece = false;
        for (int y = yDim - 2; y >= 0; y--){
            for (int x = 0; x < xDim; x++){
                GamePiece piece = pieces[x, y];
                if (piece.IsMoveable()){
                    GamePiece pieceBelow = pieces[x, y + 1];
                    if (pieceBelow.Type == PieceType.EMPTY){
                        piece.MoveableComponent.Move(x, y + 1);
                        pieces[x, y + 1] = piece;
                        SpawnnewPiece(x, y, PieceType.EMPTY);
                        movedPiece = true;
                    }
                }
            }
        }
        for (int x = 0; x < xDim; x++){
            GamePiece pieceBelow = pieces[x, 0];
            if (pieceBelow.Type == PieceType.EMPTY){
                GameObject newPiece = (GameObject)Instantiate(
                piecePrefabDict[PieceType.EARTH], GetScreenPosition(x,-1), Quaternion.identity);
                newPiece.transform.parent = transform;
                pieces[x, 0] = newPiece.GetComponent<GamePiece>();
                pieces[x, 0].Init(x, -1, this, PieceType.EARTH);
                pieces[x, 0].MoveableComponent.Move(x, 0);
                pieces[x, 0].ColorComponent.SetColor(
                    (ColorPiece.ColorType)Random.Range(0, pieces[x, 0].ColorComponent.NumColors)
                );
            }
    movedPiece = true;
    
}

return movedPiece;
    }
    public Vector2 GetScreenPosition(int x, int y)
    {
        return new Vector2(
                       (x * cellSize) - (cellSize * xDim / 2) + (cellSize / 2),
                       (y * cellSize) - (cellSize * yDim / 2) + (cellSize / 2)

                   );
    }
    public GamePiece SpawnnewPiece(int x, int y, PieceType type)
    {
        GameObject newPiece = (GameObject)Instantiate(piecePrefabDict[type], GetScreenPosition(x, y), Quaternion.identity);
        newPiece.transform.parent = transform;
        pieces[x, y] = newPiece.GetComponent<GamePiece>();
        pieces[x, y].Init(x, y, this, type);
        return pieces[x, y];
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

    
