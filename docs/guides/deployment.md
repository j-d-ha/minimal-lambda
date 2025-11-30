# Deployment

This guide covers deploying AWS Lambda functions built with aws-lambda-host using various Infrastructure as Code (IaC) tools and CI/CD pipelines.

---

## Deployment Options

You can deploy Lambda functions using:

- **AWS SAM (Serverless Application Model)** - AWS-native IaC framework optimized for serverless
- **AWS CDK (Cloud Development Kit)** - Type-safe infrastructure using C#
- **Terraform** - Cloud-agnostic IaC tool
- **Manual Deployment** - AWS CLI or Console
- **CI/CD Pipelines** - Automated deployment with GitHub Actions, GitLab CI, or Azure DevOps

---

## Project Configuration

### Standard Lambda (.csproj)

For traditional Lambda functions with JIT compilation:

```xml title="MyLambda.csproj" linenums="1"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>

    <!-- Lambda-specific settings -->
    <AWSProjectType>Lambda</AWSProjectType>
    <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
    <PublishReadyToRun>true</PublishReadyToRun>

    <!-- Required for source generation -->
    <InterceptorsNamespaces>$(InterceptorsNamespaces);AwsLambda.Host</InterceptorsNamespaces>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="AwsLambda.Host" Version="1.0.1-beta.5" />
    <PackageReference Include="Amazon.Lambda.RuntimeSupport" Version="1.10.0" />
  </ItemGroup>
</Project>
```

**Build and publish**:

```bash
dotnet publish -c Release -o ./publish
```

### Native AOT Lambda (.csproj)

For optimized Lambda functions with Native AOT (faster cold starts, smaller package size):

```xml title="MyLambdaAot.csproj" linenums="1"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>

    <!-- Native AOT settings -->
    <PublishAot>true</PublishAot>
    <StripSymbols>true</StripSymbols>
    <JsonSerializerIsReflectionEnabledByDefault>false</JsonSerializerIsReflectionEnabledByDefault>
    <TrimMode>full</TrimMode>
    <PublishTrimmed>true</PublishTrimmed>

    <!-- Lambda requires 'bootstrap' as the executable name for custom runtimes -->
    <AssemblyName>bootstrap</AssemblyName>

    <!-- Lambda-specific settings -->
    <AWSProjectType>Lambda</AWSProjectType>
    <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>

    <!-- Required for source generation -->
    <InterceptorsNamespaces>$(InterceptorsNamespaces);AwsLambda.Host</InterceptorsNamespaces>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="AwsLambda.Host" Version="1.0.1-beta.5" />
    <PackageReference Include="Amazon.Lambda.RuntimeSupport" Version="1.10.0" />
  </ItemGroup>
</Project>
```

**Build and publish for AOT**:

```bash
# Publish with Native AOT (requires Docker for cross-compilation on non-Linux)
dotnet publish -c Release -o ./publish /p:PublishAot=true

# Or use the AWS Lambda build container
docker run --rm -v $(pwd):/var/task public.ecr.aws/sam/build-dotnet8:latest \
  dotnet publish -c Release -o /var/task/publish /p:PublishAot=true
```

!!! tip "Native AOT Benefits"
    - **Faster cold starts**: 50-80% reduction in cold start times
    - **Smaller package size**: Trimming removes unused code
    - **Lower memory usage**: More efficient resource utilization
    - **Predictable performance**: No JIT compilation overhead

---

## AWS SAM Deployment

AWS SAM provides the simplest deployment experience for Lambda functions.

### SAM Template

Create a `template.yaml` file in your project root:

