﻿using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.NewtonsoftJson.Generation;
using NSwag.Generation.AspNetCore.Tests.Web.Controllers.Parameters;

namespace NSwag.Generation.AspNetCore.Tests.Parameters
{
    public class QueryParametersTests : AspNetCoreTestsBase
    {
        [Fact]
        public async Task When_complex_query_parameters_are_nullable_and_set_to_null_they_are_optional_in_spec()
        {
            // Arrange
            var settings = new AspNetCoreOpenApiDocumentGeneratorSettings
            {
                RequireParametersWithoutDefault = true,
                SchemaSettings = new NewtonsoftJsonSchemaGeneratorSettings { SchemaType = SchemaType.Swagger2 }
            };

            // Act
            var document = await GenerateDocumentAsync(settings, typeof(ComplexQueryParametersController));
            var json = document.ToJson();
            Assert.NotNull(json);

            // Assert
            var operation = document.Operations.First().Operation;

            Assert.True(operation.ActualParameters[0].IsRequired);
            Assert.True(operation.ActualParameters[operation.ActualParameters.Count - 1].IsRequired);

            Assert.Equal(2, operation.ActualParameters.Count);

            Assert.Equal("Bar.", operation.ActualParameters[0].Description);
            Assert.Equal(JToken.Parse("42"), operation.ActualParameters[0].Example);

            Assert.Equal("Baz.", operation.ActualParameters[operation.ActualParameters.Count - 1].Description);
        }

        [Fact]
        public async Task When_simple_query_parameters_are_nullable_and_set_to_null_they_are_optional_in_spec()
        {
            // Arrange
            var settings = new AspNetCoreOpenApiDocumentGeneratorSettings
            {
                RequireParametersWithoutDefault = true,
                SchemaSettings = new NewtonsoftJsonSchemaGeneratorSettings { SchemaType = SchemaType.OpenApi3 }
            };

            // Act
            var document = await GenerateDocumentAsync(settings, typeof(SimpleQueryParametersController));

            // Assert
            var operation = document.Operations.First().Operation;

            Assert.Equal(3, operation.ActualParameters.Count);
            Assert.True(operation.ActualParameters.Skip(0).First().IsRequired);
            Assert.False(operation.ActualParameters.Skip(1).First().IsRequired);
            Assert.True(operation.ActualParameters.Skip(2).First().IsRequired);
        }

        [Fact]
        public async Task When_simple_query_parameter_has_BindRequiredAttribute_then_it_is_required()
        {
            // Arrange
            var settings = new AspNetCoreOpenApiDocumentGeneratorSettings
            {
                RequireParametersWithoutDefault = false,
                SchemaSettings = new NewtonsoftJsonSchemaGeneratorSettings { SchemaType = SchemaType.OpenApi3 }
            };

            // Act
            var document = await GenerateDocumentAsync(settings, typeof(SimpleQueryParametersController));

            // Assert
            var operation = document.Operations.First().Operation;

            Assert.Equal(3, operation.ActualParameters.Count);
            Assert.False(operation.ActualParameters.Skip(0).First().IsRequired);
            Assert.False(operation.ActualParameters.Skip(1).First().IsRequired);
            Assert.True(operation.ActualParameters.Skip(2).First().IsRequired);
        }

        [Fact]
        public async Task When_parameter_is_overwritten_then_original_name_is_set()
        {
            // Arrange
            var settings = new AspNetCoreOpenApiDocumentGeneratorSettings
            {
                RequireParametersWithoutDefault = false,
                SchemaSettings = new NewtonsoftJsonSchemaGeneratorSettings { SchemaType = SchemaType.OpenApi3 }
            };

            // Act
            var document = await GenerateDocumentAsync(settings, typeof(RenamedQueryParameterController));

            // Assert
            var parameter = document.Operations.First().Operation.ActualParameters[0];

            Assert.Equal("month", parameter.Name);
            Assert.Equal("months", parameter.OriginalName);
        }

        [Fact]
        public async Task When_parameter_has_bind_never_then_it_is_ignored()
        {
            // Arrange
            var settings = new AspNetCoreOpenApiDocumentGeneratorSettings
            {
                RequireParametersWithoutDefault = false,
                SchemaSettings = new NewtonsoftJsonSchemaGeneratorSettings
                {
                    SchemaType = SchemaType.OpenApi3
                }
            };

            // Act
            var document = await GenerateDocumentAsync(settings, typeof(BindNeverQueryParameterController));

            // Assert
            var parameters = document.Operations.First().Operation.ActualParameters.ToArray();

            Assert.Single(parameters);
            Assert.Equal("a", parameters.First().Name);
        }
    }
}