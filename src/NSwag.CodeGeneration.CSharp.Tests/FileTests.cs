﻿using Microsoft.AspNetCore.Mvc;
using NJsonSchema.NewtonsoftJson.Generation;
using NSwag.CodeGeneration.Tests;
using NSwag.Generation.WebApi;

namespace NSwag.CodeGeneration.CSharp.Tests
{
    public class FileTests
    {
        public class FileDownloadController : Controller
        {
            [Route("DownloadFile")]
            public HttpResponseMessage DownloadFile()
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public async Task When_file_is_generated_system_alias_is_there()
        {
            // Arrange
            var swaggerGenerator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings
            {
                SchemaSettings = new NewtonsoftJsonSchemaGeneratorSettings()
            });

            var document = await swaggerGenerator.GenerateForControllerAsync<FileDownloadController>();

            // Act
            var codeGen = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings
            {
                GenerateClientInterfaces = true
            });
            var code = codeGen.GenerateFile();

            // Assert
            await VerifyHelper.Verify(code);
            CSharpCompiler.AssertCompile(code);
        }
    }
}
