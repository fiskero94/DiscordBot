using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace SurvivioData
{
    public static class ExportReader
    {
        public static ExportCollection Read()
        {
            WebClient client = new WebClient();
            string survivio = client.DownloadString("http://surviv.io/js/app.5857a9f2.js");
            string exports = GetExports(survivio);
            return new ExportCollection(ReadCollection(exports), "exports");
        }

        private static string GetExports(string survivio)
        {
            Regex regex = new Regex("(exports)");
            Match match = regex.Match(survivio);
            string exports = survivio.Substring(match.Index);
            int startIndex = FindSeperatorStartIndex(exports, '{');
            int endIndex = FindSeperatorEndIndex(exports, '{', '}');
            return exports.Substring(startIndex, endIndex - startIndex);
        }

        private static Dictionary<string, object> ReadCollection(string exports)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            int index = 0;
            char[] characters = exports.ToCharArray();
            for (int i = 0; i < exports.Length; i++)
            {
                if (characters[i] == ':')
                {
                    data.Add(exports.Substring(index, i - index), ReadValue(exports.Substring(i + 1), out int charactersRead));
                    index += i - index + charactersRead + 1;
                    i = index;
                }
            }
            return data;
        }

        private static object ReadValue(string exports, out int charactersRead)
        {
            char[] characters = exports.ToCharArray();
            if (characters[0] == '{')
            {
                int endIndex = FindSeperatorEndIndex(exports, '{', '}');
                charactersRead = endIndex + 2;
                return ReadCollection(exports.Substring(1, endIndex - 1));
            }
            else if (characters[0] == '[')
            {
                int endIndex = FindSeperatorEndIndex(exports, '[', ']');
                charactersRead = endIndex + 2;
                string[] valueStrings = exports.Substring(1, endIndex - 1).Split(',');
                object[] values = new object[valueStrings.Count()];
                for (int i = 0; i < valueStrings.Count(); i++)
                    values[i] = ParseValue(valueStrings[i]);
                return values;
            }
            else
            {
                for (int i = 0; i < exports.Length; i++)
                {
                    if (characters[i] == ',')
                    {
                        charactersRead = i + 1;
                        return ParseValue(exports.Substring(0, i));
                    }
                }
                charactersRead = exports.Length;
                return ParseValue(exports);
            }
        }

        private static object ParseValue(string str)
        {
            if (str.Length == 2 && str.StartsWith("!") && (str.ToCharArray()[1] == '0' || str.ToCharArray()[1] == '1'))
                return str.ToCharArray()[1] == '0';
            if (double.TryParse(str, out double result))
                return result;
            else
                return str.Trim('\"');
        }

        public static int FindSeperatorStartIndex(string str, char startSeperator)
        {
            char[] characters = str.ToCharArray();
            for (int i = 0; i < str.Length; i++)
                if (characters[i] == startSeperator)
                    return i + 1;
            return -1;
        }

        public static int FindSeperatorEndIndex(string str, char startSeperator, char endSeperator)
        {
            int brackets = 0;
            bool firstFound = false;
            char[] characters = str.ToCharArray();
            for (int i = 0; i < str.Length; i++)
            {
                if (characters[i] == startSeperator)
                {
                    brackets += 1;
                    firstFound = true;
                }
                else if (characters[i] == endSeperator)
                    brackets -= 1;
                if (firstFound && brackets == 0)
                    return i;
            }
            return -1;
        }
    }
}