using System;
using System.Collections.Generic;

namespace TemplateWriter
{
    public class BuilderArguments
    {
        public string Language { get; set; }

        public string FileExtension
        {
            get { return ".java"; }   //TODO: Hardcoding to Java while I figure out where to put this. Shouldn't be an argument.
        }

        public string OutputDir { get; set; }
        public string InputFile { get; set; }
        public string TemplatesDir { get; set; }
        public string[] Plugins { get; set; }
        public bool ShowHelp { get; set; }
    }
}