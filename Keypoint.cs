namespace SIFT;

public class Keypoint
{
    public int Row { get; init; }

    public int Column { get; init; }

    public float Magnitude { get; set; }

    public float Sigma { get; init; }

    public float PrincipalOrientation { get; set; }
}