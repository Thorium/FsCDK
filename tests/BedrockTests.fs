module FsCDK.Tests.BedrockTests

open Expecto
open FsCDK
open Amazon.CDK.AwsBedrock

[<Tests>]
let bedrockAgentTests =
    testList
        "Bedrock Agent DSL"
        [ test "fails when foundation model is missing" {
              let thrower () =
                  bedrockAgent "MyAgent" { () } |> ignore

              Expect.throws thrower "Bedrock Agent builder should throw when foundation model is missing"
          }

          test "sets agent name" {
              let spec =
                  bedrockAgent "MyAgent" {
                      foundationModel "anthropic.claude-3-sonnet-20240229-v1:0"
                  }

              Expect.equal spec.AgentName "MyAgent" "Agent name should match"
          }

          test "defaults constructId to agent name" {
              let spec =
                  bedrockAgent "MyAgent" {
                      foundationModel "anthropic.claude-3-sonnet-20240229-v1:0"
                  }

              Expect.equal spec.ConstructId "MyAgent" "ConstructId should default to agent name"
          }

          test "sets custom constructId" {
              let spec =
                  bedrockAgent "MyAgent" {
                      foundationModel "anthropic.claude-3-sonnet-20240229-v1:0"
                      constructId "CustomId"
                  }

              Expect.equal spec.ConstructId "CustomId" "ConstructId should match custom value"
          }

          test "sets foundation model" {
              let spec =
                  bedrockAgent "MyAgent" {
                      foundationModel "anthropic.claude-3-sonnet-20240229-v1:0"
                  }

              Expect.equal spec.Props.FoundationModel "anthropic.claude-3-sonnet-20240229-v1:0" "Foundation model should be set"
          }

          test "sets instruction" {
              let spec =
                  bedrockAgent "MyAgent" {
                      foundationModel "anthropic.claude-3-sonnet-20240229-v1:0"
                      instruction "You are a helpful assistant."
                  }

              Expect.equal spec.Props.Instruction "You are a helpful assistant." "Instruction should be set"
          }

          test "sets description" {
              let spec =
                  bedrockAgent "MyAgent" {
                      foundationModel "anthropic.claude-3-sonnet-20240229-v1:0"
                      description "A test agent"
                  }

              Expect.equal spec.Props.Description "A test agent" "Description should be set"
          }

          test "sets agentResourceRoleArn" {
              let spec =
                  bedrockAgent "MyAgent" {
                      foundationModel "anthropic.claude-3-sonnet-20240229-v1:0"
                      agentResourceRoleArn "arn:aws:iam::123456789012:role/BedrockAgentRole"
                  }

              Expect.equal
                  spec.Props.AgentResourceRoleArn
                  "arn:aws:iam::123456789012:role/BedrockAgentRole"
                  "AgentResourceRoleArn should be set"
          }

          test "sets idleSessionTtlInSeconds" {
              let spec =
                  bedrockAgent "MyAgent" {
                      foundationModel "anthropic.claude-3-sonnet-20240229-v1:0"
                      idleSessionTtlInSeconds 600
                  }

              Expect.isNotNull (box spec.Props.IdleSessionTtlInSeconds) "IdleSessionTtlInSeconds should be set"
          }

          test "autoPrepare defaults to true" {
              let spec =
                  bedrockAgent "MyAgent" {
                      foundationModel "anthropic.claude-3-sonnet-20240229-v1:0"
                  }

              Expect.isNotNull (box spec.Props.AutoPrepare) "AutoPrepare should be set"
          }

          test "mutable Agent starts as None" {
              let spec =
                  bedrockAgent "MyAgent" {
                      foundationModel "anthropic.claude-3-sonnet-20240229-v1:0"
                  }

              Expect.isNone spec.Agent "Agent should start as None"
          } ]
    |> testSequenced

