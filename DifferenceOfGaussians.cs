namespace SIFT;

public static class DifferenceOfGaussians
{
    private static float[] CreateScaleSpace(int size, float sigmaInit, float k)
    {
        var blurRadii = new float[size];
        for (var i = 0; i < blurRadii.Length; i++)
        {
            blurRadii[i] = sigmaInit * (float)Math.Pow(k, i);
        }

        return blurRadii;
    }

    private static (float[][][], (int, int)[]) GetBlurredImages(float[] greyPixels, IReadOnlyList<float> blurRadii, int octaves, int rows, int cols)
    {
        // Generate Gaussian pyramid
        var thisRows = rows;
        var thisCols = cols;
        var thisGrey = greyPixels;
        var nextGrey = greyPixels;
        var blurredImages = new float[octaves][][];
        var scales = new (int, int)[octaves];
        for (var i = 0; i < octaves; i++)
        {
            blurredImages[i] = new float[blurRadii.Count][];
            scales[i] = (thisRows, thisCols);
            for (var j = 0; j < blurredImages[i].Length; j++)
            {
                var greyCopy = new float[thisRows * thisCols];
                Buffer.BlockCopy(thisGrey, 0, greyCopy, 0, thisGrey.Length * sizeof(float));

                blurredImages[i][j] = new float[thisRows * thisCols];
                GaussianBlur.Blur(greyCopy, blurredImages[i][j], thisRows, thisCols, blurRadii[j]);

                if (j == blurredImages[i].Length / 2)
                {
                    nextGrey = new float[(thisRows / 2) * (thisCols / 2)];
                    for (var r = 0; r < thisRows; r += 2)
                    {
                        for (var c = 0; c < thisCols; c += 2)
                        {
                            nextGrey[(r / 2) * (thisCols / 2) + (c / 2)] = blurredImages[i][j][r * thisCols + c];
                        }
                    }
                }
            }

            thisRows /= 2;
            thisCols /= 2;
            thisGrey = nextGrey;
        }

        return (blurredImages, scales);
    }

    private static IList<Keypoint> CalculateKeypoints(float[][][] blurredImages, (int, int)[] octaveScales, float[] blurRadii)
    {
        var octaves = octaveScales.Length;
        var keypoints = new List<Keypoint>();

        // Calculate difference images
        var differenceImages = new float[octaves][][];
        for (var o = 0; o < differenceImages.Length; o++)
        {
            var (thisRows, thisCols) = octaveScales[o];
            differenceImages[o] = new float[blurRadii.Length - 1][];
            for (var s = 0; s < differenceImages[o].Length; s++)
            {
                differenceImages[o][s] = new float[thisRows * thisCols];
                for (var r = 0; r < thisRows; r++)
                {
                    for (var c = 0; c < thisCols; c++)
                    {
                        differenceImages[o][s][r * thisCols + c] = blurredImages[o][s + 1][r * thisCols + c] - blurredImages[o][s][r * thisCols + c];
                    }
                }
            }
        }

        // Calculate keypoints at each octave by sliding a 3x3x3 window through the stack and finding local maxima
        for (var o = 0; o < differenceImages.Length; o++)
        {
            var (thisRows, thisCols) = octaveScales[o];
            for (var z = 0; z < differenceImages[o].Length; z++)
            {
                var scaleFactor = (int)Math.Pow(2, o);

                for (var y = 0; y < thisRows; y++)
                {
                    for (var x = 0; x < thisCols; x++)
                    {
                        var maximaCount = 0;
                        var px = differenceImages[o][z][y * thisCols + x];

                        // Check 3x3x3 window
                        for (var kZ = 0; kZ < 3; kZ++)
                        {
                            for (var kY = 0; kY < 3; kY++)
                            {
                                for (var kX = 0; kX < 3; kX++)
                                {
                                    var nextZ = z + kZ - 1;
                                    if (nextZ < 0 || nextZ >= differenceImages[o].Length) continue;

                                    var nextY = y + kY - 1;
                                    if (nextY < 0 || nextY >= thisRows) continue;

                                    var nextX = x + kX - 1;
                                    if (nextX < 0 || nextX >= thisCols) continue;

                                    var nextPx = differenceImages[o][nextZ][nextY * thisCols + nextX];
                                    if (nextPx >= px)
                                    {
                                        maximaCount++;
                                    }
                                }
                            }
                        }

                        if (maximaCount <= 1)
                        {
                            keypoints.Add(new Keypoint
                            {
                                Row = y * scaleFactor,
                                Column = x * scaleFactor,
                                Magnitude = px * scaleFactor,
                                Sigma = (blurRadii[z] + blurRadii[z + 1]) / 2 * scaleFactor,
                            });
                        }
                    }
                }
            }
        }

        return keypoints;
    }

    public static IList<Keypoint> GetKeypoints(int scales, int octaves, float[] img, int rows, int cols)
    {
        // Create the scale space
        var blurRadii = CreateScaleSpace(scales, 1.4f, (float)Math.Sqrt(2));
        
        // Calculate blurred images
        var (blurredImages, octaveScales) = GetBlurredImages(img, blurRadii, octaves, rows, cols);

        // Calculate difference images and keypoints at each scale
        var keypoints = CalculateKeypoints(blurredImages, octaveScales, blurRadii);

        if (keypoints.Any())
        {
            // Normalize the keypoint magnitudes to [0, 1] and remove any weak keypoints
            var diffMagMax = keypoints.Select(kp => kp.Magnitude).Max();
            var diffMagMin = keypoints.Select(kp => kp.Magnitude).Min();
            for (var i = keypoints.Count - 1; i >= 0; i--)
            {
                keypoints[i].Magnitude = (keypoints[i].Magnitude - diffMagMin) / diffMagMax;
                if (keypoints[i].Magnitude < 0.2)
                {
                    keypoints.RemoveAt(i);
                }
            }
        }

        return keypoints;
    }
}