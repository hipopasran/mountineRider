using UnityEngine;
using System.Collections.Generic;
using System;
using ClipperLib;
using System.Text.RegularExpressions;
using System.Linq;

namespace SimplySVG {
    public class PathElement : GraphicalElement {
        // Paths parsed from the 'd' attribute
        public List<SubPath> subPaths;

        public PathElement() {
            subPaths = new List<SubPath>();
        }

        public override bool AddShapeAttribute(string attributeName, string attributeValue) {
            bool parsedWithoutErrors = true;

            if (attributeName == "d") {
                parsedWithoutErrors = ParseControlPoints(attributeValue);

            } else {
                return false;
            }

            if (!parsedWithoutErrors) {
                throw new Exception("Failed to parse Path attribute " + attributeName + " with value " + attributeValue);
            }

            return true;
        }

        protected override List<ContourPath> BuildShape(ImportSettings options) {
            List<ContourPath> contourPaths = new List<ContourPath>();

            foreach (SubPath subPath in subPaths) {
                ContourPath contourPath = new ContourPath(subPath.closed);
                contourPaths.Add(contourPath);

                for (int i = 1; i < subPath.path.Count; i++) {
                    PathComponent pathComp = subPath.path[i];
                    PathComponent prevPathComp = subPath.path[i - 1];

                    if (contourPath.path.Count < 1) {
                        // Implicitly add the starting point of this section if there is no previous segment end point present already
                        contourPath.path.Add(prevPathComp.pos);
                    }

                    if (pathComp.segmentType == PathComponent.SegmentType.arc) {
                        // Start point
                        float x1 = prevPathComp.pos.x;
                        float y1 = prevPathComp.pos.y;

                        // End point
                        float x2 = pathComp.pos.x;
                        float y2 = pathComp.pos.y;

                        // Rotation parameter
                        float rho = pathComp.arcRotation;

                        // Step 1: Compute (x1', y1')
                        float x1prim = Mathf.Cos(rho) * ((x1 - x2) / 2f) + Mathf.Sin(rho) * ((y1 - y2) / 2f);
                        float y1prim = (-Mathf.Sin(rho)) * ((x1 - x2) / 2f) + Mathf.Cos(rho) * ((y1 - y2) / 2f);
                        float x1primsq = x1prim * x1prim;
                        float y1primsq = y1prim * y1prim;

                        // Ensure the radius is valid
                        float rx = pathComp.arcRadius.x;
                        float ry = pathComp.arcRadius.y;

                        float rscaler = (x1primsq / (rx * rx)) + (y1primsq / (ry * ry));
                        if (rscaler > 1) {
                            float rscalersqrt = Mathf.Sqrt(rscaler);
                            rx *= rscalersqrt;
                            ry *= rscalersqrt;
                        }
                        float rxsq = rx * rx;
                        float rysq = ry * ry;

                        // Step 2: Compute (cxtick, cytick)
                        float step2_sqrt = Mathf.Sqrt(
                            Mathf.Abs(
                                (
                                    rxsq * rysq - rxsq * y1primsq - rysq * x1primsq
                                ) / (
                                    rxsq * y1primsq + rysq * x1primsq
                                )
                            )
                        );

                        float cxprim = step2_sqrt * ((rx * y1prim) / ry);
                        float cyprim = step2_sqrt * (-((ry * x1prim) / rx));

                        // Select sign
                        if (pathComp.arcLarge == pathComp.arcSweep) {
                            cxprim = -cxprim;
                            cyprim = -cyprim;
                        }

                        // Step 3: Compute (cx, cy) from (cxtick, cytick)
                        float cx = (Mathf.Cos(rho) * cxprim + (-Mathf.Sin(rho)) * cyprim) + ((x1 + x2) / 2f);
                        float cy = (Mathf.Sin(rho) * cxprim + Mathf.Cos(rho) * cyprim) + ((y1 + y2) / 2f);

                        // Step 4: Compute theta1 and dtheta
                        float theta1 = GeneralUtilities.AngleBetweenVectors(
                            new Vector2(1f, 0f),
                            new Vector2(
                                (x1prim - cxprim) / rx,
                                (y1prim - cyprim) / ry
                            )
                        );

                        float dtheta = GeneralUtilities.AngleBetweenVectors(
                            new Vector2(
                                (x1prim - cxprim) / rx,
                                (y1prim - cyprim) / ry
                            ),
                            new Vector2(
                                (-x1prim - cxprim) / rx,
                                (-y1prim - cyprim) / ry
                            )
                        ) % (2f * Mathf.PI);

                        if (!pathComp.arcSweep && dtheta > 0f) {
                            dtheta -= 2f * Mathf.PI;
                        } else if (pathComp.arcSweep && dtheta < 0f) {
                            dtheta += 2f * Mathf.PI;
                        }

                        // Stroke the arc
                        DynamicallySubdivide(
                            options,
                            delegate (float t) {
                                float theta = theta1 + t * dtheta;

                                float arcPointX = (Mathf.Cos(rho) * rx * Mathf.Cos(theta) + (-Mathf.Sin(rho)) * ry * Mathf.Sin(theta)) + cx;
                                float arcPointY = (Mathf.Sin(rho) * rx * Mathf.Cos(theta) + Mathf.Cos(rho) * ry * Mathf.Sin(theta)) + cy;

                                return new Vector2(arcPointX, arcPointY);
                            },
                            0,
                            0f,
                            1f,
                            prevPathComp.pos,
                            pathComp.pos,
                            ref contourPath.path
                        );

                    } else if (pathComp.segmentType == PathComponent.SegmentType.cubic) {
                        // The segement is curved
                        Vector2 p_0 = prevPathComp.pos;
                        Vector2 p_1 = pathComp.useStartCurvePos ? pathComp.startCurvePos : pathComp.pos;
                        Vector2 p_2 = pathComp.useEndCurvePos ? pathComp.endCurvePos : pathComp.pos;
                        Vector2 p_3 = pathComp.pos;

                        DynamicallySubdivide(
                            options,
                            delegate (float t) {
                                Vector2 B_t =
                                    Mathf.Pow((1 - t), 3f) * p_0 +
                                    3 * Mathf.Pow((1 - t), 2f) * t * p_1 +
                                    3 * (1 - t) * Mathf.Pow(t, 2f) * p_2 +
                                    Mathf.Pow(t, 3f) * p_3;

                                return B_t;
                            },
                            0,
                            0f,
                            1f,
                            p_0,
                            p_3,
                            ref contourPath.path
                        );

                    } else if (pathComp.segmentType == PathComponent.SegmentType.quadratic) {
                        Vector3 p_0 = prevPathComp.pos;
                        Vector3 p_1 = pathComp.startCurvePos;
                        Vector3 p_2 = pathComp.pos;

                        DynamicallySubdivide(
                            options,
                            delegate (float t) {
                                return Mathf.Pow(1f - t, 2f) * p_0 +
                                    2f * (1f - t) * t * p_1 +
                                    Mathf.Pow(t, 2f) * p_2;
                            },
                            0,
                            0f,
                            1f,
                            p_0,
                            p_2,
                            ref contourPath.path
                        );
                    }// else if (pathComp.segmentType == PathComponent.SegmentType.line) {
                        // No need to subdivide
                     //}

                    // Add segement end point
                    contourPath.path.Add(pathComp.pos);
                }
            }

            return contourPaths;
        }

