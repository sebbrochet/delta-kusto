using DeltaKustoLib.CommandModel;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DeltaKustoFileIntegrationTest.EmptyTarget
{
    public class FunctionWithCurrentTest : IntegrationTestBase
    {
        [Fact]
        public async Task EmptyBoth()
        {
            var parameters = await RunParametersAsync(
                "Functions/WithCurrent/EmptyBoth/delta-params.json");
            var outputPath = parameters.Jobs!.First().Value.Action!.FilePath!;
            var commands = await LoadScriptAsync(outputPath);

            Assert.Empty(commands);
        }

        [Fact]
        public async Task Same()
        {
            var parameters = await RunParametersAsync(
                "Functions/WithCurrent/Same/delta-params.json");
            var outputPath = parameters.Jobs!.First().Value.Action!.FilePath!;
            var commands = await LoadScriptAsync(outputPath);

            Assert.Empty(commands);
        }

        [Fact]
        public async Task TargetMore()
        {
            var parameters = await RunParametersAsync(
                "Functions/WithCurrent/TargetMore/delta-params.json");
            var outputPath = parameters.Jobs!.First().Value.Action!.FilePath!;
            var commands = await LoadScriptAsync(outputPath);

            Assert.Single(commands);

            var function = (CreateFunctionCommand)commands.First();

            Assert.Equal("YourFunction", function.ObjectName);
        }

        [Fact]
        public async Task TargetLess()
        {
            var parameters = await RunParametersAsync(
                "Functions/WithCurrent/TargetLess/delta-params.json");
            var outputRootFolder = parameters.Jobs!.First().Value.Action!.FolderPath!;
            var outputPath = Path.Combine(outputRootFolder, "functions/drop/YourFunction.kql");
            var commands = await LoadScriptAsync(outputPath);

            Assert.Single(commands);

            var function = (DropFunctionCommand)commands.First();

            Assert.Equal("YourFunction", function.ObjectName);
        }
    }
}