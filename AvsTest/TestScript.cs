using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AvsCommon;
using AvsTest.Exceptions;

namespace AvsTest
{
   public class TestScript
    {
        private readonly string _text;
        public string Name { get; private set; }
        private readonly List<List<TestParameter>> _parameters;
        private int _frame;
        private AccessType _accessType;


        public TestScript(string path)
        {
            _text = File.ReadAllText(path);
            Name = Path.GetFileNameWithoutExtension(path);
            _parameters = new List<List<TestParameter>>();
            ParseText();
        }

        //searching params in avs comments in format name: value, only one line per param
        //moving this to some json would simplify configuration a lot but probably less convenient for testers
        private void ParseText()
        {
            var lines = _text.Split(new[] {Environment.NewLine}, StringSplitOptions.None);
            //checking only comments
            for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
            {
                var line = lines[lineNumber];
                if (line.Length == 0 || !line.StartsWith("#"))
                {
                    continue;
                }
                var split = line.Split(new[] {':'}, 2, StringSplitOptions.RemoveEmptyEntries);
                if (split.Length != 2)
                {
                    continue;
                }
                 var paramValue = split[1].Trim();
                switch (split[0].TrimStart('#').Trim().ToLowerInvariant()) //parameter name
                {
                    case "test case":
                        var pairs = split[1].Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                        var testCase = new List<TestParameter>();
                        foreach (var pair in pairs)
                        {
                            var splittedPair = pair.Split(new[] {'='}, StringSplitOptions.RemoveEmptyEntries);
                            if (splittedPair.Length != 2)
                            {
                                throw new ParsingException(string.Format("Invalid parameter pair on line {0}: {1}",
                                    lineNumber, pair));
                            }
                            testCase.Add(new TestParameter
                            {
                                Name = splittedPair[0].Trim(),
                                Value = splittedPair[1].Trim()
                            });
                        }
                        _parameters.Add(testCase);
                        break;
                    case "frame":
                        if (!int.TryParse(paramValue, out _frame))
                        {
                            throw new ParsingException(string.Format("Invalid frame parameter on line {0}: {1}",
                                lineNumber, paramValue));
                        }
                        break;
                    case "access":
                        if (!Enum.TryParse(paramValue, out _accessType))
                        {
                            throw new ParsingException(string.Format("Invalid access time specified on line {0}: {1}",
                                lineNumber, paramValue));
                        }
                        break;
                    case "name":
                        Name = paramValue;
                        break;
                }
            }
        }

        public IEnumerable<TestCase> GetTestCases()
        {
            if (_parameters.Count == 0)
            {
                return new List<TestCase>
                {
                    new TestCase
                    {
                        AccessType = _accessType,
                        Frame = _frame,
                        Parameters = new List<TestParameter>(),
                        ScriptText = _text,
                        TestName = Name
                    }
                };
            }
            return _parameters.Select(testCase => new TestCase
            {
                AccessType = _accessType,
                Frame = _frame,
                ScriptText = PrepareScriptText(_text, testCase),
                TestName = Name,
                Parameters = testCase.AsReadOnly()
            });
        }

        private static string PrepareScriptText(string text, IEnumerable<TestParameter> parameters)
        {
            //we can assume that most variables will be replaced with regex rather than appended
            string variableDeclarations = "";
            string currentText = text;
            foreach (var param in parameters)
            {
                var pattern = string.Format(@"^\s*{0}\s*=\s*.+?\s*{1}", param.Name, Environment.NewLine);
                var regex = new Regex(pattern,
                    RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

                var newValue = string.Format("{0}={1}{2}", param.Name, param.Value, Environment.NewLine);

                if (regex.IsMatch(currentText))
                {
                    currentText = regex.Replace(currentText, newValue, 1);
                }
                else
                {
                    variableDeclarations += newValue;
                }
            }
            return variableDeclarations + currentText;
        }
    }
}