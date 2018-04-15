// Copyright © 2015 NordicEdu Ltd.

using UnityEngine;
using System.Collections.Generic;
using System;
using ClipperLib;

namespace SimplySVG
{
    [System.Serializable]
    public class GraphicalAttributes
    {
        public float? opacity = null;

        public bool? useFill = null;
        public Color? fillColor = null;
        public float? fillOpacity = null;

        public bool? useStroke = null;
        public float? strokeWidth = null;
        public Color? strokeColor = null;
        public float? strokeOpacity = null;
        public float? strokeMiterLimit = null;

        public PolyFillType? fillRule = null;

        public string clipPath = null;
        public PolyFillType? clipRule = null;

        public void Gather(GraphicalAttributes other)
        {
            if (other.opacity != null)
            {
                opacity *= other.opacity;
            }

            if (other.useFill != null)
            {
                useFill = other.useFill;
            }

            if (other.fillColor != null)
            {
                fillColor = other.fillColor;
            }

            if (other.fillOpacity != null)
            {
                fillOpacity *= other.fillOpacity;
            }

            if (other.strokeWidth != null)
            {
                strokeWidth = other.strokeWidth;
            }

            if (other.useStroke != null)
            {
                useStroke = other.useStroke;
            }

            if (other.strokeColor != null)
            {
                strokeColor = other.strokeColor;
            }

            if (other.strokeOpacity != null)
            {
                strokeOpacity *= other.strokeOpacity;
            }

            if (other.strokeMiterLimit != null)
            {
                strokeMiterLimit = other.strokeMiterLimit;
            }

            if (other.fillRule != null)
            {
                fillRule = other.fillRule;
            }

            if (other.clipPath != null)
            {
                clipPath = other.clipPath;
            }

            if (other.clipRule != null)
            {
                clipRule = other.clipRule;
            }
        }

        public bool AddAttribute(string attributeName, string attributeValue)
        {
            bool parsedWithoutErrors = true;
            if (attributeName == "opacity")
            {
                float parsedValue;
                parsedWithoutErrors = float.TryParse(attributeValue, out parsedValue);

                if (parsedWithoutErrors)
                {
                    opacity = parsedValue;
                }

            }
            else if (attributeName == "fill")
            {
                if (attributeValue == "none")
                {
                    // Disable fill
                    useFill = false;

                }
                else
                {
                    // Parse color value
                    Color? parsedValue = null;
                    try
                    {
                        parsedValue = ImportUtilities.HexToColor(attributeValue);
                    }
                    catch (Exception)
                    {
                        parsedWithoutErrors = false;
                    }

                    if (parsedWithoutErrors)
                    {
                        fillColor = parsedValue;
                        useFill = true;
                    }
                }

            }
            else if (attributeName == "fill-opacity")
            {
                float parsedValue;
                parsedWithoutErrors = float.TryParse(attributeValue, out parsedValue);

                if (parsedWithoutErrors)
                {
                    fillOpacity = parsedValue;
                }

            }
            else if (attributeName == "stroke-width")
            {
                if (attributeValue.Length > 2)
                {
                    if (attributeValue.Substring(attributeValue.Length - 2, 2) == "px")
                    {
                        attributeValue = attributeValue.Substring(0, attributeValue.Length - 2);
                    }
                }

                float parsedValue;
                parsedWithoutErrors = float.TryParse(attributeValue, out parsedValue);

                if (parsedWithoutErrors)
                {
                    if (parsedValue > 0f)
                    {
                        strokeWidth = parsedValue;
                    }
                }

            }
            else if (attributeName == "stroke")
            {
                if (attributeValue == "none")
                {
                    // Disable stroke
                    useStroke = false;

                }
                else
                {
                    Color? parsedValue = null;
                    try
                    {
                        parsedValue = ImportUtilities.HexToColor(attributeValue);

                    }
                    catch (Exception)
                    {
                        parsedWithoutErrors = false;
                    }

                    if (parsedWithoutErrors)
                    {
                        strokeColor = parsedValue;
                        useStroke = true;
                    }
                }
            }
            else if (attributeName == "stroke-opacity")
            {
                float parsedValue;
                parsedWithoutErrors = float.TryParse(attributeValue, out parsedValue);

                if (parsedWithoutErrors)
                {
                    strokeOpacity = parsedValue;
                }

            }
            else if (attributeName == "stroke-miterlimit")
            {
                float parsedValue;
                parsedWithoutErrors = float.TryParse(attributeValue, out parsedValue);

                if (parsedWithoutErrors)
                {
                    strokeMiterLimit = parsedValue;
                }

            }
            else if (attributeName == "fill-rule")
            {
                if (attributeValue == "nonzero")
                {
                    fillRule = PolyFillType.pftNonZero;

                }
                else if (attributeValue == "evenodd")
                {
                    fillRule = PolyFillType.pftEvenOdd;

                }
                else
                {
                    parsedWithoutErrors = false;
                }

            }
            else if (attributeName == "clip-path")
            {
                string id;
                if (!ImportUtilities.ParseIdFromURL(attributeValue, out id))
                {
                    parsedWithoutErrors = false;
                }

                if (parsedWithoutErrors)
                {
                    clipPath = id;
                }

            }
            else if (attributeName == "clip-rule")
            {
                if (attributeValue == "nonzero")
                {
                    clipRule = PolyFillType.pftNonZero;

                }
                else if (attributeValue == "evenodd")
                {
                    clipRule = PolyFillType.pftEvenOdd;

                }
                else
                {
                    parsedWithoutErrors = false;
                }

            }
            else if (attributeName == "style")
            {
                // example: style="fill:none;stroke:#4c4c4c;stroke-width:20.88px;"
                // attributes separated by ";" , attibute names and values deparated by ":"
                bool result = true;
                char[] sep1 = new char[] { ';' };
                string[] attributes = attributeValue.Split(sep1, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < attributes.Length; ++i)
                {
                    char[] sep2 = new char[] { ':' };
                    string[] nameValue = attributes[i].Split(sep2, StringSplitOptions.None);
                    result &= this.AddAttribute(nameValue[0], nameValue[1]);
                }
                if (!result)
                {
                    return false;
                }
            }
            else
            {
                // Attribute is not recognized
                return false;
            }

            if (!parsedWithoutErrors)
            {
                throw new Exception(
                    "Failed to parse Presentation Attribute " + attributeName +
                    " with value " + attributeValue
                );
            }

            return true;
        }

        public static GraphicalAttributes CreateDefault()
        {
            GraphicalAttributes attributes = new GraphicalAttributes();
            attributes.opacity = 1f;

            attributes.useFill = true;
            attributes.fillColor = Color.black;
            attributes.fillOpacity = 1f;

            attributes.useStroke = false;
            attributes.strokeWidth = 1f;
            attributes.strokeColor = Color.black;
            attributes.strokeOpacity = 1f;
            attributes.strokeMiterLimit = 4f;

            attributes.fillRule = PolyFillType.pftNonZero;

            attributes.clipPath = null;
            attributes.clipRule = PolyFillType.pftNonZero;

            return attributes;
        }
    }
}
