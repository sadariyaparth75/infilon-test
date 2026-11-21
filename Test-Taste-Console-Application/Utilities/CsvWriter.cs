using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Test_Taste_Console_Application.Utilities
{
    ///<summary>
    /// Writes simple RFC 4180 CSV files (comma separated, values quoted when needed).
    ///</summary>
    public static class CsvWriter
    {
        private static readonly char[] CharactersThatNeedQuoting = { ',', '"', '\r', '\n' };

        public static void Write(string filePath, IEnumerable<string> header, IEnumerable<IEnumerable<string>> rows)
        {
            var stringBuilder = new StringBuilder();
            AppendRow(stringBuilder, header);

            foreach (var row in rows)
            {
                AppendRow(stringBuilder, row);
            }

            //UTF-8 with BOM so spreadsheet tools show French names with accents (e.g. "Amalthée") correctly.
            File.WriteAllText(filePath, stringBuilder.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        }

        private static void AppendRow(StringBuilder stringBuilder, IEnumerable<string> values)
        {
            stringBuilder.AppendLine(string.Join(",", values.Select(Escape)));
        }

        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            //A value with a separator, quote or line break is wrapped in quotes; quotes inside are doubled.
            return value.IndexOfAny(CharactersThatNeedQuoting) >= 0
                ? $"\"{value.Replace("\"", "\"\"")}\""
                : value;
        }
    }
}
