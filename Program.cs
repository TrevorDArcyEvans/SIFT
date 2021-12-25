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

            var circleDiameter = (float)kp.Sigma * 2;
            var circle = new EllipsePolygon(kp.Column, kp.Row, circleDiameter, circleDiameter);
            x.Draw(pen, circle);
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

Console.WriteLine("Total keypoints: {0}", keypoints.Count);

// Save the image with the final keypoint blobs
await SaveImageWithKeypoints("keypoints.jpg", img, keypoints);