[<Tests>]
let bedrockKnowledgeBaseTests =
    let dummyKBConfig =
        CfnKnowledgeBase.KnowledgeBaseConfigurationProperty(Type = "VECTOR")

    let dummyStorageConfig =
        CfnKnowledgeBase.StorageConfigurationProperty(Type = "OPENSEARCH_SERVERLESS")

    testList
        "Bedrock Knowledge Base DSL"
        [ test "fails when roleArn is missing" {
              let thrower () =
                  bedrockKnowledgeBase "MyKB" {
                      knowledgeBaseConfiguration dummyKBConfig
                      storageConfiguration dummyStorageConfig
                  }
                  |> ignore

              Expect.throws thrower "Should throw when roleArn is missing"
          }

          test "fails when knowledgeBaseConfiguration is missing" {
              let thrower () =
                  bedrockKnowledgeBase "MyKB" {
                      roleArn "arn:aws:iam::123456789012:role/KBRole"
                      storageConfiguration dummyStorageConfig
                  }
                  |> ignore

              Expect.throws thrower "Should throw when knowledgeBaseConfiguration is missing"
          }

          test "fails when storageConfiguration is missing" {
              let thrower () =
                  bedrockKnowledgeBase "MyKB" {
                      roleArn "arn:aws:iam::123456789012:role/KBRole"
                      knowledgeBaseConfiguration dummyKBConfig
                  }
                  |> ignore

              Expect.throws thrower "Should throw when storageConfiguration is missing"
          }

          test "fails when all required fields are missing" {
              let thrower () =
                  bedrockKnowledgeBase "MyKB" { () } |> ignore

              Expect.throws thrower "Should throw when all required fields are missing"
          }

          test "sets knowledge base name" {
              let spec =
                  bedrockKnowledgeBase "MyKB" {
                      roleArn "arn:aws:iam::123456789012:role/KBRole"
                      knowledgeBaseConfiguration dummyKBConfig
                      storageConfiguration dummyStorageConfig
                  }

              Expect.equal spec.KnowledgeBaseName "MyKB" "Knowledge base name should match"
          }

          test "sets description" {
              let spec =
                  bedrockKnowledgeBase "MyKB" {
                      roleArn "arn:aws:iam::123456789012:role/KBRole"
                      knowledgeBaseConfiguration dummyKBConfig
                      storageConfiguration dummyStorageConfig
                      description "Product docs KB"
                  }

              Expect.equal spec.Props.Description "Product docs KB" "Description should be set"
          }

          test "mutable KnowledgeBase starts as None" {
              let spec =
                  bedrockKnowledgeBase "MyKB" {
                      roleArn "arn:aws:iam::123456789012:role/KBRole"
                      knowledgeBaseConfiguration dummyKBConfig
                      storageConfiguration dummyStorageConfig
                  }

              Expect.isNone spec.KnowledgeBase "KnowledgeBase should start as None"
          } ]
    |> testSequenced

[<Tests>]
let bedrockDataSourceTests =
    let dummyDSConfig =
        CfnDataSource.DataSourceConfigurationProperty(Type = "S3")

    testList
        "Bedrock Data Source DSL"
        [ test "fails when knowledgeBaseId is missing" {
              let thrower () =
                  bedrockDataSource "MyDS" {
                      dataSourceConfiguration dummyDSConfig
                  }
                  |> ignore

              Expect.throws thrower "Should throw when knowledgeBaseId is missing"
          }

          test "fails when dataSourceConfiguration is missing" {
              let thrower () =
                  bedrockDataSource "MyDS" {
                      knowledgeBaseId "KB123"
                  }
                  |> ignore

              Expect.throws thrower "Should throw when dataSourceConfiguration is missing"
          }

          test "fails when all required fields are missing" {
              let thrower () =
                  bedrockDataSource "MyDS" { () } |> ignore

              Expect.throws thrower "Should throw when all required fields are missing"
          }

          test "sets data source name" {
              let spec =
                  bedrockDataSource "MyDS" {
                      knowledgeBaseId "KB123"
                      dataSourceConfiguration dummyDSConfig
                  }

              Expect.equal spec.DataSourceName "MyDS" "Data source name should match"
          }

          test "sets description" {
              let spec =
                  bedrockDataSource "MyDS" {
                      knowledgeBaseId "KB123"
                      dataSourceConfiguration dummyDSConfig
                      description "S3 data source"
                  }

              Expect.equal spec.Props.Description "S3 data source" "Description should be set"
          }

          test "sets dataDeletionPolicy" {
              let spec =
                  bedrockDataSource "MyDS" {
                      knowledgeBaseId "KB123"
                      dataSourceConfiguration dummyDSConfig
                      dataDeletionPolicy "RETAIN"
                  }

              Expect.equal spec.Props.DataDeletionPolicy "RETAIN" "DataDeletionPolicy should be set"
          }

          test "mutable DataSource starts as None" {
              let spec =
                  bedrockDataSource "MyDS" {
                      knowledgeBaseId "KB123"
                      dataSourceConfiguration dummyDSConfig
                  }

              Expect.isNone spec.DataSource "DataSource should start as None"
          } ]
    |> testSequenced