```yaml title="template.yaml" linenums="1"
AWSTemplateFormatVersion: '2010-09-09'
Transform: AWS::Serverless-2016-10-31
Description: My Lambda Function

Globals:
  Function:
    Timeout: 30
    MemorySize: 512
    Runtime: provided.al2023
    Architectures:
      - x86_64

Resources:
  MyFunction:
    Type: AWS::Serverless::Function
    Properties:
      Handler: bootstrap
      CodeUri: ./publish
      Environment:
        Variables:
          ENVIRONMENT: production
          LOG_LEVEL: info
      Events:
        ApiEvent:
          Type: Api
          Properties:
            Path: /orders
            Method: post

Outputs:
  MyFunctionArn:
    Description: Lambda Function ARN
    Value: !GetAtt MyFunction.Arn
  ApiUrl:
    Description: API Gateway endpoint
    Value: !Sub "https://${ServerlessRestApi}.execute-api.${AWS::Region}.amazonaws.com/Prod/orders"
```

### SAM Commands

```bash title="Deployment commands"
# Build the Lambda function
dotnet publish -c Release -o ./publish

# Package and deploy (first time - guided)
sam deploy --guided

# Subsequent deployments
sam deploy

# Delete the stack
sam delete
```

**sam deploy --guided** prompts:

```
Stack Name: my-lambda-stack
AWS Region: us-east-1
Confirm changes before deploy: Y
Allow SAM CLI IAM role creation: Y
Disable rollback: N
Save arguments to configuration file: Y
SAM configuration file: samconfig.toml
```

### SAM with Native AOT

```yaml title="template.yaml (AOT)" linenums="1"
Resources:
  MyFunctionAot:
    Type: AWS::Serverless::Function
    Metadata:
      BuildMethod: dotnet8
      BuildArchitecture: x86_64
    Properties:
      Handler: bootstrap
      Runtime: provided.al2023
      CodeUri: ./
      Architectures:
        - x86_64
      MemorySize: 512
      Timeout: 30
```

```bash
# SAM builds with Docker automatically
sam build

# Deploy
sam deploy --guided
```

---

## AWS CDK Deployment

AWS CDK allows you to define infrastructure using C#.

### CDK Stack

```csharp title="MyLambdaStack.cs" linenums="1"
using Amazon.CDK;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.APIGateway;
using Constructs;

namespace MyCdkApp
{
    public class MyLambdaStack : Stack
    {
        public MyLambdaStack(Construct scope, string id, IStackProps props = null)
            : base(scope, id, props)
        {
            // Lambda function with Native AOT
            var myFunction = new Function(this, "MyFunction", new FunctionProps
            {
                Runtime = Runtime.PROVIDED_AL2023,
                Handler = "bootstrap",
                Code = Code.FromAsset("../MyLambda/publish"),
                MemorySize = 512,
                Timeout = Duration.Seconds(30),
                Architecture = Architecture.X86_64,
                Environment = new Dictionary<string, string>
                {
                    ["ENVIRONMENT"] = "production",
                    ["LOG_LEVEL"] = "info"
                }
            });

            // API Gateway
            var api = new RestApi(this, "MyApi", new RestApiProps
            {
                RestApiName = "My API",
                Description = "API for My Lambda Function"
            });

            var integration = new LambdaIntegration(myFunction);
            var orders = api.Root.AddResource("orders");
            orders.AddMethod("POST", integration);

            // Outputs
            new CfnOutput(this, "FunctionArn", new CfnOutputProps
            {
                Value = myFunction.FunctionArn,
                Description = "Lambda Function ARN"
            });

            new CfnOutput(this, "ApiUrl", new CfnOutputProps
            {
                Value = api.Url,
                Description = "API Gateway URL"
            });
        }
    }
}
```

### CDK Program

```csharp title="Program.cs" linenums="1"
using Amazon.CDK;

namespace MyCdkApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new App();

            new MyLambdaStack(app, "MyLambdaStack", new StackProps
            {
                Env = new Amazon.CDK.Environment
                {
                    Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                    Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION")
                }
            });

            app.Synth();
        }
    }
}
```

### CDK Commands

