(**
---
title: Amazon Bedrock
category: Resources
categoryindex: 22
---

# Amazon Bedrock: Generative AI with FsCDK

Amazon Bedrock is a fully managed service that makes foundation models (FMs) from leading AI companies available through a unified API. FsCDK provides type-safe builders for Bedrock Agents, Knowledge Bases, Data Sources, and Guardrails.

## What is Amazon Bedrock?

Amazon Bedrock lets you build and scale generative AI applications using foundation models from Anthropic (Claude), Amazon (Titan), Meta (Llama), and others. Key components include:

- **Agents** - Autonomous AI assistants that can plan and execute tasks
- **Knowledge Bases** - RAG (Retrieval-Augmented Generation) with your data
- **Data Sources** - Connect S3, web crawlers, and other data to Knowledge Bases
- **Guardrails** - Content filtering and safety controls for AI responses

## Quick Start
*)

#r "../src/bin/Release/net8.0/publish/Amazon.JSII.Runtime.dll"
#r "../src/bin/Release/net8.0/publish/Constructs.dll"
#r "../src/bin/Release/net8.0/publish/Amazon.CDK.Lib.dll"
#r "../src/bin/Release/net8.0/publish/System.Text.Json.dll"
#r "../src/bin/Release/net8.0/publish/FsCDK.dll"

open Amazon.CDK
open Amazon.CDK.AwsBedrock
open FsCDK

