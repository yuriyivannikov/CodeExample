using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Assets.System.Scripts.Localization;
using UIKit;
using UnityEngine;

public class POFileHelper
{
    #region Parse
    public static Dictionary<string, LocalizationStringElement> ParsedData { get; private set; }
    public static Func<int, int> EvaluationMethod { get; private set; }

    private static int _lineNum = 0;

    private static PluralFormExpressionParser _pluralFormExpressionParser;
    private static bool _isHeaderProcessed;

    public static Dictionary<string, LocalizationStringElement> Parse(string txtFromFile)
    {
        _lineNum = 0;
        ParsedData = new Dictionary<string, LocalizationStringElement>();

        if (string.IsNullOrEmpty(txtFromFile))
        {
            return ParsedData;
        }

        var msgpluralIndex = -1;
        string msgid = null;
        string msgstr = null;
        var msgstrPlural = new List<string>();

        var lines = txtFromFile.Split('\n');

        foreach (var line in lines)
        {
            _lineNum++;

            var trimmedLine = line.Trim();

            if (trimmedLine.Length == 0 || line[0] == '#') //empty or comment
            {
                continue;
            }

            if (trimmedLine.StartsWith("msgid "))
            {
                if (msgid == null && msgstr != null)
                    Debug.LogWarning("Found 2 consecutive msgid. Line: " + _lineNum);

                // A new msgid has been encountered, so commit the last one
                if (msgid != null && msgstr != null)
                {
                    AddLocalization(msgid, msgstr, msgpluralIndex > -1 ? msgstrPlural : null);
                }

                msgid = GetValue(trimmedLine);
                msgstr = null;
                msgstrPlural.Clear();
                msgpluralIndex = -1;

                continue;
            }

            if (trimmedLine.StartsWith("msgstr "))
            {
                if (msgid == null)
                    Debug.LogWarning("msgstr with no msgid. Line: " + _lineNum);

                msgstr = GetValue(trimmedLine);
                continue;
            }

            if (trimmedLine.StartsWith("msgid_plural"))
            {
                if (msgid == null)
                    Debug.LogWarning("msgid_plural with no msgid. Line: " + _lineNum);

                msgstr = GetValue(trimmedLine);
                continue;
            }

            if (trimmedLine.StartsWith("msgstr["))
            {
                if (msgid == null)
                    Debug.LogWarning("Plural msgstr with no msgid. Line: " + _lineNum);

                msgpluralIndex = Convert.ToInt32(trimmedLine.Split(']')[0].Substring("msgstr[".Length));

                //In case Strings ain't ordered in file
                for (int i = msgstrPlural.Count; i < msgpluralIndex; i++)
                    msgstrPlural.Add("");

                msgstrPlural.Add(GetValue(trimmedLine));
            }

            if (trimmedLine[0] == '"')
            {
                if (msgid == null && msgstr == null)
                {
                    Debug.LogWarning("Invalid format. Line: " + _lineNum);
                }

                var value = GetValue(trimmedLine).Replace("\\r", "\r").Replace("\\n", "\n");
                if (msgstr == null)
                {
                    msgid += value;
                }
                else
                {
                    msgstr += value;
                }
            }
        }

        if (msgid != null)
        {
            if (msgstr == null)
                Debug.LogWarning("Expecting msgstr. Line: " + _lineNum);

            AddLocalization(msgid, msgstr, msgpluralIndex > -1 ? msgstrPlural : null);
        }

        return ParsedData;
    }

    private static void AddLocalization(string msgid, string msgstr, List<string> msgstrPlural = null)
    {
        if (string.IsNullOrEmpty(msgid))
        {
            if (_isHeaderProcessed)
                Debug.LogWarning("Error: Found empty msgid - will skip it. Line: " + _lineNum);
            else
                ProcessHeader(msgstr);
        }
        else
        {
            var item = new LocalizationStringElement();
            item.Translation = msgstr;

            if (ParsedData.ContainsKey(msgid))
            {
                Debug.LogWarning(string.Format("Error: Found duplicate msgid {0} at line {1} - will overwrite the value from earlier instances.", msgid, _lineNum));
            }

            if (msgstrPlural != null && msgstrPlural.Count > 0)
                item.PluralTranslations = msgstrPlural;

            ParsedData[msgid] = item;
        }
    }