        bool ParseControlPoints(string data) {
            // these letters are valid SVG
            // commands. Whenever we find one, a new command is
            // starting. Let's split the string there.
            string separators = @"(?=[MZLHVCSQTAmzlhvcsqta])";
            var tokens = Regex.Split(data, separators).Where(t => !string.IsNullOrEmpty(t));
            //Generate components
            Vector3 penPos = Vector3.zero;
            List<char> separators2 = new List<char>(ImportUtilities.wps);
            separators2.Add(',');

            subPaths.Add(new SubPath());

            SubPath pathComponents = new SubPath();
            subPaths.Add(pathComponents);

            foreach (string s in tokens) {
                //print("new part of path: " + s);
                char c = s[0];

                LinkedList<string> coordinatesLinked = new LinkedList<string>(s.Substring(1).Replace("-", ",-").Replace("e,-", "e-").Split(separators2.ToArray(), System.StringSplitOptions.RemoveEmptyEntries));
                LinkedListNode<string> node = coordinatesLinked.First;

                while(node != null)
                {
                    if (node.Value.Length - node.Value.Replace(".", "").Length > 1)
                    {
                        //dual or more dots in one coordinate
                        
                        string orginal = node.Value;
                        string[] separated = orginal.Split('.');
                        
                        node.Value = separated[0] + "." + separated[1];

                        for (int j = 2; j < separated.Length; ++j)
                        {
                            node = coordinatesLinked.AddAfter(node, "." + separated[j]);
                        }
                    }
                    node = node.Next;
                }

                string[] coordinates = coordinatesLinked.ToArray();
                

                bool absolute = false;

                if (c == ' ') {
                    // Skip empty lines etc.
                    continue;
                }

                switch (c) {
                    //Moveto
                    case 'M':
                    case 'm':
                        absolute = (c == 'M');

                        if (pathComponents.path.Count != 0) {
                            // Start a new sub path
                            pathComponents = new SubPath();
                            subPaths.Add(pathComponents);

                            if (absolute)
                                penPos = Vector3.zero;
                        }

                        for (int i = 0; i < coordinates.Length; i += 2) {
                            penPos = new Vector3(
                                float.Parse(coordinates[i]) + (absolute ? 0f : penPos.x),
                                float.Parse(coordinates[i + 1]) + (absolute ? 0f : penPos.y)
                            );

                            PathComponent pc = new PathComponent(penPos);
                            pathComponents.path.Add(pc);
                        }

                        break;

                    case 'Z':
                    case 'z':
                        pathComponents.closed = true;
                        pathComponents.path.Add(new PathComponent(pathComponents.path[0].pos));
                        penPos = pathComponents.path[0].pos;
                        break;

                    case 'L':
                    case 'l':
                        absolute = (c == 'L');

                        for (int i = 0; i < coordinates.Length; i += 2) {
                            pathComponents.path.Add(PathComponent.LineTo(float.Parse(coordinates[i]) + (absolute ? 0f : penPos.x), float.Parse(coordinates[i + 1]) + (absolute ? 0f : penPos.y)));
                            penPos = pathComponents.path.Last().pos;
                        }
                        break;

                    case 'H':
                    case 'h':
                        absolute = (c == 'H');

                        for (int i = 0; i < coordinates.Length; ++i) {
                            pathComponents.path.Add(PathComponent.LineTo(float.Parse(coordinates[i]) + (absolute ? 0f : penPos.x), penPos.y));
                            penPos = pathComponents.path.Last().pos;
                        }
                        break;

                    case 'V':
                    case 'v':
                        absolute = (c == 'V');

                        for (int i = 0; i < coordinates.Length; ++i) {
                            pathComponents.path.Add(PathComponent.LineTo(penPos.x, float.Parse(coordinates[i]) + (absolute ? 0f : penPos.y)));
                            penPos = pathComponents.path.Last().pos;
                        }
                        break;

                    case 'C':
                    case 'c':
                        absolute = (c == 'C');

                        for (int i = 0; i < coordinates.Length; i += 6) {
                            PathComponent pc = new PathComponent(
                                // Pos
                                float.Parse(coordinates[i + 4]) + (absolute ? 0f : penPos.x),
                                float.Parse(coordinates[i + 5]) + (absolute ? 0f : penPos.y),

                                // Curve start
                                float.Parse(coordinates[i]) + (absolute ? 0f : penPos.x),
                                float.Parse(coordinates[i + 1]) + (absolute ? 0f : penPos.y),

                                // Curve End
                                float.Parse(coordinates[i + 2]) + (absolute ? 0f : penPos.x),
                                float.Parse(coordinates[i + 3]) + (absolute ? 0f : penPos.y)
                            );

                            pathComponents.path.Add(pc);
                            penPos = pc.pos;
                        }
                        break;
                    case 'S':
                    case 's':
                        absolute = (c == 'S');

                        for (int i = 0; i < coordinates.Length; i += 4) {
                            Vector3 startCurve;

                            if (pathComponents.path.Count < 1) {
                                startCurve = penPos;
                            } else {
                                startCurve = pathComponents.path.Last().GetMirroredCurveEndControlPoint();
                            }

                            PathComponent pc = new PathComponent(
                                float.Parse(coordinates[i + 2]) + (absolute ? 0f : penPos.x),
                                float.Parse(coordinates[i + 3]) + (absolute ? 0f : penPos.y),
                                startCurve.x,
                                startCurve.y,
                                float.Parse(coordinates[i]) + (absolute ? 0f : penPos.x),
                                float.Parse(coordinates[i + 1]) + (absolute ? 0f : penPos.y)
                            );

                            pathComponents.path.Add(pc);
                            penPos = pc.pos;
                        }
                        break;

                    case 'Q':
                    case 'q':
                        absolute = (c == 'Q');

                        for (int i = 0; i < coordinates.Length; i += 4) {
                            PathComponent pc = new PathComponent(
                                float.Parse(coordinates[i + 2]) + (absolute ? 0f : penPos.x),
                                float.Parse(coordinates[i + 3]) + (absolute ? 0f : penPos.y),
                                float.Parse(coordinates[i]) + (absolute ? 0f : penPos.x),
                                float.Parse(coordinates[i + 1]) + (absolute ? 0f : penPos.y)
                            );

                            if (pathComponents.path.Count != 0) {
                                pathComponents.path.Last().endCurvePos = new Vector3(
                                    float.Parse(coordinates[i]),
                                    float.Parse(coordinates[i + 1])
                                );
                            }

                            pathComponents.path.Add(pc);
                            penPos = pc.pos;
                        }
                        break;

                    case 'A':
                    case 'a':
                        absolute = (c == 'A');

                        for (int i = 0; i < coordinates.Length; i += 7) {
                            Vector2 targetPosition = new Vector2(float.Parse(coordinates[i + 5]) + (absolute ? 0f : penPos.x), float.Parse(coordinates[i + 6]) + (absolute ? 0f : penPos.y));
                            Vector2 radius = new Vector2(Mathf.Abs(float.Parse(coordinates[i + 0])), Mathf.Abs(float.Parse(coordinates[i + 1]))); // radius

                            PathComponent pc;
                            if (radius.x > 0 && radius.y > 0) {
                                pc = new PathComponent(
                                    targetPosition, // target position
                                    radius,
                                    ((float.Parse(coordinates[i + 2]) % 360f) / 360f) * (2f * Mathf.PI), // rotation
                                    int.Parse(coordinates[i + 3]) > 0, // large arc
                                    int.Parse(coordinates[i + 4]) > 0 // sweep
                                );

                            } else {
                                // Straight line
                                pc = new PathComponent(
                                    targetPosition
                                );
                            }

                            pathComponents.path.Add(pc);
                            penPos = pc.pos;
                        }
                        break;
                    case 'T':
                    case 't':
                        absolute = (c == 'T');

                        for (int i = 0; i < coordinates.Length; i += 4) {
                            float posX = float.Parse(coordinates[i]) + (absolute ? 0f : penPos.x);
                            float posY = float.Parse(coordinates[i + 1]) + (absolute ? 0f : penPos.y);

                            Vector3 reflectedPrevControlPoint;

                            if (pathComponents.path.Count < 1) {
                                reflectedPrevControlPoint = new Vector3(posX, posY, 0f);
                            } else {
                                reflectedPrevControlPoint = pathComponents.path.Last().GetMirroredCurveStartControlPoint();
                            }

                            PathComponent pc = new PathComponent(
                                posX,
                                posY,
                                reflectedPrevControlPoint.x,
                                reflectedPrevControlPoint.y
                            );

                            if (pathComponents.path.Count != 0) {
                                pathComponents.path.Last().endCurvePos = new Vector3(
                                    float.Parse(coordinates[i]),
                                    float.Parse(coordinates[i + 1])
                                );
                            }

                            pathComponents.path.Add(pc);
                            penPos = pc.pos;
                        }

                        break;

                    default:
                        if (GlobalSettings.Get().levelOfLog >= LogLevel.ERRORS_WARNINGS_AND_INFO) {
                            Debug.LogWarning("There's no use for " + c + " command in path");
                        }
                        break;
                }
            }

            return true;
        }