```bash title="CDK deployment"
# Install CDK CLI
npm install -g aws-cdk

# Initialize CDK project (one time)
mkdir cdk && cd cdk
cdk init app --language csharp

# Build Lambda function
cd ../MyLambda
dotnet publish -c Release -o ./publish

# Deploy
cd ../cdk
cdk deploy

# Destroy
cdk destroy
```

---

## Terraform Deployment

Terraform provides cloud-agnostic infrastructure management.

### Terraform Configuration

```hcl title="main.tf" linenums="1"
terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

provider "aws" {
  region = var.aws_region
}

# Lambda IAM Role
resource "aws_iam_role" "lambda_role" {
  name = "my-lambda-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "lambda.amazonaws.com"
        }
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "lambda_basic" {
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
  role       = aws_iam_role.lambda_role.name
}

# Lambda Function
resource "aws_lambda_function" "my_function" {
  filename         = "../MyLambda/publish.zip"
  function_name    = "my-lambda-function"
  role            = aws_iam_role.lambda_role.arn
  handler         = "bootstrap"
  runtime         = "provided.al2023"
  architectures   = ["x86_64"]
  memory_size     = 512
  timeout         = 30

  source_code_hash = filebase64sha256("../MyLambda/publish.zip")

  environment {
    variables = {
      ENVIRONMENT = "production"
      LOG_LEVEL   = "info"
    }
  }
}

# API Gateway
resource "aws_apigatewayv2_api" "api" {
  name          = "my-api"
  protocol_type = "HTTP"
}

resource "aws_apigatewayv2_integration" "lambda_integration" {
  api_id           = aws_apigatewayv2_api.api.id
  integration_type = "AWS_PROXY"
  integration_uri  = aws_lambda_function.my_function.invoke_arn
}

resource "aws_apigatewayv2_route" "post_orders" {
  api_id    = aws_apigatewayv2_api.api.id
  route_key = "POST /orders"
  target    = "integrations/${aws_apigatewayv2_integration.lambda_integration.id}"
}

resource "aws_apigatewayv2_stage" "default" {
  api_id      = aws_apigatewayv2_api.api.id
  name        = "$default"
  auto_deploy = true
}

resource "aws_lambda_permission" "api_gateway" {
  statement_id  = "AllowAPIGatewayInvoke"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.my_function.function_name
  principal     = "apigateway.amazonaws.com"
  source_arn    = "${aws_apigatewayv2_api.api.execution_arn}/*/*"
}

# Outputs
output "function_arn" {
  value = aws_lambda_function.my_function.arn
}

output "api_url" {
  value = aws_apigatewayv2_stage.default.invoke_url
}
```

```hcl title="variables.tf" linenums="1"
variable "aws_region" {
  description = "AWS region"
  type        = string
  default     = "us-east-1"
}
```

### Terraform Commands

```bash title="Terraform deployment"
# Build and package Lambda
cd MyLambda
dotnet publish -c Release -o ./publish
cd publish && zip -r ../publish.zip . && cd ..

# Initialize Terraform
cd ../terraform
terraform init

# Plan deployment
terraform plan

# Apply deployment
terraform apply

# Destroy
terraform destroy
```

---

## CI/CD with GitHub Actions

Automate builds and deployments using GitHub Actions.

### GitHub Actions Workflow (SAM)

```yaml title=".github/workflows/deploy.yml" linenums="1"
name: Deploy Lambda

on:
  push:
    branches:
      - main

permissions:
  id-token: write
  contents: read

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Build Lambda
        run: |
          cd src/MyLambda
          dotnet publish -c Release -o ./publish

      - name: Configure AWS credentials
        uses: aws-actions/configure-aws-credentials@v4
        with:
          role-to-assume: arn:aws:iam::${{ secrets.AWS_ACCOUNT_ID }}:role/GitHubActionsRole
          aws-region: us-east-1

      - name: Setup SAM
        uses: aws-actions/setup-sam@v2

      - name: Deploy with SAM
        run: |
          sam deploy --no-confirm-changeset --no-fail-on-empty-changeset
```