[<Tests>]
let bedrockGuardrailTests =
    testList
        "Bedrock Guardrail DSL"
        [ test "fails when blockedInputMessaging is missing" {
              let thrower () =
                  bedrockGuardrail "MyGuardrail" {
                      blockedOutputsMessaging "Output blocked."
                  }
                  |> ignore

              Expect.throws thrower "Should throw when blockedInputMessaging is missing"
          }

          test "fails when blockedOutputsMessaging is missing" {
              let thrower () =
                  bedrockGuardrail "MyGuardrail" {
                      blockedInputMessaging "Input blocked."
                  }
                  |> ignore

              Expect.throws thrower "Should throw when blockedOutputsMessaging is missing"
          }

          test "sets guardrail name" {
              let spec =
                  bedrockGuardrail "MyGuardrail" {
                      blockedInputMessaging "Input blocked."
                      blockedOutputsMessaging "Output blocked."
                  }

              Expect.equal spec.GuardrailName "MyGuardrail" "Guardrail name should match"
          }

          test "defaults constructId to guardrail name" {
              let spec =
                  bedrockGuardrail "MyGuardrail" {
                      blockedInputMessaging "Input blocked."
                      blockedOutputsMessaging "Output blocked."
                  }

              Expect.equal spec.ConstructId "MyGuardrail" "ConstructId should default to guardrail name"
          }

          test "sets blocked messaging" {
              let spec =
                  bedrockGuardrail "MyGuardrail" {
                      blockedInputMessaging "Input blocked."
                      blockedOutputsMessaging "Output blocked."
                  }

              Expect.equal spec.Props.BlockedInputMessaging "Input blocked." "BlockedInputMessaging should be set"
              Expect.equal spec.Props.BlockedOutputsMessaging "Output blocked." "BlockedOutputsMessaging should be set"
          }

          test "sets description" {
              let spec =
                  bedrockGuardrail "MyGuardrail" {
                      blockedInputMessaging "Input blocked."
                      blockedOutputsMessaging "Output blocked."
                      description "Safety guardrail"
                  }

              Expect.equal spec.Props.Description "Safety guardrail" "Description should be set"
          }

          test "sets kmsKeyArn" {
              let spec =
                  bedrockGuardrail "MyGuardrail" {
                      blockedInputMessaging "Input blocked."
                      blockedOutputsMessaging "Output blocked."
                      kmsKeyArn "arn:aws:kms:us-east-1:123456789012:key/my-key"
                  }

              Expect.equal
                  spec.Props.KmsKeyArn
                  "arn:aws:kms:us-east-1:123456789012:key/my-key"
                  "KmsKeyArn should be set"
          }

          test "sets tags" {
              let spec =
                  bedrockGuardrail "MyGuardrail" {
                      blockedInputMessaging "Input blocked."
                      blockedOutputsMessaging "Output blocked."
                      tags [ "Environment", "Production"; "Team", "AI" ]
                  }

              Expect.isNotNull spec.Props.Tags "Tags should be set"
              Expect.equal (spec.Props.Tags |> Array.length) 2 "Should have 2 tags"
          }

          test "tags are empty by default" {
              let spec =
                  bedrockGuardrail "MyGuardrail" {
                      blockedInputMessaging "Input blocked."
                      blockedOutputsMessaging "Output blocked."
                  }

              Expect.isNull spec.Props.Tags "Tags should be null when no tags set"
          }

          test "mutable Guardrail starts as None" {
              let spec =
                  bedrockGuardrail "MyGuardrail" {
                      blockedInputMessaging "Input blocked."
                      blockedOutputsMessaging "Output blocked."
                  }

              Expect.isNone spec.Guardrail "Guardrail should start as None"
          } ]
    |> testSequenced
