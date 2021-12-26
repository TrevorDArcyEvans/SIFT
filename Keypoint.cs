namespace SIFT;

public class Keypoint
{
    public int Row { get; init; }

    public int Column { get; init; }

    public float Magnitude { get; set; }

    public float Sigma { get; init; }

    public float PrincipalOrientation { get; set; }

    private readonly float[] _gradientLocalNeighborhood;

    public Keypoint()
    {
        _gradientLocalNeighborhood = new float[128];
    }

    public SIFTKeypoint ToSIFTKeypoint()
    {
        var descriptor = new float[128];
        var bucketWidth = 2 * MathF.PI / 8;

        var gradientsXCopy = new float[64];
        var gradientsXGaussian = new float[64];
        Buffer.BlockCopy(_gradientLocalNeighborhood, 0, gradientsXCopy, 0, gradientsXCopy.Length * sizeof(float));
        GaussianBlur.Blur(gradientsXCopy, gradientsXGaussian, 8, 8, 2);

        var gradientsYCopy = new float[64];
        var gradientsYGaussian = new float[64];
        Buffer.BlockCopy(_gradientLocalNeighborhood, gradientsYCopy.Length, gradientsXCopy, 0, gradientsYCopy.Length * sizeof(float));
        GaussianBlur.Blur(gradientsYCopy, gradientsYGaussian, 8, 8, 2);

        // Bucket gradients
        for (var quadrant = 0; quadrant < 4; quadrant++)
        {
            for (var r = 0; r < 4; r++)
            {
                for (var c = 0; c < 4; c++)
                {
                    var nextR = r;
                    var nextC = c;
                    if (quadrant is 1 or 3) nextR += 4;
                    if (quadrant is 2 or 3) nextC += 4;

                    var gradX = gradientsXGaussian[nextR * 8 + nextC];
                    var gradY = gradientsYGaussian[nextR * 8 + nextC];
                    if (gradX == 0) continue;

                    var orientation = PrincipalOrientations.GetGradientOrientation(gradX, gradY);
                    orientation %= 2 * MathF.PI;
                    while (orientation < 0) orientation += 2 * MathF.PI;

                    var bucket = (quadrant * descriptor.Length / 4) + (int)Math.Floor(orientation / bucketWidth);

                    descriptor[bucket]++;
                    descriptor[bucket + 1]++;
                }
            }
        }

        // Normalize vector
        var magnitude = MathF.Sqrt(descriptor.Sum(t => t * t));
        for (var i = 0; i < descriptor.Length; i++)
        {
            descriptor[i] /= magnitude;
        }

        return new SIFTKeypoint
        {
            Row = Row,
            Column = Column,
            Magnitude = Magnitude,
            Sigma = Sigma,
            PrincipalOrientation = PrincipalOrientation,
            Descriptor = descriptor,
        };
    }

    public (float, float) GetGradient(int r, int c)
    {
        return (_gradientLocalNeighborhood[r * 8 + c], _gradientLocalNeighborhood[r * 8 + c + 64]);
    }

    public void SetGradient(int r, int c, (float, float) gradient)
    {
        var (gradX, gradY) = gradient;
        _gradientLocalNeighborhood[r * 8 + c] = gradX;
        _gradientLocalNeighborhood[r * 8 + c + 64] = gradY;
    }

    public void CopyGradientsFrom(Keypoint other)
    {
        for (var i = 0; i < 64; i++)
        {
            _gradientLocalNeighborhood[i] = other._gradientLocalNeighborhood[i];
        }
    }
}