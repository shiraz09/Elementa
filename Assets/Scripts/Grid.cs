using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class Grid : MonoBehaviour
{
    public enum PieceType { EARTH, GRASS, WATER, SUN, GRASSOBS, ICEOBS }

    public ResourceManagement bank;
    public Level level;
    public GameUI gameUI;
    

    [System.Serializable]
    public struct PiecePrefab
    {
        public PieceType type;
        public GameObject prefab;
    }
    [System.Serializable]
    public struct PiecePosition
    {
        public PieceType type;
        public int x;
        public int y;
    }

    public int xDim = 8;
    public int yDim = 8;
    public float cellSize = 1f;
    public float fillTime = 0.1f;
    
    [Header("Obstacle Settings")]
    public int maxObstacles = 8; // מקסימום מכשולים בלוח
    public float obstacleSpawnChance = 0.15f; // סיכוי להופעת מכשול בכל משבצת
    
    [Header("Level System")]
    public int baseMoves = 20; // מהלכים בסיסיים לכל שלב
    public int movesPerObstacle = 5; // מהלכים נוספים לכל מכשול
    public int currentMoves; // מהלכים נוכחיים
    public int currentScore; // ניקוד נוכחי
    
    [Header("Scoring System")]
    public int match3Score = 50; // ניקוד למאצ' של 3
    public int match4Score = 80; // ניקוד למאצ' של 4
    public int match5Score = 120; // ניקוד למאצ' של 5
    
    [Header("Star System")]
    public int star1Score = 1000; // ניקוד לכוכב 1
    public int star2Score = 1500; // ניקוד לכוכב 2
    public int star3Score = 2000; // ניקוד לכוכב 3

    private GamePiece pressedPiece;
    private GamePiece enteredPiece;

    public PiecePrefab[] piecePrefabs;
    public GameObject backgroundPrefab;

    private PieceType[] availableTypes;
    private Dictionary<PieceType, GameObject> piecePrefabDict;
    private GamePiece[,] pieces;
    private bool gameOver = false;
    public PiecePosition[] initialPieces;

    // Initializes the board, background, and pieces
    void Start()
    {
        piecePrefabDict = new Dictionary<PieceType, GameObject>();
        for (int i = 0; i < piecePrefabs.Length; i++)
        {
            PiecePrefab e = piecePrefabs[i];
            if (e.prefab != null && !piecePrefabDict.ContainsKey(e.type))
                piecePrefabDict.Add(e.type, e.prefab);
        }

        availableTypes = new PieceType[piecePrefabDict.Count];
        piecePrefabDict.Keys.CopyTo(availableTypes, 0);

        RectTransform boardRT = (RectTransform)transform;
        boardRT.sizeDelta = new Vector2(xDim * cellSize, yDim * cellSize);
        boardRT.pivot = new Vector2(0.5f, 0.5f);

        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                GameObject bg = Instantiate(backgroundPrefab);
                bg.name = $"BG({x},{y})";
                bg.transform.SetParent(transform, false);
                RectTransform rt = bg.GetComponent<RectTransform>();
                if (rt)
                {
                    rt.sizeDelta = new Vector2(cellSize, cellSize);
                    rt.anchoredPosition = GetUIPos(x, y);
                }
                var img = bg.GetComponent<UnityEngine.UI.Image>();
                if (img) img.raycastTarget = false;
            }
        }

        pieces = new GamePiece[xDim, yDim];
        
        // יצירת חתיכות רגילות
        for (int x = 0; x < xDim; x++)
            for (int y = 0; y < yDim; y++)
                SpawnNewPiece(x, y, availableTypes[Random.Range(0, availableTypes.Length)]);
        
        // הוספת מכשולים רנדומליים
        SpawnRandomObstacles();

        StartCoroutine(Fill());
        
        // הפעלת בדיקת מהלכים תקופתית
        StartMoveChecking();
        
        // חישוב מהלכים לפי מכשולים
        CalculateMovesForLevel();
    }

    // Returns a random available piece type (excluding obstacles)
    private PieceType GetRandomType()
    {
        if (availableTypes == null || availableTypes.Length == 0) return PieceType.EARTH;
        
        // סינון מכשולים מהרשימה
        List<PieceType> regularTypes = new List<PieceType>();
        foreach (PieceType type in availableTypes)
        {
            if (type != PieceType.ICEOBS && type != PieceType.GRASSOBS)
                regularTypes.Add(type);
        }
        
        if (regularTypes.Count == 0) return PieceType.EARTH;
        return regularTypes[Random.Range(0, regularTypes.Count)];
    }
    
    // Returns a random obstacle type
    private PieceType GetRandomObstacleType()
    {
        PieceType[] obstacleTypes = { PieceType.ICEOBS, PieceType.GRASSOBS };
        return obstacleTypes[Random.Range(0, obstacleTypes.Length)];
    }
    
    // Spawns random obstacles on the board
    private void SpawnRandomObstacles()
    {
        int obstaclesSpawned = 0;
        
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                // בדיקה שלא נגמרו המכשולים המקסימליים
                if (obstaclesSpawned >= maxObstacles) break;
                
                // סיכוי רנדומלי להופעת מכשול
                if (Random.Range(0f, 1f) < obstacleSpawnChance)
                {
                    // וידוא שהמשבצת פנויה
                    if (pieces[x, y] == null)
                    {
                        PieceType obstacleType = GetRandomObstacleType();
                        SpawnNewPiece(x, y, obstacleType);
                        obstaclesSpawned++;
                    }
                }
            }
            if (obstaclesSpawned >= maxObstacles) break;
        }
        
        Debug.Log($"Spawned {obstaclesSpawned} obstacles on the board");
    }
    
    // Respawns obstacles after they are cleared (for level progression)
    public void RespawnObstacles()
    {
        int currentObstacles = GetObstacleCount();
        int obstaclesToSpawn = maxObstacles - currentObstacles;
        
        if (obstaclesToSpawn > 0)
        {
            for (int i = 0; i < obstaclesToSpawn; i++)
            {
                // מציאת משבצת פנויה רנדומלית
                List<Vector2Int> emptyCells = new List<Vector2Int>();
                for (int x = 0; x < xDim; x++)
                {
                    for (int y = 0; y < yDim; y++)
                    {
                        if (pieces[x, y] == null)
                            emptyCells.Add(new Vector2Int(x, y));
                    }
                }
                
                if (emptyCells.Count > 0)
                {
                    Vector2Int randomCell = emptyCells[Random.Range(0, emptyCells.Count)];
                    PieceType obstacleType = GetRandomObstacleType();
                    SpawnNewPiece(randomCell.x, randomCell.y, obstacleType);
                }
            }
        }
    }
    
    // Clears all obstacles from the board (for testing or special events)
    public void ClearAllObstacles()
    {
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                GamePiece piece = pieces[x, y];
                if (piece != null && (piece.Type == PieceType.ICEOBS || piece.Type == PieceType.GRASSOBS))
                {
                    RemoveObstacleAt(x, y);
                }
            }
        }
    }
    
    // Gets all obstacles on the board
    public List<GamePiece> GetAllObstacles()
    {
        List<GamePiece> obstacles = new List<GamePiece>();
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                GamePiece piece = pieces[x, y];
                if (piece != null && (piece.Type == PieceType.ICEOBS || piece.Type == PieceType.GRASSOBS))
                {
                    obstacles.Add(piece);
                }
            }
        }
        return obstacles;
    }
    
    // Checks if a specific cell contains an obstacle
    public bool IsObstacleAt(int x, int y)
    {
        if (x < 0 || x >= xDim || y < 0 || y >= yDim) return false;
        GamePiece piece = pieces[x, y];
        return piece != null && (piece.Type == PieceType.ICEOBS || piece.Type == PieceType.GRASSOBS);
    }
    
    // Gets the obstacle type at a specific cell
    public Grid.PieceType GetObstacleTypeAt(int x, int y)
    {
        if (x < 0 || x >= xDim || y < 0 || y >= yDim) return PieceType.EARTH;
        GamePiece piece = pieces[x, y];
        if (piece != null && (piece.Type == PieceType.ICEOBS || piece.Type == PieceType.GRASSOBS))
            return piece.Type;
        return PieceType.EARTH;
    }
    
    // Spawns a specific obstacle at a specific location
    public void SpawnObstacleAt(int x, int y, PieceType obstacleType)
    {
        if (x < 0 || x >= xDim || y < 0 || y >= yDim) return;
        if (obstacleType != PieceType.ICEOBS && obstacleType != PieceType.GRASSOBS) return;
        
        // הסרת חתיכה קיימת אם יש
        if (pieces[x, y] != null)
            RemoveAt(x, y, false);
            
        SpawnNewPiece(x, y, obstacleType);
    }
    
    // Checks if there are any possible moves on the board
    public bool HasPossibleMoves()
    {
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                GamePiece piece = pieces[x, y];
                if (piece == null || !piece.IsMoveable()) continue;
                
                // בדיקת החלפות אפשריות עם משבצות סמוכות
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue; // אותה משבצת
                        if (Mathf.Abs(dx) + Mathf.Abs(dy) != 1) continue; // רק משבצות סמוכות
                        
                        int newX = x + dx;
                        int newY = y + dy;
                        
                        if (newX < 0 || newX >= xDim || newY < 0 || newY >= yDim) continue;
                        
                        GamePiece adjacentPiece = pieces[newX, newY];
                        if (adjacentPiece == null || !adjacentPiece.IsMoveable()) continue;
                        
                        // בדיקה אם החלפה זו יוצרת מאצ'
                        List<GamePiece> match1 = GetMatch(piece, newX, newY);
                        List<GamePiece> match2 = GetMatch(adjacentPiece, x, y);
                        
                        if ((match1 != null && match1.Count >= 3) || 
                            (match2 != null && match2.Count >= 3))
                        {
                            return true; // יש מהלך אפשרי
                        }
                    }
                }
            }
        }
        return false; // אין מהלכים אפשריים
    }
    
    // Shuffles the board by rearranging all moveable pieces
    public void ShuffleBoard()
    {
        Debug.Log("Shuffling board - no moves available!");
        
        // איסוף כל החתיכות הניתנות להזזה
        List<GamePiece> moveablePieces = new List<GamePiece>();
        List<Vector2Int> emptyPositions = new List<Vector2Int>();
        
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                GamePiece piece = pieces[x, y];
                if (piece != null)
                {
                    if (piece.IsMoveable())
                    {
                        moveablePieces.Add(piece);
                        pieces[x, y] = null; // ניקוי המיקום
                    }
                    else
                    {
                        emptyPositions.Add(new Vector2Int(x, y)); // מכשולים נשארים במקומם
                    }
                }
                else
                {
                    emptyPositions.Add(new Vector2Int(x, y));
                }
            }
        }
        
        // ערבוב החתיכות
        for (int i = 0; i < moveablePieces.Count; i++)
        {
            GamePiece temp = moveablePieces[i];
            int randomIndex = Random.Range(i, moveablePieces.Count);
            moveablePieces[i] = moveablePieces[randomIndex];
            moveablePieces[randomIndex] = temp;
        }
        
        // הצבת החתיכות במיקומים חדשים
        int pieceIndex = 0;
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (pieces[x, y] == null && pieceIndex < moveablePieces.Count)
                {
                    GamePiece piece = moveablePieces[pieceIndex];
                    pieces[x, y] = piece;
                    piece.X = x;
                    piece.Y = y;
                    piece.MoveableComponent.Move(x, y);
                    pieceIndex++;
                }
            }
        }
        
        // בדיקה אם עדיין אין מהלכים (נדיר מאוד)
        if (!HasPossibleMoves())
        {
            Debug.LogWarning("Still no moves after shuffle! Trying again...");
            ShuffleBoard(); // ניסיון נוסף
        }
    }
    
    // Checks for possible moves and shuffles if none found
    public void CheckAndRefreshBoard()
    {
        if (!HasPossibleMoves())
        {
            Debug.Log("No moves available - refreshing board!");
            ShuffleBoard();
        }
    }
    
    // Public method to manually refresh the board
    public void ManualRefreshBoard()
    {
        Debug.Log("Manual board refresh requested!");
        ShuffleBoard();
    }
    
    // Coroutine for periodic move checking
    private IEnumerator PeriodicMoveCheck()
    {
        while (!gameOver)
        {
            yield return new WaitForSeconds(2f); // בדיקה כל 2 שניות
            if (!gameOver)
            {
                CheckAndRefreshBoard();
            }
        }
    }
    
    // Starts the periodic move checking
    public void StartMoveChecking()
    {
        StartCoroutine(PeriodicMoveCheck());
    }
    
    // Stops the periodic move checking
    public void StopMoveChecking()
    {
        StopAllCoroutines();
    }
    
    // Calculates total moves for the level based on obstacles
    public void CalculateMovesForLevel()
    {
        int obstacleCount = GetObstacleCount();
        currentMoves = baseMoves + (obstacleCount * movesPerObstacle);
        currentScore = 0;
        
        Debug.Log($"Level started with {currentMoves} moves (Base: {baseMoves} + Obstacles: {obstacleCount} x {movesPerObstacle})");
        
        // עדכון UI אחרי חישוב מהלכים
        if (gameUI != null)
        {
            gameUI.UpdateUI();
        }
    }
    
    // Reduces moves by 1 and checks for game over
    public bool UseMove()
    {
        if (currentMoves <= 0) return false;
        
        currentMoves--;
        Debug.Log($"Moves remaining: {currentMoves}");
        
        // עדכון UI מיד אחרי שימוש במהלך
        if (gameUI != null)
        {
            gameUI.UpdateUI();
        }
        
        // אזהרה על מהלכים נגמרים
        if (currentMoves <= 3 && gameUI != null)
        {
            gameUI.ShowNoMovesWarning();
        }
        
        if (currentMoves <= 0)
        {
            Debug.Log("No moves left! Game Over!");
            GameOver();
            return false;
        }
        
        return true;
    }
    
    // Adds score based on match size
    public void AddMatchScore(int matchSize)
    {
        int scoreToAdd = 0;
        
        switch (matchSize)
        {
            case 3:
                scoreToAdd = match3Score;
                break;
            case 4:
                scoreToAdd = match4Score;
                break;
            case 5:
                scoreToAdd = match5Score;
                break;
            default:
                if (matchSize > 5)
                    scoreToAdd = match5Score + ((matchSize - 5) * 20); // 20 נקודות נוספות לכל חתיכה מעל 5
                break;
        }
        
        int oldStars = GetStarRating();
        currentScore += scoreToAdd;
        int newStars = GetStarRating();
        
        Debug.Log($"Match of {matchSize} pieces! +{scoreToAdd} points. Total: {currentScore}");
        
        // עדכון UI מיד אחרי שינוי ניקוד
        if (gameUI != null)
        {
            gameUI.UpdateUI();
        }
        
        // הודעה על כוכב חדש
        if (newStars > oldStars && gameUI != null)
        {
            gameUI.ShowStarEarnedMessage(newStars);
        }
    }
    
    // Gets current star rating based on score
    public int GetStarRating()
    {
        if (currentScore >= star3Score) return 3;
        if (currentScore >= star2Score) return 2;
        if (currentScore >= star1Score) return 1;
        return 0;
    }
    
    // Gets the score needed for next star
    public int GetScoreForNextStar()
    {
        int currentStars = GetStarRating();
        switch (currentStars)
        {
            case 0: return star1Score;
            case 1: return star2Score;
            case 2: return star3Score;
            default: return star3Score; // כבר יש 3 כוכבים
        }
    }

    // Repeatedly applies gravity/refill until stable, then removes any initial matches
    public IEnumerator Fill()
    {
        while (FillStep())
            yield return new WaitForSeconds(fillTime);

        HashSet<GamePiece> startMatches = FindAllMatchesOnBoard();
        if (startMatches.Count >= 3)
        {
            foreach (var p in startMatches)
                if (p != null) RemoveAt(p.X, p.Y, true);

            yield return new WaitForSeconds(0.05f);
            yield return StartCoroutine(FillAndResolve());
        }
    }

    // Performs one gravity/refill step
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
                RectTransform rt = newPiece.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.sizeDelta = new Vector2(cellSize, cellSize);
                    rt.anchoredPosition = GetUIPos(x, -1);
                }
                GamePiece gp = newPiece.GetComponent<GamePiece>();
                if (gp == null) gp = newPiece.AddComponent<GamePiece>();
                gp.Init(x, -1, this, type);
                gp.MoveableComponent.Move(x, 0);
                pieces[x, 0] = gp;
                movedPiece = true;
            }
        }

        return movedPiece;
    }

    // Computes the UI position for a cell (y is flipped so falling is from top)
    public Vector2 GetUIPos(int x, int y)
    {
        int yFlip = (yDim - 1) - y;
        return new Vector2(
            (x * cellSize) - (cellSize * xDim / 2f) + (cellSize / 2f),
            (yFlip * cellSize) - (cellSize * yDim / 2f) + (cellSize / 2f)
        );
    }

    // Spawns a new piece at a cell
    public GamePiece SpawnNewPiece(int x, int y, PieceType type)
    {
        GameObject go = Instantiate(piecePrefabDict[type]);
        go.name = $"Piece({x},{y})-{type}";
        go.transform.SetParent(transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.sizeDelta = new Vector2(cellSize, cellSize);
            rt.anchoredPosition = GetUIPos(x, y);
        }
        GamePiece gp = go.GetComponent<GamePiece>();
        if (gp == null) gp = go.AddComponent<GamePiece>();
        gp.Init(x, y, this, type);

        pieces[x, y] = gp;
        return gp;
    }

    // Checks if two pieces are orthogonally adjacent
    public bool IsAdjacent(GamePiece p1, GamePiece p2)
    {
        return (p1.X == p2.X && Mathf.Abs(p1.Y - p2.Y) == 1) ||
               (p1.Y == p2.Y && Mathf.Abs(p1.X - p2.X) == 1);
    }

    // Handles pointer down on a piece
    public void PressPiece(GamePiece piece)
    {
        pressedPiece = piece;
    }

    // Handles pointer enter on a piece while dragging
    public void EnterPiece(GamePiece piece)
    {
        if (pressedPiece != null && piece != pressedPiece)
            enteredPiece = piece;
    }

    // Handles pointer up and attempts a swap
    public void ReleasePiece()
    {
        if (pressedPiece != null && enteredPiece != null && IsAdjacent(pressedPiece, enteredPiece))
            SwapPieces(pressedPiece, enteredPiece);

        pressedPiece = null;
        enteredPiece = null;
    }

    // Finds a horizontal/vertical match list around a center
    public List<GamePiece> GetMatch(GamePiece piece, int newX, int newY)
    {
        if (piece == null) return null;

        int centerX = newX >= 0 ? newX : piece.X;
        int centerY = newY >= 0 ? newY : piece.Y;

        PieceType type = piece.Type;
        List<GamePiece> result = new List<GamePiece>();
        List<GamePiece> line = new List<GamePiece>();
        line.Add(piece);

        int x;
        for (x = centerX - 1; x >= 0; x--)
        {
            GamePiece p = pieces[x, centerY];
            if (p == null || p.Type != type) break;
            line.Add(p);
        }
        for (x = centerX + 1; x < xDim; x++)
        {
            GamePiece p = pieces[x, centerY];
            if (p == null || p.Type != type) break;
            line.Add(p);
        }
        if (line.Count >= 3)
            for (int i = 0; i < line.Count; i++)
                if (!result.Contains(line[i])) result.Add(line[i]);

        line.Clear();
        line.Add(piece);

        int y;
        for (y = centerY - 1; y >= 0; y--)
        {
            GamePiece p = pieces[centerX, y];
            if (p == null || p.Type != type) break;
            line.Add(p);
        }
        for (y = centerY + 1; y < yDim; y++)
        {
            GamePiece p = pieces[centerX, y];
            if (p == null || p.Type != type) break;
            line.Add(p);
        }
        if (line.Count >= 3)
            for (int i = 0; i < line.Count; i++)
                if (!result.Contains(line[i])) result.Add(line[i]);

        return result.Count >= 3 ? result : null;
    }
    
    // Finds obstacles that should be destroyed by nearby matches
    public List<GamePiece> GetObstaclesAffectedByMatch(List<GamePiece> matchPieces)
    {
        List<GamePiece> affectedObstacles = new List<GamePiece>();
        
        foreach (GamePiece matchPiece in matchPieces)
        {
            // בדיקה של כל המשבצות הסמוכות למאצ'
            for (int x = matchPiece.X - 1; x <= matchPiece.X + 1; x++)
            {
                for (int y = matchPiece.Y - 1; y <= matchPiece.Y + 1; y++)
                {
                    if (x >= 0 && x < xDim && y >= 0 && y < yDim)
                    {
                        GamePiece nearbyPiece = pieces[x, y];
                        if (nearbyPiece != null && 
                            (nearbyPiece.Type == PieceType.ICEOBS || nearbyPiece.Type == PieceType.GRASSOBS) &&
                            !affectedObstacles.Contains(nearbyPiece))
                        {
                            affectedObstacles.Add(nearbyPiece);
                        }
                    }
                }
            }
        }
        
        return affectedObstacles;
    }

    // Removes a piece at coordinates and optionally awards resources
    private void RemoveAt(int x, int y, bool awardResource = false)
    {
        GamePiece piece = pieces[x, y];
        if (piece != null)
        {
            // מכשולים לא נותנים משאבים
            if (awardResource && bank != null && piece.Type != PieceType.ICEOBS && piece.Type != PieceType.GRASSOBS)
                bank.Add(piece.Type, 1);

            Destroy(piece.gameObject);
            pieces[x, y] = null;
        }
    }
    
    // Removes obstacles specifically (for power-ups)
    public void RemoveObstacleAt(int x, int y)
    {
        GamePiece piece = pieces[x, y];
        if (piece != null && (piece.Type == PieceType.ICEOBS || piece.Type == PieceType.GRASSOBS))
        {
            Destroy(piece.gameObject);
            pieces[x, y] = null;
        }
    }

    // Swaps two adjacent pieces; resolves matches and cascades
    public void SwapPieces(GamePiece firstPiece, GamePiece secondPiece)
    {
        if (gameOver){ return; }
        if (firstPiece == null || secondPiece == null) return;
        if (!firstPiece.IsMoveable() || !secondPiece.IsMoveable()) return;
        if (!IsAdjacent(firstPiece, secondPiece)) return;

        int firstX = firstPiece.X;
        int firstY = firstPiece.Y;
        int secondX = secondPiece.X;
        int secondY = secondPiece.Y;

        pieces[firstX, firstY] = secondPiece;
        firstPiece.MoveableComponent.Move(secondX, secondY);

        pieces[secondX, secondY] = firstPiece;
        secondPiece.MoveableComponent.Move(firstX, firstY);

        List<GamePiece> matchForFirst = GetMatch(firstPiece, secondX, secondY);
        List<GamePiece> matchForSecond = GetMatch(secondPiece, firstX, firstY);

        HashSet<GamePiece> piecesToClear = new HashSet<GamePiece>();
        if (matchForFirst != null) for (int i = 0; i < matchForFirst.Count; i++) piecesToClear.Add(matchForFirst[i]);
        if (matchForSecond != null) for (int i = 0; i < matchForSecond.Count; i++) piecesToClear.Add(matchForSecond[i]);

        if (piecesToClear.Count < 3)
        {
            pieces[firstX, firstY] = firstPiece;
            firstPiece.MoveableComponent.Move(firstX, firstY);

            pieces[secondX, secondY] = secondPiece;
            secondPiece.MoveableComponent.Move(secondX, secondY);
            return;
        }

        // שימוש במהלך
        if (!UseMove())
        {
            // אם אין מהלכים, החזרת החתיכות למקום
            pieces[firstX, firstY] = firstPiece;
            firstPiece.MoveableComponent.Move(firstX, firstY);

            pieces[secondX, secondY] = secondPiece;
            secondPiece.MoveableComponent.Move(secondX, secondY);
            return;
        }

        level?.OnMove();

        if (ClearAllValidMatches())
            StartCoroutine(FillAndResolve());
    }

    // Scans the board and returns all pieces belonging to any match
    private HashSet<GamePiece> FindAllMatchesOnBoard()
    {
        HashSet<GamePiece> set = new HashSet<GamePiece>();

        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                GamePiece piece = pieces[x, y];
                if (piece == null) continue;

                List<GamePiece> m = GetMatch(piece, x, y);
                if (m != null && m.Count >= 3)
                    for (int i = 0; i < m.Count; i++) set.Add(m[i]);
            }
        }
        return set;
    }

    // Cascades: apply gravity/refill and keep clearing until stable
    private IEnumerator FillAndResolve()
    {
        while (FillStep())
            yield return new WaitForSeconds(fillTime);

        HashSet<GamePiece> more = FindAllMatchesOnBoard();
        if (more.Count >= 3)
        {
            foreach (GamePiece p in more)
                if (p != null) RemoveAt(p.X, p.Y, true);

            yield return new WaitForSeconds(0.05f);
            StartCoroutine(FillAndResolve());
        }
        else
        {
            // אחרי שהלוח התייצב, בדיקה אם יש מהלכים אפשריים
            yield return new WaitForSeconds(0.1f);
            CheckAndRefreshBoard();
        }
    }

    public enum TargetMode { None, Row, Column, Cell3x3, AllOfType }
    private TargetMode pendingMode = TargetMode.None;

    // Enters a targeting mode for power-ups
    public void EnterTargetMode(TargetMode mode)
    {
        pendingMode = mode;
    }

    // Handles a cell click for power-ups
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

    // Clears an entire row
    public void ClearRow(int y)
    {
        if (y < 0 || y >= yDim) return;
        for (int x = 0; x < xDim; x++)
            if (pieces[x, y] != null)
            {
                GamePiece piece = pieces[x, y];
                if (piece.Type == PieceType.ICEOBS || piece.Type == PieceType.GRASSOBS)
                    RemoveObstacleAt(x, y);
                else
                    RemoveAt(x, y, true); // נותן משאבים לחתיכות רגילות
            }
    }

    // Clears an entire column
    public void ClearColumn(int x)
    {
        if (x < 0 || x >= xDim) return;
        for (int y = 0; y < yDim; y++)
            if (pieces[x, y] != null)
            {
                GamePiece piece = pieces[x, y];
                if (piece.Type == PieceType.ICEOBS || piece.Type == PieceType.GRASSOBS)
                    RemoveObstacleAt(x, y);
                else
                    RemoveAt(x, y, true); // נותן משאבים לחתיכות רגילות
            }
    }

    // Clears all pieces of a specific type
    public void ClearAllOfType(PieceType type)
    {
        for (int x = 0; x < xDim; x++)
            for (int y = 0; y < yDim; y++)
            {
                GamePiece piece = pieces[x, y];
                if (piece != null && piece.Type == type)
                {
                    if (piece.Type == PieceType.ICEOBS || piece.Type == PieceType.GRASSOBS)
                        RemoveObstacleAt(x, y);
                    else
                        RemoveAt(x, y, true); // נותן משאבים לחתיכות רגילות
                }
            }
    }

    // Clears all valid matches currently on the board
    public bool ClearAllValidMatches()
    {
        bool needsRefill = false;
        HashSet<GamePiece> toClear = new HashSet<GamePiece>();
        HashSet<GamePiece> obstaclesToClear = new HashSet<GamePiece>();

        for (int y = 0; y < yDim; y++)
        {
            for (int x = 0; x < xDim; x++)
            {
                GamePiece piece = pieces[x, y];
                if (piece == null) continue;

                List<GamePiece> match = GetMatch(piece, x, y);
                if (match != null && match.Count >= 3)
                {
                    // הוספת חתיכות המאצ' לרשימת המחיקה
                    for (int i = 0; i < match.Count; i++)
                        toClear.Add(match[i]);
                    
                    // מציאת מכשולים שנפגעו מהמאצ'
                    List<GamePiece> affectedObstacles = GetObstaclesAffectedByMatch(match);
                    for (int i = 0; i < affectedObstacles.Count; i++)
                        obstaclesToClear.Add(affectedObstacles[i]);
                }
            }
        }

        // מחיקת חתיכות רגילות עם חישוב ניקוד
        GamePiece[] arr = new GamePiece[toClear.Count];
        toClear.CopyTo(arr);

        // חישוב ניקוד לפי גודל המאצ'
        if (arr.Length >= 3)
        {
            AddMatchScore(arr.Length);
        }

        for (int i = 0; i < arr.Length; i++)
        {
            GamePiece gp = arr[i];
            if (gp != null)
            {
                RemoveAt(gp.X, gp.Y, true);
                needsRefill = true;
            }
        }
        
        // מחיקת מכשולים שנפגעו
        GamePiece[] obstacleArr = new GamePiece[obstaclesToClear.Count];
        obstaclesToClear.CopyTo(obstacleArr);

        for (int i = 0; i < obstacleArr.Length; i++)
        {
            GamePiece gp = obstacleArr[i];
            if (gp != null)
            {
                RemoveObstacleAt(gp.X, gp.Y);
                needsRefill = true;
            }
        }

        return needsRefill;
    }
    public void GameOver()
    {
        gameOver = true;
        StopMoveChecking(); // הפסקת בדיקת מהלכים
        
        // הצגת תוצאות השלב
        int finalStars = GetStarRating();
        Debug.Log($"=== LEVEL COMPLETE ===");
        Debug.Log($"Final Score: {currentScore}");
        Debug.Log($"Stars Earned: {finalStars}/3");
        Debug.Log($"Moves Used: {baseMoves + (GetObstacleCount() * movesPerObstacle) - currentMoves}");
        
        // הצגת תוצאות ב-UI
        if (gameUI != null)
        {
            gameUI.ShowLevelComplete(currentScore, finalStars);
        }
        
        // קריאה ל-Level לטפל בתוצאות
        if (level != null)
        {
            level.OnLevelComplete(currentScore, finalStars);
        }
    }
    
    // Checks if level objectives are met
    public bool IsLevelComplete()
    {
        // כאן תוכל להוסיף תנאים נוספים כמו:
        // - ניקוד מינימלי
        // - מספר מכשולים למחיקה
        // - זמן מוגבל
        return currentScore >= star1Score; // לפחות כוכב אחד
    }
    
    // Gets level progress information
    public string GetLevelProgress()
    {
        int stars = GetStarRating();
        int nextStarScore = GetScoreForNextStar();
        int scoreNeeded = nextStarScore - currentScore;
        
        return $"Score: {currentScore} | Stars: {stars}/3 | Moves: {currentMoves} | Next Star: {scoreNeeded} points";
    }
    
    // Resets the level with new parameters
    public void ResetLevel(int newBaseMoves = -1, int newMaxObstacles = -1)
    {
        if (newBaseMoves > 0) baseMoves = newBaseMoves;
        if (newMaxObstacles > 0) maxObstacles = newMaxObstacles;
        
        gameOver = false;
        CalculateMovesForLevel();
        
        Debug.Log($"Level reset! {GetLevelProgress()}");
        
        // עדכון UI אחרי איפוס שלב
        if (gameUI != null)
        {
            gameUI.UpdateUI();
        }
    }
    
    // Returns the count of obstacles on the board
    public int GetObstacleCount()
    {
        int count = 0;
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                GamePiece piece = pieces[x, y];
                if (piece != null && (piece.Type == PieceType.ICEOBS || piece.Type == PieceType.GRASSOBS))
                    count++;
            }
        }
        return count;
    }
    
    // Returns true if there are any obstacles on the board
    public bool HasObstacles()
    {
        return GetObstacleCount() > 0;
    }
    
    // Checks if a specific obstacle is affected by any nearby matches
    public bool IsObstacleAffectedByMatch(GamePiece obstacle)
    {
        if (obstacle == null || (obstacle.Type != PieceType.ICEOBS && obstacle.Type != PieceType.GRASSOBS))
            return false;
            
        // בדיקה של כל המשבצות הסמוכות למכשול
        for (int x = obstacle.X - 1; x <= obstacle.X + 1; x++)
        {
            for (int y = obstacle.Y - 1; y <= obstacle.Y + 1; y++)
            {
                if (x >= 0 && x < xDim && y >= 0 && y < yDim)
                {
                    GamePiece nearbyPiece = pieces[x, y];
                    if (nearbyPiece != null && nearbyPiece.Type != PieceType.ICEOBS && nearbyPiece.Type != PieceType.GRASSOBS)
                    {
                        List<GamePiece> match = GetMatch(nearbyPiece, x, y);
                        if (match != null && match.Count >= 3)
                            return true;
                    }
                }
            }
        }
        return false;
    }

    // Performs a 3x3 bomb clear centered at a cell
    public void Bomb3x3(int centerX, int centerY)
    {
        for (int x = centerX - 1; x <= centerX + 1; x++)
        {
            for (int y = centerY - 1; y <= centerY + 1; y++)
            {
                if (x >= 0 && x < xDim && y >= 0 && y < yDim)
                {
                    GamePiece piece = pieces[x, y];
                    if (piece != null)
                    {
                        if (piece.Type == PieceType.ICEOBS || piece.Type == PieceType.GRASSOBS)
                            RemoveObstacleAt(x, y);
                        else
                            RemoveAt(x, y, true); // נותן משאבים לחתיכות רגילות
                    }
                }
            }
        }
    }
}