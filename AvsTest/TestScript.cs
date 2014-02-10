using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AvsCommon;
using AvsCommon.Enums;
using AvsTest.Exceptions;

namespace AvsTest
{
    public class TestCaseData
    {
        public string Name { get; set; }
        public string ImageName { get; set; }
        public TestKind Kind { get; set; }
        public int SkipFrames { get; set; }
        public int FrameCount { get; set; }
        public List<TestParameter> Variables { get; set; }
    }

   public class TestScript
    {
        private readonly string _text;
        public string Name { get; private set; }
        private readonly List<TestCaseData> _parameters;
        private int _frame;
        private AccessType _accessType;


        public TestScript(string path)
        {
            _text = File.ReadAllText(path);
            Name = Path.GetFileNameWithoutExtension(path);
            _parameters = new List<TestCaseData>();
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
                switch (split[0].TrimStart(new [] {'#', '~'}).Trim().ToLowerInvariant()) //parameter name
                {
                    case "test case":
                        if (paramValue.StartsWith("{{"))
                        {
                            if (!paramValue.EndsWith("}}"))
                            {
                                throw new ParsingException(
                                    string.Format("Invalid test case at line {0}: no closing }} found. " +
                                                  "Multiline test cases are not yet supported", lineNumber));
                            }
                            _parameters.Add(ParseTestCase(paramValue.TrimStart('{').TrimEnd('}'), true));
                        }
                        else
                        {
                            _parameters.Add(ParseTestCase(paramValue, false));
                        }
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

       private static TestCaseData ParseTestCase(string value, bool complex)
       {
           var td = new TestCaseData();
           if (complex)
           {
               var split = value.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
               foreach (var pair in split)
               {
                   var paramArray = pair.Split(new[] {':'}, 2, StringSplitOptions.RemoveEmptyEntries);
                   if (paramArray.Length != 2)
                   {
                       throw new ParsingException(string.Format("Invalid param value: {0}", pair));
                   }
                   var paramName = paramArray[0].Trim().ToLowerInvariant();
                   var paramValue = paramArray[1].Trim();
                   switch (paramName)
                   {
                       case "name":
                           td.Name = paramValue;
                           break;
                       case "img-name":
                       case "image-name":
                           td.ImageName = paramValue;
                           break;
                       case "vars":
                       case "variables":
                           td.Variables = ParseVariablesString(paramValue);
                           break;
                       case "kind":
                           td.Kind = paramValue == "fps" || paramValue == "perf" || paramValue == "performance"
                               ? TestKind.Performance
                               : TestKind.Correctness;
                           break;
                       case "frames":
                           int frames;
                           if (!int.TryParse(paramValue, out frames))
                           {
                               throw new ParsingException(string.Format("Invalid frames value, should be integer: {0}", paramValue));
                           }
                           td.FrameCount = frames;
                           break;
                       case "skip":
                           int skip;
                           if (!int.TryParse(paramValue, out skip))
                           {
                               throw new ParsingException(string.Format("Invalid skip value, should be integer: {0}", paramValue));
                           }
                           td.SkipFrames = skip;
                           break;
                       default:
                           throw new ParsingException(string.Format("Unknown parameter: {0}", paramName));
                   }
               }
           }
           else
           {
               td.Variables = ParseVariablesString(value);
           }
           return td;
       }

       private static List<TestParameter> ParseVariablesString(string value)
       {
           var pairs = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
           var parameters = new List<TestParameter>();
           foreach (var pair in pairs)
           {
               var splittedPair = pair.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
               if (splittedPair.Length != 2)
               {
                   throw new ParsingException(string.Format("Invalid parameter pair: {0}", pair));
               }
               parameters.Add(new TestParameter
               {
                   Name = splittedPair[0].Trim(),
                   Value = splittedPair[1].Trim()
               });
           }
           return parameters;
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
                        TestName = Name,
                        Kind = TestKind.Correctness
                    }
                };
            }
            return _parameters.Select(testCase => new TestCase
            {
                AccessType = _accessType,
                Frame = _frame,
                ScriptText = PrepareScriptText(_text, testCase.Variables),
                TestName = testCase.Name ?? Name,
                ImageName = testCase.ImageName,
                Parameters = testCase.Variables.AsReadOnly(),
                FrameCount = testCase.FrameCount,
                SkipFirst = testCase.SkipFrames,
                Kind = testCase.Kind
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