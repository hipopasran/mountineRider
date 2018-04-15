// Copyright © 2015 NordicEdu Ltd.

using UnityEngine;
using System.Collections.Generic;

public static class GeneralUtilities {
    public static bool PointInsideTriangle(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2) {
        // Source: http://www.blackpawn.com/texts/pointinpoly/

        // Compute vectors        
        Vector2 v0 = p2 - p0;
        Vector2 v1 = p1 - p0;
        Vector2 v2 = p - p0;

        // Compute dot products
        float dot00 = Vector2.Dot(v0, v0);
        float dot01 = Vector2.Dot(v0, v1);
        float dot02 = Vector2.Dot(v0, v2);
        float dot11 = Vector2.Dot(v1, v1);
        float dot12 = Vector2.Dot(v1, v2);

        // Compute barycentric coordinates
        float invDenom = 1f / (dot00 * dot11 - dot01 * dot01);
        float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
        float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

        // Check if point is in triangle
        return (u >= 0) && (v >= 0) && (u + v < 1f);
    }

    public static float AngleBetweenVectors(Vector2 u, Vector2 v) {
        float sign = (u.x * v.y - u.y * v.x) > 0f ? 1f : -1f;

        return sign * Mathf.Acos(Vector2.Dot(u, v) / (u.magnitude * v.magnitude));
    }
}
