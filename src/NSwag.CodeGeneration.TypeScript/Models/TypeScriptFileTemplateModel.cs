//-----------------------------------------------------------------------
// <copyright file="ClientTemplateModel.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/RicoSuter/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using NJsonSchema.CodeGeneration;
using NJsonSchema.CodeGeneration.TypeScript;

namespace NSwag.CodeGeneration.TypeScript.Models
{
    /// <summary>The TypeScript file template model.</summary>
    public class TypeScriptFileTemplateModel
    {
        private readonly TypeScriptClientGeneratorSettings _settings;
#pragma warning disable IDE0052
        private readonly TypeScriptTypeResolver _resolver;
#pragma warning restore IDE0052
        private readonly string _clientCode;
        private readonly IEnumerable<CodeArtifact> _clientTypes;
        private readonly OpenApiDocument _document;
        private readonly TypeScriptExtensionCode _extensionCode;

        /// <summary>Initializes a new instance of the <see cref="TypeScriptFileTemplateModel" /> class.</summary>
        /// <param name="clientTypes">The client types.</param>
        /// <param name="dtoTypes">The DTO types.</param>
        /// <param name="document">The Swagger document.</param>
        /// <param name="extensionCode">The extension code.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="resolver">The resolver.</param>
        public TypeScriptFileTemplateModel(
            IEnumerable<CodeArtifact> clientTypes,
            IEnumerable<CodeArtifact> dtoTypes,
            OpenApiDocument document,
            TypeScriptExtensionCode extensionCode,
            TypeScriptClientGeneratorSettings settings,
            TypeScriptTypeResolver resolver)
        {
            _document = document;
            _extensionCode = extensionCode;
            _settings = settings;
            _resolver = resolver;

            _clientCode = clientTypes.OrderByBaseDependency().Concatenate();
            _clientTypes = clientTypes;

            Types = dtoTypes.OrderByBaseDependency().Concatenate();
            ExtensionCodeBottom = GenerateExtensionCodeAfter();
            Framework = new TypeScriptFrameworkModel(settings);
        }

        /// <summary>Gets framework specific information.</summary>
        public TypeScriptFrameworkModel Framework { get; set; }

        /// <summary>Gets a value indicating whether to generate client classes.</summary>
        public bool GenerateClientClasses => _settings.GenerateClientClasses;

        /// <summary>Gets or sets a value indicating whether DTO exceptions are wrapped in a SwaggerException instance.</summary>
        public bool WrapDtoExceptions => _settings.WrapDtoExceptions;

        /// <summary>Gets or sets a value indicating whether to wrap success responses to allow full response access.</summary>
        public bool WrapResponses => _settings.WrapResponses;

        /// <summary>Gets or sets a value indicating whether to generate the response class (only applied when WrapResponses == true, default: true).</summary>
        public bool GenerateResponseClasses => _settings.GenerateResponseClasses;

        /// <summary>Gets the response class names.</summary>
        public IEnumerable<string> ResponseClassNames
        {
            get
            {
                // TODO: Merge with ResponseClassNames of C#
                if (_settings.OperationNameGenerator.SupportsMultipleClients)
                {
                    return _document.GetOperations()
                        .GroupBy(o => _settings.OperationNameGenerator.GetClientName(_document, o.Path, o.Method, o.Operation))
                        .Select(g => _settings.ResponseClass.Replace("{controller}", g.Key))
                        .Where(a => _settings.TypeScriptGeneratorSettings.ExcludedTypeNames?.Contains(a) != true)
                        .Distinct();
                }

                return [_settings.ResponseClass.Replace("{controller}", string.Empty)];
            }
        }

        /// <summary>Gets a value indicating whether required types should be imported.</summary>
        public bool ImportRequiredTypes => _settings.ImportRequiredTypes;

        /// <summary>Gets a value indicating whether to call 'transformOptions' on the base class or extension class.</summary>
        public bool UseTransformOptionsMethod => _settings.UseTransformOptionsMethod;

        /// <summary>Gets a value indicating whether to include the httpContext parameter (Angular template only, default: false).</summary>
        public bool IncludeHttpContext => _settings.IncludeHttpContext;

        /// <summary>Gets the clients code.</summary>
        public string Clients => _settings.GenerateClientClasses ? _clientCode : string.Empty;

        /// <summary>Gets the types code.</summary>
        public string Types { get; }

        /// <summary>Gets or sets the extension code imports.</summary>
        public string ExtensionCodeImport => _extensionCode.ImportCode;

