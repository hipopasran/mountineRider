// Copyright © 2015 NordicEdu Ltd.

using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SimplySVG
{
    public static class ImportUtilities
    {

        public static Color HexToColor(string hex)
        {

            if (hex.StartsWith("#"))
            {
                hex = hex.Replace("#", "");
            }

            //pre determined 16 "safe" colors
            switch (hex)
            {
                case "none": return Color.clear;
                case "black": hex = "000000"; break;
                case "gray": hex = "808080"; break;
                case "silver": hex = "C0C0C0"; break;
                case "white": hex = "FFFFFF"; break;
                case "maroon": hex = "800000"; break;
                case "red": hex = "FF0000"; break;
                case "olive": hex = "808000"; break;
                case "yellow": hex = "FFFF00"; break;
                case "green": hex = "008000"; break;
                case "lime": hex = "00FF00"; break;
                case "teal": hex = "008080"; break;
                case "aqua": hex = "00FFFF"; break;
                case "navy": hex = "000080"; break;
                case "blue": hex = "0000FF"; break;
                case "purple": hex = "800080"; break;
                case "fuchsia": hex = "FF00FF"; break;
            }

            if (hex.Length == 3)
            {
                hex = "" + hex[0] + hex[0] + hex[1] + hex[1] + hex[2] + hex[2];
            }

            if (hex.Length != 6 && hex.Length != 8)
            {
                if (GlobalSettings.Get().levelOfLog >= LogLevel.ERRORS)
                {
                    Debug.LogError("Hex number must be 6 or 8 digit lenght for color. Input was: \"" + hex + "\"");
                }
                return Color.clear;
            }

            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

            if (hex.Length == 8)
            {
                byte a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
                return new Color32(r, g, b, a);
            }

            return new Color32(r, g, b, 255);
        }

        public static bool ParseFloat(string s, out float f)
        {
            Regex reg = new Regex(@"[-+]?(\d*[.])?\d+");
            Match match = reg.Match(s);

            if (!match.Success)
            {
                f = 0f;
                return false;
            }

            if (!float.TryParse(match.Value, out f))
            {
                return false;
            }

            return true;
        }

        public static bool ParseIdFromURL(string s, out string id)
        {
            string urlHeader = "url(#";
            int urlHeaderPos = s.IndexOf(urlHeader);
            int idBeginPos = urlHeaderPos + urlHeader.Length;
            int idEndPos = s.LastIndexOf(")");

            if (
                urlHeaderPos < 0 ||
                idEndPos < 0 ||
                idEndPos < idBeginPos ||
                (idEndPos - idBeginPos) < 1
            )
            {
                id = null;
                return false;
            }

            id = s.Substring(idBeginPos, idEndPos - idBeginPos);
            return true;
        }

        public static char[] wps = new char[] { '\x20', '\x09', '\x0d', '\x0a' };

        public static Vector3 ConvertToVector3(ClipperLib.IntPoint point, bool useScaling = true)
        {
            return new Vector3(point.X, point.Y, 0f) / (useScaling ? (float)GraphicalElement.clipperCoordinateScale : 1f);
        }

        public static Vector3 ConvertToVector3(Poly2Tri.Point2D point)
        {
            return new Vector3(point.Xf, point.Yf, 0f);
        }

        public static List<Vector3> ConvertToVector3List(IList<ClipperLib.IntPoint> points, bool useScaling = true)
        {
            List<Vector3> list = new List<Vector3>();

            foreach (ClipperLib.IntPoint point in points)
            {
                list.Add(ConvertToVector3(point, useScaling));
            }

            return list;
        }

        public static List<Vector3> ConvertToVector3List(IList<Poly2Tri.Point2D> points)
        {
            List<Vector3> list = new List<Vector3>();

            foreach (Poly2Tri.Point2D point in points)
            {
                list.Add(ConvertToVector3(point));
            }

            return list;
        }

        public static ClipperLib.IntPoint ConvertToScaledClipperPoint(Vector2 point)
        {
            ClipperLib.IntPoint scaledPoint = new ClipperLib.IntPoint(
                point.x * GraphicalElement.clipperCoordinateScale,
                point.y * GraphicalElement.clipperCoordinateScale
            );
            return scaledPoint;
        }

        /// <summary>
        /// Convert Clipper's point presentation to 
        /// </summary>
        /// <param name="scaledPoint">Assumed to be scaled by the Clipper scaling factor</param>
        /// <returns>Scaled down polygon point</returns>
        public static Poly2Tri.PolygonPoint ConvertToTriangulationPoint(ClipperLib.IntPoint scaledPoint)
        {
            Poly2Tri.PolygonPoint point = new Poly2Tri.PolygonPoint(
                (double)scaledPoint.X / GraphicalElement.clipperCoordinateScale,
                (double)scaledPoint.Y / GraphicalElement.clipperCoordinateScale
            );
            return point;
        }

        public static void DestroyChildren(Transform parent, bool destroyAssets = false)
        {
            while (parent.childCount != 0)
            {
#if UNITY_EDITOR
                GameObject.DestroyImmediate(parent.GetChild(0).gameObject, destroyAssets);
#else
                GameObject.Destroy(parent.GetChild(0).gameObject);
#endif
            }
        }
    }
}
