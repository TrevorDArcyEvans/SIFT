namespace SIFT;

public class Keypoint
{
    public int Row { get; init; }

    public int Column { get; init; }

    public float Magnitude { get; set; }

    public double Sigma { get; init; }
}