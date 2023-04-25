using System.Text;

public static class StringUtils
{
    public static string ToLength(this string self, int length)
    {
        if (self == null)
            return null;

        if (self.Length == length)
            return self;

        if (self.Length > length)
            return self.Substring(0, length);

        StringBuilder str = new StringBuilder();
        str.Append(self);

        while (str.Length != length)
            str.Append('\0');

        return str.ToString();
    }
}
