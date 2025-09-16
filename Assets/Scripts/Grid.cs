using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine.UI;

public class Grid : MonoBehaviour
{
    // Piece types (includes obstacles)
    public enum PieceType { EARTH, GRASS, WATER, SUN, ICEOBS, GRASSOBS }

    // External systems
    public ResourceManagement bank;
    public Level level;           // optional, for OnMove / completion callbacks
    public GameUI gameUI;         // optional, for UI updates
    public FlowerAbility activeAbility;

    [System.Serializable]
    public struct PiecePrefab
    {
        public PieceType type;
        public GameObject prefab;
    }

    // Board dimensions and timings
    public int xDim = 8;
    public int yDim = 8;
    public float cellSize = 1f;
    public float fillTime = 0.1f;

    // Obstacles configuration
    [Header("Obstacle Settings")]
    public int maxObstacles = 8;
    [Range(0f, 1f)] public float obstacleSpawnChance = 0.15f;

    // Move/score/star systems
    [Header("Level System")]
    public int baseMoves = 20;
    public int movesPerObstacle = 5;
    public int currentMoves;
    public int currentScore;

    [Header("Scoring System")]
    public int match3Score = 50;
    public int match4Score = 80;
    public int match5Score = 120;

    [Header("Star System")]
    public int star1Score = 1000;
    public int star2Score = 1500;
    public int star3Score = 2000;

    // Prefabs and grid data
    public PiecePrefab[] piecePrefabs;
    public GameObject backgroundPrefab;

    private PieceType[] availableTypes;
    private Dictionary<PieceType, GameObject> piecePrefabDict;
    private GamePiece[,] pieces;

    // Input state
    private GamePiece pressedPiece;
    private GamePiece enteredPiece;

    // Game state
    private bool gameOver = false;

    // Intro fill state (אם תרצי לחסום סוייפים בזמן פתיחה)
    private bool isIntroFilling = false;
    public bool IsIntroFilling => isIntroFilling;

    // ————————————————————— Lifecycle —————————————————————
    void Start()
    {
        // Build prefab dict
        piecePrefabDict = new Dictionary<PieceType, GameObject>();
        for (int i = 0; i < piecePrefabs.Length; i++)
        {
            var e = piecePrefabs[i];
            if (e.prefab != null && !piecePrefabDict.ContainsKey(e.type))
                piecePrefabDict.Add(e.type, e.prefab);
        }

        // Available types
        availableTypes = new PieceType[piecePrefabDict.Count];
        piecePrefabDict.Keys.CopyTo(availableTypes, 0);

        // Resize board rect
        RectTransform boardRT = (RectTransform)transform;
        boardRT.sizeDelta = new Vector2(xDim * cellSize, yDim * cellSize);
        boardRT.pivot = new Vector2(0.5f, 0.5f);

        // Background tiles
        for (int x = 0; x < xDim; x++)
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
                var img = bg.GetComponent<Image>();
                if (img) img.raycastTarget = false;
            }

        // Pieces array
        pieces = new GamePiece[xDim, yDim];

