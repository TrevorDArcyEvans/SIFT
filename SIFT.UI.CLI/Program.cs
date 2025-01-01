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
    foreach (var inputFile in opt.InputFiles)
    {
      using var img = await Image.LoadAsync<L8>(inputFile);
      using var sift = SIFTImage.From(img);

      AddKeypoints(sift);
      
      var imgFileName = System.IO.Path.GetFileNameWithoutExtension(inputFile);
      var outFileName = $"keypoints-{imgFileName}.jpg";
      await sift.Image.SaveAsJpegAsync(outFileName);

      Console.WriteLine($"{imgFileName} --> {outFileName}");
    }
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
