using GradientDotNet;
using SIFT;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

static async Task SaveImageWithKeypoints(string path, Image<L8> img, IList<Keypoint> keypoints)
{
    var pen = Pens.Solid(Color.White, 1);

    img.Mutate(x =>
    {
        foreach (var kp in keypoints)
        {
            if (kp.Row - kp.Sigma < 0 || kp.Row + kp.Sigma >= img.Height || kp.Column - kp.Sigma < 0 ||
                kp.Column + kp.Sigma >= img.Width) continue;

            var circleDiameter = kp.Sigma * 2;
            var circle = new EllipsePolygon(kp.Column, kp.Row, circleDiameter, circleDiameter);
            x.Draw(pen, circle);
            x.DrawLines(Color.White, 1, new PointF(kp.Column, kp.Row),
                new PointF(kp.Column + kp.Sigma * MathF.Cos(kp.PrincipalOrientation), kp.Row + kp.Sigma * MathF.Sin(kp.PrincipalOrientation)));
        }
    });

    await img.SaveAsJpegAsync(path);
}

static float[] ImageSharpImageToArray(Image<L8> img)
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

static async Task SaveAsMagnitudesImage(string path, IReadOnlyList<float> gradX, IReadOnlyList<float> gradY, int rows, int cols)
{
    var magImg = new Image<L8>(cols, rows);
    for (var r = 0; r < rows; r++)
    {
        for (var c = 0; c < cols; c++)
        {
            magImg[c, r] = new L8((byte)Math.Sqrt(gradX[r * cols + c] * gradX[r * cols + c] +
                                                  gradY[r * cols + c] * gradY[r * cols + c]));
        }
    }
    await magImg.SaveAsJpegAsync(path);
}

static float CompareSIFTDescriptors(IEnumerable<float> d0, IEnumerable<float> d1)
{
    return MathF.Sqrt(d0.Zip(d1).Sum(dK => (dK.First - dK.Second) * (dK.First - dK.Second)));
}

// Download greyscale image to test with
using var http = new HttpClient();
http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:95.0) Gecko/20100101 Firefox/95.0");

// Davidwkennedy, CC BY-SA 3.0 <https://creativecommons.org/licenses/by-sa/3.0>, via Wikimedia Commons
await using var raw = await http.GetStreamAsync("https://upload.wikimedia.org/wikipedia/commons/3/3f/Bikesgray.jpg");

using var img = Image.Load<L8>(raw);

await img.SaveAsJpegAsync("original.jpg");

// Convert image to raw value array
var greyImage = ImageSharpImageToArray(img);

// Calculate DoG keypoints
var keypoints = DifferenceOfGaussians.GetKeypoints(3, 5, greyImage, img.Height, img.Width);

// Calculate image gradients
var gradXImg = new float[img.Height * img.Width];
var gradYImg = new float[img.Height * img.Width];
Gradients.CentralDifference(greyImage, gradXImg, gradYImg, img.Height, img.Width);

await SaveAsMagnitudesImage("gradient.jpg", gradXImg, gradYImg, img.Height, img.Width);

// Calculate the principal orientation for each keypoint
PrincipalOrientations.Update(keypoints, gradXImg, gradYImg, img.Height, img.Width);

// Calculate SIFT descriptors
foreach (var keypoint in keypoints)
{
    var descriptor = keypoint.ToSIFTDescriptor();
    Console.WriteLine(descriptor.Aggregate("[", (agg, next) => agg + " " + next) + "]");
}

Console.WriteLine("Total keypoints: {0}", keypoints.Count);

// Save the image with the final keypoint blobs
await SaveImageWithKeypoints("keypoints.jpg", img, keypoints);
