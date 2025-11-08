# Contributing to AWS Lambda Host

Thank you for your interest in contributing to the AWS Lambda Host project! This document provides guidelines and instructions for contributing.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Code Formatting](#code-formatting)
- [Making Changes](#making-changes)
- [Commit Guidelines](#commit-guidelines)
- [Pull Request Process](#pull-request-process)
- [Code Style](#code-style)
- [Testing](#testing)
- [Documentation](#documentation)
- [GitHub Actions](#github-actions)

## Code of Conduct

We are committed to providing a welcoming and inclusive environment for all contributors. Please be respectful and constructive in all interactions.

## Getting Started

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/YOUR_USERNAME/aws-lambda-host.git
   cd aws-lambda-host
   ```
3. **Add the upstream remote**:
   ```bash
   git remote add upstream https://github.com/j-d-ha/aws-lambda-host.git
   ```
4. **Create a feature branch** (see [Branching Strategy](#branching-strategy))

## Development Setup

### Prerequisites

- **.NET SDK 8.0+** (8.0, 9.0, or 10.0 RC)
- **Task** (optional, for running automated tasks) - see [Taskfile.yml](/Taskfile.yml)
- **Git** with configured `user.name` and `user.email`

### Building the Project

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test

# Clean packages (if needed)
dotnet clean
```

### Using Task Automation

If you have [Task](https://taskfile.dev) installed, you can use:

```bash
task clean    # Clean NuGet packages
task pack     # Build NuGet packages
task lambda-test  # Start AWS Lambda test tool (port 5050)
```

## Code Formatting

Code formatting is automated using **CleanupCode** and **CSharpier**, ensuring consistent style across the project. All PRs are checked for formatting compliance in CI/CD.

### Formatting Tools

The project uses two complementary tools:

1. **CleanupCode**: JetBrains code cleanup tool
   - Applies code organization and style rules
   - Uses the "Built-in: Full Cleanup" profile configured in `AwsLambda.Host.sln.DotSettings`
   - Handles code structure and consistency

2. **CSharpier**: Opinionated C# code formatter
   - Enforces consistent formatting (similar to Prettier for C#)
   - Handles spacing, line breaks, and code layout

### Running Format Commands

With [Task](https://taskfile.dev) installed:

```bash
# Format entire solution (both CleanupCode and CSharpier)
task format:all

# Run only CleanupCode
task format:cleanupcode

# Run only CSharpier
task format:csharpier
```

Without Task (using dotnet directly):

```bash
# CleanupCode formatting
dotnet jb cleanupcode \
  AwsLambda.Host.sln \
  --settings="AwsLambda.Host.sln.DotSettings" \
  --profile="Built-in: Full Cleanup" \
  --verbosity=WARN \
  --no-build

# CSharpier formatting
dotnet csharpier format .
```

### Before Committing

**Always format your code before committing**:

```bash
# Format your changes
task format:all

# Stage and commit the formatted files
git add .
git commit -m "style: format code with CleanupCode and CSharpier"
```

Failing to format code may cause CI/CD checks to fail, as the GitHub Actions workflow (`pr-build.yaml`) includes a code quality check that runs `task format:all` and validates no files were modified.

### IDE Integration

If you're using **JetBrains Rider** or **Visual Studio** with ReSharper:

- The solution includes `AwsLambda.Host.sln.DotSettings` with formatting rules
- Format on save can be configured in your IDE settings
- This ensures consistency even before running Task commands

## Making Changes

### Task Classification

Before starting work, classify your task:

- **Simple tasks**: Single file edits < 20 lines, documentation, comments, formatting
- **Complex tasks**: Multi-file changes, new features, architecture changes

### Branching Strategy

Create branches following this naming convention:

```
feature/#<issue-number>-description     # New features
bug/description                         # Bug fixes (or bug/#<issue-number>-description)
chore/#<issue-number>-description       # Maintenance, tooling, build changes
docs/description                        # Documentation updates
```

**Example**:
```bash
git checkout -b feature/#42-add-custom-serializer
git checkout -b bug/on-shutdown-generator-trying-to-return-void
```

### Planning (Required for Complex Tasks)

For complex tasks, **create a PLAN.md file** before implementation:

```markdown
# Task: [Description]

## Objective
[What needs to be accomplished]

## Steps
1. [Main step 1]
   - [Sub-step 1.1]
2. [Main step 2]

## Deliverables
- [Expected output]
```

- Break down work into numbered steps and sub-steps
- Submit plan for review before implementing
- Wait for approval before proceeding to implementation

## Commit Guidelines

We follow [Conventional Commits](https://www.conventionalcommits.org/) format:

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

### Commit Types

- **feat**: A new feature (e.g., `feat(source-generators): add OnShutdown support`)
- **fix**: A bug fix (e.g., `fix(#73): update MapHandler source generation`)
- **docs**: Documentation updates (e.g., `docs(openTelemetry): add XML documentation`)
- **refactor**: Code refactoring without feature changes (e.g., `refactor(source-generators): simplify logic`)
- **test**: Adding or updating tests (e.g., `test: add coverage for edge cases`)
- **chore**: Build, CI/CD, tooling, dependencies (e.g., `chore: update build targets`)
- **style**: Code style changes without logic changes (e.g., `style: simplify formatting`)

### Example Commits

```
feat(source-generators): add OnShutdown support for block lambdas and test coverage

Implements support for block lambda syntax in OnShutdown operations.
Includes comprehensive unit test coverage.

Closes #92
```

```
fix(#73): correct MapHandler detection in source generator

The source generator was not correctly identifying MapHandler methods
when using extension method syntax.
```

## Pull Request Process

1. **Ensure your branch is up to date**:
   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

2. **Push to your fork**:
   ```bash
   git push origin your-branch-name
   ```

3. **Create a Pull Request** using the [PR template](/.github/pull_request_template.md):
   - Use clear, descriptive title following Conventional Commits format
   - Reference related issues (e.g., `Closes #42`)
   - Complete the PR template checklist
   - Request review from maintainers

4. **Respond to feedback**:
   - Address review comments promptly
   - Push additional commits to the same branch
   - Avoid force-pushing after review has started

5. **Merge**:
   - Ensure all checks pass (CI/CD, tests, code review)
   - Maintainer will merge the PR
   - Your branch will be deleted after merge

## Code Style

### C# Conventions

- **Indentation**: 4 spaces
- **File-scoped namespaces**: Use `namespace AwsLambda.Host;` format
- **Nullable reference types**: Always enabled (`<Nullable>enable</Nullable>`)
- **Modern syntax**: Use records, top-level statements, and nullable annotations where appropriate
- **Line length**: Keep to 100 characters when practical

### XML Documentation

- **Document all public APIs** with XML documentation tags
- **Use only standard C# XML tags**: `<summary>`, `<param>`, `<returns>`, `<exception>`, `<remarks>`, `<example>`
- **Do NOT use unsupported tags** like `<strong>`, `<em>`, etc. (Use `<c>` for code/emphasis)
- **Example**:
  ```csharp
  /// <summary>
  /// Creates a new Lambda application builder.
  /// </summary>
  /// <param name="hostBuilder">The underlying host builder.</param>
  /// <returns>A configured <see cref="LambdaApplication"/> instance.</returns>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="hostBuilder"/> is null.</exception>
  public static LambdaApplication CreateBuilder(IHostBuilder hostBuilder) { }
  ```

### Code Organization

```csharp
// File-scoped namespace
namespace AwsLambda.Host;

// Using statements
using System;
using Microsoft.Extensions.DependencyInjection;

// Class/interface declaration
public sealed class LambdaApplication
{
    // Fields
    private readonly IHost _host;

    // Constructors
    public LambdaApplication(IHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    // Properties
    public IHost Host => _host;

    // Public methods
    public async Task RunAsync(CancellationToken cancellationToken = default) { }

    // Private/internal methods
    private void ValidateConfiguration() { }
}
```

## Testing

### Testing Framework

- **Framework**: xUnit v3
- **Assertions**: AwesomeAssertions (fluent assertions)
- **Mocking**: NSubstitute
- **Test Location**: `/tests/` directory with project name pattern `*.UnitTests`

### Test Conventions

- **Test class naming**: `[SubjectClass]Test` (e.g., `LambdaApplicationTest`)
- **Test method naming**: Descriptive, following pattern `[Method]_[Condition]_[Expected]`
- **Test organization**: Use Arrange-Act-Assert pattern
- **Mark test subject**: Use `[TestSubject(typeof(...))]` attribute on test class

### Example Test

```csharp
[TestSubject(typeof(LambdaApplication))]
public class LambdaApplicationTest
{
    [Fact]
    public void RunAsync_WhenCalled_InvokesHandler()
    {
        // Arrange
        var mockHost = Substitute.For<IHost>();
        var application = new LambdaApplication(mockHost);

        // Act
        var result = await application.RunAsync();

        // Assert
        result.Should().BeSuccessful();
    }

    [Theory]
    [InlineData(null)]
    public void Constructor_WithNullHost_ThrowsArgumentNullException(IHost host)
    {
        // Act & Assert
        var act = () => new LambdaApplication(host);
        act.Should().Throw<ArgumentNullException>()
            .WithParameter(nameof(host));
    }
}
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests for specific project
dotnet test tests/AwsLambda.Host.UnitTests

# Run with verbose output
dotnet test --verbosity detailed

# Run with coverage (if configured)
dotnet test /p:CollectCoverage=true
```

### Test Requirements

- **All public methods** should have corresponding unit tests
- **Happy path**: Test expected behavior with valid inputs
- **Error cases**: Test exception handling and validation
- **Edge cases**: Test boundary conditions and special cases
- **Coverage**: Aim for >80% code coverage on new features

## Documentation

### Project Documentation

- Update **README.md** for user-facing changes
- Add/update **XML documentation** for all public APIs
- Create/update docs in **/docs/claude/** for developer guides (if applicable)

### PR Documentation

- Include clear description of what changed and why
- Reference related issues
- Include examples for new features
- Document breaking changes prominently

### Documentation Standards

- **Clear and concise**: Write for developers unfamiliar with the code
- **Examples**: Provide code examples for complex features
- **Links**: Cross-reference related documentation
- **Keep current**: Update docs when implementation changes

## Target Frameworks

This project supports multiple .NET versions:

- **.NET 8.0** (primary target)
- **.NET 9.0** (supported)
- **.NET 10.0 RC** (experimental)

Test your changes against all supported frameworks:

```bash
dotnet build -f net8.0
dotnet build -f net9.0
```

## Central Package Management

Package versions are managed centrally in `Directory.Packages.props`. When adding dependencies:

1. Add the package version to `Directory.Packages.props`
2. Reference in project `.csproj` without version specifier
3. Document why the dependency is needed

## CI/CD Integration

All pull requests run through automated checks:

- **Build**: `dotnet build`
- **Tests**: `dotnet test`
- **Code analysis**: StyleCop Analyzers, FxCop

Ensure your changes pass all checks before requesting review.

## GitHub Actions

### Automated Workflows

The project includes GitHub Actions workflows that run automatically on pull requests and pushes to `main`:

- **pr-build.yaml**: Runs on every PR and push to main
  - Restores dependencies
  - Builds the solution
  - Runs all unit tests
  - Reports results in the PR

### Running Workflows Locally with `act`

You can test GitHub Actions workflows locally using [act](https://github.com/nektos/act), which runs workflows in Docker containers matching the CI/CD environment.

#### Installation

**macOS** (using Homebrew):
```bash
brew install act
```

**Linux** (using package manager):
```bash
# Ubuntu/Debian
sudo apt-get install act

# Fedora
sudo dnf install act
```

**Windows** (using Chocolatey):
```bash
choco install act
```

**Or download** from [GitHub releases](https://github.com/nektos/act/releases)

#### Testing Workflows Locally

Before pushing your changes, run the PR build workflow locally:

```bash
# Run the default workflow (pr-build)
act pull_request

# Run a specific workflow by name
act -W .github/workflows/pr-build.yaml

# Run with verbose output
act -v pull_request

# Run with specific .NET version
act pull_request --env DOTNET_VERSION=8.0.x
```

#### Common `act` Options

```bash
# List all workflows
act -l

# Run a specific job
act pull_request -j build

# Run with custom Docker image (if needed)
act -P ubuntu-latest=ubuntu:latest

# Run with dry-run mode (shows what would run)
act --dry-run pull_request

# Set environment variables
act pull_request -e /path/to/env-file
```

#### Troubleshooting `act`

**Issue**: "Cannot connect to Docker daemon"
- Solution: Ensure Docker is installed and running
  ```bash
  docker ps  # Verify Docker is working
  ```

**Issue**: "Workflow not found"
- Solution: Check that workflow files are in `.github/workflows/` directory
  ```bash
  ls -la .github/workflows/
  ```

**Issue**: "Permission denied" errors
- Solution: Ensure your user is in the `docker` group (Linux)
  ```bash
  sudo usermod -aG docker $USER
  newgrp docker
  ```

**Issue**: Container image pull failures
- Solution: Ensure you have internet connectivity and Docker image registry access

### Understanding Workflow Results

When a workflow runs (locally or on GitHub):

1. **Build Phase**: Compiles all projects
   - Failure here means syntax or compilation errors
   - Check output for error details

2. **Test Phase**: Executes all unit tests
   - Failure here means a test assertion failed
   - Review test output for specific failure

3. **Results**: Summary of what passed/failed
   - All steps must succeed for the workflow to pass
   - If any step fails, the PR cannot be merged

### Workflow Best Practices

- **Run locally first**: Use `act` to test before pushing
- **Check all frameworks**: Test against supported .NET versions
- **Review logs carefully**: GitHub Actions logs show detailed information
- **Push early, push often**: Get feedback from CI/CD early in development
- **Don't ignore failures**: Address workflow failures immediately

### Adding New Workflows

If you need to add a new GitHub Actions workflow:

1. Create the workflow file in `.github/workflows/`
2. Follow GitHub Actions YAML syntax
3. Test locally with `act`
4. Document the workflow in this section
5. Ensure it adds clear value to the development process

## Questions or Need Help?

- **GitHub Issues**: For bug reports or feature requests
- **GitHub Discussions**: For questions or design discussions
- **PR Comments**: For implementation feedback

## License

By contributing to this project, you agree that your contributions will be licensed under the same MIT License as the project.

---

**Thank you for contributing to AWS Lambda Host!** 🚀