        public delegate Vector2 MakePathPointDelegate(float t);
        public static void DynamicallySubdivide(ImportSettings options, MakePathPointDelegate pointMaker, int depth, float t1, float t2, Vector2 v1, Vector2 v2, ref List<Vector2> path) {
            float t = t2 - (t2 - t1) / 2.0f;
            Vector2 v = pointMaker(t);

            // Calculate distance
            Vector2 v_v1 = v - v1;
            float d = Mathf.Abs(Mathf.Sin(GeneralUtilities.AngleBetweenVectors(v_v1, v2 - v1)) * v_v1.magnitude);

            if (
                (d > options.minSubdivisionDistanceDelta && depth <= options.maxSubdivisonDepth)
                || (depth < 1)  // Make sure the curve gets subdiveded atleast once. Otherwise S shaped curves would not be divided at all.
                                // For S shaped curves, the t = 0.5 has d = 0 and this would cause premature termination.
            ) {
                // First half
                DynamicallySubdivide(
                    options,
                    pointMaker,
                    depth + 1,
                    t1,
                    t,
                    v1,
                    v,
                    ref path);

                // Add midpoint
                path.Add(new Vector2(v.x, v.y));

                // Second half
                DynamicallySubdivide(
                    options,
                    pointMaker,
                    depth + 1,
                    t,
                    t2,
                    v,
                    v2,
                    ref path);
            }
        }

