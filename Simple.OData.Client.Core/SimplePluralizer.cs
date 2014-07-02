﻿using System;
using System.Collections.Generic;

namespace Simple.OData.Client
{
    // Based on gist by Juliën Hanssens
    // https://gist.github.com/hanssens/2835960
    public class SimplePluralizer : IPluralizer
    {
        static Dictionary<string, Word> _specialSingulars;
        static Dictionary<string, Word> _specialPlurals;
        static List<SuffixRule> _suffixRules;

        static SimplePluralizer()
        {
            PopulateLookupTables();
            PopulateSuffixRules();
        }

        public string Pluralize(string noun)
        {
            return AdjustCase(ToPluralInternal(noun), noun);
        }

        public string Singularize(string noun)
        {
            return AdjustCase(ToSingularInternal(noun), noun);
        }

        public bool IsNounPluralOfNoun(string plural, string singular)
        {
            return String.Compare(ToSingularInternal(plural), singular, StringComparison.OrdinalIgnoreCase) == 0;
        }

        static readonly string[] _specialWordsStringTable = 
        {
            "agendum", "agenda", "",
            "albino", "albinos", "",
            "alga", "algae", "",
            "alumna", "alumnae", "",
            "apex", "apices", "apexes",
            "archipelago", "archipelagos", "",
            "bacterium", "bacteria", "",
            "beef", "beefs", "beeves",
            "bison", "", "",
            "brother", "brothers", "brethren",
            "candelabrum", "candelabra", "",
            "carp", "", "",
            "casino", "casinos", "",
            "child", "children", "",
            "chassis", "", "",
            "chinese", "", "",
            "clippers", "", "",
            "cod", "", "",
            "codex", "codices", "",
            "commando", "commandos", "",
            "corps", "", "",
            "cortex", "cortices", "cortexes",
            "cow", "cows", "kine",
            "criterion", "criteria", "",
            "datum", "data", "",
            "debris", "", "",
            "diabetes", "", "",
            "ditto", "dittos", "",
            "djinn", "", "",
            "dynamo", "", "",
            "elk", "", "",
            "embryo", "embryos", "",
            "ephemeris", "ephemeris", "ephemerides",
            "erratum", "errata", "",
            "extremum", "extrema", "",
            "fiasco", "fiascos", "",
            "fish", "fishes", "fish",
            "flounder", "", "",
            "focus", "focuses", "foci",
            "fungus", "fungi", "funguses",
            "gallows", "", "",
            "genie", "genies", "genii",
            "ghetto", "ghettos", "",
            "graffiti", "", "",
            "headquarters", "", "",
            "herpes", "", "",
            "homework", "", "",
            "index", "indices", "indexes",
            "inferno", "infernos", "",
            "japanese", "", "",
            "jumbo", "jumbos", "",
            "latex", "latices", "latexes",
            "lingo", "lingos", "",
            "mackerel", "", "",
            "macro", "macros", "",
            "manifesto", "manifestos", "",
            "measles", "", "",
            "money", "moneys", "monies",
            "mongoose", "mongooses", "mongoose",
            "mumps", "", "",
            "murex", "murecis", "",
            "mythos", "mythos", "mythoi",
            "news", "", "",
            "octopus", "octopuses", "octopodes",
            "ovum", "ova", "",
            "ox", "ox", "oxen",
            "photo", "photos", "",
            "pincers", "", "",
            "pliers", "", "",
            "pro", "pros", "",
            "rabies", "", "",
            "radius", "radiuses", "radii",
            "rhino", "rhinos", "",
            "salmon", "", "",
            "scissors", "", "",
            "series", "", "",
            "shears", "", "",
            "silex", "silices", "",
            "simplex", "simplices", "simplexes",
            "soliloquy", "soliloquies", "soliloquy",
            "species", "", "",
            "stratum", "strata", "",
            "swine", "", "",
            "trout", "", "",
            "tuna", "", "",
            "vertebra", "vertebrae", "",
            "vertex", "vertices", "vertexes",
            "vortex", "vortices", "vortexes",
        };

