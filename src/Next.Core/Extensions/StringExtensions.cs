using System.Linq;

namespace System.Collections.Generic
{
    public static class StringExtensions
    {
        public static string ToCamelCase(this string s) 
        {
            return char.ToLowerInvariant(s[0]) + s[1..];
        }
        
        public static string RemoveWhitespace(this string s)
        {
            return new(s.ToCharArray()
                .Where(c => !char.IsWhiteSpace(c))
                .ToArray());
        }
        
        public static string ToSnakeCaseNoSpan(this string str)
        {
            return string.Concat(
                str.Select(
                    (x, i) => i > 0 && char.IsUpper(x)
                        ? "_" + x
                        : x.ToString()
                )
            ).ToLower();
        }
        
        /// <summary>
        /// Added in version from https://stackoverflow.com/a/57535237 with some tidying
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToSnakeCase(this string str) {
            if (str == null)
            {
                return string.Empty;
            }

            var upperCaseLength = str.Count(t => t >= 'A' && t <= 'Z' && t != str[0]);
            var bufferSize = str.Length + upperCaseLength;
            Span<char> buffer = new char[bufferSize];
            var bufferPosition = 0;
            var namePosition = 0;
            while (bufferPosition < buffer.Length)
            {
                if (namePosition > 0 && str[namePosition] >= 'A' && str[namePosition] <= 'Z')
                {
                    buffer[bufferPosition] = '_';
                    buffer[bufferPosition + 1] = str[namePosition];
                    bufferPosition += 2;
                    namePosition++;
                    continue;
                }
                buffer[bufferPosition] = str[namePosition];
                bufferPosition++;
                namePosition++;
            }

            return new string(buffer).ToLower();
        }
        
        public static string ToSnakeCaseAsSpan(this string str)
        {
            const char separator = '_';
            
            if (str == null)
            {
                return string.Empty;
            }

            var strSpan = str.AsSpan();

            var bufferSize = str.Length + str.Count(t => t >= 'A' && t <= 'Z' && t != str[0]);
            Span<char> buffer = new char[bufferSize];
            var bufferPosition = 0;
            var namePosition = 0;
            
            while (bufferPosition < buffer.Length)
            {
                if (namePosition > 0 && strSpan[namePosition] >= 'A' && strSpan[namePosition] <= 'Z')
                {
                    buffer[bufferPosition] = separator;
                    buffer[bufferPosition + 1] = strSpan[namePosition];
                    bufferPosition += 2;
                    namePosition++;
                    continue;
                }
                buffer[bufferPosition] = strSpan[namePosition];
                bufferPosition++;
                namePosition++;
            }

            return new string(buffer).ToLower();
        }
    }
}