(*** hide ***)
module Config =
    let get () =
        {| Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT")
           Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION") |}

let config = Config.get ()

stack "BasicBedrockStack" {
    description "Basic Bedrock Agent"

    let myAgent =
        bedrockAgent "MyAssistant" {
            foundationModel "anthropic.claude-3-sonnet-20240229-v1:0"
            instruction "You are a helpful assistant that answers questions about our products."
            description "Customer support assistant"
            agentResourceRoleArn "arn:aws:iam::123456789012:role/BedrockAgentRole"
        }

    ()
}

(**
## Use Cases

### AI Agent with Custom Instructions
*)

stack "AgentStack" {
    description "Bedrock Agent with full configuration"

    let supportAgent =
        bedrockAgent "CustomerSupportAgent" {
            foundationModel "anthropic.claude-3-sonnet-20240229-v1:0"

            instruction
                "You are a customer support agent. Help users with order tracking, returns, and product questions."

            description "Handles customer inquiries"
            agentResourceRoleArn "arn:aws:iam::123456789012:role/BedrockAgentRole"
            idleSessionTtlInSeconds 1800
            autoPrepare true
            tags [ "Team", "AI"; "Environment", "Production" ]
        }

    ()
}

(**
### Knowledge Base with RAG
*)

stack "KnowledgeBaseStack" {
    description "Bedrock Knowledge Base for document retrieval"

    let kbConfig =
        CfnKnowledgeBase.KnowledgeBaseConfigurationProperty(
            Type = "VECTOR",
            VectorKnowledgeBaseConfiguration =
                CfnKnowledgeBase.VectorKnowledgeBaseConfigurationProperty(
                    EmbeddingModelArn = "arn:aws:bedrock:us-east-1::foundation-model/amazon.titan-embed-text-v2:0"
                )
        )

    let storageConfig =
        CfnKnowledgeBase.StorageConfigurationProperty(
            Type = "OPENSEARCH_SERVERLESS",
            OpensearchServerlessConfiguration =
                CfnKnowledgeBase.OpenSearchServerlessConfigurationProperty(
                    CollectionArn = "arn:aws:aoss:us-east-1:123456789012:collection/my-collection",
                    FieldMapping =
                        CfnKnowledgeBase.OpenSearchServerlessFieldMappingProperty(
                            MetadataField = "metadata",
                            TextField = "text",
                            VectorField = "vector"
                        ),
                    VectorIndexName = "my-index"
                )
        )

    let productKB =
        bedrockKnowledgeBase "ProductDocsKB" {
            description "Knowledge base for product documentation"
            roleArn "arn:aws:iam::123456789012:role/BedrockKBRole"
            knowledgeBaseConfiguration kbConfig
            storageConfiguration storageConfig
            tags [ "Team", "AI" ]
        }

    ()
}

(**
### Data Source from S3
*)

stack "DataSourceStack" {
    description "Bedrock Data Source connected to S3"

    let s3Config =
        CfnDataSource.DataSourceConfigurationProperty(
            Type = "S3",
            S3Configuration = CfnDataSource.S3DataSourceConfigurationProperty(BucketArn = "arn:aws:s3:::my-docs-bucket")
        )

    let docsSource =
        bedrockDataSource "ProductDocsSource" {
            knowledgeBaseId "KBXXXXXXXX"
            description "S3 bucket containing product documentation"
            dataSourceConfiguration s3Config
            dataDeletionPolicy "RETAIN"
        }

    ()
}

(**
### Content Safety Guardrail
*)

stack "GuardrailStack" {
    description "Bedrock Guardrail for content safety"

    let safetyGuardrail =
        bedrockGuardrail "ContentSafety" {
            description "Filters harmful and inappropriate content"
            blockedInputMessaging "Your input contains content that is not allowed."
            blockedOutputsMessaging "The response was filtered for safety."

            contentPolicyConfig (
                CfnGuardrail.ContentPolicyConfigProperty(
                    FiltersConfig =
                        [| CfnGuardrail.ContentFilterConfigProperty(
                               Type = "SEXUAL",
                               InputStrength = "HIGH",
                               OutputStrength = "HIGH"
                           )
                           CfnGuardrail.ContentFilterConfigProperty(
                               Type = "VIOLENCE",
                               InputStrength = "HIGH",
                               OutputStrength = "HIGH"
                           )
                           CfnGuardrail.ContentFilterConfigProperty(
                               Type = "HATE",
                               InputStrength = "HIGH",
                               OutputStrength = "HIGH"
                           ) |]
                )
            )

            tags [ "Environment", "Production" ]
        }

    ()
}

(**
## Complete Production Example
*)

stack "ProductionBedrockStack" {
    env (
        environment {
            account config.Account
            region config.Region
        }
    )

    description "Production Bedrock AI platform"
    tags [ "Environment", "Production"; "ManagedBy", "FsCDK" ]

    // Content safety guardrail
    let guardrail =
        bedrockGuardrail "ProductionSafety" {
            description "Production content safety filters"
            blockedInputMessaging "This input is not permitted."
            blockedOutputsMessaging "This response has been filtered."

            contentPolicyConfig (
                CfnGuardrail.ContentPolicyConfigProperty(
                    FiltersConfig =
                        [| CfnGuardrail.ContentFilterConfigProperty(
                               Type = "SEXUAL",
                               InputStrength = "HIGH",
                               OutputStrength = "HIGH"
                           )
                           CfnGuardrail.ContentFilterConfigProperty(
                               Type = "VIOLENCE",
                               InputStrength = "HIGH",
                               OutputStrength = "HIGH"
                           ) |]
                )
            )

            topicPolicyConfig (
                CfnGuardrail.TopicPolicyConfigProperty(
                    TopicsConfig =
                        [| CfnGuardrail.TopicConfigProperty(
                               Name = "Financial Advice",
                               Definition = "Providing specific investment or financial planning advice",
                               Type = "DENY"
                           ) |]
                )
            )

            kmsKeyArn "arn:aws:kms:us-east-1:123456789012:key/my-key"
            tags [ "Environment", "Production" ]
        }

    // AI Agent
    let agent =
        bedrockAgent "ProductionAgent" {
            foundationModel "anthropic.claude-3-sonnet-20240229-v1:0"
            instruction "You are a helpful product assistant. Answer questions accurately based on the knowledge base."
            description "Production customer-facing AI agent"
            agentResourceRoleArn "arn:aws:iam::123456789012:role/BedrockAgentRole"
            idleSessionTtlInSeconds 3600
            autoPrepare true
            tags [ "Environment", "Production" ]
        }

    ()
}

(**
## Best Practices

### Security
- Use **KMS encryption** for guardrails and agents handling sensitive data
- Apply **least-privilege IAM roles** for agent resource roles
- Enable **guardrails** on all customer-facing agents to prevent harmful content
- Use **topic policies** to prevent the agent from discussing prohibited subjects

### Cost
- Choose the right **foundation model** for your use case (smaller models for simpler tasks)
- Set appropriate **idle session TTL** to avoid unnecessary session costs
- Use **data deletion policies** (RETAIN vs DELETE) based on compliance needs

### Reliability
- Set **autoPrepare** to true (default) so agents are ready after deployment
- Use **content filters** at HIGH strength for production workloads
- Test guardrails thoroughly before deploying to production

### Operational Excellence
- Tag all Bedrock resources for cost allocation and tracking
- Use separate agents for different use cases rather than one monolithic agent
- Version your knowledge base data sources for reproducibility

## Default Settings

| Setting | Default | Rationale |
|---------|---------|-----------|
| `autoPrepare` | `true` | Agent is immediately available after deployment |

## Escape Hatch

Access the underlying CDK resources via the mutable `Agent`, `KnowledgeBase`, `DataSource`, or `Guardrail` properties on the spec records after stack synthesis for advanced scenarios not covered by these builders.

## Learning Resources

- [Amazon Bedrock Developer Guide](https://docs.aws.amazon.com/bedrock/latest/userguide/) - Official AWS documentation
- [Bedrock Agents](https://docs.aws.amazon.com/bedrock/latest/userguide/agents.html) - Building autonomous AI agents
- [Knowledge Bases for Bedrock](https://docs.aws.amazon.com/bedrock/latest/userguide/knowledge-base.html) - RAG with your data
- [Bedrock Guardrails](https://docs.aws.amazon.com/bedrock/latest/userguide/guardrails.html) - Content safety controls
*)

(*** hide ***)
()