### GitHub Actions Workflow (Native AOT with Docker)

```yaml title=".github/workflows/deploy-aot.yml" linenums="1"
name: Deploy Lambda (AOT)

on:
  push:
    branches:
      - main

permissions:
  id-token: write
  contents: read

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Build Lambda with Native AOT
        run: |
          docker run --rm \
            -v $(pwd)/src/MyLambda:/var/task \
            -v $(pwd)/publish:/var/task/publish \
            public.ecr.aws/sam/build-dotnet8:latest \
            dotnet publish -c Release -o /var/task/publish /p:PublishAot=true

      - name: Configure AWS credentials
        uses: aws-actions/configure-aws-credentials@v4
        with:
          role-to-assume: arn:aws:iam::${{ secrets.AWS_ACCOUNT_ID }}:role/GitHubActionsRole
          aws-region: us-east-1

      - name: Setup SAM
        uses: aws-actions/setup-sam@v2

      - name: Deploy with SAM
        run: |
          sam deploy --no-confirm-changeset --no-fail-on-empty-changeset
```

### CDK Deployment with GitHub Actions

```yaml title=".github/workflows/cdk-deploy.yml" linenums="1"
name: CDK Deploy

on:
  push:
    branches:
      - main

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - uses: actions/setup-node@v4
        with:
          node-version: '20'

      - name: Build Lambda
        run: |
          cd src/MyLambda
          dotnet publish -c Release -o ./publish

      - name: Configure AWS credentials
        uses: aws-actions/configure-aws-credentials@v4
        with:
          role-to-assume: ${{ secrets.AWS_ROLE_ARN }}
          aws-region: us-east-1

      - name: Install CDK
        run: npm install -g aws-cdk

      - name: Deploy CDK Stack
        run: |
          cd cdk
          dotnet build
          cdk deploy --require-approval never
```

---

## Manual Deployment

### Using AWS CLI

```bash title="Manual deployment with AWS CLI"
# Build Lambda
dotnet publish -c Release -o ./publish

# Package
cd publish && zip -r ../function.zip . && cd ..

# Create function (first time)
aws lambda create-function \
  --function-name my-lambda-function \
  --runtime provided.al2023 \
  --role arn:aws:iam::ACCOUNT_ID:role/lambda-role \
  --handler bootstrap \
  --zip-file fileb://function.zip \
  --architectures x86_64 \
  --memory-size 512 \
  --timeout 30

# Update function code
aws lambda update-function-code \
  --function-name my-lambda-function \
  --zip-file fileb://function.zip
```

---

## Environment-Specific Configuration

### Using appsettings.{Environment}.json

```json title="appsettings.Production.json"
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "MyApp": {
    "DatabaseConnectionString": "production-connection-string",
    "CacheEnabled": true
  }
}
```

**Set environment in Lambda**:

```yaml title="template.yaml"
Environment:
  Variables:
    ASPNETCORE_ENVIRONMENT: Production
```

### Using AWS Systems Manager Parameter Store

```csharp title="Program.cs" linenums="1"
using Amazon.Extensions.NETCore.Setup;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

var builder = LambdaApplication.CreateBuilder(args);

// Add AWS Systems Manager configuration
var ssmClient = new AmazonSimpleSystemsManagementClient();
var parameter = await ssmClient.GetParameterAsync(new GetParameterRequest
{
    Name = "/myapp/database-connection-string",
    WithDecryption = true
});

builder.Configuration.AddInMemoryCollection(new Dictionary<string, string>
{
    ["ConnectionStrings:Default"] = parameter.Parameter.Value
});

var lambda = builder.Build();
// ...
```

---

## Monitoring and Observability

### CloudWatch Logs

Lambda automatically sends logs to CloudWatch Logs. View logs:

