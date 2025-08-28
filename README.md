# SmartCenter CI/CD Pipeline

## Project Overview
SmartCenter is a comprehensive .NET application suite consisting of multiple microservices and web applications.

## Applications
- **AFFZ_API** - REST API service
- **AFFZ_Admin** - Administrative web application
- **AFFZ_Customer** - Customer-facing web application
- **AFFZ_Provider** - Provider management application

## Environment Structure
```
SM2025/
├── sit/                    # System Integration Testing
├── uat/                    # User Acceptance Testing
├── production/             # Production environment
│   ├── AFFZ_API/
│   ├── AFFZ_Admin/
│   ├── AFFZ_Customer/
│   └── AFFZ_Provider/
├── environments/           # Environment configurations
├── scripts/               # Deployment scripts
└── .github/workflows/     # CI/CD workflows
```

## CI/CD Pipeline

### Workflow Triggers
- **Push to main**: Triggers production deployment
- **Push to uat**: Triggers UAT deployment and production promotion
- **Push to sit**: Triggers SIT deployment
- **Pull requests**: Triggers build and test validation

### Deployment Process

#### 1. SIT Environment
- Automated build and test
- Deploy to SIT folder
- Environment-specific configuration applied

#### 2. UAT Environment
- Automated build and test
- Deploy to UAT folder
- Environment-specific configuration applied
- Option to promote to production

#### 3. Production Environment
- Automated build and test
- Deploy to production folder
- Create GitHub release
- Environment-specific configuration applied

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- Git
- PowerShell (for Windows deployment scripts)

### Local Development
```bash
# Clone the repository
git clone https://github.com/syedfahadiqbal7/SM2025.git

# Navigate to project directory
cd SM2025

# Restore dependencies
dotnet restore AFFZ_11012025.sln

# Build solution
dotnet build AFFZ_11012025.sln
```

### Deployment Scripts

#### SIT Deployment
```powershell
.\scripts\deploy-sit.ps1
```

#### UAT Deployment
```powershell
.\scripts\deploy-uat.ps1
```

#### Production Deployment
```powershell
.\scripts\deploy-production.ps1
```

## Branch Strategy

### Main Branches
- **main**: Production-ready code
- **develop**: Development integration branch
- **sit**: System Integration Testing
- **uat**: User Acceptance Testing

### Feature Development
- Create feature branches from `develop`
- Submit pull requests to `develop`
- Merge to `sit` for testing
- Promote to `uat` after SIT validation
- Deploy to `production` after UAT approval

## Environment Configuration

### SIT Environment
- Database: `SmartCenter_SIT`
- Log Level: Information
- Base URLs: `sit-*.smartcenter.com`

### UAT Environment
- Database: `SmartCenter_UAT`
- Log Level: Information
- Base URLs: `uat-*.smartcenter.com`

### Production Environment
- Database: `SmartCenter_Production`
- Log Level: Warning/Error
- Base URLs: `*.smartcenter.com`

## Monitoring and Logging

### Deployment Verification
- Each deployment creates a manifest file
- GitHub Actions provide deployment status
- Automated testing ensures application health

### Log Management
- Environment-specific logging configurations
- Centralized log aggregation
- Performance monitoring and alerting

## Security Considerations

### Environment Protection
- GitHub environment protection rules
- Required reviewers for production deployments
- Automated security scanning

### Configuration Management
- Environment-specific appsettings.json files
- Secure connection string management
- API key and secret protection

## Troubleshooting

### Common Issues
1. **Build Failures**: Check .NET SDK version compatibility
2. **Deployment Errors**: Verify environment configurations
3. **Database Connection**: Ensure connection strings are correct

### Support
For technical support and questions, please contact the development team or create an issue in the repository.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request
5. Ensure all tests pass
6. Get code review approval

## License
This project is proprietary and confidential.
