# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- `httpApi` creates a real HTTP API: Lambda/integration routes, `publicRoute`, `routeSpec`, Cognito and JWT authorizers, custom domain, default-stage throttling, JSON access logs, `let!` bind to `HttpApi`
- `webAcl` WAFv2 builder: managed rule groups, rate limits, REGIONAL/CLOUDFRONT scope, associations, logging with redacted fields
- `bucketDeployment` builder (asset upload with CloudFront invalidation) and `cfnOutput` builder
- CloudFront `OriginAccessControlOrigin` origin (refuses `aws/s3`-key buckets OAC cannot read), `errorResponse`, `spaErrorResponses`, `spaRewrite` (CloudFront Function on the default behavior); `let!` bind to `IDistribution`
- `githubActionsRole` builder for GitHub Actions OIDC deploy roles (repository + branch/tag/environment `sub` conditions, `cdkDeployAccess`)
- `cloudwatchAlarm` `alarmAction`, `okAction`, `insufficientDataAction` (SNS) and `actionsEnabled`
- `let!` binds for `rdsInstance` (`DatabaseInstance`), `rdsProxy`, `oidcProvider`, `userPoolResourceServer`, `bucketDeployment`, `webAcl`
- `samples/serverless-web`: CloudFront + OAC SPA, HTTP API with Cognito JWT to a .NET Lambda (SnapStart), DynamoDB, WAF

### Fixed
- `rdsInstance` and `cloudFrontDistribution` constructs are stored on the spec instead of discarded
- `userPoolClient` stores its construct, so `let!` on it works
- `rdsProxy` `proxyTarget` accepts database instances (the overload always resolved to clusters)
- `HttpApiHelpers.corsPermissive` no longer combines `*` origins with credentials

## [0.2.0] - 2025-12-10

### Added
- SNS subscription builders
- DynamoDB Global Secondary Index (GSI) and Local Secondary Index (LSI) builders
- CloudWatch Dashboard builder

### Fixed
- DeadLetterQueue CE: fixed strict evaluation bug in Option.defaultValue
- SNS and SQS builder improvements

## [0.1.0] - 2025-11-25

### Added
- Lambda functions
- S3 buckets
- DynamoDB tables
- SNS topics
- SQS queues
- VPC with public
- Security Groups
- RDS PostgreSQL
- CloudFront

[unreleased]: https://github.com/totallymoney/FsCDK/compare/0.2.0...HEAD
[0.2.0]: https://github.com/totallymoney/FsCDK/compare/0.1.0...0.2.0
[0.1.0]: https://github.com/totallymoney/FsCDK/releases/tag/0.1.0
