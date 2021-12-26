using SIFT;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

static async Task SaveImageWithKeypoints(string path, Image<L8> img, IEnumerable<Keypoint> keypoints)
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

// Download greyscale image to test with
using var http = new HttpClient();
http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:95.0) Gecko/20100101 Firefox/95.0");

// Davidwkennedy, CC BY-SA 3.0 <https://creativecommons.org/licenses/by-sa/3.0>, via Wikimedia Commons
await using var raw = await http.GetStreamAsync("https://upload.wikimedia.org/wikipedia/commons/3/3f/Bikesgray.jpg");

// Process SIFT image
var siftImage = SIFTImage.From(Image.Load<L8>(raw));
await siftImage.Image.SaveAsJpegAsync("original.jpg");

// Calculate SIFT descriptors
foreach (var keypoint in siftImage.Keypoints)
{
    Console.WriteLine(keypoint.Descriptor.Aggregate("[", (agg, next) => agg + " " + next) + "]");
}

Console.WriteLine("Total keypoints: {0}", siftImage.Keypoints.Count);

// Save the image with the final keypoint blobs
await SaveImageWithKeypoints("keypoints.jpg", siftImage.Image, siftImage.Keypoints);