        /// <summary>Gets or sets the extension code to insert at the beginning.</summary>
        public string ExtensionCodeTop => _settings.ConfigurationClass != null && _extensionCode.ExtensionClasses.TryGetValue(_settings.ConfigurationClass, out string value)
            ? value + "\n\n" + _extensionCode.TopCode
            : _extensionCode.TopCode;

        /// <summary>Gets or sets the extension code to insert at the end.</summary>
        public string ExtensionCodeBottom { get; }

        /// <summary>Gets a value indicating whether the file has module name.</summary>
        public bool HasModuleName => !string.IsNullOrEmpty(_settings.TypeScriptGeneratorSettings.ModuleName);

        /// <summary>Gets the name of the module.</summary>
        public string ModuleName => _settings.TypeScriptGeneratorSettings.ModuleName;

        /// <summary>Gets a value indicating whether the file has a namespace.</summary>
        public bool HasNamespace => !string.IsNullOrEmpty(_settings.TypeScriptGeneratorSettings.Namespace);

        /// <summary>Gets the namespace.</summary>
        public string Namespace => _settings.TypeScriptGeneratorSettings.Namespace;

        /// <summary>Gets whether the export keyword should be added to all classes and enums.</summary>
        public bool ExportTypes => _settings.TypeScriptGeneratorSettings.ExportTypes;

        /// <summary>Gets a value indicating whether the FileParameter interface should be rendered.</summary>
        public bool RequiresFileParameterInterface
        {
            get
            {
                if (_settings.TypeScriptGeneratorSettings.ExcludedTypeNames.Contains("FileParameter"))
                {
                    return false;
                }

                var operations = _document.GetOperations().ToList();
                return operations.Any(o => o.Operation.GetActualParameters().Any(static p => p.ActualTypeSchema.IsBinary)) ||
                       operations.Any(o => o.Operation?.ActualRequestBody?._content?.Any(static c => c.Value.Schema?.IsBinary == true ||
                                                                                              c.Value.Schema?.ActualProperties.Any(p => p.Value.IsBinary ||
                                                                                                                                        p.Value.Item?.IsBinary == true ||
                                                                                                                                        p.Value.Items.Any(i => i.IsBinary)
                                                                                              ) == true) == true);
            }
        }

        /// <summary>Gets a value indicating whether the FileResponse interface should be rendered.</summary>
        public bool RequiresFileResponseInterface =>
            !Framework.IsJQuery &&
            !_settings.TypeScriptGeneratorSettings.ExcludedTypeNames.Contains("FileResponse") &&
            _document.GetOperations().Any(static o => o.Operation.HasActualResponse((_, response) => response.IsBinary(o.Operation)));

        /// <summary>Gets a value indicating whether the client functions are required.</summary>
        public bool RequiresClientFunctions =>
            _settings.GenerateClientClasses &&
            !string.IsNullOrEmpty(Clients);

        /// <summary>Gets the exception class name.</summary>
        public string ExceptionClassName => _settings.ExceptionClass;

        /// <summary>Gets a value indicating whether the SwaggerException class is required. Note that if RequiresClientFunctions returns true this returns true since the client functions require it. </summary>
        public bool RequiresExceptionClass =>
            RequiresClientFunctions &&
            !_settings.TypeScriptGeneratorSettings.ExcludedTypeNames.Contains(_settings.ExceptionClass);

        /// <summary>Gets a value indicating whether to handle references.</summary>
        public bool HandleReferences => _settings.TypeScriptGeneratorSettings.HandleReferences;

        /// <summary>Gets a value indicating whether MomentJS duration format is needed (moment-duration-format package).</summary>
        public bool RequiresMomentJSDuration => Types?.Contains("moment.duration(") == true;

        /// <summary>Gets a value indicating whether the target TypeScript version supports override keyword.</summary>
        public bool SupportsOverrideKeyword => _settings.TypeScriptGeneratorSettings.SupportsOverrideKeyword;

        /// <summary>Gets a value indicating whether the target TypeScript version supports Type-Only imports</summary>
        public bool SupportsTypeOnlyImports => _settings.TypeScriptGeneratorSettings.TypeScriptVersion >= 3.8m;

        private string GenerateExtensionCodeAfter()
        {
            var clientClassesVariable = "{" + string.Join(", ", _clientTypes
                .Where(c => c.Category != CodeArtifactCategory.Utility)
                .Select(c => "'" + c.TypeName + "': " + c.TypeName)) + "}";

            return _extensionCode.BottomCode.Replace("{clientClasses}", clientClassesVariable);
        }
    }
}