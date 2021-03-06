﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoRest.Core;
using AutoRest.Core.Model;
using AutoRest.Core.Utilities.Collections;

namespace AutoRest.AzureResourceSchema
{
    public class CodeGeneratorArs : CodeGenerator
    {
        public CodeGeneratorArs()
        {
        }

        public override string ImplementationFileExtension => ".json";

        public override string UsageInstructions => $"Your Azure Resource Schema(s) can be found in the specified `output-folder`.";

        public override async Task Generate(CodeModel serviceClient)
        {
            var apiVersions = serviceClient.Methods
                .SelectMany(method => method.XMsMetadata.apiVersions)
                .Concat(new [] { serviceClient.ApiVersion })
                .Where(each => each != null)
                .Distinct().ToArray();

            foreach(var version in apiVersions)
            { 
                var resourceSchemas = ResourceSchemaParser.Parse(serviceClient, version);

                foreach (string resourceProvider in resourceSchemas.Keys)
                {
                    using (var stringWriter = new StringWriter())
                    {
                        ResourceSchemaWriter.Write(stringWriter, resourceSchemas[resourceProvider]);

                        await Write(stringWriter.ToString(), Path.Combine(version, resourceProvider + ".json"), true);
                    }
                }
            }
        }
    }
}
