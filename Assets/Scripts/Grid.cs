using UnityEngine;
using System.Collections.Generic;

public class Grid : MonoBehaviour
{
    public enum PieceType
    {
        WATER,
        SUN,
        EARTH,
        GRASS
    };

    [System.Serializable]
    public struct PiecePrefab
    {
        public PieceType type;     // סוג החתיכה
        public GameObject prefab;  // הפריפאב של החתיכה
    };

    public int xDim = 8;                          // רוחב הלוח (מספר עמודות)
    public int yDim = 8;                          // גובה הלוח (מספר שורות)
    public float cellSize = 1f;                   // גודל תא (אם תרצי להשתמש בהמשך)
    public PiecePrefab[] piecePrefabs;            // רשימה של סוגים ופריפאבים
    public GameObject backgroundPrefab;           // פריפאב של משבצת רקע

    private Dictionary<PieceType, GameObject> piecePrefabDict; // מילון: סוג → פריפאב
    private GameObject[,] pieces;                                 // מטריצה של החתיכות על הלוח

    void Start()
    {
        // בונים מילון מהמערך piecePrefabs
        piecePrefabDict = new Dictionary<PieceType, GameObject>();
        for (int i = 0; i < piecePrefabs.Length; i++)
        {
            if (!piecePrefabDict.ContainsKey(piecePrefabs[i].type) && piecePrefabs[i].prefab != null)
            {
                piecePrefabDict.Add(piecePrefabs[i].type, piecePrefabs[i].prefab);
            }
        }

        // בונים לוח של רקעים
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                GameObject background = Instantiate(
                    backgroundPrefab,
                    new Vector3(x, y, 0f),
                    Quaternion.identity
                );
                background.transform.parent = transform;
                
                background.GetComponent<RectTransform>().anchoredPosition = new Vector3((x * cellSize) - (cellSize * xDim / 2) + (cellSize / 2), (y * cellSize) - (cellSize * yDim / 2) + (cellSize / 2), 0f);

                // אם צריך להבטיח גודל תא (במיוחד אם ה-PPU לא תואם):
                // background.transform.localScale = new Vector3(cellSize, cellSize, 1f);
            }
        }

        // בונים את החתיכות עצמן – רנדומלי מתוך הרשימה
        pieces = new GameObject[xDim, yDim];
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                // בוחרים רשומה רנדומלית מהמערך (בטוח יותר מללהק מספר ל-enum)
                PiecePrefab entry = piecePrefabs[Random.Range(0, piecePrefabs.Length)];
                GameObject prefab = piecePrefabDict[entry.type];

                GameObject go = Instantiate(prefab, new Vector3(x, y, -0.1f), Quaternion.identity);
                go.name = $"Piece({x},{y})";
                go.transform.parent = transform;
                
                go.GetComponent<RectTransform>().anchoredPosition = new Vector3((x * cellSize) - (cellSize * xDim / 2) + (cellSize / 2), (y * cellSize) - (cellSize * yDim / 2) + (cellSize / 2), 0f);

                // אם צריך להבטיח גודל תא:
                // go.transform.localScale = new Vector3(cellSize, cellSize, 1f);

                pieces[x, y] = go;
            }
        }
    }
}