﻿#region Copyright © 2008 Paul Welter. All rights reserved.
/*
Copyright © 2008 Paul Welter. All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

1. Redistributions of source code must retain the above copyright
   notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright
   notice, this list of conditions and the following disclaimer in the
   documentation and/or other materials provided with the distribution.
3. The name of the author may not be used to endorse or promote products
   derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE AUTHOR "AS IS" AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
*/
#endregion

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Win32;



namespace MSBuild.Community.Tasks.Sandcastle
{
    /// <summary>
    /// ChmBuilder task for Sandcastle.
    /// </summary>
    public class ChmBuilder : SandcastleToolBase
    {
        /// <summary>
        /// Gets or sets the HTML directory.
        /// </summary>
        /// <value>The HTML directory.</value>
        public string HtmlDirectory { get; set; }

        /// <summary>
        /// Gets or sets the name of the project.
        /// </summary>
        /// <value>The name of the project.</value>
        public string ProjectName { get; set; }

        /// <summary>
        /// Gets or sets the toc file.
        /// </summary>
        /// <value>The toc file.</value>
        public ITaskItem TocFile { get; set; }

        /// <summary>
        /// Gets or sets the output file.
        /// </summary>
        /// <value>The output file.</value>
        public string OutputDirectory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ChmBuilder"/> is metadata.
        /// </summary>
        /// <value><c>true</c> if metadata; otherwise, <c>false</c>.</value>
        public bool Metadata { get; set; }

        /// <summary>
        /// Gets or sets the language id.
        /// </summary>
        /// <value>The language id.</value>
        public string LanguageId { get; set; }

        /// <summary>
        /// Gets the name of the executable file to run.
        /// </summary>
        /// <value></value>
        /// <returns>The name of the executable file to run.</returns>
        protected override string ToolName
        {
            get { return "ChmBuilder.exe"; }
        }

        /// <summary>
        /// Returns a string value containing the command line arguments to pass directly to the executable file.
        /// </summary>
        /// <returns>
        /// A string value containing the command line arguments to pass directly to the executable file.
        /// </returns>
        protected override string GenerateCommandLineCommands()
        {
            CommandLineBuilder builder = new CommandLineBuilder();
            builder.AppendSwitchIfNotNull("/html:", HtmlDirectory);
            builder.AppendSwitchIfNotNull("/project:", ProjectName);
            builder.AppendSwitchIfNotNull("/toc:", TocFile);
            builder.AppendSwitchIfNotNull("/out:", OutputDirectory);
            builder.AppendSwitchIfNotNull("/lcid:", LanguageId);

            if (Metadata)
                builder.AppendSwitch("/metadata+");

            return builder.ToString();
        }
    }
}
