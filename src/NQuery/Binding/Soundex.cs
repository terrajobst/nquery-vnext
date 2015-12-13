using System;

namespace NQuery.Binding
{
    // From https://en.wikipedia.org/wiki/Soundex:
    //
    //    1. Retain the first letter of the name and drop all other occurrences of:
    //    
    //             a, e, i, o, u, y, h, w
    //    
    //    2. Replace consonants with digits as follows(after the first letter):
    //    
    //             b, f, p, v             -> 1
    //             c, g, j, k, q, s, x, z -> 2
    //             d, t                   -> 3
    //             l                      -> 4
    //             m, n                   -> 5
    //             r                      -> 6
    //    
    //    3. If two or more letters with the same number are adjacent in the original name
    //       (before step 1), only retain the first letter; also two letters with the same
    //       number separated by 'h' or 'w' are coded as a single number, whereas such
    //       letters separated by a vowel are coded twice. This rule also applies to the
    //       first letter.
    //    
    //    4. If you have too few letters in your word that you can't assign three numbers,
    //       append with zeros until there are three numbers. If you have more than 3
    //       letters, just retain the first 3 numbers.
    internal static class Soundex
    {
        public static string GetCode(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "0000";

            var result = new[] { char.ToUpperInvariant(text[0]), '0', '0', '0'};
            var count = 1;
            var lastCode = Encode(result[0]);
            var lastLetter = result[0];

            for (var i = 1; i < text.Length; i++)
            {
                var letter = char.ToUpperInvariant(text[i]);
                if (letter < 'A' || 'Z' < letter)
                    break;

                var codedLetter = Encode(letter);
                if (!char.IsDigit(codedLetter))
                {
                    // Drop occurrence
                    lastLetter = letter;
                    continue;
                }

                if (lastCode == codedLetter)
                {
                    // If two or more letters with the same number are adjacent in the original name
                    // only retain the first letter;
                    //
                    // Two letters with the same number separated by 'h' or 'w' are
                    // coded as a single number, whereas such letters separated by
                    // a vowel are coded twice.

                    if (lastLetter == 'H' || lastLetter == 'W' || !IsVowel(lastLetter))
                        continue;
                }

                result[count++] = codedLetter;
                lastLetter = letter;
                lastCode = codedLetter;

                if (count == 4)
                    break;
            }

            return new string(result);
        }

        private static bool IsVowel(char c)
        {
            var encoded = Encode(c);
            return encoded != '\0' && !char.IsDigit(encoded);
        }

        private static char Encode(char c)
        {
            switch (c)
            {
                case 'A': case 'E': case 'I': case 'O': case 'U': case 'Y': case 'H': case 'W':
                    return c;
                case 'B': case 'F': case 'P': case 'V':
                    return '1';
                case 'C': case 'G': case 'J': case 'K': case 'Q': case 'S': case 'X': case 'Z':
                    return '2';
                case 'D': case 'T':
                    return '3';
                case 'L':
                    return '4';
                case 'M': case 'N':
                    return '5';
                case 'R':
                    return '6';
                default:
                    return '\0';
            }
        }
    }
}