        static readonly string[] _suffixRulesStringTable = 
        {
            "ch", "ches",
            "sh", "shes",
            "ss", "sses",
            "ay", "ays",
            "ey", "eys",
            "iy", "iys",
            "oy", "oys",
            "uy", "uys",
            "y", "ies",
            "ao", "aos",
            "eo", "eos",
            "io", "ios",
            "oo", "oos",
            "uo", "uos",
            "o", "oes",
            "cis", "ces",
            "sis", "ses",
            "xis", "xes",
            "louse", "lice",
            "mouse", "mice",
            "zoon", "zoa",
            "man", "men",
            "deer", "deer",
            "fish", "fish",
            "sheep", "sheep",
            "itis", "itis",
            "ois", "ois",
            "pox", "pox",
            "ox", "oxes",
            "foot", "feet",
            "goose", "geese",
            "tooth", "teeth",
            "alf", "alves",
            "elf", "elves",
            "olf", "olves",
            "arf", "arves",
            "leaf", "leaves",
            "nife", "nives",
            "life", "lives",
            "wife", "wives",
        };

        class Word
        {
            public readonly string Singular;
            public readonly string Plural;
            public Word(string singular, string plural, string plural2)
            {
                Singular = singular;
                Plural = plural;
            }
        }

        class SuffixRule
        {
            readonly string _singularSuffix;
            readonly string _pluralSuffix;
            public SuffixRule(string singular, string plural)
            {
                _singularSuffix = singular;
                _pluralSuffix = plural;
            }
            public bool TryToPlural(string word, out string plural)
            {
                if (word.EndsWith(_singularSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    plural = word.Substring(0, word.Length - _singularSuffix.Length) + _pluralSuffix;
                    return true;
                }
                else
                {
                    plural = null;
                    return false;
                }
            }
            public bool TryToSingular(string word, out string singular)
            {
                if (word.EndsWith(_pluralSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    singular = word.Substring(0, word.Length - _pluralSuffix.Length) + _singularSuffix;
                    return true;
                }
                else
                {
                    singular = null;
                    return false;
                }
            }
        }

        private static void PopulateLookupTables()
        {
            _specialSingulars = new Dictionary<string, Word>(StringComparer.OrdinalIgnoreCase);
            _specialPlurals = new Dictionary<string, Word>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < _specialWordsStringTable.Length; i += 3)
            {
                var s = _specialWordsStringTable[i];
                var p = _specialWordsStringTable[i + 1];
                var p2 = _specialWordsStringTable[i + 2];
                if (string.IsNullOrEmpty(p))
                {
                    p = s;
                }

                var w = new Word(s, p, p2);
                _specialSingulars.Add(s, w);
                _specialPlurals.Add(p, w);
                if (!string.IsNullOrEmpty(p2))
                {
                    _specialPlurals.Add(p2, w);
                }
            }
        }

        private static void PopulateSuffixRules()
        {
            _suffixRules = new List<SuffixRule>();
            for (var i = 0; i < _suffixRulesStringTable.Length; i += 2)
            {
                var singular = _suffixRulesStringTable[i];
                var plural = _suffixRulesStringTable[i + 1];
                _suffixRules.Add(new SuffixRule(singular, plural));
            }
        }

        private string ToPluralInternal(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }
            // lookup special words
            Word word;
            if (_specialSingulars.TryGetValue(s, out word))
            {
                return word.Plural;
            }
            // apply suffix rules
            string plural;
            foreach (var rule in _suffixRules)
            {
                if (rule.TryToPlural(s, out plural))
                {
                    return plural;
                }
            }
            // apply the default rule
            return s + "s";
        }

        private string ToSingularInternal(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }
            // lookup special words
            Word word;
            if (_specialPlurals.TryGetValue(s, out word))
            {
                return word.Singular;
            }
            // apply suffix rules
            string singular;
            foreach (var rule in _suffixRules)
            {
                if (rule.TryToSingular(s, out singular))
                {
                    return singular;
                }
            }
            // apply the default rule
            if (s.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            {
                return s.Substring(0, s.Length - 1);
            }
            return s;
        }

        private string AdjustCase(string s, string template)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }

            // determine the type of casing of the template string
            var foundUpperOrLower = false;
            var allLower = true;
            var allUpper = true;
            var firstUpper = false;
            for (var i = 0; i < template.Length; i++)
            {
                if (Char.IsUpper(template[i]))
                {
                    if (i == 0) firstUpper = true;
                    allLower = false;
                    foundUpperOrLower = true;
                }
                else if (Char.IsLower(template[i]))
                {
                    allUpper = false;
                    foundUpperOrLower = true;
                }
            }

            // change the case according to template
            if (foundUpperOrLower)
            {
                if (allLower)
                {
                    s = s.ToLowerInvariant();
                }
                else if (allUpper)
                {
                    s = s.ToUpperInvariant();
                }
                else if (firstUpper)
                {
                    if (!Char.IsUpper(s[0]))
                    {
                        s = s.Substring(0, 1).ToUpperInvariant() + s.Substring(1);
                    }
                }
            }
            return s;
        }
    }
}