        [System.Serializable]
        public class SubPath {
            public bool closed;
            public List<PathComponent> path;

            public SubPath() {
                closed = false;
                path = new List<PathComponent>();
            }
        }

        [System.Serializable]
        public class PathComponent {
            public enum SegmentType {
                line,
                cubic,
                quadratic,
                arc
            }

            public SegmentType segmentType;

            public Vector3 pos;

            public Vector3 startCurvePos;
            public Vector3 endCurvePos;
            public bool useStartCurvePos;
            public bool useEndCurvePos;

            public Vector2 arcRadius;
            public float arcRotation;
            public bool arcLarge;
            public bool arcSweep;

            public PathComponent(float x, float y, float startCurveX, float startCurveY, float endCurveX, float endCurveY) {
                segmentType = SegmentType.cubic;

                pos = new Vector3(x, y);
                startCurvePos = new Vector3(startCurveX, startCurveY);
                endCurvePos = new Vector3(endCurveX, endCurveY);
                useEndCurvePos = true;
                useStartCurvePos = true;
            }

            public PathComponent(float x, float y, float startCurveX, float startCurveY) {
                segmentType = SegmentType.quadratic;

                pos = new Vector3(x, y);
                startCurvePos = new Vector3(startCurveX, startCurveY);
                endCurvePos = pos;
                useEndCurvePos = false;
                useStartCurvePos = true;
            }

