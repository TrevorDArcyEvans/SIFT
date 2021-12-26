using GradientDotNet;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SIFT;

public class SIFTImage
{
    public IReadOnlyList<SIFTKeypoint> Keypoints { get; private init; }

    private SIFTImage()
    {
        Keypoints = Array.Empty<SIFTKeypoint>();
    }

    private static float[] ImageSharpImageToArray(Image<L8> img)
    {
        var greyPixels = new float[img.Height * img.Width];
        for (var r = 0; r < img.Height; r++)
        {
            for (var c = 0; c < img.Width; c++)
            {
                greyPixels[r * img.Width + c] = img[c, r].PackedValue;
            }
        }

        return greyPixels;
    }

    public static SIFTImage From(Image<L8> img)
    {
        // Convert image to raw value array
        var greyImage = ImageSharpImageToArray(img);

        // Calculate DoG keypoints
        var keypoints = DifferenceOfGaussians.GetKeypoints(3, 5, greyImage, img.Height, img.Width);

        // Calculate image gradients
        var gradXImg = new float[img.Height * img.Width];
        var gradYImg = new float[img.Height * img.Width];
        Gradients.CentralDifference(greyImage, gradXImg, gradYImg, img.Height, img.Width);

        // Calculate the principal orientation for each keypoint
        PrincipalOrientations.Update(keypoints, gradXImg, gradYImg, img.Height, img.Width);

        return new SIFTImage
        {
            Keypoints = keypoints.Select(s => s.ToSIFTKeypoint()).ToList(),
        };
    }
}