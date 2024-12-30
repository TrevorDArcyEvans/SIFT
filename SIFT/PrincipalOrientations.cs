namespace SIFT;

public class PrincipalOrientations
{
  public static float GetGradientOrientation(float gradX, float gradY)
  {
    return MathF.Atan(gradY / gradX);
  }

  public static void Update(IList<Keypoint> keypoints, float[] gradXImg, float[] gradYImg, int rows, int cols)
  {
    var bucketWidth = 2 * MathF.PI / 36;
    var extraKeypoints = new List<Keypoint>();
    for (var k = keypoints.Count - 1; k >= 0; k--)
    {
      var keypoint = keypoints[k];

      // Populate the orientation buckets
      var orientationBuckets = new int[36];
      for (var r = 0; r < 8; r++)
      {
        for (var c = 0; c < 8; c++)
        {
          var gradXAvg = 0f;
          var gradYAvg = 0f;

          var rScaledStart = (int) Math.Truncate(keypoint.Row + (r - 4) * keypoint.Sigma);
          var rScaledEnd = (int) Math.Truncate(keypoint.Row + (r - 3) * keypoint.Sigma);
          var rWidth = rScaledEnd - rScaledStart + 1;
          var cScaledStart = (int) Math.Truncate(keypoint.Column + (c - 4) * keypoint.Sigma);
          var cScaledEnd = (int) Math.Truncate(keypoint.Column + (c - 3) * keypoint.Sigma);
          var cWidth = cScaledEnd - cScaledStart + 1;

          for (var rScaled = rScaledStart; rScaled <= rScaledEnd; rScaled++)
          {
            for (var cScaled = cScaledStart; cScaled <= cScaledEnd; cScaled++)
            {
              if (rScaled < 0 || rScaled >= rows || cScaled < 0 || cScaled >= cols)
              {
                continue;
              }

              gradXAvg += gradXImg[rScaled * cols + cScaled];
              gradYAvg += gradYImg[rScaled * cols + cScaled];
            }
          }

          gradXAvg /= cWidth;
          gradYAvg /= rWidth;
          keypoint.SetGradient(r, c, (gradXAvg, gradYAvg));
          if (gradXAvg == 0)
          {
            continue;
          }

          var orientation = GetGradientOrientation(gradXAvg, gradYAvg);
          orientation %= 2 * MathF.PI;
          while (orientation < 0)
          {
            orientation += 2 * MathF.PI;
          }

          orientationBuckets[(int) Math.Floor(orientation / bucketWidth)]++;
        }
      }

      // Find the buckets with the highest orientation
      IList<int> maxBuckets = new List<int>();
      var maxValue = 0;
      for (var o = 0; o < orientationBuckets.Length; o++)
      {
        if (orientationBuckets[o] > maxValue)
        {
          maxValue = orientationBuckets[o];
          maxBuckets = new List<int> {o};
        }
        else if (orientationBuckets[o] == maxValue)
        {
          maxBuckets.Add(o);
        }
      }

      // Remove the keypoint if its gradients are all 0
      if (!maxBuckets.Any())
      {
        keypoints.RemoveAt(k);
      }

      // Update the principal orientation of the keypoint
      keypoint.PrincipalOrientation = maxBuckets[0] * bucketWidth;

      // Add more keypoints if there are multiple equal buckets
      for (var b = 1; b < maxBuckets.Count; b++)
      {
        var newKeypoint = new Keypoint
        {
          Row = keypoint.Row,
          Column = keypoint.Column,
          Magnitude = keypoint.Magnitude,
          Sigma = keypoint.Sigma,
          PrincipalOrientation = maxBuckets[b] * bucketWidth,
        };

        newKeypoint.CopyGradientsFrom(keypoint);

        extraKeypoints.Add(newKeypoint);
      }
    }

    // Add new keypoints
    foreach (var keypoint in extraKeypoints)
    {
      keypoints.Add(keypoint);
    }
  }
}