```bash
# Tail logs in real-time
sam logs --tail --name MyFunction

# Or with AWS CLI
aws logs tail /aws/lambda/my-lambda-function --follow
```

### CloudWatch Metrics

Monitor Lambda performance:

```yaml title="template.yaml (SAM)"
Resources:
  MyFunction:
    Type: AWS::Serverless::Function
    Properties:
      # ... other properties

  # Alarm for errors
  ErrorAlarm:
    Type: AWS::CloudWatch::Alarm
    Properties:
      AlarmName: !Sub "${MyFunction}-errors"
      MetricName: Errors
      Namespace: AWS/Lambda
      Statistic: Sum
      Period: 300
      EvaluationPeriods: 1
      Threshold: 5
      ComparisonOperator: GreaterThanThreshold
      Dimensions:
        - Name: FunctionName
          Value: !Ref MyFunction
```

### OpenTelemetry Integration

```bash
dotnet add package AwsLambda.Host.OpenTelemetry
```

```csharp title="Program.cs" linenums="1"
using AwsLambda.Host.OpenTelemetry;

var builder = LambdaApplication.CreateBuilder(args);

// Add OpenTelemetry tracing
builder.Services.AddLambdaOpenTelemetry();

var lambda = builder.Build();
// ...
```

---

## Best Practices

### ✅ Do

- **Use Native AOT for production** - Faster cold starts and lower costs
- **Set appropriate memory and timeout** - Right-size based on profiling
- **Use environment variables for configuration** - Avoid hardcoding values
- **Implement health checks** - Monitor Lambda function health
- **Use blue/green deployments** - Zero-downtime deployments with SAM or CDK
- **Monitor with CloudWatch** - Set up alarms for errors and performance
- **Version your functions** - Use aliases for staged rollouts
- **Enable X-Ray tracing** - Understand performance bottlenecks
- **Use Parameter Store or Secrets Manager** - Secure configuration management
- **Automate deployments with CI/CD** - Consistent, repeatable deployments

### ❌ Don't

- **Don't hardcode secrets** - Use Secrets Manager or Parameter Store
- **Don't over-provision memory** - Start small and increase based on metrics
- **Don't deploy without testing** - Use sam local or integration tests
- **Don't ignore cold starts** - Use provisioned concurrency or AOT compilation
- **Don't skip monitoring** - Always configure CloudWatch alarms
- **Don't use default timeouts** - Tune based on actual execution time
- **Don't deploy directly to production** - Use staging environments

---

## Versioning

The current version of aws-lambda-host is **1.0.1-beta.5** (from `Directory.Build.props`).

```xml
<VersionPrefix>1.0.1-beta.5</VersionPrefix>
```

Update package references:

```bash
dotnet add package AwsLambda.Host --version 1.0.1-beta.5
```

---

## Key Takeaways

1. **Native AOT recommended** - Use for fastest cold starts and lowest costs
2. **Multiple deployment options** - SAM for simplicity, CDK for type safety, Terraform for multi-cloud
3. **CI/CD essential** - Automate builds and deployments with GitHub Actions
4. **Right-size resources** - Profile and adjust memory/timeout settings
5. **Monitor everything** - CloudWatch Logs, Metrics, and X-Ray tracing
6. **Secure configuration** - Use Parameter Store or Secrets Manager for sensitive data
7. **Infrastructure as Code** - Never manually configure Lambda in the console
8. **Environment separation** - Use separate stacks for dev/staging/production

---

## Next Steps

- **[Configuration](configuration.md)** - Configure Lambda host options
- **[Error Handling](error-handling.md)** - Handle errors and configure DLQs
- **[Testing](testing.md)** - Test Lambda functions before deployment
- **[OpenTelemetry Package](https://github.com/j-d-ha/aws-lambda-host/tree/main/src/AwsLambda.Host.OpenTelemetry)** - Add distributed tracing
