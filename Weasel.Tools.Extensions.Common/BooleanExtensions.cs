namespace Weasel.Tools.Extensions.Common;

public static class BooleanExtensions
{
    public static string? ToYesNoString(this bool? flag, string? nullValue = "Не указано", string yesValue = "Да", string noValue = "Нет")
    {
        switch (flag)
        {
            case true:
                return yesValue;
            case false:
                return noValue;
            default:
                return nullValue;
        }
    }
}
