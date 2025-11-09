# CLAUDE.md

## GENERAL

- Repo link: https://github.com/j-d-ha/aws-lambda-host
- GitHub Project Name: Lambda.Host Development
- All code in this project is designed to run on AWS Lambda or generate code that will then be run
  on AWS Lambda.
- When writing PRs, ALWAYS use `./.github/pull_request_template.md` as the template for the PR.
-

## Code Style

### C# XML Documentation

- Only use XML tags that are supported by C#. As an example, do not use `<strong>`.

## MANDATORY WORKFLOW

### Task Classification

- **Simple tasks**: Single file edits <20 lines, docs, comments, formatting
- **Complex tasks**: Multi-file changes, new features, architecture changes

### Required Process

#### 1. Planning (MANDATORY)

- **MUST create PLAN.md** before starting any task
- Break down into numbered steps and sub-steps
- No implementation until plan approved

#### 2. Execution

- **Simple tasks**: Execute with minimal check-ins
- **Complex tasks**: Complete ONE step at a time, wait for user approval before proceeding
- **NEVER** continue to next step without explicit user sign-off

#### 3. Git Workflow

- Commit only after user approval
- Use clear commit messages
- Create feature branches for substantial changes
- Use conventional commits format:
  - ```text
    <type>[optional scope]: <description>

    [optional body]
    
    [optional footer(s)]
    ```  
  - Types: feat, fix, docs, refactor, test, chore

#### 4. Plan Template

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

## Release & Versioning Workflow

### Commit Message Requirements

When Claude makes commits, ALWAYS follow conventional commits format:

```
<type>(scope): <description>

[optional body]

[optional footer(s)]
```

**Types:** `feat`, `fix`, `docs`, `refactor`, `test`, `chore`, `ci`

**Scope (optional but recommended):** `host`, `abstractions`, `opentelemetry`, or omit for general
changes

**Examples:**

- `feat(host): add new Lambda handler support`
- `fix(abstractions): resolve dependency issue`
- `docs: update README with examples`
- `chore(deps): bump dependency version`

**Breaking Changes:** Include `BREAKING CHANGE:` in footer:

```
feat(host): redesign handler pipeline

BREAKING CHANGE: Handler API has changed
```

### Pull Request Title Requirements

When creating PRs, the title MUST follow conventional commits format (same rules as commit
messages):

- Strict validation is enforced by CI
- Format: `<type>(scope): <description>`
- Dependabot PRs are exempt from this requirement

**Examples:**

- `feat(host): add new Lambda handler support`
- `fix(abstractions): resolve dependency issue`
- `docs: update README with examples`

### Versioning

All 3 packages are versioned synchronously:

- `AwsLambda.Host`
- `AwsLambda.Host.Abstractions`
- `AwsLambda.Host.OpenTelemetry`

Versions are stored in `/Directory.Build.props` as `<VersionPrefix>`.

**Version bumping (automatic):**

- `fix:` commits → patch version bump (e.g., 1.0.0 → 1.0.1)
- `feat:` commits → minor version bump (e.g., 1.0.0 → 1.1.0)
- `BREAKING CHANGE` footer → major version bump (e.g., 1.0.0 → 2.0.0)

### Release Process

1. **Draft Release:** Release Drafter automatically creates a draft release on each push to main,
   organizing changes by type
2. **Manual Release:** User manually publishes the draft release from GitHub
3. **Automated Publishing:** Publishing a release triggers automatic NuGet package publishing to
   nuget.org
4. **Pre-release Designation:** Manual - user designates whether a release is pre-release (
   alpha/beta) or stable

### Release Notes & Changelog

The release process includes two automated tools:

1. **Release Drafter**: Creates draft releases with organized changelog from PR titles
2. **GREN**: Generates detailed release notes and maintains the `CHANGELOG.md` file

When a release is published:

- GREN automatically generates release notes with PR summaries and author attribution
- `CHANGELOG.md` is automatically updated with all releases
- GitHub release description is updated with the changelog

### Claude's Role in Release Workflow

- **DO** create commits following conventional commits format
- **DO** title PRs following conventional commits format
- **DO** reference the PR/commit scope when relevant
- **DO** include breaking change footers for incompatible changes
- **DO NOT** manually bump versions in Directory.Build.props (automatic)
- **DO NOT** publish to NuGet manually (automated on release)
- **DO NOT** create GitHub releases directly (use Release Drafter)
- **DO NOT** manually edit CHANGELOG.md (GREN updates it automatically)