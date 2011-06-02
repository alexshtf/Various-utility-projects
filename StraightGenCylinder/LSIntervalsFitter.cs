using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meta.Numerics.Matrices;

namespace StraightGenCylinder
{
    static class LSIntervalsFitter
    {


        public static double[][] FitIntervals(double[] values, params int[] breakIndices)
        {
            var intervalsCount = breakIndices.Length;

            // build linear system of equations to solve the approximation problem
            var systemSize = 5 * intervalsCount - 1; // matrix size
            var leftHandMatrix = new SquareMatrix(systemSize);
            var rightHandVector = new ColumnVector(systemSize);
            for (int intervalIndex = 0; intervalIndex < intervalsCount; ++intervalIndex)
            {
                var intervalStart = intervalIndex == 0 ? 0 : breakIndices[intervalIndex - 1];
                var intervalEnd = breakIndices[intervalIndex];
                var pointsCount = intervalEnd - intervalStart + 1;
                var baseMatrixIndex = 5 * intervalIndex;
                var ts = Enumerable.Range(intervalStart, pointsCount);

                // fill the 4x4 part of the matrix
                foreach (var row in Enumerable.Range(baseMatrixIndex, 4))
                {
                    foreach (var col in Enumerable.Range(baseMatrixIndex, 4))
                    {
                        var sum = ts.Select(t => Math.Pow(t, row + col - 2 * baseMatrixIndex)).Sum();
                        leftHandMatrix[row, col] = sum;
                    }
                    rightHandVector[row] = ts.Sum(t => values[t] * Math.Pow(t, row - baseMatrixIndex));
                }

                if (intervalIndex < intervalsCount - 1) // we add additional matrix elements for non-last intervals
                {
                    // fill the 5th row and column of the matrix
                    foreach (var i in Enumerable.Range(baseMatrixIndex + 1, 3))
                    {
                        var relativeIdx = i - baseMatrixIndex;
                        var value = relativeIdx * Math.Pow(ts.Last(), relativeIdx - 1);
                        leftHandMatrix[i, baseMatrixIndex + 4] = value;
                        leftHandMatrix[i + 5, baseMatrixIndex + 4] = -value;
                        leftHandMatrix[baseMatrixIndex + 4, i] = value;
                        leftHandMatrix[baseMatrixIndex + 4, i + 5] = -value;
                    }
                }
            }

            // solve the system
            var qr = leftHandMatrix.QRDecomposition();
            var solution = qr.Solve(rightHandVector);

            // create the result (5*i, ..., 5*i + 3 are the coefficients for interval i)
            var result = new double[intervalsCount][];
            for (int i = 0; i < intervalsCount; ++i)
            {
                result[i] = new double[4];
                for (int j = 0; j < 4; ++j)
                    result[i][j] = solution[5 * i + j];
            }

            return result;
        }
    }
}
