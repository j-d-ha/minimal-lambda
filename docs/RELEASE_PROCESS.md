# Release Process

This document describes the complete release process for AWS Lambda Host packages on NuGet.

## Overview

The release process is fully automated using GitHub Actions workflows and Release Drafter. The process involves:

1. **Conventional Commits** - All commits follow conventional commits format
2. **Release Drafter** - Automatically prepares changelog
3. **Manual Release** - Maintainer publishes the release
4. **Automated Publishing** - GitHub Actions publishes to NuGet.org

## Step-by-Step Release Process

### Prerequisites

- You have write access to the repository
- `NUGET_API_KEY` secret is configured in GitHub (see [Setup](#setup))
- All changes are merged to `main` branch
- All CI checks pass

### Release Steps

#### 1. Review the Draft Release

1. Go to [GitHub Releases](https://github.com/j-d-ha/aws-lambda-host/releases)
2. You should see a **Draft** release labeled "v[next-version]"
3. Review the changelog - it's automatically generated from PR titles
4. Check if the version number is correct

**Note:** The version is calculated automatically based on commit types:
- `fix:` commits → patch version bump (e.g., 1.0.0 → 1.0.1)
- `feat:` commits → minor version bump (e.g., 1.0.0 → 1.1.0)
- `BREAKING CHANGE` footer → major version bump (e.g., 1.0.0 → 2.0.0)

#### 2. (Optional) Manually Bump Version

If automatic version calculation isn't suitable, manually trigger the version bump:

1. Go to [Actions > Bump Version](https://github.com/j-d-ha/aws-lambda-host/actions/workflows/bump-version.yaml)
2. Click "Run workflow"
3. Select version bump type:
   - `patch` - Bug fixes
   - `minor` - New features
   - `major` - Breaking changes
   - `prerelease` - Pre-release version

This will:
- Update `Directory.Build.props` with new version
- Create a commit and tag
- Trigger Release Drafter to update the draft

#### 3. Publish the Release

1. Go to [GitHub Releases](https://github.com/j-d-ha/aws-lambda-host/releases)
2. Click on the Draft release
3. Edit the release if needed:
   - Update release name/description
   - Mark as pre-release if alpha/beta (optional)
4. Click **Publish release**

This automatically triggers the publish workflow which:
- Builds all packages
- Validates versions haven't been published
- Publishes to NuGet.org
- Uploads packages as release artifacts

#### 4. Verify Publishing

1. Check the [Publish to NuGet workflow](https://github.com/j-d-ha/aws-lambda-host/actions/workflows/publish-to-nuget.yaml)
2. Wait for the workflow to complete (usually 2-5 minutes)
3. Verify packages on [NuGet.org](https://www.nuget.org/packages?q=AwsLambda.Host)

**Published packages:**
- [AwsLambda.Host](https://www.nuget.org/packages/AwsLambda.Host/)
- [AwsLambda.Host.Abstractions](https://www.nuget.org/packages/AwsLambda.Host.Abstractions/)
- [AwsLambda.Host.OpenTelemetry](https://www.nuget.org/packages/AwsLambda.Host.OpenTelemetry/)

#### 5. Release Notes & Changelog

After the release is published, two additional workflows run automatically:

1. **Release Drafter** - Updates the GitHub release description with a generated changelog
2. **GREN (GitHub Release Notes)** - Generates detailed release notes and updates the `CHANGELOG.md` file

The changelog includes:
- Features, bug fixes, documentation, and other changes organized by type
- Links to related issues and PRs
- Author attribution
- Complete commit history links

Check the generated `CHANGELOG.md` in the repository and the release description on GitHub.

## Release Notes & Changelog Management

### GREN (GitHub Release Notes)

GREN automatically generates detailed release notes and maintains a `CHANGELOG.md` file.

#### What GREN Does

- Extracts information from merged PRs (titles, labels, authors)
- Groups changes by category (Features, Bug Fixes, etc.)
- Creates detailed release notes with links and attribution
- Updates `CHANGELOG.md` file with all releases
- Follows the established changelog format

#### GREN Configuration

GREN is configured in [`.grenrc.js`](.grenrc.js) with:
- **Data source**: PR information
- **Labels**: Maps PR labels to changelog sections
- **Template**: Customizable format for release notes
- **Changelog file**: `CHANGELOG.md` in repository root

#### How GREN Workflow Works

1. When a release is published on GitHub
2. GREN workflow triggers automatically
3. It analyzes all PRs since the previous release
4. Generates detailed release notes
5. Updates `CHANGELOG.md` with all releases
6. Commits the updated changelog back to repository

### Release Drafter

#### What is Release Drafter?

Release Drafter automatically creates draft releases by:
- Parsing PR titles (which follow conventional commits)
- Organizing changes by type (Features, Bug Fixes, etc.)
- Calculating version numbers based on change types
- Creating a nice changelog

### How it Works

1. Every push to `main` triggers Release Drafter
2. It analyzes all merged PRs since the last release
3. It updates the draft release with new changes
4. A maintainer publishes the draft when ready

### Configuration

Release Drafter is configured in [`.github/release-drafter.yml`](.github/release-drafter.yml).

Key settings:
- **Categories**: How changes are organized in changelog
- **Version Resolver**: Rules for calculating version bumps
- **Template**: Format of the changelog

## Versioning Strategy

### Semantic Versioning

All packages use [Semantic Versioning](https://semver.org/):

- **Major** (X.0.0): Breaking changes
- **Minor** (0.X.0): New features (backward compatible)
- **Patch** (0.0.X): Bug fixes (backward compatible)

### Version Location

Version is defined once in `/Directory.Build.props`:

```xml
<VersionPrefix>1.2.3</VersionPrefix>
```

This version applies to all 3 packages:
- AwsLambda.Host
- AwsLambda.Host.Abstractions
- AwsLambda.Host.OpenTelemetry

### Pre-release Versions

Pre-release versions use the format: `1.2.3-alpha.1`

Pre-release versions are handled via Release Drafter:
1. When you publish a release, check "Pre-release" checkbox
2. Pre-release packages are published to NuGet but marked as pre-release
3. Package managers don't install pre-release by default

## Conventional Commits Enforcement

### Why Conventional Commits?

Conventional commits allow:
- Automatic version calculation
- Automated changelog generation
- Clear communication of changes

### Format

```
<type>(scope): <description>

[optional body]

[optional footer(s)]
```

### Validation

Conventional commits are validated at:
1. **Pre-commit hook** (local) - Validates before committing (via commitlint)
2. **PR title validation** (GitHub) - Validates PR title follows conventional commits format

### Example Commits

```bash
# New feature
git commit -m "feat(host): add Lambda context helper"

# Bug fix
git commit -m "fix(abstractions): resolve NullReferenceException"

# Documentation
git commit -m "docs: add getting started guide"

# Breaking change
git commit -m "feat(host): redesign handler API

BREAKING CHANGE: Old Handler interface removed"
```

## Troubleshooting

### Draft Release Shows Wrong Version

**Cause**: Commits don't follow conventional commits format

**Solution**:
- Check PR titles follow `type(scope): description` format
- Rerun Release Drafter or manually bump version

### Publishing Failed

**Cause**: Package version already exists on NuGet

**Solution**:
- Check [NuGet.org](https://www.nuget.org/packages?q=AwsLambda.Host)
- If version exists, manually bump to new version
- Re-publish with new version

### NUGET_API_KEY Not Set

**Cause**: GitHub secret not configured

**Solution**:
1. Go to Repository Settings > Secrets and variables > Actions
2. Add new secret `NUGET_API_KEY` with your API key from [nuget.org](https://www.nuget.org/account/apikeys)
3. See [Setup](#setup) below

## Setup

### Configure NUGET_API_KEY Secret

1. Generate API key on [NuGet.org](https://www.nuget.org/account/apikeys):
   - Log in to your account
   - Go to API Keys
   - Create new key with "Push" scope
   - Copy the key (shown only once)

2. Add to GitHub repository:
   - Go to Repository Settings > Secrets and variables > Actions
   - Click "New repository secret"
   - Name: `NUGET_API_KEY`
   - Value: Paste your API key
   - Click "Add secret"

3. Verify secret is set (see in publish workflow logs)

### Local Setup

To enable commit message validation locally:

```bash
# Install dependencies
npm install

# Husky and commitlint will be automatically set up
# Try committing with invalid message to test:
git commit -m "invalid commit message"
# Should be rejected with helpful message
```

## FAQ

### Can I publish manually?

No, publishing is automated. Use the Release Drafter and GitHub Actions workflow.

### Can I release multiple versions?

No, you can only release one version at a time. Each release must be published separately.

### What if I need to release from an older version?

You can, but it's not recommended. Instead:
1. Ensure `main` is ahead of the old version
2. Manually bump version back to old version (if needed)
3. Follow normal release process

### How do I handle urgent patches?

1. Create a hotfix branch from the old release tag
2. Make your fix with conventional commits
3. Merge to `main`
4. Release normally (will bump patch version)

## See Also

- [CONTRIBUTING.md](.github/CONTRIBUTING.md) - Contribution guidelines
- [CLAUDE.md](CLAUDE.md) - Claude's role in the release workflow
- [Release Drafter Docs](https://github.com/release-drafter/release-drafter)
