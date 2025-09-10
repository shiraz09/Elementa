
using TMPro;
using UnityEngine;

public class ResourceManagement : MonoBehaviour
{
    public int water = 0, sun = 0, earth = 0, grass = 0;
    public TMP_Text waterText, sunText, earthText, grassText;

    //Refreshes the on-screen counters 
    void UpdateUI()
    {
        if (waterText) waterText.text = water.ToString();
        if (sunText) sunText.text = sun.ToString();
        if (earthText) earthText.text = earth.ToString();
        if (grassText) grassText.text = grass.ToString();
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
    public void Add(Grid.elemType type, int amount = 1)
    {
        switch (type)
        {
            case Grid.elemType.WATER:
                water += amount;
                break;
            case Grid.elemType.SUN:
                sun += amount;
                break;
            case Grid.elemType.EARTH:
                earth += amount;
                break;
            case Grid.elemType.GRASS:
                grass += amount;
                break;
        }
        UpdateUI();
    }
}
