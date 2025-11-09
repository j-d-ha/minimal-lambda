module.exports = {
  dataSource: 'prs',
  prefix: 'v',
  grouped: true,
  groupBy: 'label',
  onlyMilestones: false,
  ignoreLabels: ['skip-changelog', 'internal', 'wontfix'],
  changelogFilename: 'CHANGELOG.md',
  template: {
    release: (variables) => {
      return `# Release ${variables.release}

> ${variables.date}

## Changes in this Release

${variables.body}

## Packages

All 3 packages are included in this release:
- \`AwsLambda.Host\`
- \`AwsLambda.Host.Abstractions\`
- \`AwsLambda.Host.OpenTelemetry\`

These packages have been automatically published to [NuGet.org](https://www.nuget.org/packages?q=AwsLambda.Host).

---

**Full Changelog:** [\`${variables.previousTag}...${variables.currentTag}\`](https://github.com/j-d-ha/aws-lambda-host/compare/${variables.previousTag}...${variables.currentTag})
`;
    },
    group: (variables) => {
      return `\n### ${variables.heading}\n\n${variables.body}`;
    },
    changelogTitle: '# Changelog\n\nAll notable changes to this project will be documented in this file.\n',
    issue: (variables) => {
      return `- [${variables.name}](${variables.url}) - @${variables.author}`;
    },
  },
  labels: {
    'type: feat': { name: '🚀 Features', description: 'New Features' },
    'type: fix': { name: '🐛 Bug Fixes', description: 'Bug Fixes' },
    'type: docs': { name: '📚 Documentation', description: 'Documentation' },
    'type: refactor': { name: '🔄 Refactoring', description: 'Code Refactoring' },
    'type: test': { name: '✅ Tests', description: 'Test Updates' },
    'type: chore': { name: '🔧 Maintenance', description: 'Maintenance' },
    'type: ci': { name: '⚙️ CI/CD', description: 'CI/CD Changes' },
    'breaking': { name: '⚠️ Breaking Changes', description: 'Breaking Changes' },
    'breaking-change': { name: '⚠️ Breaking Changes', description: 'Breaking Changes' },
    'feature': { name: '🚀 Features', description: 'Features' },
    'bug': { name: '🐛 Bug Fixes', description: 'Bug Fixes' },
    'performance': { name: '⚡ Performance', description: 'Performance' },
  },
};
