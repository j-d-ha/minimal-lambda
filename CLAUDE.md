# CLAUDE.md

## GENERAL

- Repo link: https://github.com/j-d-ha/aws-lambda-host
- GitHub Project Name: Lambda.Host Development
- All code in this project is designed to run on AWS Lambda or generate code that will then be run
  on AWS Lambda.
- When writing PRs, make sure to include any issues that where closed by the PR.

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