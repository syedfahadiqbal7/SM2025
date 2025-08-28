# GitHub Branch Protection and Environment Setup

## Branch Protection Rules

### 1. Main Branch Protection
Navigate to: `Settings > Branches > Add rule`

**Rule name**: `main`
**Branch name pattern**: `main`

**Protect matching branches**:
- ✅ Require a pull request before merging
- ✅ Require approvals: `2` (minimum)
- ✅ Dismiss stale PR approvals when new commits are pushed
- ✅ Require review from code owners
- ✅ Require status checks to pass before merging
- ✅ Require branches to be up to date before merging
- ✅ Require signed commits
- ✅ Require linear history
- ✅ Require deployments to succeed before merging
- ✅ Restrict pushes that create files that match the specified pattern

### 2. UAT Branch Protection
**Rule name**: `uat`
**Branch name pattern**: `uat`

**Protect matching branches**:
- ✅ Require a pull request before merging
- ✅ Require approvals: `1` (minimum)
- ✅ Require status checks to pass before merging
- ✅ Require branches to be up to date before merging

### 3. SIT Branch Protection
**Rule name**: `sit`
**Branch name pattern**: `sit`

**Protect matching branches**:
- ✅ Require a pull request before merging
- ✅ Require approvals: `1` (minimum)
- ✅ Require status checks to pass before merging

### 4. Develop Branch Protection
**Rule name**: `develop`
**Branch name pattern**: `develop`

**Protect matching branches**:
- ✅ Require a pull request before merging
- ✅ Require approvals: `1` (minimum)
- ✅ Require status checks to pass before merging

## Environment Setup

### 1. SIT Environment
Navigate to: `Settings > Environments > New environment`

**Environment name**: `sit`
**Environment protection rules**:
- ✅ Required reviewers: Add team members
- ✅ Wait timer: `0` minutes
- ✅ Deployment branches**: `sit` branch only

### 2. UAT Environment
**Environment name**: `uat`
**Environment protection rules**:
- ✅ Required reviewers: Add team members
- ✅ Wait timer: `0` minutes
- ✅ Deployment branches**: `uat` branch only

### 3. Production Environment
**Environment name**: `production`
**Environment protection rules**:
- ✅ Required reviewers: Add senior team members
- ✅ Wait timer: `5` minutes (for safety)
- ✅ Deployment branches**: `main` branch only

## Required Status Checks

### Build and Test Checks
- `build-and-test` (required)
- `deploy-sit` (required for sit branch)
- `deploy-uat` (required for uat branch)
- `deploy-production` (required for main branch)

## Code Owners Setup

Create `.github/CODEOWNERS` file:

```
# Global code owners
* @syedfahadiqbal7

# API specific
AFFZ_API/ @api-team

# Admin specific
AFFZ_Admin/ @admin-team

# Customer specific
AFFZ_MVC/ @customer-team

# Provider specific
AFFZ_Provider/ @provider-team

# CI/CD and infrastructure
.github/ @devops-team
scripts/ @devops-team
environments/ @devops-team
```

## Security Settings

### 1. Repository Security
- Enable Dependabot alerts
- Enable Dependabot security updates
- Enable secret scanning
- Enable push protection

### 2. Actions Security
- Require approval for first-time contributors
- Allow GitHub Actions to create and approve pull requests
- Allow GitHub Actions to create and approve pull requests from outside collaborators

### 3. Advanced Security
- Enable CodeQL analysis
- Enable secret scanning
- Enable dependency review

## Deployment Workflow

### 1. Feature Development
```
feature-branch → develop → sit → uat → main
```

### 2. Hotfix Process
```
hotfix-branch → main → production
```

### 3. Release Process
```
develop → sit → uat → main → production
```

## Monitoring and Alerts

### 1. Required Checks
- All status checks must pass
- All required reviews must be approved
- Branch must be up to date

### 2. Deployment Verification
- Environment deployment status
- Application health checks
- Performance metrics

### 3. Rollback Procedures
- Automated rollback on deployment failure
- Manual rollback procedures
- Database rollback scripts

## Best Practices

### 1. Code Quality
- Use meaningful commit messages
- Write comprehensive tests
- Follow coding standards
- Regular code reviews

### 2. Security
- Never commit secrets
- Use environment variables
- Regular security audits
- Dependency updates

### 3. Documentation
- Keep README updated
- Document deployment procedures
- Maintain troubleshooting guides
- Update environment configurations
