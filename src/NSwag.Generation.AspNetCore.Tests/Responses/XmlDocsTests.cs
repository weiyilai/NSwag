﻿using NJsonSchema;
using NJsonSchema.NewtonsoftJson.Generation;
using NSwag.CodeGeneration.Tests;
using NSwag.Generation.AspNetCore.Tests.Web.Controllers;

namespace NSwag.Generation.AspNetCore.Tests.Responses
{
    public class XmlDocsTests : AspNetCoreTestsBase
    {
        [Fact]
        public async Task When_operation_has_SwaggerResponseAttribute_with_description_it_is_in_the_spec()
        {
            // Arrange
            var settings = new AspNetCoreOpenApiDocumentGeneratorSettings { SchemaSettings = new NewtonsoftJsonSchemaGeneratorSettings { SchemaType = SchemaType.OpenApi3 } };

            // Act
            var document = await GenerateDocumentAsync(settings, typeof(XmlDocsController));
            var json = document.ToJson();

            // Assert
            await VerifyHelper.Verify(json);
        }
    }
}