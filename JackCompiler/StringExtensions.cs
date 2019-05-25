public static class StringExtensions
{
    public static string ChopRight(this string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        
        return  value.Substring(0, value.Length - 1);
    }

    public static string ChopLeft(this string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        
        return value.Substring(1, value.Length-1);
    }
}