            // Constructor for straight line section
            public PathComponent(Vector3 penPos) {
                segmentType = SegmentType.line;

                this.pos = this.startCurvePos = this.endCurvePos = penPos;
                useEndCurvePos = false;
                useStartCurvePos = false;
                useEndCurvePos = false;
                useStartCurvePos = false;
            }

            // Constructor for arc section
            public PathComponent(Vector2 position, Vector2 radius, float rotation, bool largeArc, bool sweep) {
                segmentType = SegmentType.arc;

                pos = new Vector3(position.x, position.y);
                arcRadius = radius;
                arcRotation = rotation;
                arcLarge = largeArc;
                arcSweep = sweep;
            }

            public static PathComponent LineTo(float x, float y) {
                return new PathComponent(new Vector3(x, y));
            }

            public void MirrorStartPointToEndpoint() {
                if (useStartCurvePos) {
                    Vector3 temp = startCurvePos - pos;
                    temp = -temp;
                    endCurvePos = pos + temp;
                    useEndCurvePos = true;
                }
            }

            public Vector3 GetMirroredCurveEndControlPoint() {
                if (!useEndCurvePos) {
                    return pos;
                }

                return pos - (endCurvePos - pos);
            }

            public Vector3 GetMirroredCurveStartControlPoint() {
                if (!useStartCurvePos) {
                    return pos;
                }

                return pos - (startCurvePos - pos);
            }
        }
    }
}
