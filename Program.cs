using System.Text.RegularExpressions;
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
await using var raw0 = await http.GetStreamAsync("https://upload.wikimedia.org/wikipedia/commons/3/3f/Bikesgray.jpg");
var img0 = Image.Load<L8>(raw0);
img0.Mutate(x => x.Crop(new Rectangle(0, 0, 480, 480)));

await using var raw1 = await http.GetStreamAsync("https://upload.wikimedia.org/wikipedia/commons/3/3f/Bikesgray.jpg");
var img1 = Image.Load<L8>(raw1);
img1.Mutate(x =>
{
    x.Crop(new Rectangle(160, 0, 480, 480));
    x.Rotate(12);
});

// Process SIFT images
var siftImage0 = SIFTImage.From(img0);
await siftImage0.Image.SaveAsJpegAsync("original_left.jpg");

var siftImage1 = SIFTImage.From(img1);
await siftImage1.Image.SaveAsJpegAsync("original_right.jpg");

// Save the images with the keypoint blobs
await SaveImageWithKeypoints("keypoints_left.jpg", siftImage0.Image, siftImage0.Keypoints);
await SaveImageWithKeypoints("keypoints_right.jpg", siftImage1.Image, siftImage1.Keypoints);

// Estimate affine transformation
var transformation = siftImage0.MatchWith(siftImage1);

Console.WriteLine("Estimated transformation:");
Console.WriteLine($"\tScale: {transformation.Scale}");
Console.WriteLine($"\tdX: {transformation.TranslationX}");
Console.WriteLine($"\tdY: {transformation.TranslationY}");
Console.WriteLine($"\tRotation: {transformation.Rotation * 180 / MathF.PI}*");

// Correct affine transformation
siftImage1.Image.Mutate(x =>
{
    x.Rotate(transformation.Rotation * 180 / MathF.PI);
    x.Resize((int)(siftImage1.Image.Width * transformation.Scale), (int)(siftImage1.Image.Height * transformation.Scale));
});

await siftImage1.Image.SaveAsJpegAsync("corrected_right.jpg");

// Merge aligned images
using var outImage = new Image<L8>((int)(siftImage0.Image.Width - transformation.TranslationX), (int)(siftImage0.Image.Height - transformation.TranslationY));
outImage.Mutate(x =>
{
    x.DrawImage(siftImage0.Image, new Point(0, 0), 1.0f);
    x.DrawImage(siftImage1.Image, new Point(-(int)transformation.TranslationX, -(int)transformation.TranslationY), 1.0f);
});

await outImage.SaveAsJpegAsync("corrected.jpg");