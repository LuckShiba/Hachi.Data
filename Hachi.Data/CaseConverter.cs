using System.Text;
using System.Text.RegularExpressions;

namespace Hachi.Data;

public static class CaseConverter
{
    public static string ToPascalCase(string input)
    {
        input = Sanitize(input);
        
        var sb = new StringBuilder();
        var nextToUpper = true;
        
        foreach (var c in input)
        {
            if (c == '_')
            {
                nextToUpper = true;
                continue;
            }

            if (char.IsDigit(c))
            {
                if (sb.Length == 0)
                    sb.Append('_');
                
                sb.Append(c);
                continue;
            }
            
            sb.Append(nextToUpper ? char.ToUpper(c) : c);
            nextToUpper = false;
        }

        return sb.ToString();
    }
    
    private static string Sanitize(string input)
    {
        return ReplaceCharactersRegex.Replace(input, "_");
    }
    
    private static Regex ReplaceCharactersRegex => new("[^a-zA-Z0-9-]");
}