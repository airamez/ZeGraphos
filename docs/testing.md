# Testing Guide

This guide covers how to run tests, write new tests, and understand the testing structure of ZeGraphos.

## 🧪 Running Tests

### Prerequisites

- .NET 10.0 SDK or later
- Visual Studio 2022, VS Code, or compatible IDE
- Git (for cloning the repository)

### Running All Tests

From the root directory:

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run with coverage and generate HTML report
dotnet test --collect:"XPlat Code Coverage" && reportgenerator -reports:TestResults/**/*.xml -targetdir:CoverageReport -reporttypes:Html
```

### Running Specific Test Categories

```bash
# Run only core functionality tests
dotnet test --filter "FullyQualifiedName~Core"

# Run only algorithm tests
dotnet test --filter "FullyQualifiedName~Algorithms"

# Run only performance benchmarks
dotnet test --filter "FullyQualifiedName~Performance"

# Run specific test file
dotnet test --filter "ClassName~GraphTests"

# Run specific test method
dotnet test --filter "TestMethodName~TestName"
```

### Coverage Reports

```bash
# Install ReportGenerator tool (one-time setup)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Run tests with coverage and generate HTML report
dotnet test --collect:"XPlat Code Coverage" && reportgenerator -reports:TestResults/**/*.xml -targetdir:CoverageReport -reporttypes:Html

# Generate multiple report formats
reportgenerator -reports:TestResults/**/*.xml -targetdir:CoverageReport -reporttypes:Html;Badges;Cobertura;JsonSummary

# Open coverage report (on macOS/Linux)
open CoverageReport/index.html
# On Windows
start CoverageReport/index.html
```

### Running Tests by Project

```bash
# Run only library tests
dotnet test test/ZeGraphos.Tests/

# Run with specific configuration
dotnet test test/ZeGraphos.Tests/ --configuration Release
```

## 📊 Test Categories

### 1. Core Functionality Tests (`Core/`)

Tests for basic graph operations and data structures:

- **GraphTests.cs**: Tests for all graph types (directed, undirected, weighted, unweighted)
- **GraphBuilderTests.cs**: Tests for the fluent GraphBuilder API
- **EdgeTests.cs**: Tests for edge operations and weight handling

**Example:**
```csharp
[Fact]
public void DirectedGraph_ShouldAddNodes()
{
    var graph = new DirectedGraph<string>();
    graph.AddNode("A");
    graph.AddNode("B");
    
    graph.NodeCount.Should().Be(2);
    graph.ContainsNode("A").Should().BeTrue();
}
```

### 2. Algorithm Tests (`Algorithms/`)

Tests for all implemented algorithms with known results:

- **AlgorithmValidationTests.cs**: Tests with expected results for validation
- Tests for shortest path, maximum flow, MST, coloring, and topological sort algorithms

**Example:**
```csharp
[Fact]
public void Dijkstra_ShouldFindOptimalPath_TriangleGraph()
{
    var graph = GraphBuilder.CreateWeightedUndirected<string, double>();
    graph.AddEdge("A", "B", 2.0);
    graph.AddEdge("B", "C", 3.0);
    graph.AddEdge("A", "C", 10.0);

    var result = Dijkstra<string, double>.FindShortestPath(graph, "A", "C");
    
    result.Path.Should().BeEquivalentTo(new[] { "A", "B", "C" });
    result.Distance.Should().Be(5.0);
}
```

### 3. Performance Benchmarks (`Performance/`)

Performance tests using BenchmarkDotNet:

- **AlgorithmBenchmarks.cs**: Performance tests for different graph sizes
- **ScenarioBenchmarks.cs**: Tests for specific graph scenarios (dense, sparse, etc.)

**Example:**
```csharp
[Benchmark]
public void Dijkstra_MediumGraph()
{
    var result = Dijkstra<string, double>.FindShortestPath(_mediumGraph, "0", "999");
}
```

## 🛠️ Writing New Tests

### Test Structure

Follow these patterns when writing new tests:

```csharp
public class NewFeatureTests
{
    private readonly IGraph<string> _graph;

    public NewFeatureTests()
    {
        _graph = new UndirectedGraph<string>();
        // Setup common test data
    }

