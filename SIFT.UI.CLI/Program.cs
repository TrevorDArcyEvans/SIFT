namespace SIFT.UI.CLI;

using CommandLine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Path = SixLabors.ImageSharp.Drawing.Path;

internal static class Program
{
  public static async Task Main(string[] args)
  {
    var result = await Parser.Default.ParseArguments<Options>(args)
      .WithParsedAsync(Run);
    await result.WithNotParsedAsync(HandleParseError);
  }

  private static async Task Run(Options opt)
  {
    using var img1 = await Image.LoadAsync<L8>(opt.ImageFile1Path);
    using var img2 = await Image.LoadAsync<L8>(opt.ImageFile2Path);
    using var sift1 = SIFTImage.From(img1);
    using var sift2 = SIFTImage.From(img2);

    AddKeypoints(sift1);
    AddKeypoints(sift2);

    using var outImage = new Image<L8>(sift1.Image.Width + sift2.Image.Width, Math.Max(sift1.Image.Height, sift2.Image.Height));
    outImage.Mutate(x =>
    {
      x.DrawImage(sift1.Image, new Point(0, 0), 1.0f);
      x.DrawImage(sift2.Image, new Point(sift1.Image.Width, 0), 1.0f);
    });

    var img1FileName = System.IO.Path.GetFileNameWithoutExtension(opt.ImageFile1Path);
    var img2FileName = System.IO.Path.GetFileNameWithoutExtension(opt.ImageFile2Path);
    var outFileName = $"comparison-{img1FileName}-vs-{img2FileName}.jpg";
    await outImage.SaveAsJpegAsync(outFileName);

    Console.WriteLine($"ImageFile1 = {opt.ImageFile1Path}");
    Console.WriteLine($"ImageFile2 = {opt.ImageFile2Path}");
    Console.WriteLine($"  Compare  = {outFileName}");
  }

  private static void AddKeypoints(SIFTImage img)
  {
    var pen = Pens.Solid(Color.White, 1);

    img.Image.Mutate(x =>
    {
      foreach (var kp in img.Keypoints)
      {
        if (kp.Row - kp.Sigma < 0 ||
            kp.Row + kp.Sigma >= img.Image.Height ||
            kp.Column - kp.Sigma < 0 ||
            kp.Column + kp.Sigma >= img.Image.Width)
        {
          continue;
        }

        var circleDiameter = kp.Sigma * 2;
        var circle = new EllipsePolygon(kp.Column, kp.Row, circleDiameter, circleDiameter);
        x.Draw(pen, circle);
        x.Draw(pen,
          new Path(new LinearLineSegment(new PointF(kp.Column, kp.Row),
            new PointF(kp.Column + kp.Sigma * MathF.Cos(kp.PrincipalOrientation), kp.Row + kp.Sigma * MathF.Sin(kp.PrincipalOrientation)))));
      }
    });
  }

  private static Task HandleParseError(IEnumerable<Error> errs)
  {
    if (errs.IsVersion())
    {
      Console.WriteLine("Version Request");
      return Task.CompletedTask;
    }

    if (errs.IsHelp())
    {
      Console.WriteLine("Help Request");
      return Task.CompletedTask;
      ;
    }

    Console.WriteLine("Parser Fail");
    return Task.CompletedTask;
  }
}
