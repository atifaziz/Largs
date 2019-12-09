#region Copyright (c) 2019 Atif Aziz. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

namespace Arqs
{
    using System;
    using System.Diagnostics;
    using System.Text;

    public sealed class OptionSpec
    {
        static readonly char[] PipeSeparator = { '|' };

        public static OptionSpec Parse(string spec)
        {
            var tokens = spec.Split((char[])null, 2, StringSplitOptions.RemoveEmptyEntries);

            var description = tokens.Length > 1 && tokens[1].Length > 0 ? tokens[1] : null;

            var names = tokens[0].Split(PipeSeparator, 3, StringSplitOptions.RemoveEmptyEntries);
            string name1 = null, name2 = null, name3 = null, valueName = null;
            bool isValueOptional = false, isFlag = false;

            var i = 0;
            foreach (var name in names)
            {
                var nameToken = name;

                var ei = name.IndexOf('=');
                if (ei < 0)
                {
                    isFlag = true;
                }
                else
                {
                    isFlag = false;

                    var bi = name.IndexOf('[');

                    if (ei == 0 || bi == 0)
                        throw new ArgumentException("Option specification is missing name.", nameof(spec));

                    if (bi > 0 && name[name.Length - 1] != ']')
                        throw new ArgumentException("Option specification has invalid syntax (missing ']').", nameof(spec));

                    isValueOptional = bi > 0 && bi + 1 == ei;
                    if (ei > 0)
                    {
                        nameToken = name.Substring(0, bi > 0 ? bi : ei);
                        var valueNameLength = name.Length - (ei + 1) - (isValueOptional ? 1 : 0);
                        if (valueNameLength > 0)
                            valueName = name.Substring(ei + 1, valueNameLength);
                    }
                }

                switch (i)
                {
                    case 0: name1 = nameToken; break;
                    case 1: name2 = nameToken; break;
                    case 2: name3 = nameToken; break;
                    default: Debug.Fail($"0 <= '{nameof(i)}' < 3"); break;
                }
                i++;
            }

            return new OptionSpec(OptionNames.Guess(name1, name2, name3), isFlag, isValueOptional, valueName, description);
        }

        OptionSpec(OptionNames names, bool isFlag, bool isValueOptional, string valueName, string description)
        {
            Names = names ?? throw new ArgumentNullException(nameof(names));
            IsFlag = isFlag;
            IsValueOptional = isValueOptional;
            ValueName = valueName;
            Description = description;
        }

        public OptionNames Names { get; }
        public bool IsFlag { get; }
        public string ValueName { get; }
        public bool IsValueOptional { get; }
        public string Description { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (Names.ShortName is ShortOptionName sn)
                sb.Append(sn);

            if (Names.AbbreviatedName is string an)
            {
                if (sb.Length > 0)
                    sb.Append('|');
                sb.Append(an);
            }

            if (Names.LongName is string ln)
            {
                if (sb.Length > 0)
                    sb.Append('|');
                sb.Append(ln);
            }

            if (IsValueOptional)
                sb.Append('[');

            if (ValueName is string vn)
                sb.Append('=').Append(vn);

            if (IsValueOptional)
                sb.Append(']');

            if (Description is string desc)
                sb.Append(' ').Append(desc);

            return sb.ToString();
        }
    }
}