    [Fact]
    public void Feature_ShouldWork_WhenConditionMet()
    {
        // Arrange
        // Setup test data

        // Act
        // Execute the feature

        // Assert
        // Verify results
        result.Should().Be(expected);
    }
}
```

### Best Practices

1. **Descriptive Test Names**: Use `Should_When_` pattern
2. **Arrange-Act-Assert**: Structure tests clearly
3. **Test Data**: Use GraphBuilder for consistent test data
4. **Assertions**: Use FluentAssertions for readable tests
5. **Edge Cases**: Test boundary conditions and error cases

### Example Test for New Algorithm

```csharp
[Fact]
public void NewAlgorithm_ShouldReturnCorrectPath_WhenGraphIsConnected()
{
    // Arrange
    var graph = GraphBuilder.CreateWeightedUndirected<string, double>()
        .AddEdges(("A", "B", 1.0), ("B", "C", 2.0));

    // Act
    var result = NewAlgorithm<string, double>.FindShortestPath(graph, "A", "C");

    // Assert
    result.Path.Should().BeEquivalentTo(new[] { "A", "B", "C" });
    result.Distance.Should().Be(3.0);
    result.Algorithm.Should().Be("NewAlgorithm");
}
```

## 📈 Performance Testing

### Running Benchmarks

```bash
# Run all benchmarks
dotnet run --project test/ZeGraphos.Tests --configuration Release

# Run specific benchmark
dotnet run --project test/ZeGraphos.Tests --filter "*Dijkstra*"

# Export results
dotnet run --project test/ZeGraphos.Tests --configuration Release -- --exporters json
```

### Writing Benchmarks

```csharp
[MemoryDiagnoser]
[SimpleJob]
public class MyAlgorithmBenchmark
{
    private IGraph<string> _graph;

    [GlobalSetup]
    public void Setup()
    {
        _graph = CreateTestGraph(1000);
    }

    [Benchmark]
    public void MyAlgorithm()
    {
        var result = MyAlgorithm<string, double>.FindShortestPath(_graph, "0", "999");
    }
}
```

## 🔍 Debugging Tests

### Running Tests in Debug Mode

```bash
# Run specific test in debug
dotnet test --filter "TestMethodName~TestName" --logger "console;verbosity=detailed"

# Run with debugger attached
dotnet test --filter "TestMethodName~TestName" --logger "console;verbosity=detailed" --no-build
```

### Common Test Issues

1. **Null Reference Exceptions**: Check that nodes are added before use
2. **Type Mismatches**: Ensure generic types match (T, TWeight)
3. **Graph State**: Verify graph is in expected state before test
4. **Algorithm Limitations**: Some algorithms have specific requirements

### Test Output Analysis

```
Test run passed.
Total tests: 38
     Passed: 38
     Failed: 0
     Skipped: 0
 Total time: 1.2s
```

## 📋 Test Coverage

### Setting Up Coverage Tools

```bash
# Install ReportGenerator tool (one-time setup)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Verify installation
reportgenerator -version
```

### Generating Coverage Reports

#### Basic Coverage
```bash
# Run tests with coverage collection
dotnet test --collect:"XPlat Code Coverage"

# This creates coverage files in TestResults/ directory
ls TestResults/**/*.xml
```

#### HTML Coverage Report
```bash
# Run tests and generate HTML report
dotnet test --collect:"XPlat Code Coverage" && reportgenerator -reports:TestResults/**/*.xml -targetdir:CoverageReport -reporttypes:Html

# Open the report
# macOS/Linux:
open CoverageReport/index.html
# Windows:
start CoverageReport/index.html
```

#### Multiple Report Formats
```bash
# Generate comprehensive reports
reportgenerator -reports:TestResults/**/*.xml -targetdir:CoverageReport -reporttypes:Html;Badges;Cobertura;JsonSummary;CsvSummary

# Available report types:
# - Html: Interactive HTML report
# - Badges: Coverage badges for README
# - Cobertura: XML format for CI integration
# - JsonSummary: JSON summary for automation
# - CsvSummary: CSV format for spreadsheets
```

### Coverage Configuration

#### Create coverlet.runsettings
```xml
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="XPlat Code Coverage">
        <Configuration>
          <Format>cobertura</Format>
          <Exclude>[*]*.Tests]*</Exclude>
          <Include>[ZeGraphos]*</Include>
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>
</RunSettings>
```

#### Use with Settings File
```bash
# Run with custom settings
dotnet test --settings:coverlet.runsettings

# Generate coverage with settings
dotnet test --settings:coverlet.runsettings --collect:"XPlat Code Coverage"
```

### Coverage Thresholds

#### Set Minimum Coverage
```xml
<!-- In coverlet.runsettings -->
<Configuration>
  <Threshold>80</Threshold>
  <ThresholdType>line</ThresholdType>
  <ThresholdStat>minimum</ThresholdStat>
