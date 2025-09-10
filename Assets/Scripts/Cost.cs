using System;
public struct Cost
{
    public int water;
    public int sun;
    public int earth;
    public int grass;
    public static Cost Multiply(Cost c, int k) => new Cost {
        water = c.water * k,
        sun   = c.sun   * k,
        earth = c.earth * k,
        grass = c.grass * k
    };
}