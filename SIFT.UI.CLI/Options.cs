namespace SIFT.UI.CLI;

using CommandLine;

internal sealed class Options
{
  [Value(index: 0, Required = true, HelpText = "Paths to image files to be processed.")]
  public IEnumerable<string> InputFiles { get; set; }
}