    private static void ProcessHeader(string header)
    {
        var pluralFormsIndex = header.IndexOf("nplurals=", StringComparison.InvariantCulture);
        if (pluralFormsIndex == -1)
        {
            Debug.LogWarning("No information about plural forms supplied");
            return;
        }
        var informationString = header.Substring(pluralFormsIndex).Replace(" ", string.Empty).Split(';');
        if (informationString.Length < 2)
        {
            Debug.LogError("Plural forms information has incorrect format");
            return;
        }
        //var pluralFormsAmount = Convert.ToInt32(informationString[0].SubString("nplurals=".Length));
        var pluralFormsExpression = informationString[1];

        if (_pluralFormExpressionParser == null)
            _pluralFormExpressionParser = new PluralFormExpressionParser();

        EvaluationMethod = _pluralFormExpressionParser.GetEvaluatingMethod(pluralFormsExpression);

        _isHeaderProcessed = true;
    }

    private static string Unescape(string escapedString)
    {
        var result = new StringBuilder();

        // System.Text.RegularExpressions.Regex.Unescape(result) would unescape many chars that
        // .po files don't escape (it escapes \, *, +, ?, |, {, [, (,), ^, $,., #, and white space), so I'm
        // doing it manually.

        var lastChar = '\0';
        var escapeCompleted = false;
        foreach (var currentChar in escapedString)
        {
            if (lastChar == '\\')
            {
                escapeCompleted = true;

                switch (currentChar)
                {
                    case 'n':
                    {
                        result.Append('\n');
                        break;
                    }
                    case 'r':
                    {
                        result.Append('\r');
                        break;
                    }
                    case 't':
                    {
                        result.Append('\t');
                        break;
                    }
                    case '\\':
                    {
                        result.Append('\\');
                        break;
                    }
                    case '"':
                    {
                        result.Append('\"');
                        break;
                    }
                    default:
                    {
                        escapeCompleted = false;

                        result.Append(lastChar);
                        result.Append(currentChar);
                        break;
                    }
                }
            }
            else if (currentChar != '\\')
            {
                result.Append(currentChar);
            }

            if (escapeCompleted)
            {
                lastChar = '\0';
                escapeCompleted = false;
            }
            else
            {
                lastChar = currentChar;
            }
        }

        return result.ToString();
    }

    private static string GetValue(string line)
    {
        var begin = line.IndexOf('"');
        if (begin == -1)
        {
            Debug.LogError(string.Format("No begin quote at line {0}: {1}", _lineNum, line));
            return "";
        }

        var end = line.LastIndexOf('"');
        if (end == -1 || begin == end)
        {
            Debug.LogError(string.Format("No closing quote at line {0}: {1}", _lineNum, line));
            return "";
        }

        return Unescape(line.Substring(begin + 1, end - begin - 1));
    }
    #endregion

    private static string Escape(string unescapedString)
    {
        var result = "";
        if (unescapedString != null)
        {
            foreach (var character in unescapedString)
            {
                switch (character)
                {
                    case '\n':
                    {
                        result += "\\n";
                        break;
                    }
                    case '\r':
                    {
                        result += "\\r";
                        break;
                    }
                    case '\t':
                    {
                        result += "\\t";
                        break;
                    }
                    case '\"':
                    {
                        result += "\\\"";
                        break;
                    }
                    case '\\':
                    {
                        result += "\\\\";
                        break;
                    }
                    default:
                    {
                        result += character;
                        break;
                    }
                }
            }
        }
        return result;
    }

    public static bool Save(string fullFilename, Dictionary<string, string> data)
    {
        var directoryPath = Path.GetDirectoryName(fullFilename);
        if (directoryPath == null)
        {
            return false;
        }

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        using (var file = File.CreateText(fullFilename))
        {
            foreach (var dataElement in data)
            {
                file.WriteLine("msgid \"{0}\"", dataElement.Key);
                file.WriteLine("msgstr \"{0}\"", Escape(dataElement.Value));
                file.WriteLine();
            }
        }
        return true;
    }
}