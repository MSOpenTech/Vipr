// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using ODataReader.v4;
using Vipr.Core;
using Vipr.Properties;

namespace Vipr
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ODataToFile<Reader>("odataDemo.cs", Resources.sharepoint_full_edmx);
        }

        private static void ODataToFile<T>(string fileName, string edmxString) where T : IReader, new()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..\\..\\", fileName);
            using (var writer = new StreamWriter(path, false, Encoding.UTF8))
            {
                writer.Write(ODataToString<T>(edmxString));
            }
        }

        private static string ODataToString<T>(string edmxString) where T : IReader, new()
        {
            var reader = new T();
            var model = reader.GenerateOdcmModel(new Dictionary<string, string>(){{"$metadata", edmxString}});
            var writer = new CSharpWriter.CSharpWriter(model, null);
            return writer.GenerateProxy();
        }
    }
}
