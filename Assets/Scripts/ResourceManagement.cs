
using TMPro;
using UnityEngine;
using System;

public class ResourceManagement : MonoBehaviour
{
    public int water = 0, sun = 0, earth = 0, grass = 0;
    public TMP_Text waterText, sunText, earthText, grassText;
    public event Action OnChanged;
    //Refreshes the on-screen counters 
    void UpdateUI()
    {
        if (waterText != null) waterText.text = water.ToString();
        if (sunText != null) sunText.text = sun.ToString();
        if (earthText != null) earthText.text = earth.ToString();
        if (grassText != null) grassText.text = grass.ToString();
        OnChanged?.Invoke(); 
    }
    public bool Has(Cost c)
    {
        return water >= c.water
        && sun >= c.sun
        && earth >= c.earth
        && grass >= c.grass;
    }

    public bool Spend(Cost c)
    {
        if (!Has(c))
            return false;
        water -= c.water;
        sun -= c.sun;
        earth -= c.earth;
        grass -= c.grass;
        UpdateUI();
        return true;

    }
    public void Add(Grid.PieceType type, int amount = 1)
    {
        switch (type)
        {
            case Grid.PieceType.WATER:
                water += amount;
                break;
            case Grid.PieceType.SUN:
                sun += amount;
                break;
            case Grid.PieceType.EARTH:
                earth += amount;
                break;
            case Grid.PieceType.GRASS:
                grass += amount;
                break;
        }
        UpdateUI();
    }
}
