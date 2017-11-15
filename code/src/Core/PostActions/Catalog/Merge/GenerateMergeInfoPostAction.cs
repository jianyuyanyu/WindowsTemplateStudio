﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.Templates.Core.Gen;

namespace Microsoft.Templates.Core.PostActions.Catalog.Merge
{
    public class GenerateMergeInfoPostAction : PostAction<string>
    {
        private const string Suffix = "postaction";
        private const string NewSuffix = "failedpostaction";

        public const string Extension = "_" + Suffix + ".";
        public const string PostactionRegex = @"(\$\S*)?(_" + Suffix + "|_g" + Suffix + @")\.";

        public GenerateMergeInfoPostAction(string config)
            : base(config)
        {
        }

        public override void Execute()
        {
            var postAction = File.ReadAllText(Config).AsUserFriendlyPostAction();
            var sourceFile = GetFilePath();
            var mergeType = GetMergeType();
            var relFilePath = sourceFile.Replace(GenContext.Current.OutputPath + Path.DirectorySeparatorChar, string.Empty);

            if (GenContext.Current.MergeFilesFromProject.ContainsKey(relFilePath))
            {
                var mergeFile = GenContext.Current.MergeFilesFromProject[relFilePath];

                mergeFile.Add(new MergeInfo { Format = mergeType, PostActionCode = postAction });
            }
        }

        private string GetMergeType()
        {
            switch (Path.GetExtension(Config).ToLowerInvariant())
            {
                case ".cs":
                    return "csharp";
                case ".vb":
                    return "vb.net";
                case ".csproj":
                case ".vbproj":
                case ".xaml":
                    return "xml";
                default:
                    return string.Empty;
            }
        }

        private string GetFilePath()
        {
            if (Path.GetFileName(Config).StartsWith(Extension, StringComparison.OrdinalIgnoreCase))
            {
                var extension = Path.GetExtension(Config);
                var directory = Path.GetDirectoryName(Config);

                return Directory.EnumerateFiles(directory, $"*{extension}").FirstOrDefault(f => !f.Contains(Suffix));
            }
            else
            {
                var path = Regex.Replace(Config, PostactionRegex, ".");

                return path;
            }
        }
    }
}
