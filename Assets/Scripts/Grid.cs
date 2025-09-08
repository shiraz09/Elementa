using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;

public class Grid : MonoBehaviour
{
    // Types of pieces
    public enum PieceType { WATER, SUN, EARTH, GRASS };

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
    private GameObject[,] pieces;

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
                    new Vector3(x, y, 0f),   // temporary position
                    Quaternion.identity
                );
                background.transform.parent = transform;

                // Position background in centered grid
                background.GetComponent<RectTransform>().anchoredPosition =
                    new Vector3(
                        (x * cellSize) - (cellSize * xDim / 2) + (cellSize / 2),
                        (y * cellSize) - (cellSize * yDim / 2) + (cellSize / 2),
                        0f
                    );
            }
        }

        // Create random pieces on the board
        pieces = new GameObject[xDim, yDim];
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                // Pick random type from list
                PiecePrefab entry = piecePrefabs[Random.Range(0, piecePrefabs.Length)];
                GameObject prefab = piecePrefabDict[entry.type];

                // Create piece
                GameObject go = Instantiate(prefab, new Vector3(x, y, -0.1f), Quaternion.identity);
                go.name = $"Piece({x},{y})";
                go.transform.parent = transform;

                // Position piece in centered grid
                go.GetComponent<RectTransform>().anchoredPosition =
                    new Vector3(
                        (x * cellSize) - (cellSize * xDim / 2) + (cellSize / 2),
                        (y * cellSize) - (cellSize * yDim / 2) + (cellSize / 2),
                        0f
                    );

                pieces[x, y] = go;
            }
        }
    }


    public enum TargetMode { None,Row, Column, Cell3x3, AllOfType }
    private TargetMode pendingMode = TargetMode.None;
    public void EnterTargetMode(TargetMode mode)
    {
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

    
