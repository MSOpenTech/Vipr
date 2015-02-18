﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TemplateWriter;

namespace Vipr.CLI
{
    public class Template
    {
        public bool IsBase { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public string FolderName { get; set; }

        public string ResourceName { get; set; }

        public Template(string name, string resourceName)
        {
            Name = name;
            ResourceName = resourceName;
        }

        protected bool Equals(Template other)
        {
            return string.Equals(Name, other.Name) && string.Equals(ResourceName, other.ResourceName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Template) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0)*397) ^ (ResourceName != null ? ResourceName.GetHashCode() : 0);
            }
        }

        public static bool operator ==(Template left, Template right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Template left, Template right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return ResourceName;
        }

        public bool IsForLanguage(string language)
        {
            return ResourceName.ToLower().IndexOf(language.ToLower(), StringComparison.Ordinal) >= 0;
        }
    }
}