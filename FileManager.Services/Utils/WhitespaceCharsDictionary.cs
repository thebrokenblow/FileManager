namespace FileManager.Services.Utils;

public class WhitespaceCharsDictionary
{
    public const char Space = ' ';
    public const char Tab = '\t';
    public const char NewLine = '\n';
    public const char CarriageReturn = '\r';
    public const char FormFeed = '\f';
    public const char VerticalTab = '\v';

    public static readonly char[] AllWhitespace = [Space, Tab, NewLine, CarriageReturn, FormFeed, VerticalTab];
    public static readonly char[] SpaceAndTab = [Space, Tab];
}