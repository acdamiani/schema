using UnityEngine;

namespace Schema.Utilities
{
    public class Bezier
    {
        public Bezier next;
        public Vector2 p0;
        public Vector2 p1;
        public Vector2 p2;
        public Vector2 p3;

        public Bezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, Bezier next = null)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
            this.next = next;
        }

        public Vector2 Position(float t)
        {
            Vector2 a = Mathf.Pow(1 - t, 3) * p0;
            Vector2 b = Mathf.Pow(1 - t, 2) * 3 * t * p1;
            Vector2 c = (1 - t) * 3 * Mathf.Pow(t, 2) * p2;
            Vector2 d = Mathf.Pow(t, 3) * p3;

            return a + b + c + d;
        }

        public Rect Bounds()
        {
            float x = Mathf.Min(p0.x, p1.x, p2.x, p3.x);
            float y = Mathf.Min(p0.y, p1.y, p2.y, p3.y);
            float xMax = Mathf.Max(p0.x, p1.x, p2.x, p3.x);
            float yMax = Mathf.Max(p0.y, p1.y, p2.y, p3.y);

            return new Rect(x, y, xMax - x, yMax - y);
        }

        public bool Hit(Vector2 position, float maxDist, int subdivisions = 8)
        {
            float step = 1f / subdivisions;

            Vector2 closest = Vector2.positiveInfinity;

            for (int i = 1; i <= subdivisions; i++)
            {
                Vector2 a = Position(step * (i - 1));
                Vector2 b = Position(step * i);

                Vector2 ap = position - a;
                Vector2 ab = b - a;

                float abMag = ab.sqrMagnitude;
                float dot = Vector2.Dot(ap, ab);
                float dist = dot / abMag;

                Vector2 c;

                if (dist < 0)
                    c = a;
                else if (dist > 1)
                    c = b;
                else
                    c = a + ab * dist;

                closest = Vector2.Distance(closest, position) < Vector2.Distance(c, position) ? closest : c;
            }

            return Vector2.Distance(closest, position) < maxDist;
        }

        public void Split()
        {
            Vector2 e = (p0 + p1) / 2f;
            Vector2 f = (p1 + p2) / 2f;
            Vector2 g = (p2 + p3) / 2f;
            Vector2 h = (e + f) / 2f;
            Vector2 j = (f + g) / 2f;
            Vector2 k = (h + j) / 2f;

            Bezier next = new Bezier(k, j, g, p3, this.next);

            p1 = e;
            p2 = h;
            p3 = k;

            this.next = next;
        }

        public bool Intersect(Rect rect)
        {
            Rect bounds = Bounds();

            if (!bounds.Overlaps(rect))
                return false;

            if (bounds.width < 1f)
                return true;

            if (rect.Contains(p0) || rect.Contains(p3))
                return true;

            Bezier bezier = new Bezier(p0, p1, p2, p3);
            bezier.Split();

            return bezier.Intersect(rect) || bezier.next.Intersect(rect);
        }
    }
    // public static float Slope(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    // {
    //     Vector2 a = -3 * Nodes/Mathf.Pow(1 - t, 2) * p0;
    //     Vector2 b = 3 * Nodes/Mathf.Pow((1 - t), 2) * p1;
    //     Vector2 c = -6 * t * (1 - t) * p1;
    //     Vector2 d = -3 * Nodes/Mathf.Pow(t, 2) * p2;
    //     Vector2 e = 6 * t * (1 - t) * p2;
    //     Vector2 f = 3 * Nodes/Mathf.Pow(t, 2) * p3;

    //     Vector2 g = a + b + c + d + e + f;

    //     return g.y / g.x;
    // }
    // public static float Theta(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    // {
    //     float s = Slope(t, p0, p1, p2, p3);

    //     return Nodes/Mathf.Atan(Nodes/Mathf.Pow(s, -1));
    // }
}