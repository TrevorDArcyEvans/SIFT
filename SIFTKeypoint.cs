namespace SIFT;

public class SIFTKeypoint : Keypoint
{
    public IReadOnlyList<float> Descriptor { get; init; }

    public SIFTKeypoint()
    {
        Descriptor = Array.Empty<float>();
    }

    private float CompareDescriptor(IEnumerable<float> other)
    {
        return MathF.Sqrt(Descriptor.Zip(other).Sum(dK => (dK.First - dK.Second) * (dK.First - dK.Second)));
    }

    public int GetClosestDescriptor(IReadOnlyList<IReadOnlyList<float>> descriptors)
    {
        var bestMatch = -1;
        var bestScore = float.MaxValue;
        for (var i = 0; i < descriptors.Count; i++)
        {
            var score = CompareDescriptor(descriptors[i]);
            if (score < bestScore)
            {
                bestMatch = i;
                bestScore = score;
            }
        }

        return bestMatch;
    }
}