</Configuration>
```

#### Fail on Low Coverage
```bash
# Fail tests if coverage drops below threshold
dotnet test --collect:"XPlat Code Coverage" --settings:coverlet.runsettings --logger:"trx;LogFileName=testresults.trx"
```

### Coverage Reports Analysis

#### Understanding Coverage Metrics
- **Line Coverage**: Percentage of executable lines covered
- **Branch Coverage**: Percentage of decision branches covered
- **Method Coverage**: Percentage of methods called
- **Cyclomatic Complexity**: Code complexity measurement

#### Coverage Goals for ZeGraphos
- **Core Classes**: 95%+ line coverage
- **Algorithm Classes**: 90%+ line coverage
- **Extension Methods**: 85%+ line coverage
- **Overall Target**: 90%+ line coverage

### CI/CD Integration

#### GitHub Actions Example
```yaml
- name: Run Tests with Coverage
  run: |
    dotnet test --collect:"XPlat Code Coverage" --settings:coverlet.runsettings
    
- name: Generate Coverage Report
  run: |
    reportgenerator -reports:TestResults/**/*.xml -targetdir:CoverageReport -reporttypes:Html;Badges;Cobertura
    
- name: Upload Coverage Reports
  uses: actions/upload-artifact@v3
  with:
    name: coverage-reports
    path: CoverageReport/
```

#### Azure DevOps Example
```yaml
- task: DotNetCoreCLI@2
  displayName: 'Run Tests with Coverage'
  inputs:
    command: 'test'
    projects: '**/*Tests.csproj'
    arguments: '--collect:"XPlat Code Coverage" --settings:coverlet.runsettings'

- task: PublishCodeCoverageResults@1
  displayName: 'Publish Code Coverage'
  inputs:
    codeCoverageTool: Cobertura
    summaryFileLocation: '$(Agent.TempDirectory)/**/coverage.cobertura.xml'
```

### Coverage Best Practices

1. **Aim for High Coverage**: Target 90%+ line coverage
2. **Focus on Critical Code**: Prioritize core algorithms and data structures
3. **Exclude Test Code**: Don't count test code in coverage metrics
4. **Regular Monitoring**: Track coverage trends over time
5. **Branch Coverage**: Ensure both success and failure paths are tested

### Current Coverage Areas

- ✅ **Core Graph Operations**: All graph types and basic operations
- ✅ **Algorithm Validation**: Known results for all algorithms
- ✅ **Edge Cases**: Empty graphs, single nodes, disconnected components
- ✅ **Performance**: Benchmarks for various scenarios
- ✅ **Error Handling**: Invalid inputs and edge cases

### Coverage Reports

```bash
# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"

# View coverage (requires tool like ReportGenerator)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:TestResults/**/*.xml -targetdir:CoverageReport
```

## 🚀 Continuous Integration

### GitHub Actions

The project includes CI configuration for:

- Running tests on multiple .NET versions
- Performance regression detection
- Code coverage reporting
- NuGet package validation

### Local CI Testing

```bash
# Run full CI test suite
./scripts/run-ci-tests.sh

# Run performance regression tests
./scripts/run-performance-tests.sh
```

## 📚 Test Resources

### Test Data Files

Test data is generated programmatically using GraphBuilder:

```csharp
// Standard test graphs
var smallGraph = GraphBuilder.CreateCompleteGraph(10);
var mediumGraph = GraphBuilder.CreateRandomGraph(100);
var largeGraph = GraphBuilder.CreateRandomGraph(1000);
```

### Mock Objects

For testing external dependencies:

```csharp
public class MockGraph : IGraph<string>
{
    // Mock implementation for testing
}
```

## 🔧 Troubleshooting

### Common Issues

1. **Tests Not Found**: Ensure test project references main project
2. **Compilation Errors**: Check using statements and namespace references
3. **Performance Issues**: Use Release configuration for benchmarks
4. **Flaky Tests**: Check for timing dependencies and shared state

### Getting Help

- Check existing tests for patterns
- Review algorithm documentation
- Use debug mode to step through failing tests
- Check GitHub Issues for known problems

## 📖 Additional Resources

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Guide](https://fluentassertions.com/)
- [BenchmarkDotNet Documentation](https://benchmarkdotnet.org/)
- [Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/)

This testing guide should help you effectively test and validate ZeGraphos functionality. Happy testing! 🧪
