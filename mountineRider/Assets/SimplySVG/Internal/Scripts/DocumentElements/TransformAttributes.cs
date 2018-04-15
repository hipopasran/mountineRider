using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

namespace SimplySVG {
    public class TransformAttributes {
        List<Matrix> transforms;
        Matrix _combinedTransform = null;
        public Matrix combinedTransform {
            get {
                if (_combinedTransform == null) {
                    _combinedTransform = Matrix.IdentityMatrix(3, 3);
                    foreach (Matrix transMatrix in transforms) {
                        _combinedTransform = _combinedTransform * transMatrix;
                    }
                }
                return _combinedTransform;
            }

            private set {
                _combinedTransform = value;
            }
        }

        public TransformAttributes() {
            transforms = new List<Matrix>();
        }

        public void Gather(TransformAttributes other) {
            combinedTransform = combinedTransform * other.combinedTransform;

            if (other.transforms.Count > 0) {
                transforms.AddRange(other.transforms);
            }
        }

        public bool AddAttribute(string attributeName, string attributeValue) {
            bool parsedWithoutErrors = true;

            if (attributeName == "transform") {
                parsedWithoutErrors = ParseTransform(attributeValue, ref transforms);

            } else if (attributeName == "x") {
                float parsedValue;
                parsedWithoutErrors = float.TryParse(attributeValue, out parsedValue);

                if (parsedWithoutErrors) {
                    transforms.Add(MatrixUtils.Translate(parsedValue));
                }

            } else if (attributeName == "y") {
                float parsedValue;
                parsedWithoutErrors = float.TryParse(attributeValue, out parsedValue);

                if (parsedWithoutErrors) {
                    transforms.Add(MatrixUtils.Translate(0f, parsedValue));
                }

            } else {
                // Attribute is not recognized
                return false;
            }

            if (!parsedWithoutErrors) {
                throw new Exception(
                    "Failed to parse Transformation Attribute " + attributeName +
                    " with value " + attributeValue
                );
            }

            return true;
        }

        public static TransformAttributes CreateDefault() {
            return new TransformAttributes();
        }

        enum TransformCommand {
            none,
            matrix,
            translate,
            rotate,
            scale,
            skewx,
            skewy
        };

        static bool ParseTransform(string data, ref List<Matrix> parsedMatrices) {
            data = data.ToLower();

            Regex commandRegex = new Regex(@"([a-z]+\()([\-\+\de,+.\s]*)(\))", RegexOptions.IgnoreCase);

            foreach (Match commandMatch in commandRegex.Matches(data)) {
                // Parse function
                Regex functionNameRegex = new Regex(@"([a-z]+)(?=\()", RegexOptions.IgnoreCase);
                string functionNameString = functionNameRegex.Match(commandMatch.Value).Value;

                if (functionNameString == null) {
                    return false;
                }

                TransformCommand command;

                try {
                    command = (TransformCommand)System.Enum.Parse(typeof(TransformCommand), functionNameString);
                } catch (System.Exception) {
                    command = TransformCommand.none;
                }

                if (command == TransformCommand.none) {
                    return false;
                }

                // Parse function parameter
                int paramsStartIndex = commandMatch.Value.IndexOf('(') + 1;
                int paramsEndIndex = commandMatch.Value.IndexOf(')', paramsStartIndex) - 1;
                string paramsString = commandMatch.Value.Substring(paramsStartIndex, paramsEndIndex - paramsStartIndex + 1);

                // Match floating point numbers in a comma separated list
                Regex paramsRegex = new Regex(@"(?=\s*)([\+\-]?\d+(\.\d+)?(e[\+\-]?\d+)?)((?=(\s*[, ]\s*))|(\s*$))", RegexOptions.IgnoreCase);
                List<float> functionParameters = new List<float>(6);

                foreach (Match numberMatch in paramsRegex.Matches(paramsString)) {
                    float number;
                    if (!ImportUtilities.ParseFloat(numberMatch.Value, out number)) {
                        return false;
                    }

                    functionParameters.Add(number);
                }

                if (functionParameters.Count < 1) {
                    return false;
                }

                // Build matrix
                Matrix matrix = Matrix.IdentityMatrix(3, 3);

                switch (command) {
                    case TransformCommand.matrix:
                        if (functionParameters.Count != 6) {
                            return false;
                        }

                        matrix = MatrixUtils.Transform(
                            functionParameters[0],
                            functionParameters[1],
                            functionParameters[2],
                            functionParameters[3],
                            functionParameters[4],
                            functionParameters[5]
                        );

                        break;

                    case TransformCommand.translate:
                        if (functionParameters.Count > 2) {
                            return false;
                        }

                        if (functionParameters.Count == 2) {
                            matrix = MatrixUtils.Translate(
                                functionParameters[0],
                                functionParameters[1]
                            );

                        } else {
                            matrix = MatrixUtils.Translate(functionParameters[0]);
                        }

                        break;

                    case TransformCommand.rotate:
                        if (functionParameters.Count > 3 || (functionParameters.Count > 1 && functionParameters.Count < 3)) {
                            return false;
                        }

                        float a = (functionParameters[0] / 180f) * Mathf.PI;

                        if (functionParameters.Count == 3) {
                            matrix =
                                MatrixUtils.Translate(-functionParameters[1], -functionParameters[2]) *
                                MatrixUtils.Rotate(a) *
                                MatrixUtils.Translate(functionParameters[1], functionParameters[2]);

                        } else {
                            matrix = MatrixUtils.Rotate(a);
                        }

                        break;

                    case TransformCommand.scale:
                        if (functionParameters.Count > 2) {
                            return false;
                        }

                        matrix = MatrixUtils.Scale(
                            functionParameters[0],
                            functionParameters.Count == 2 ? functionParameters[1] : functionParameters[0]
                        );

                        break;

                    case TransformCommand.skewx:
                        matrix = MatrixUtils.SkewX((functionParameters[0] / 180f) * Mathf.PI);

                        break;

                    case TransformCommand.skewy:
                        matrix = MatrixUtils.SkewY((functionParameters[0] / 180f) * Mathf.PI);

                        break;
                }

                parsedMatrices.Add(matrix);
            }

            return true;
        }
    }
}
