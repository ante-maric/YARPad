using CodingCell.YARPad.Hosting.LetsEncrypt;

namespace CodingCell.YARPad.Hosting;

public class YARPadProxyOptions
{
    public const string SECTION_NAME = "YARPadProxy";

    public bool IsLetsEncryptEnabled { get; set; }

    public string RootDataPath { get; set; } = ".";

    public LetsEncryptOptions LetsEncrypt { get; set; } = new();
}
