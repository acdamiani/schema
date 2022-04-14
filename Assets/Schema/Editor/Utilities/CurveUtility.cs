using UnityEngine;

public static class CurveUtility
{
    public static Vector2 Position(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        Vector2 a = Mathf.Pow(1 - t, 3) * p0;
        Vector2 b = Mathf.Pow(1 - t, 2) * 3 * t * p1;
        Vector2 c = (1 - t) * 3 * Mathf.Pow(t, 2) * p2;
        Vector2 d = Mathf.Pow(t, 3) * p3;

        return a + b + c + d;
    }
    // public static float Slope(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    // {
    //     Vector2 a = -3 * Mathf.Pow(1 - t, 2) * p0;
    //     Vector2 b = 3 * Mathf.Pow((1 - t), 2) * p1;
    //     Vector2 c = -6 * t * (1 - t) * p1;
    //     Vector2 d = -3 * Mathf.Pow(t, 2) * p2;
    //     Vector2 e = 6 * t * (1 - t) * p2;
    //     Vector2 f = 3 * Mathf.Pow(t, 2) * p3;

    //     Vector2 g = a + b + c + d + e + f;

    //     return g.y / g.x;
    // }
    // public static float Theta(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    // {
    //     float s = Slope(t, p0, p1, p2, p3);

    //     return Mathf.Atan(Mathf.Pow(s, -1));
    // }
}