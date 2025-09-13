using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;
using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;

public class Grid : MonoBehaviour
{
    // Types of pieces
    public enum PieceType { EARTH, GRASS, WATER, SUN };
    public ResourceManagement bank;

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
        HashSet<GamePiece> startMatches = FindAllMatchesOnBoard();
        if (startMatches.Count >= 3)
        {
            foreach (var p in startMatches)
            {
                if (p != null) RemoveAt(p.X, p.Y,true);
            }
            yield return new WaitForSeconds(0.05f);
            yield return StartCoroutine(FillAndResolve());
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
       
        int yFlip = (yDim - 1) - y;  

        return new Vector2(
            (x * cellSize) - (cellSize * xDim / 2f) + (cellSize / 2f),
            (yFlip * cellSize) - (cellSize * yDim / 2f) + (cellSize / 2f)
        );
    }
    public GamePiece SpawnNewPiece(int x, int y, PieceType type)
    {
        GameObject gameObject = Instantiate(piecePrefabDict[type]);
        gameObject.name = $"Piece({x},{y})-{type}";
        gameObject.transform.SetParent(transform, false); // UI
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        if (rectTransform != null) { rectTransform.sizeDelta = new Vector2(cellSize, cellSize); rectTransform.anchoredPosition = GetUIPos(x, y); }
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
    public List<GamePiece> GetMatch(GamePiece piece, int newX, int newY)
    {
        if (piece == null) return null;
        int centerX;
        if (newX >= 0)
        {
            centerX = newX;
        }
        else
        {
            centerX = piece.X;
        }

        int centerY;
        if (newY >= 0)
        {
            centerY = newY;
        }
        else
        {
            centerY = piece.Y;
        }
        PieceType pieceType = piece.Type;
        List<GamePiece> result = new List<GamePiece>();
        List<GamePiece> linePieces = new List<GamePiece>();
        linePieces.Add(piece);
        int x;
        for (x = centerX - 1; x >= 0; x--)
        {
            GamePiece p = pieces[x, centerY];
            if (p == null || p.Type != pieceType) break;
            linePieces.Add(p);
        }
        for (x = centerX + 1; x < xDim; x++)
        {
            GamePiece p = pieces[x, centerY];
            if (p == null || p.Type != pieceType) break;
            linePieces.Add(p);
        }
        if (linePieces.Count >= 3)
        {
            for (int i = 0; i < linePieces.Count; i++)
                if (!result.Contains(linePieces[i])) result.Add(linePieces[i]);
        }
        linePieces.Clear();
        linePieces.Add(piece);

        int y;
        for (y = centerY - 1; y >= 0; y--)
        {
            GamePiece p = pieces[centerX, y];
            if (p == null || p.Type != pieceType) break;
            linePieces.Add(p);
        }
        for (y = centerY + 1; y < yDim; y++)
        {
            GamePiece p = pieces[centerX, y];
            if (p == null || p.Type != pieceType) break;
            linePieces.Add(p);
        }
        if (linePieces.Count >= 3)
        {
            for (int i = 0; i < linePieces.Count; i++)
                if (!result.Contains(linePieces[i])) result.Add(linePieces[i]);
        }

        if (result.Count >= 3) return result;
        return null;

    }
    private void RemoveAt(int x, int y, bool awardResource= false)
    {
        GamePiece piece = pieces[x, y];
        if (piece != null)
        {
            if (awardResource && bank != null)
                bank.Add(piece.Type,1);
        
        
            Destroy(piece.gameObject);
            pieces[x, y] = null;
        }
        

    }
   public void SwapPieces(GamePiece firstPiece, GamePiece secondPiece)
    {
    if (firstPiece == null || secondPiece == null) return;
    if (!firstPiece.IsMoveable() || !secondPiece.IsMoveable()) return;
    if (!IsAdjacent(firstPiece, secondPiece)) return;

    int firstX = firstPiece.X;
    int firstY = firstPiece.Y;
    int secondX = secondPiece.X;
    int secondY = secondPiece.Y;

    // עדכון במערך + אנימציית מעבר
    pieces[firstX, firstY] = secondPiece;
    firstPiece.MoveableComponent.Move(secondX, secondY);

    pieces[secondX, secondY] = firstPiece;
    secondPiece.MoveableComponent.Move(firstX, firstY);

    // בדיקת מאצ'ים שנוצרו
    List<GamePiece> matchForFirst = GetMatch(firstPiece, secondX, secondY);
    List<GamePiece> matchForSecond = GetMatch(secondPiece, firstX, firstY);

    HashSet<GamePiece> piecesToClear = new HashSet<GamePiece>();
    if (matchForFirst != null) { for (int i = 0; i < matchForFirst.Count; i++) piecesToClear.Add(matchForFirst[i]); }
    if (matchForSecond != null) { for (int i = 0; i < matchForSecond.Count; i++) piecesToClear.Add(matchForSecond[i]); }

    // אין מאץ' -> מחזירים לאחור
    if (piecesToClear.Count < 3)
    {
        pieces[firstX, firstY] = firstPiece;
        firstPiece.MoveableComponent.Move(firstX, firstY);

        pieces[secondX, secondY] = secondPiece;
        secondPiece.MoveableComponent.Move(secondX, secondY);
        return;
    }

    // יש מאץ' -> נקה את כל המאצים בלוח ואח"כ קסקדה
    if (ClearAllValidMatches())
    {
        StartCoroutine(FillAndResolve());
    }
    }
    private HashSet<GamePiece> FindAllMatchesOnBoard()
    {
        HashSet<GamePiece> set = new HashSet<GamePiece>();

        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                GamePiece piece = pieces[x, y];
                if (piece == null) continue;

                List<GamePiece> lineMatch = GetMatch(piece, x, y);
                if (lineMatch != null && lineMatch.Count >= 3)
                {
                    for (int i = 0; i < lineMatch.Count; i++) set.Add(lineMatch[i]);
                }
            }
        }
        return set;
    }
    private System.Collections.IEnumerator FillAndResolve()
    {
        // Drop and refill until no piece moved this step
        while (FillStep())
        {
            yield return new WaitForSeconds(fillTime);
        }

        // After gravity/refill, see if new matches were created
        HashSet<GamePiece> more = FindAllMatchesOnBoard();
        if (more.Count >= 3)
        {
            foreach (GamePiece p in more)
            {
                if (p != null) RemoveAt(p.X, p.Y,true);
            }

            // Run another cascade cycle
            yield return new WaitForSeconds(0.05f);
            StartCoroutine(FillAndResolve());
        }
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
        StartCoroutine(FillAndResolve());  
    }
    public void ClearRow(int y) {
        if (y < 0 || y >= yDim) return; // Invalid row
        for (int x = 0; x < xDim; x++)
        {
            if (pieces[x, y] != null)
            {
                RemoveAt(x, y, false); 
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
                RemoveAt(x, y, false); 
            }
        }
    }

    public void ClearAllOfType(PieceType type) {
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                GamePiece piece = pieces[x, y];
                if (piece != null && piece.Type == type)
                    RemoveAt(x, y, false);  // no resource award
        
            }
        }
    }
    
    public bool ClearAllValidMatches()
    {
        bool needsRefill = false;
        HashSet<GamePiece> toClear = new HashSet<GamePiece>();

        //collect all matches (>=3) across the board
        for (int y = 0; y < yDim; y++)
        {
            for (int x = 0; x < xDim; x++)
            {
                GamePiece piece = pieces[x, y];
                if (piece == null) continue;                // ← משאירים רק את בדיקת ה-null

                List<GamePiece> match = GetMatch(piece, x, y);
                if (match != null && match.Count >= 3)
                {
                    for (int i = 0; i < match.Count; i++)
                        toClear.Add(match[i]);
                }
            }
        }
        //clear them (awardResource = true since these are match clears)
        GamePiece[] toClearArray = new GamePiece[toClear.Count];
        toClear.CopyTo(toClearArray);

        for (int i = 0; i < toClearArray.Length; i++)
        {
        GamePiece gp = toClearArray[i];
        if (gp != null)
        {
            RemoveAt(gp.X, gp.Y, true); // true => add resource to bank
            needsRefill = true;
        }
        }

        return needsRefill;
    }
    public void Bomb3x3(int centerX, int centerY)
    {

    }
    
        
        
}

    
