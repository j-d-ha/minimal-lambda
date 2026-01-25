# Open PR for Current Branch

## Overview
Creates a pull request for the current Git branch with strict conventional commit format validation.

**Usage:** When the user asks to "open a PR", "create a pull request", or similar.

**Additional Instructions:** $ARGUMENTS

---

## Workflow

### 1. Pre-flight Checks
Run these checks in parallel:
- Current branch is NOT `main`, `master`, or `develop`
- Branch has commits that aren't on the base branch
- No PR already exists for this branch (check with `gh pr list --head <branch>`)

If branch is not pushed to remote, push it automatically (git will prompt for confirmation if needed).

### 2. Determine PR Title
Be autonomous about title selection:

1. **If the most recent commit follows conventional format** → Use it automatically
2. **If commits are unclear** → Analyze the diff and suggest a title
3. **Only ask the user if there's genuine ambiguity** (e.g., changes span multiple scopes)

**Validate title format** before proceeding (see format rules below)

### 3. Create the PR Body
Analyze the changes and create appropriate PR body content:
- For simple PRs (1-2 commits, single file): Use minimal template
- For complex PRs (multiple commits, many files): Use full template with all sections

### 4. Create the PR
Use heredoc to avoid creating temporary files:

```bash
gh pr create \
  --title "<validated-title>" \
  --base main \
  --body "$(cat <<'EOF'
# 🚀 Pull Request

## 📋 Summary

[Your analysis of what changed and why]

---

## ✅ Checklist

- [x] My changes build cleanly
- [x] I've added/updated relevant tests
- [ ] I've added/updated documentation or README
- [x] I've followed the coding style for this project
- [x] I've tested the changes locally (if applicable)

---

## 🧪 Related Issues or PRs

[If applicable: Closes #...]

---

## 💬 Notes for Reviewers

[Any specific areas to look at, or remove this section]
EOF
)"
```

---

## PR Title Format (STRICT VALIDATION)

**Format:** `<type>(<scope>): <description>`

- **Type**: REQUIRED - Must be one of the valid types below
- **Scope**: OPTIONAL - If used, must be one of the valid scopes below
- **Description**: REQUIRED - Brief summary in imperative mood

⚠️ **CI will fail if the title format is incorrect**

### Valid Types (REQUIRED)

- `feat` - New feature
- `fix` - Bug fix
- `docs` - Documentation changes
- `refactor` - Code refactoring
- `test` - Test changes
- `chore` - Maintenance tasks
- `ci` - CI/CD changes

### Valid Scopes (OPTIONAL)

The `<scope>` is OPTIONAL. If included, it MUST be one of these exact values:

- `host` - Core Lambda hosting functionality
- `envelopes` - Lambda envelope/event handling
- `abstractions` - Abstractions package
- `opentelemetry` - OpenTelemetry integration
- `source-generators` - Source generator code
- `deps` - Dependency updates
- `build` - Build system changes
- `ci` - CI/CD configuration
- `github` - GitHub-specific files
- `core` - Core library changes
- `docs` - Documentation changes
- `testing` - Test infrastructure
- `tests` - Test files

### Scope Rules (CRITICAL)

❌ **DO NOT** use:
- Class names as scopes (e.g., `test(DefaultLambdaCancellationFactory)`)
- Method names as scopes (e.g., `fix(CreateHandler)`)
- Arbitrary text as scopes (e.g., `feat(new-thing)`)
- File names as scopes (e.g., `fix(Program.cs)`)

✅ **DO** use:
- One of the predefined scopes above (e.g., `test(testing)`)
- No scope at all if none fit (e.g., `test: add failing test`)

### Title Examples

**Valid:**
- `feat(host): add new Lambda handler support`
- `fix(abstractions): resolve dependency issue`
- `test(testing): add intentionally failing test`
- `docs: update README with examples` ← no scope is fine
- `refactor: improve code organization` ← no scope is fine
- `chore(deps): update NuGet packages`
- `ci: add build caching`

**Invalid (will fail CI):**
- `test(DefaultLambdaCancellationFactory): add test` ← class name not allowed
- `fix(SomeMethod): fix bug` ← method name not allowed
- `feat(cool-feature): add feature` ← arbitrary scope not allowed
- `Add new feature` ← missing type
- `feat(core) add handler` ← missing colon
- `FEAT(core): add handler` ← type must be lowercase

### Handling Type/Scope Overlap

When a type and scope have the same name (e.g., `docs`, `ci`):
- `docs: update README` ← Use when it's ONLY documentation changes
- `feat(docs): add documentation generator` ← Use when it's a feature that affects docs
- `ci: update GitHub Actions workflow` ← Use when it's ONLY CI changes
- `fix(ci): correct build script path` ← Use when fixing something in the CI scope

---

## Additional Template Sections (for complex PRs)

For complex PRs with many changes, add these optional sections:

```markdown
## 🔄 Changes

- Change 1
- Change 2
- Change 3

---

## 🧑‍💻 Testing

Describe how the changes were tested, specific test cases run, or manual testing performed.
```

Add these sections AFTER the Summary but BEFORE the Checklist when appropriate.

---

## Title Validation Process

Before creating the PR, validate the title:

1. **Check format**: Must match `<type>(<scope>): <description>` or `<type>: <description>`
2. **Validate type**: Must be one of: `feat`, `fix`, `docs`, `refactor`, `test`, `chore`, `ci`
3. **Validate scope** (if present): Must be one of the predefined scopes listed above
4. **Check description**: Must be present and not empty

If validation fails, explain the error and ask for a corrected title.

---

## Tips

- **Auto-suggest titles**: If the most recent commit message follows conventional format, use it automatically
- **Infer scope**: Based on which files were changed, suggest an appropriate scope
- **Draft PRs**: Add `--draft` flag if the user mentions the PR is work-in-progress
- **Reviewers**: Add `--reviewer <username>` if the user specifies reviewers
- **Labels**: Add `--label <label>` if the user mentions specific labels