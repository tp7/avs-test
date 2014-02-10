using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace AvsTest
{
    internal class Options
    {
        [Option('r', "ref-dll", DefaultValue = "AviSynth.dll", HelpText="Path to reference AviSynth.dll")]
        public string StableAvs { get; set; }

        [Option('t', "test-dll", Required = true, HelpText="Path to AviSynth.dll under test")]
        public string TestAvs { get; set; }

        [Option('s', "scripts", Required = true, HelpText="Path to folder with test scripts")]
        public string TestScripsFolder { get; set; }

        //syntax is --include herp:derp:hurr:durr
        [OptionList("include", HelpText = "Include only specific tests", MutuallyExclusiveSet = "includes")]
        public IList<string> Include { get; set; }

        [OptionList("exclude", HelpText = "Exclude specific tests", MutuallyExclusiveSet = "includes")]
        public IList<string> Exclude { get; set; }

        [ParserState]
        public IParserState LastParsingState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Heading = new HeadingInfo("Avisynth black-box testing app", "v0.000000001"),
                Copyright = new CopyrightInfo("Victor Efimov", 2014),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };
            help.AddPreOptionsLine(
                @"Usage: AvsTest --stable-dll C:\stable\avisynth.dll --ref-dll C:\unstable\avisynth.dll --scripts C:\scripts");

            if (LastParsingState.Errors.Count > 0)
            {
                var errors = help.RenderParsingErrorsText(this, 2); // indent with two spaces
                if (!string.IsNullOrEmpty(errors))
                {
                    help.AddPreOptionsLine(string.Concat(Environment.NewLine, "ERROR(S):"));
                    help.AddPreOptionsLine(errors);
                }
            }
            else
            {
                help.AddOptions(this);
            }

            return help;
        }
    }
}