namespace SIFT.UI.CLI;

using CommandLine;

internal sealed class Options
{
  [Value(index: 0, Required = true, HelpText = "Path to image file 1")]
  public string ImageFile1Path { get; set; }

  [Value(index: 1, Required = true, HelpText = "Path to image file 2")]
  public string ImageFile2Path { get; set; }
}
