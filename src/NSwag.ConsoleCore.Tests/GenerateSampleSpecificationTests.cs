using System.Diagnostics;
using NSwag.CodeGeneration.CSharp.Tests;
using NSwag.CodeGeneration.TypeScript.Tests;

namespace NSwag.ConsoleCore.Tests
{
    public class GenerateSampleSpecificationTests
    {
        [Theory]
        [InlineData("NSwag.Sample.NET80", "net8.0", true)]
        [InlineData("NSwag.Sample.NET80Minimal", "net8.0", true)]
        [InlineData("NSwag.Sample.NET90", "net9.0", true)]
        [InlineData("NSwag.Sample.NET90Minimal", "net9.0", true)]
        public async Task Should_generate_openapi_for_project(string projectName, string targetFramework, bool generatesCode)
        {
            // Arrange
#if DEBUG
            const string configuration = "debug";
#else
            const string configuration = "release";
#endif
            var executablePath = Path.GetFullPath($"../../../../artifacts/bin/NSwag.ConsoleCore/{configuration}_{targetFramework}/dotnet-nswag.dll");
            var nswagJsonPath = Path.GetFullPath($"../../../../src/{projectName}/nswag.json");
            var openApiJsonPath = Path.GetFullPath($"../../../../src/{projectName}/openapi.json");

            var generatedClientsCsPath = Path.GetFullPath($"../../../../src/{projectName}/GeneratedClientsCs.gen");
            var generatedClientsTsPath = Path.GetFullPath($"../../../../src/{projectName}/GeneratedClientsTs.gen");
            var generatedControllersCsPath = Path.GetFullPath($"../../../../src/{projectName}/GeneratedControllersCs.gen");

            File.Delete(openApiJsonPath);
            File.Delete(generatedClientsTsPath);
            File.Delete(generatedClientsCsPath);
            File.Delete(generatedControllersCsPath);
            Assert.False(File.Exists(openApiJsonPath));

            // Act
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = executablePath + " run " + nswagJsonPath,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                Environment =
                {
                    { "NSWAG_NOVERSION", "true" }
                }
            });

            try
            {
                process.WaitForExit(20000);
            }
            finally
            {
                process.Kill();
            }

            // Assert
            if (process.ExitCode != 0)
            {
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                Assert.Fail(output + error);
            }

            var json = await File.ReadAllTextAsync(openApiJsonPath);
            await Verifier
                .Verify(json)
                .ScrubLinesContaining("x-generator")
                .UseParameters(projectName, targetFramework, generatesCode)
                .AutoVerify(includeBuildServer: false);

            if (generatesCode)
            {
                await CheckTypeScriptAsync(projectName, targetFramework, generatesCode, generatedClientsTsPath);
                await CheckCSharpClientsAsync(projectName, targetFramework, generatesCode, generatedClientsCsPath);
                await CheckCSharpControllersAsync(projectName, targetFramework, generatesCode, generatedControllersCsPath);
            }
        }

        private static async Task CheckCSharpControllersAsync(string projectName, string targetFramework, bool generatesCode, string generatedControllersCsPath)
        {
            var code = await File.ReadAllTextAsync(generatedControllersCsPath);
            await Verifier.Verify(code)
                .UseMethodName(nameof(CheckCSharpControllersAsync))
                .UseParameters(projectName, targetFramework, generatesCode)
                .AutoVerify(includeBuildServer: false);

            CSharpCompiler.AssertCompile(code);
        }

        private static async Task CheckCSharpClientsAsync(string projectName, string targetFramework, bool generatesCode, string generatedClientsCsPath)
        {
            var code = await File.ReadAllTextAsync(generatedClientsCsPath);
            await Verifier.Verify(code)
                .UseMethodName(nameof(CheckCSharpClientsAsync))
                .UseParameters(projectName, targetFramework, generatesCode)
                .AutoVerify(includeBuildServer: false);

            CSharpCompiler.AssertCompile(code);
        }

        private static async Task CheckTypeScriptAsync(string projectName, string targetFramework, bool generatesCode, string generatedClientsTsPath)
        {
            var code = await File.ReadAllTextAsync(generatedClientsTsPath);
            await Verifier.Verify(code)
                .UseMethodName(nameof(CheckTypeScriptAsync))
                .UseParameters(projectName, targetFramework, generatesCode)
                .AutoVerify(includeBuildServer: false);

            TypeScriptCompiler.AssertCompile(code);
        }
    }
}