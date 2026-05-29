namespace CodingCell.YARPad;

public static class EnumExtensions
{
    extension(Enum @enum)
    {
        public string HumanizeEnum() => @enum.ToString().Humanize(LetterCasing.Title);
    }
}