        // Bootstrap flow
        StartCoroutine(Bootstrap());
    }

    private IEnumerator Bootstrap()
    {
        // פתיחה: לוח ללא מאצ'ים + נפילה מלמעלה (ללא ניקוי/ניקוד)
        yield return StartCoroutine(IntroPopulateBoardWithoutMatches());

        // מכשולים אקראיים
        SpawnRandomObstacles();

        // חישוב מהלכים ו־UI
        CalculateMovesForLevel();
        gameUI?.UpdateUI();

        // בדיקות "אין מהלכים"
        StartMoveChecking();

        // הבטחת מהלך פתיחה חוקי
        if (!HasPossibleMoves())
            ShuffleBoard();
    }

    // ————————————————————— Helpers —————————————————————
    private PieceType GetRandomType()
    {
        if (availableTypes == null || availableTypes.Length == 0)
            return PieceType.EARTH;

        // filter obstacles out
        List<PieceType> regular = new List<PieceType>();
        foreach (PieceType t in availableTypes)
        {
            if (t != PieceType.ICEOBS && t != PieceType.GRASSOBS)
                regular.Add(t);
        }
        if (regular.Count == 0) return PieceType.EARTH;
        return regular[Random.Range(0, regular.Count)];
    }

    private PieceType GetRandomObstacleType()
    {
        PieceType[] obs = { PieceType.ICEOBS, PieceType.GRASSOBS };
        return obs[Random.Range(0, obs.Length)];
    }

    // ————————————————————— Obstacles —————————————————————
    private void SpawnRandomObstacles()
    {
        int spawned = 0;
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (spawned >= maxObstacles) break;
                if (Random.Range(0f, 1f) < obstacleSpawnChance)
                {
                    SpawnObstacleAt(x, y, GetRandomObstacleType());
                    spawned++;
                }
            }
            if (spawned >= maxObstacles) break;
        }
        Debug.Log($"Spawned {spawned} obstacles.");
    }

    public void SpawnObstacleAt(int x, int y, PieceType obstacleType)
    {
        if (x < 0 || x >= xDim || y < 0 || y >= yDim) return;
        if (obstacleType != PieceType.ICEOBS && obstacleType != PieceType.GRASSOBS) return;

        if (pieces[x, y] != null)
            RemoveAt(x, y, false);

        GamePiece gp = SpawnNewPiece(x, y, obstacleType);

        // make sure the obstacle has ObstaclePiece configured
        var ob = gp.GetComponent<ObstaclePiece>();
        if (ob == null) ob = gp.gameObject.AddComponent<ObstaclePiece>();

        if (obstacleType == PieceType.GRASSOBS)
        {
            ob.kind = ObstaclePiece.Kind.Grass;
            ob.hitsToClear = 3;
        }
        else
        {
            ob.kind = ObstaclePiece.Kind.Ice;
            ob.hitsToClear = 6;
        }
    }

    // Apply damage to obstacle; returns true if destroyed
    public bool DamageObstacleAt(int x, int y, int amount = 1)
    {
        GamePiece p = pieces[x, y];
        if (p == null) return false;

        if (p.Type == PieceType.ICEOBS || p.Type == PieceType.GRASSOBS)
        {
            var ob = p.GetComponent<ObstaclePiece>();
            if (ob != null)
            {
                bool destroyed = ob.Damage(amount);
                if (destroyed)
                {
                    pieces[x, y] = null;                 // לפנות סלוט קודם
                    StartCoroutine(AnimateAndDestroy(p)); // אנימציה ואז Destroy
                }
                return destroyed;
            }
        }
        return false;
    }

    public void RemoveObstacleAt(int x, int y)
    {
        GamePiece p = pieces[x, y];
        if (p != null && (p.Type == PieceType.ICEOBS || p.Type == PieceType.GRASSOBS))
        {
            var ob = p.GetComponent<ObstaclePiece>();
            if (ob != null) ob.KillTweens();
            Destroy(p.gameObject);
            pieces[x, y] = null;
        }
    }

    public int GetObstacleCount()
    {
        int count = 0;
        for (int x = 0; x < xDim; x++)
            for (int y = 0; y < yDim; y++)
            {
                GamePiece p = pieces[x, y];
                if (p != null && (p.Type == PieceType.ICEOBS || p.Type == PieceType.GRASSOBS))
                    count++;
            }
        return count;
    }

    // ————————————————————— Level: moves/score/stars —————————————————————
    public void CalculateMovesForLevel()
    {
        int obs = GetObstacleCount();
        currentMoves = baseMoves + (obs * movesPerObstacle);
        currentScore = 0;
        Debug.Log($"Level started with {currentMoves} moves (base {baseMoves}, +{obs}×{movesPerObstacle}).");
    }

    public bool UseMove()
    {
        if (currentMoves <= 0) return false;

        currentMoves--;
        gameUI?.UpdateUI();

        if (currentMoves <= 3) gameUI?.ShowNoMovesWarning();

        if (currentMoves <= 0)
        {
            Debug.Log("No moves left!");
            GameOver();
            return false;
        }
        return true;
    }

    public void AddMatchScore(int matchSize)
    {
        int add = 0;
        switch (matchSize)
        {
            case 3: add = match3Score; break;
            case 4: add = match4Score; break;
            case 5: add = match5Score; break;
            default:
                if (matchSize > 5) add = match5Score + ((matchSize - 5) * 20);
                break;
        }

        int oldStars = GetStarRating();
        currentScore += add;
        int newStars = GetStarRating();

        gameUI?.UpdateUI();

        if (newStars > oldStars)
            gameUI?.ShowStarEarnedMessage(newStars);
    }

    public int GetStarRating()
    {
        if (currentScore >= star3Score) return 3;
        if (currentScore >= star2Score) return 2;
        if (currentScore >= star1Score) return 1;
        return 0;
    }

    public int GetScoreForNextStar()
    {
        switch (GetStarRating())
        {
            case 0: return star1Score;
            case 1: return star2Score;
            case 2: return star3Score;
            default: return star3Score;
        }
    }

    public void GameOver()
    {
        if (gameOver) return;
        gameOver = true;

        StopMoveChecking(); // stop periodic checks

        int finalStars = GetStarRating();
        Debug.Log("=== LEVEL COMPLETE ===");
        Debug.Log($"Final Score: {currentScore}");
        Debug.Log($"Stars: {finalStars}/3");

        gameUI?.ShowLevelComplete(currentScore, finalStars);
        level?.OnLevelComplete(currentScore, finalStars);
    }

    // ————————————————————— Fill / Gravity / Match —————————————————————
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

    public bool FillStep()
    {
        bool moved = false;

        // fall down
        for (int y = yDim - 2; y >= 0; y--)
        {
            for (int x = 0; x < xDim; x++)
            {
                GamePiece piece = pieces[x, y];
                if (piece != null && piece.IsMoveable())
                {
                    GamePiece below = pieces[x, y + 1];
                    if (below == null)
                    {
                        piece.MoveableComponent.Move(x, y + 1);
                        pieces[x, y + 1] = piece;
                        pieces[x, y] = null;
                        moved = true;
                    }
                }
            }
        }

        // spawn at top row
        for (int x = 0; x < xDim; x++)
        {
            if (pieces[x, 0] == null)
            {
                PieceType type = GetRandomType();
                GameObject go = Instantiate(piecePrefabDict[type]);
                go.transform.SetParent(transform, false);
                RectTransform rt = go.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.sizeDelta = new Vector2(cellSize, cellSize);
                    rt.anchoredPosition = GetUIPos(x, -1);
                }
                GamePiece gp = go.GetComponent<GamePiece>();
                if (gp == null) gp = go.AddComponent<GamePiece>();
                gp.Init(x, -1, this, type);
                gp.MoveableComponent.Move(x, 0);
                pieces[x, 0] = gp;
                moved = true;
            }
        }

        return moved;
    }

    public Vector2 GetUIPos(int x, int y)
    {
        int yFlip = (yDim - 1) - y;
        return new Vector2(
            (x * cellSize) - (cellSize * xDim / 2f) + (cellSize / 2f),
            (yFlip * cellSize) - (cellSize * yDim / 2f) + (cellSize / 2f)
        );
    }

    // Spawn regular (in place) OR above (-1) then animate down
    public GamePiece SpawnNewPiece(int x, int y, PieceType type)
        => SpawnNewPiece(x, y, type, spawnAbove: false);

    public GamePiece SpawnNewPiece(int x, int y, PieceType type, bool spawnAbove)
    {
        GameObject go = Instantiate(piecePrefabDict[type]);
        go.name = $"Piece({x},{y})-{type}";
        go.transform.SetParent(transform, false);

        var rt = go.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = Vector3.one;
            rt.sizeDelta = new Vector2(cellSize, cellSize);
            rt.anchoredPosition = spawnAbove ? GetUIPos(x, -1) : GetUIPos(x, y);
        }

        GamePiece gp = go.GetComponent<GamePiece>() ?? go.AddComponent<GamePiece>();
        var mv = go.GetComponent<MoveablePiece>() ?? go.AddComponent<MoveablePiece>();
        mv.Init(this);

        // מקבעים יעד במטריצה לפני האנימציה
        gp.Init(x, y, this, type);
        pieces[x, y] = gp;

        if (spawnAbove)
            gp.MoveableComponent.Move(x, y);

        return gp;
    }

    public bool IsAdjacent(GamePiece a, GamePiece b)
    {
        return (a.X == b.X && Mathf.Abs(a.Y - b.Y) == 1) ||
               (a.Y == b.Y && Mathf.Abs(a.X - b.X) == 1);
    }

    public void PressPiece(GamePiece piece)
    {
        if (activeAbility != null)
        {
            UseActiveAbilityOn(piece);
            return;
        }
        // אם תרצי – אפשר לחסום בזמן פתיחה:
        // if (isIntroFilling) return;
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
            SwapPieces(pressedPiece, enteredPiece);

        pressedPiece = null;
        enteredPiece = null;
    }

    public List<GamePiece> GetMatch(GamePiece piece, int newX, int newY)
    {
        if (piece == null) return null;

        int cx = newX >= 0 ? newX : piece.X;
        int cy = newY >= 0 ? newY : piece.Y;
        PieceType t = piece.Type;

        List<GamePiece> result = new List<GamePiece>();

        // Horizontal
        List<GamePiece> line = new List<GamePiece>();
        line.Add(piece);

        for (int x = cx - 1; x >= 0; x--)
        {
            GamePiece p = pieces[x, cy];
            if (p == null || p.Type != t) break;
            line.Add(p);
        }
        for (int x = cx + 1; x < xDim; x++)
        {
            GamePiece p = pieces[x, cy];
            if (p == null || p.Type != t) break;
            line.Add(p);
        }
        if (line.Count >= 3)
            for (int i = 0; i < line.Count; i++)
                if (!result.Contains(line[i])) result.Add(line[i]);

        // Vertical
        line.Clear();
        line.Add(piece);

        for (int y = cy - 1; y >= 0; y--)
        {
            GamePiece p = pieces[cx, y];
            if (p == null || p.Type != t) break;
            line.Add(p);
        }
        for (int y = cy + 1; y < yDim; y++)
        {
            GamePiece p = pieces[cx, y];
            if (p == null || p.Type != t) break;
            line.Add(p);
        }
        if (line.Count >= 3)
            for (int i = 0; i < line.Count; i++)
                if (!result.Contains(line[i])) result.Add(line[i]);

        return result.Count >= 3 ? result : null;
    }

    private void RemoveAt(int x, int y, bool awardResource = false)
    {
        GamePiece piece = pieces[x, y];
        if (piece == null) return;

        // לא להעניק ריסורסים על מכשולים
        if (awardResource && bank != null &&
            piece.Type != PieceType.ICEOBS && piece.Type != PieceType.GRASSOBS)
        {
            bank.Add(piece.Type, 1);
        }

        // מפנים את התא *מיד*
        pieces[x, y] = null;

        // אנימציית "פיצוץ" קצרה ואז הורסים – ללא CanvasGroup
        StartCoroutine(AnimateAndDestroy(piece));
    }

    public void SwapPieces(GamePiece a, GamePiece b)
    {
        if (gameOver) return;
        if (a == null || b == null) return;
        if (!a.IsMoveable() || !b.IsMoveable()) return;
        if (!IsAdjacent(a, b)) return;

        int ax = a.X, ay = a.Y;
        int bx = b.X, by = b.Y;

        // להביא לקדמת ההיררכיה בזמן האנימציה
        a.transform.SetAsLastSibling();
        b.transform.SetAsLastSibling();

        pieces[ax, ay] = b; a.MoveableComponent.Move(bx, by);
        pieces[bx, by] = a; b.MoveableComponent.Move(ax, ay);

        var ma = GetMatch(a, bx, by);
        var mb = GetMatch(b, ax, ay);

        HashSet<GamePiece> toClear = new HashSet<GamePiece>();
        if (ma != null) foreach (var p in ma) toClear.Add(p);
        if (mb != null) foreach (var p in mb) toClear.Add(p);

        // Didn't match 3
        if (toClear.Count < 3)
        {
            a.transform.SetAsLastSibling();
            b.transform.SetAsLastSibling();

            pieces[ax, ay] = a; a.MoveableComponent.Move(ax, ay);
            pieces[bx, by] = b; b.MoveableComponent.Move(bx, by);
            return;
        }

        // Out of moves
        if (!UseMove())
        {
            pieces[ax, ay] = a; a.MoveableComponent.Move(ax, ay);
            pieces[bx, by] = b; b.MoveableComponent.Move(bx, by);
            return;
        }

        level?.OnMove();

        Invoke("ClearAndRefill", 0.25f);
    }

    private void ClearAndRefill()
    {
        if (ClearAllValidMatches())
            StartCoroutine(FillAndResolve());
    }

    private HashSet<GamePiece> FindAllMatchesOnBoard()
    {
        HashSet<GamePiece> set = new HashSet<GamePiece>();
        for (int x = 0; x < xDim; x++)
            for (int y = 0; y < yDim; y++)
            {
                GamePiece p = pieces[x, y];
                if (p == null) continue;
                List<GamePiece> m = GetMatch(p, x, y);
                if (m != null && m.Count >= 3)
                    for (int i = 0; i < m.Count; i++) set.Add(m[i]);
            }
        return set;
    }

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
            yield return new WaitForSeconds(0.1f);
            CheckAndRefreshBoard();
        }
    }

    // ————————————————————— Power-ups / Abilities —————————————————————
    public enum TargetMode { None, Row, Column, Cell3x3, AllOfType }

    public void EnterAbility(FlowerAbility ability) { activeAbility = ability; }

    public void UseActiveAbilityOn(GamePiece piece)
    {
        if (activeAbility == null || piece == null) return;
        FlowerAbility.AbilityMap[activeAbility.ability].Apply(this, piece.X, piece.Y, piece.Type);
        activeAbility = null;
        StartCoroutine(FillAndResolve());
    }

    public void ClearRow(int y)
    {
        if (y < 0 || y >= yDim) return;
        for (int x = 0; x < xDim; x++)
        {
            GamePiece p = pieces[x, y];
            if (p == null) continue;

            if (p.Type == PieceType.ICEOBS || p.Type == PieceType.GRASSOBS)
                DamageObstacleAt(x, y, 1);
            else
                RemoveAt(x, y, true);
        }
    }

    public void ClearColumn(int x)
    {
        if (x < 0 || x >= xDim) return;
        for (int y = 0; y < yDim; y++)
        {
            GamePiece p = pieces[x, y];
            if (p == null) continue;

            if (p.Type == PieceType.ICEOBS || p.Type == PieceType.GRASSOBS)
                DamageObstacleAt(x, y, 1);
            else
                RemoveAt(x, y, true);
        }
    }

    public void ClearAllOfType(PieceType type)
    {
        for (int x = 0; x < xDim; x++)
            for (int y = 0; y < yDim; y++)
            {
                GamePiece p = pieces[x, y];
                if (p == null) continue;
                if (p.Type != type) continue;

                RemoveAt(x, y, true);
            }
    }

    public void Bomb3x3(int cx, int cy)
    {
        for (int x = cx - 1; x <= cx + 1; x++)
            for (int y = cy - 1; y <= cy + 1; y++)
                if (x >= 0 && x < xDim && y >= 0 && y < yDim)
                {
                    GamePiece p = pieces[x, y];
                    if (p == null) continue;

                    if (p.Type == PieceType.ICEOBS || p.Type == PieceType.GRASSOBS)
                        DamageObstacleAt(x, y, 2);
                    else
                        RemoveAt(x, y, true);
                }
    }

    // ————————————————————— Clear / Score / Obstacles synergy —————————————————————
    public List<GamePiece> GetObstaclesAffectedByMatch(List<GamePiece> matchPieces)
    {
        List<GamePiece> affected = new List<GamePiece>();
        foreach (GamePiece mp in matchPieces)
        {
            for (int x = mp.X - 1; x <= mp.X + 1; x++)
                for (int y = mp.Y - 1; y <= mp.Y + 1; y++)
                    if (x >= 0 && x < xDim && y >= 0 && y < yDim)
                    {
                        GamePiece n = pieces[x, y];
                        if (n != null &&
                            (n.Type == PieceType.ICEOBS || n.Type == PieceType.GRASSOBS) &&
                            !affected.Contains(n))
                        {
                            affected.Add(n);
                        }
                    }
        }
        return affected;
    }

    public bool ClearAllValidMatches()
    {
        bool needsRefill = false;
        HashSet<GamePiece> toClear = new HashSet<GamePiece>();
        HashSet<GamePiece> obsToClear = new HashSet<GamePiece>();

        for (int y = 0; y < yDim; y++)
        {
            for (int x = 0; x < xDim; x++)
            {
                GamePiece p = pieces[x, y];
                if (p == null) continue;

                List<GamePiece> m = GetMatch(p, x, y);
                if (m != null && m.Count >= 3)
                {
                    foreach (var gp in m) toClear.Add(gp);
                    var affected = GetObstaclesAffectedByMatch(m);
                    foreach (var ob in affected) obsToClear.Add(ob);
                }
            }
        }

        GamePiece[] arr = new GamePiece[toClear.Count];
        toClear.CopyTo(arr);
        if (arr.Length >= 3) AddMatchScore(arr.Length);

        foreach (var gp in arr)
        {
            if (gp != null)
            {
                RemoveAt(gp.X, gp.Y, true);
                needsRefill = true;
            }
        }

        GamePiece[] oarr = new GamePiece[obsToClear.Count];
        obsToClear.CopyTo(oarr);
        foreach (var ob in oarr)
        {
            if (ob != null)
            {
                bool destroyed = DamageObstacleAt(ob.X, ob.Y, 1);
                if (destroyed) needsRefill = true;
            }
        }

        return needsRefill;
    }

    // ————————————————————— No-move detection / Shuffle —————————————————————
    public bool HasPossibleMoves()
    {
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                GamePiece p = pieces[x, y];
                if (p == null || !p.IsMoveable()) continue;

                int[] dxs = { 1, -1, 0, 0 };
                int[] dys = { 0, 0, 1, -1 };
                for (int i = 0; i < 4; i++)
                {
                    int nx = x + dxs[i];
                    int ny = y + dys[i];
                    if (nx < 0 || nx >= xDim || ny < 0 || ny >= yDim) continue;

                    GamePiece q = pieces[nx, ny];
                    if (q == null || !q.IsMoveable()) continue;

                    var m1 = GetMatch(p, nx, ny);
                    var m2 = GetMatch(q, x, y);
                    if ((m1 != null && m1.Count >= 3) || (m2 != null && m2.Count >= 3))
                        return true;
                }
            }
        }
        return false;
    }

    public void ShuffleBoard()
    {
        Debug.Log("Shuffling board…");

        List<GamePiece> movable = new List<GamePiece>();

        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                GamePiece p = pieces[x, y];
                if (p != null && p.IsMoveable())
                {
                    movable.Add(p);
                    pieces[x, y] = null;
                }
            }
        }

        // Fisher-Yates
        for (int i = 0; i < movable.Count; i++)
        {
            int r = Random.Range(i, movable.Count);
            (movable[i], movable[r]) = (movable[r], movable[i]);
        }

        int idx = 0;
        for (int x = 0; x < xDim; x++)
            for (int y = 0; y < yDim; y++)
                if (pieces[x, y] == null && idx < movable.Count)
                {
                    GamePiece p = movable[idx++];
                    pieces[x, y] = p;
                    p.X = x; p.Y = y;
                    p.MoveableComponent.Move(x, y);
                }

        if (!HasPossibleMoves())
        {
            Debug.LogWarning("Still no moves after shuffle, trying again…");
            ShuffleBoard();
        }
    }

    public void CheckAndRefreshBoard()
    {
        if (!HasPossibleMoves())
        {
            Debug.Log("No moves available – refreshing board.");
            ShuffleBoard();
        }
    }

    public void ManualRefreshBoard()
    {
        Debug.Log("Manual board refresh requested.");
        ShuffleBoard();
    }

    // ————————————————————— Periodic check —————————————————————
    private IEnumerator PeriodicMoveCheck()
    {
        while (!gameOver)
        {
            yield return new WaitForSeconds(2f);
            if (!gameOver)
                CheckAndRefreshBoard();
        }
    }

    public void StartMoveChecking()
    {
        StartCoroutine(PeriodicMoveCheck());
    }

    public void StopMoveChecking()
    {
        StopAllCoroutines();
    }

    private IEnumerator AnimateAndDestroy(GamePiece p, bool isObstacle = false)
    {
        if (p == null) yield break;

        var rt = p.GetComponent<RectTransform>();
        var img = p.GetComponent<Image>();
        if (img) img.raycastTarget = false;

        // פרמטרים לאנימציה (ללא פייד דרך CanvasGroup)
        float punch = isObstacle ? 0.08f : 0.12f;
        float tPunch = 0.12f;
        float tOut = 0.22f;
        float rot = Random.Range(-35f, 35f);

        if (rt == null)
        {
            Destroy(p.gameObject);
            yield break;
        }

        // הורגים טווינים ישנים על האובייקט הזה
        rt.DOKill();

        // רצף: פאנץ’ קצר -> היעלמות (סקייל + רוטציה), ואם יש Image – פייד בצבע
        Sequence seq = DOTween.Sequence().SetLink(p.gameObject, LinkBehaviour.KillOnDestroy);
        seq.Append(rt.DOPunchScale(Vector3.one * punch, tPunch, 1, 0.6f));

        // נבנה תת-סיקוונס ליציאה
        Sequence outSeq = DOTween.Sequence()
            .Join(rt.DOScale(0.0f, tOut))
            .Join(rt.DORotate(new Vector3(0, 0, rot), tOut, RotateMode.Fast));

        // אם יש Image – נבצע פייד דרך color (לא חובה)
        if (img != null)
        {
            Color c = img.color;
            outSeq.Join(img.DOColor(new Color(c.r, c.g, c.b, 0f), tOut));
        }

        seq.Append(outSeq);

        yield return seq.WaitForCompletion();
        Destroy(p.gameObject);
    }

    public GamePiece GetPieceAt(int x, int y)
    {
        if (x < 0 || x >= xDim || y < 0 || y >= yDim) return null;
        return pieces[x, y];
    }

    public void TrySwapInDirection(GamePiece from, int dx, int dy)
    {
        if (from == null) return;
        int nx = from.X + dx;
        int ny = from.Y + dy;
        GamePiece neighbor = GetPieceAt(nx, ny);
        if (neighbor == null) { ReleasePiece(); return; }
        pressedPiece = from;
        enteredPiece = neighbor;
        ReleasePiece();
    }

    public bool ApplyAbility(FlowerAbility ab, GamePiece piece)
    {
        if (ab == null || piece == null) return false;
        FlowerAbility.AbilityMap[ab.ability].Apply(this, piece.X, piece.Y, piece.Type);
        StartCoroutine(FillAndResolve());
        return true;
    }

    // ————————————————————— Intro populate (no matches) —————————————————————
    private bool IsObstacle(PieceType t) =>
        t == PieceType.ICEOBS || t == PieceType.GRASSOBS;

    private PieceType[] GetRegularTypes()
    {
        List<PieceType> reg = new List<PieceType>();
        foreach (var t in piecePrefabDict.Keys)
            if (!IsObstacle(t)) reg.Add(t);
        return reg.ToArray();
    }

    // בודק האם בחירת סוג t בתא (x,y) תיצור מיד מאצ' של 3 (אופקי/אנכי)
    private bool CausesMatchAt(int x, int y, PieceType t)
    {
        // אופקי: שניים משמאל + אני
        if (x >= 2)
        {
            var p1 = pieces[x - 1, y];
            var p2 = pieces[x - 2, y];
            if (p1 != null && p2 != null && p1.Type == t && p2.Type == t)
                return true;
        }
        // אנכי: שניים למעלה + אני
        if (y >= 2)
        {
            var p1 = pieces[x, y - 1];
            var p2 = pieces[x, y - 2];
            if (p1 != null && p2 != null && p1.Type == t && p2.Type == t)
                return true;
        }
        return false;
    }

    // פתיחת לוח עם נפילה שורתית (ללא ניקוי/ניקוד)
    private IEnumerator IntroPopulateBoardWithoutMatches()
    {
        isIntroFilling = true;

        var regularTypes = GetRegularTypes();

        for (int y = 0; y < yDim; y++)
        {
            for (int x = 0; x < xDim; x++)
            {
                // בוחרים סוג שלא יוצר מאצ' מיידי
                List<PieceType> candidates = new List<PieceType>(regularTypes);
                for (int i = 0; i < candidates.Count; i++)
                {
                    int r = Random.Range(i, candidates.Count);
                    (candidates[i], candidates[r]) = (candidates[r], candidates[i]);
                }

                PieceType chosen = candidates[0];
                foreach (var t in candidates)
                {
                    if (!CausesMatchAt(x, y, t))
                    {
                        chosen = t;
                        break;
                    }
                }
                // יצירה מעל הלוח ונפילה למיקום
                SpawnNewPiece(x, y, chosen, spawnAbove: true);
            }
            // דיליי קטן בין שורות לנראות "גלישה"
            yield return new WaitForSeconds(fillTime);
        }

        isIntroFilling = false;
    }
}