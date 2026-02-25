namespace FsCDK

open Amazon.CDK
open Amazon.CDK.AwsBedrock
open System.Collections.Generic

// ============================================================================
// AWS Bedrock Configuration DSL
// ============================================================================

// ----------------------------------------------------------------------------
// Bedrock Agent
// ----------------------------------------------------------------------------

/// <summary>Configuration for a Bedrock Agent resource.</summary>
type BedrockAgentConfig =
    { AgentName: string
      ConstructId: string option
      FoundationModel: string option
      Instruction: string option
      Description: string option
      AgentResourceRoleArn: string option
      IdleSessionTtlInSeconds: int option
      AutoPrepare: bool option
      CustomerEncryptionKeyArn: string option
      SkipResourceInUseCheckOnDelete: bool option
      Tags: (string * string) list }

/// <summary>Spec for a Bedrock Agent, holding the resolved props and mutable resource reference.</summary>
type BedrockAgentSpec =
    { AgentName: string
      ConstructId: string
      Props: CfnAgentProps
      mutable Agent: CfnAgent option }

type BedrockAgentBuilder(name: string) =
    member _.Yield(_: unit) : BedrockAgentConfig =
        { AgentName = name
          ConstructId = None
          FoundationModel = None
          Instruction = None
          Description = None
          AgentResourceRoleArn = None
          IdleSessionTtlInSeconds = None
          AutoPrepare = Some true
          CustomerEncryptionKeyArn = None
          SkipResourceInUseCheckOnDelete = None
          Tags = [] }

    member _.Zero() : BedrockAgentConfig =
        { AgentName = name
          ConstructId = None
          FoundationModel = None
          Instruction = None
          Description = None
          AgentResourceRoleArn = None
          IdleSessionTtlInSeconds = None
          AutoPrepare = Some true
          CustomerEncryptionKeyArn = None
          SkipResourceInUseCheckOnDelete = None
          Tags = [] }

    member inline _.Delay([<InlineIfLambda>] f: unit -> BedrockAgentConfig) : BedrockAgentConfig = f ()

    member _.Combine(state1: BedrockAgentConfig, state2: BedrockAgentConfig) : BedrockAgentConfig =
        { AgentName = state2.AgentName
          ConstructId = state2.ConstructId |> Option.orElse state1.ConstructId
          FoundationModel = state2.FoundationModel |> Option.orElse state1.FoundationModel
          Instruction = state2.Instruction |> Option.orElse state1.Instruction
          Description = state2.Description |> Option.orElse state1.Description
          AgentResourceRoleArn = state2.AgentResourceRoleArn |> Option.orElse state1.AgentResourceRoleArn
          IdleSessionTtlInSeconds = state2.IdleSessionTtlInSeconds |> Option.orElse state1.IdleSessionTtlInSeconds
          AutoPrepare = state2.AutoPrepare |> Option.orElse state1.AutoPrepare
          CustomerEncryptionKeyArn = state2.CustomerEncryptionKeyArn |> Option.orElse state1.CustomerEncryptionKeyArn
          SkipResourceInUseCheckOnDelete =
            state2.SkipResourceInUseCheckOnDelete
            |> Option.orElse state1.SkipResourceInUseCheckOnDelete
          Tags =
            if state2.Tags.IsEmpty then
                state1.Tags
            else
                state2.Tags @ state1.Tags }

    member inline x.For
        (
            config: BedrockAgentConfig,
            [<InlineIfLambda>] f: unit -> BedrockAgentConfig
        ) : BedrockAgentConfig =
        let newConfig = f ()
        x.Combine(config, newConfig)

    member _.Run(config: BedrockAgentConfig) : BedrockAgentSpec =
        let constructId = config.ConstructId |> Option.defaultValue config.AgentName

        let props = CfnAgentProps()

        props.AgentName <- config.AgentName

        props.FoundationModel <-
            match config.FoundationModel with
            | Some m -> m
            | None -> failwith "Bedrock Agent foundation model is required"

        config.Instruction |> Option.iter (fun i -> props.Instruction <- i)
        config.Description |> Option.iter (fun d -> props.Description <- d)

        config.AgentResourceRoleArn
        |> Option.iter (fun r -> props.AgentResourceRoleArn <- r)

        config.IdleSessionTtlInSeconds
        |> Option.iter (fun t -> props.IdleSessionTtlInSeconds <- t)

        config.AutoPrepare |> Option.iter (fun a -> props.AutoPrepare <- a)

        config.CustomerEncryptionKeyArn
        |> Option.iter (fun k -> props.CustomerEncryptionKeyArn <- k)

        config.SkipResourceInUseCheckOnDelete
        |> Option.iter (fun s -> props.SkipResourceInUseCheckOnDelete <- s)

        if not (List.isEmpty config.Tags) then
            let tagDict = Dictionary<string, string>()

            for key, value in config.Tags do
                tagDict[key] <- value

            props.Tags <- tagDict

        { AgentName = config.AgentName
          ConstructId = constructId
          Props = props
          Agent = None }

    /// <summary>Sets the construct ID for the Bedrock agent.</summary>
    /// <param name="config">The agent configuration.</param>
    /// <param name="id">The construct ID.</param>
    /// <code lang="fsharp">
    /// bedrockAgent "MyAgent" {
    ///     constructId "MyAgentConstruct"
    /// }
    /// </code>
    [<CustomOperation("constructId")>]
    member _.ConstructId(config: BedrockAgentConfig, id: string) = { config with ConstructId = Some id }

    /// <summary>Sets the foundation model for the agent (e.g., "anthropic.claude-3-sonnet-20240229-v1:0").</summary>
    /// <param name="config">The agent configuration.</param>
    /// <param name="model">The foundation model identifier string.</param>
    /// <code lang="fsharp">
    /// bedrockAgent "MyAgent" {
    ///     foundationModel "anthropic.claude-3-sonnet-20240229-v1:0"
    /// }
    /// </code>
    [<CustomOperation("foundationModel")>]
    member _.FoundationModel(config: BedrockAgentConfig, model: string) =
        { config with
            FoundationModel = Some model }

    /// <summary>Sets the foundation model using a FoundationModelIdentifier.</summary>
    /// <param name="config">The agent configuration.</param>
    /// <param name="model">The foundation model identifier.</param>
    /// <code lang="fsharp">
    /// bedrockAgent "MyAgent" {
    ///     foundationModelId FoundationModelIdentifier.ANTHROPIC_CLAUDE_3_SONNET_20240229_V1_0
    /// }
    /// </code>
    [<CustomOperation("foundationModelId")>]
    member _.FoundationModelId(config: BedrockAgentConfig, model: FoundationModelIdentifier) =
        { config with
            FoundationModel = Some model.ModelId }

    /// <summary>Sets the instruction/system prompt for the agent.</summary>
    /// <param name="config">The agent configuration.</param>
    /// <param name="instruction">The instruction text.</param>
    /// <code lang="fsharp">
    /// bedrockAgent "MyAgent" {
    ///     instruction "You are a helpful assistant that answers questions about AWS."
    /// }
    /// </code>
    [<CustomOperation("instruction")>]
    member _.Instruction(config: BedrockAgentConfig, instruction: string) =
        { config with
            Instruction = Some instruction }

    /// <summary>Sets the description for the agent.</summary>
    /// <param name="config">The agent configuration.</param>
    /// <param name="description">The agent description.</param>
    /// <code lang="fsharp">
    /// bedrockAgent "MyAgent" {
    ///     description "An agent for customer support"
    /// }
    /// </code>
    [<CustomOperation("description")>]
    member _.Description(config: BedrockAgentConfig, description: string) =
        { config with
            Description = Some description }

    /// <summary>Sets the IAM role ARN for the agent.</summary>
    /// <param name="config">The agent configuration.</param>
    /// <param name="roleArn">The IAM role ARN.</param>
    /// <code lang="fsharp">
    /// bedrockAgent "MyAgent" {
    ///     agentResourceRoleArn "arn:aws:iam::123456789012:role/BedrockAgentRole"
    /// }
    /// </code>
    [<CustomOperation("agentResourceRoleArn")>]
    member _.AgentResourceRoleArn(config: BedrockAgentConfig, roleArn: string) =
        { config with
            AgentResourceRoleArn = Some roleArn }

    /// <summary>Sets the idle session TTL in seconds.</summary>
    /// <param name="config">The agent configuration.</param>
    /// <param name="seconds">Idle session timeout in seconds.</param>
    /// <code lang="fsharp">
    /// bedrockAgent "MyAgent" {
    ///     idleSessionTtlInSeconds 600
    /// }
    /// </code>
    [<CustomOperation("idleSessionTtlInSeconds")>]
    member _.IdleSessionTtlInSeconds(config: BedrockAgentConfig, seconds: int) =
        { config with
            IdleSessionTtlInSeconds = Some seconds }

    /// <summary>Sets whether the agent should be auto-prepared after creation (default: true).</summary>
    /// <param name="config">The agent configuration.</param>
    /// <param name="value">True to auto-prepare.</param>
    /// <code lang="fsharp">
    /// bedrockAgent "MyAgent" {
    ///     autoPrepare true
    /// }
    /// </code>
    [<CustomOperation("autoPrepare")>]
    member _.AutoPrepare(config: BedrockAgentConfig, value: bool) =
        { config with AutoPrepare = Some value }

    /// <summary>Sets the KMS key ARN for customer-managed encryption.</summary>
    /// <param name="config">The agent configuration.</param>
    /// <param name="keyArn">The KMS key ARN.</param>
    /// <code lang="fsharp">
    /// bedrockAgent "MyAgent" {
    ///     customerEncryptionKeyArn "arn:aws:kms:us-east-1:123456789012:key/my-key"
    /// }
    /// </code>
    [<CustomOperation("customerEncryptionKeyArn")>]
    member _.CustomerEncryptionKeyArn(config: BedrockAgentConfig, keyArn: string) =
        { config with
            CustomerEncryptionKeyArn = Some keyArn }

    /// <summary>Sets whether to skip the resource-in-use check on delete.</summary>
    /// <param name="config">The agent configuration.</param>
    /// <param name="value">True to skip the check.</param>
    /// <code lang="fsharp">
    /// bedrockAgent "MyAgent" {
    ///     skipResourceInUseCheckOnDelete true
    /// }
    /// </code>
    [<CustomOperation("skipResourceInUseCheckOnDelete")>]
    member _.SkipResourceInUseCheckOnDelete(config: BedrockAgentConfig, value: bool) =
        { config with
            SkipResourceInUseCheckOnDelete = Some value }

    /// <summary>Adds tags to the agent.</summary>
    /// <param name="config">The agent configuration.</param>
    /// <param name="tags">List of key-value tag pairs.</param>
    /// <code lang="fsharp">
    /// bedrockAgent "MyAgent" {
    ///     tags [ "Environment", "Production"; "Team", "AI" ]
    /// }
    /// </code>
    [<CustomOperation("tags")>]
    member _.Tags(config: BedrockAgentConfig, tags: (string * string) list) =
        { config with
            Tags = tags @ config.Tags }

// ----------------------------------------------------------------------------
// Bedrock Knowledge Base
// ----------------------------------------------------------------------------

/// <summary>Configuration for a Bedrock Knowledge Base resource.</summary>
type BedrockKnowledgeBaseConfig =
    { KnowledgeBaseName: string
      ConstructId: string option
      Description: string option
      RoleArn: string option
      KnowledgeBaseConfiguration: obj option
      StorageConfiguration: obj option
      Tags: (string * string) list }

/// <summary>Spec for a Bedrock Knowledge Base, holding the resolved props and mutable resource reference.</summary>
type BedrockKnowledgeBaseSpec =
    { KnowledgeBaseName: string
      ConstructId: string
      Props: CfnKnowledgeBaseProps
      mutable KnowledgeBase: CfnKnowledgeBase option }

type BedrockKnowledgeBaseBuilder(name: string) =
    member _.Yield(_: unit) : BedrockKnowledgeBaseConfig =
        { KnowledgeBaseName = name
          ConstructId = None
          Description = None
          RoleArn = None
          KnowledgeBaseConfiguration = None
          StorageConfiguration = None
          Tags = [] }

    member _.Zero() : BedrockKnowledgeBaseConfig =
        { KnowledgeBaseName = name
          ConstructId = None
          Description = None
          RoleArn = None
          KnowledgeBaseConfiguration = None
          StorageConfiguration = None
          Tags = [] }

    member inline _.Delay([<InlineIfLambda>] f: unit -> BedrockKnowledgeBaseConfig) : BedrockKnowledgeBaseConfig = f ()

    member _.Combine
        (
            state1: BedrockKnowledgeBaseConfig,
            state2: BedrockKnowledgeBaseConfig
        ) : BedrockKnowledgeBaseConfig =
        { KnowledgeBaseName = state2.KnowledgeBaseName
          ConstructId = state2.ConstructId |> Option.orElse state1.ConstructId
          Description = state2.Description |> Option.orElse state1.Description
          RoleArn = state2.RoleArn |> Option.orElse state1.RoleArn
          KnowledgeBaseConfiguration =
            state2.KnowledgeBaseConfiguration
            |> Option.orElse state1.KnowledgeBaseConfiguration
          StorageConfiguration = state2.StorageConfiguration |> Option.orElse state1.StorageConfiguration
          Tags =
            if state2.Tags.IsEmpty then
                state1.Tags
            else
                state2.Tags @ state1.Tags }

    member inline x.For
        (
            config: BedrockKnowledgeBaseConfig,
            [<InlineIfLambda>] f: unit -> BedrockKnowledgeBaseConfig
        ) : BedrockKnowledgeBaseConfig =
        let newConfig = f ()
        x.Combine(config, newConfig)

    member _.Run(config: BedrockKnowledgeBaseConfig) : BedrockKnowledgeBaseSpec =
        let constructId = config.ConstructId |> Option.defaultValue config.KnowledgeBaseName

        let props = CfnKnowledgeBaseProps()

        props.Name <- config.KnowledgeBaseName

        props.RoleArn <-
            match config.RoleArn with
            | Some r -> r
            | None -> failwith "Bedrock Knowledge Base roleArn is required"

        props.KnowledgeBaseConfiguration <-
            match config.KnowledgeBaseConfiguration with
            | Some c -> c
            | None -> failwith "Bedrock Knowledge Base knowledgeBaseConfiguration is required"

        props.StorageConfiguration <-
            match config.StorageConfiguration with
            | Some s -> s
            | None -> failwith "Bedrock Knowledge Base storageConfiguration is required"

        config.Description |> Option.iter (fun d -> props.Description <- d)

        if not (List.isEmpty config.Tags) then
            let tagDict = Dictionary<string, string>()

            for key, value in config.Tags do
                tagDict[key] <- value

            props.Tags <- tagDict

        { KnowledgeBaseName = config.KnowledgeBaseName
          ConstructId = constructId
          Props = props
          KnowledgeBase = None }

    /// <summary>Sets the construct ID for the knowledge base.</summary>
    /// <param name="config">The knowledge base configuration.</param>
    /// <param name="id">The construct ID.</param>
    /// <code lang="fsharp">
    /// bedrockKnowledgeBase "MyKB" {
    ///     constructId "MyKBConstruct"
    /// }
    /// </code>
    [<CustomOperation("constructId")>]
    member _.ConstructId(config: BedrockKnowledgeBaseConfig, id: string) = { config with ConstructId = Some id }

    /// <summary>Sets the description for the knowledge base.</summary>
    /// <param name="config">The knowledge base configuration.</param>
    /// <param name="description">The description.</param>
    /// <code lang="fsharp">
    /// bedrockKnowledgeBase "MyKB" {
    ///     description "Knowledge base for product documentation"
    /// }
    /// </code>
    [<CustomOperation("description")>]
    member _.Description(config: BedrockKnowledgeBaseConfig, description: string) =
        { config with
            Description = Some description }

    /// <summary>Sets the IAM role ARN for the knowledge base.</summary>
    /// <param name="config">The knowledge base configuration.</param>
    /// <param name="roleArn">The IAM role ARN.</param>
    /// <code lang="fsharp">
    /// bedrockKnowledgeBase "MyKB" {
    ///     roleArn "arn:aws:iam::123456789012:role/BedrockKBRole"
    /// }
    /// </code>
    [<CustomOperation("roleArn")>]
    member _.RoleArn(config: BedrockKnowledgeBaseConfig, roleArn: string) = { config with RoleArn = Some roleArn }

    /// <summary>Sets the knowledge base configuration (embedding model, type, etc.).</summary>
    /// <param name="config">The knowledge base configuration.</param>
    /// <param name="kbConfig">The CfnKnowledgeBase.IKnowledgeBaseConfigurationProperty.</param>
    /// <code lang="fsharp">
    /// bedrockKnowledgeBase "MyKB" {
    ///     knowledgeBaseConfiguration myKBConfig
    /// }
    /// </code>
    [<CustomOperation("knowledgeBaseConfiguration")>]
    member _.KnowledgeBaseConfiguration
        (
            config: BedrockKnowledgeBaseConfig,
            kbConfig: CfnKnowledgeBase.IKnowledgeBaseConfigurationProperty
        ) =
        { config with
            KnowledgeBaseConfiguration = Some(kbConfig :> obj) }

    /// <summary>Sets the storage configuration (vector store backend).</summary>
    /// <param name="config">The knowledge base configuration.</param>
    /// <param name="storageConfig">The CfnKnowledgeBase.IStorageConfigurationProperty.</param>
    /// <code lang="fsharp">
    /// bedrockKnowledgeBase "MyKB" {
    ///     storageConfiguration myStorageConfig
    /// }
    /// </code>
    [<CustomOperation("storageConfiguration")>]
    member _.StorageConfiguration
        (
            config: BedrockKnowledgeBaseConfig,
            storageConfig: CfnKnowledgeBase.IStorageConfigurationProperty
        ) =
        { config with
            StorageConfiguration = Some(storageConfig :> obj) }

    /// <summary>Adds tags to the knowledge base.</summary>
    /// <param name="config">The knowledge base configuration.</param>
    /// <param name="tags">List of key-value tag pairs.</param>
    /// <code lang="fsharp">
    /// bedrockKnowledgeBase "MyKB" {
    ///     tags [ "Environment", "Production" ]
    /// }
    /// </code>
    [<CustomOperation("tags")>]
    member _.Tags(config: BedrockKnowledgeBaseConfig, tags: (string * string) list) =
        { config with
            Tags = tags @ config.Tags }

// ----------------------------------------------------------------------------
// Bedrock Data Source
// ----------------------------------------------------------------------------

/// <summary>Configuration for a Bedrock Data Source resource.</summary>
type BedrockDataSourceConfig =
    { DataSourceName: string
      ConstructId: string option
      KnowledgeBaseId: string option
      Description: string option
      DataSourceConfiguration: obj option
      DataDeletionPolicy: string option
      ServerSideEncryptionConfiguration: obj option
      VectorIngestionConfiguration: obj option }

/// <summary>Spec for a Bedrock Data Source, holding the resolved props and mutable resource reference.</summary>
type BedrockDataSourceSpec =
    { DataSourceName: string
      ConstructId: string
      Props: CfnDataSourceProps
      mutable DataSource: CfnDataSource option }

type BedrockDataSourceBuilder(name: string) =
    member _.Yield(_: unit) : BedrockDataSourceConfig =
        { DataSourceName = name
          ConstructId = None
          KnowledgeBaseId = None
          Description = None
          DataSourceConfiguration = None
          DataDeletionPolicy = None
          ServerSideEncryptionConfiguration = None
          VectorIngestionConfiguration = None }

    member _.Zero() : BedrockDataSourceConfig =
        { DataSourceName = name
          ConstructId = None
          KnowledgeBaseId = None
          Description = None
          DataSourceConfiguration = None
          DataDeletionPolicy = None
          ServerSideEncryptionConfiguration = None
          VectorIngestionConfiguration = None }

    member inline _.Delay([<InlineIfLambda>] f: unit -> BedrockDataSourceConfig) : BedrockDataSourceConfig = f ()

    member _.Combine(state1: BedrockDataSourceConfig, state2: BedrockDataSourceConfig) : BedrockDataSourceConfig =
        { DataSourceName = state2.DataSourceName
          ConstructId = state2.ConstructId |> Option.orElse state1.ConstructId
          KnowledgeBaseId = state2.KnowledgeBaseId |> Option.orElse state1.KnowledgeBaseId
          Description = state2.Description |> Option.orElse state1.Description
          DataSourceConfiguration = state2.DataSourceConfiguration |> Option.orElse state1.DataSourceConfiguration
          DataDeletionPolicy = state2.DataDeletionPolicy |> Option.orElse state1.DataDeletionPolicy
          ServerSideEncryptionConfiguration =
            state2.ServerSideEncryptionConfiguration
            |> Option.orElse state1.ServerSideEncryptionConfiguration
          VectorIngestionConfiguration =
            state2.VectorIngestionConfiguration
            |> Option.orElse state1.VectorIngestionConfiguration }

    member inline x.For
        (
            config: BedrockDataSourceConfig,
            [<InlineIfLambda>] f: unit -> BedrockDataSourceConfig
        ) : BedrockDataSourceConfig =
        let newConfig = f ()
        x.Combine(config, newConfig)

    member _.Run(config: BedrockDataSourceConfig) : BedrockDataSourceSpec =
        let constructId = config.ConstructId |> Option.defaultValue config.DataSourceName

        let props = CfnDataSourceProps()

        props.Name <- config.DataSourceName

        props.KnowledgeBaseId <-
            match config.KnowledgeBaseId with
            | Some id -> id
            | None -> failwith "Bedrock Data Source knowledgeBaseId is required"

        props.DataSourceConfiguration <-
            match config.DataSourceConfiguration with
            | Some c -> c
            | None -> failwith "Bedrock Data Source dataSourceConfiguration is required"

        config.Description |> Option.iter (fun d -> props.Description <- d)

        config.DataDeletionPolicy
        |> Option.iter (fun p -> props.DataDeletionPolicy <- p)

        config.ServerSideEncryptionConfiguration
        |> Option.iter (fun s -> props.ServerSideEncryptionConfiguration <- s)

        config.VectorIngestionConfiguration
        |> Option.iter (fun v -> props.VectorIngestionConfiguration <- v)

        { DataSourceName = config.DataSourceName
          ConstructId = constructId
          Props = props
          DataSource = None }

    /// <summary>Sets the construct ID for the data source.</summary>
    /// <param name="config">The data source configuration.</param>
    /// <param name="id">The construct ID.</param>
    /// <code lang="fsharp">
    /// bedrockDataSource "MyDS" {
    ///     constructId "MyDSConstruct"
    /// }
    /// </code>
    [<CustomOperation("constructId")>]
    member _.ConstructId(config: BedrockDataSourceConfig, id: string) = { config with ConstructId = Some id }

    /// <summary>Sets the knowledge base ID this data source belongs to.</summary>
    /// <param name="config">The data source configuration.</param>
    /// <param name="id">The knowledge base ID.</param>
    /// <code lang="fsharp">
    /// bedrockDataSource "MyDS" {
    ///     knowledgeBaseId "KB12345"
    /// }
    /// </code>
    [<CustomOperation("knowledgeBaseId")>]
    member _.KnowledgeBaseId(config: BedrockDataSourceConfig, id: string) =
        { config with
            KnowledgeBaseId = Some id }

    /// <summary>Sets the description for the data source.</summary>
    /// <param name="config">The data source configuration.</param>
    /// <param name="description">The description.</param>
    /// <code lang="fsharp">
    /// bedrockDataSource "MyDS" {
    ///     description "S3 data source for product docs"
    /// }
    /// </code>
    [<CustomOperation("description")>]
    member _.Description(config: BedrockDataSourceConfig, description: string) =
        { config with
            Description = Some description }

    /// <summary>Sets the data source configuration (S3, web, etc.).</summary>
    /// <param name="config">The data source configuration.</param>
    /// <param name="dsConfig">The CfnDataSource.IDataSourceConfigurationProperty.</param>
    /// <code lang="fsharp">
    /// bedrockDataSource "MyDS" {
    ///     dataSourceConfiguration myDSConfig
    /// }
    /// </code>
    [<CustomOperation("dataSourceConfiguration")>]
    member _.DataSourceConfiguration
        (
            config: BedrockDataSourceConfig,
            dsConfig: CfnDataSource.IDataSourceConfigurationProperty
        ) =
        { config with
            DataSourceConfiguration = Some(dsConfig :> obj) }

    /// <summary>Sets the data deletion policy (RETAIN or DELETE).</summary>
    /// <param name="config">The data source configuration.</param>
    /// <param name="policy">The deletion policy string.</param>
    /// <code lang="fsharp">
    /// bedrockDataSource "MyDS" {
    ///     dataDeletionPolicy "RETAIN"
    /// }
    /// </code>
    [<CustomOperation("dataDeletionPolicy")>]
    member _.DataDeletionPolicy(config: BedrockDataSourceConfig, policy: string) =
        { config with
            DataDeletionPolicy = Some policy }

    /// <summary>Sets the server-side encryption configuration.</summary>
    /// <param name="config">The data source configuration.</param>
    /// <param name="encryptionConfig">The CfnDataSource.IServerSideEncryptionConfigurationProperty.</param>
    /// <code lang="fsharp">
    /// bedrockDataSource "MyDS" {
    ///     serverSideEncryptionConfiguration myEncConfig
    /// }
    /// </code>
    [<CustomOperation("serverSideEncryptionConfiguration")>]
    member _.ServerSideEncryptionConfiguration
        (
            config: BedrockDataSourceConfig,
            encryptionConfig: CfnDataSource.IServerSideEncryptionConfigurationProperty
        ) =
        { config with
            ServerSideEncryptionConfiguration = Some(encryptionConfig :> obj) }

    /// <summary>Sets the vector ingestion configuration (chunking strategy, etc.).</summary>
    /// <param name="config">The data source configuration.</param>
    /// <param name="ingestionConfig">The CfnDataSource.IVectorIngestionConfigurationProperty.</param>
    /// <code lang="fsharp">
    /// bedrockDataSource "MyDS" {
    ///     vectorIngestionConfiguration myIngestionConfig
    /// }
    /// </code>
    [<CustomOperation("vectorIngestionConfiguration")>]
    member _.VectorIngestionConfiguration
        (
            config: BedrockDataSourceConfig,
            ingestionConfig: CfnDataSource.IVectorIngestionConfigurationProperty
        ) =
        { config with
            VectorIngestionConfiguration = Some(ingestionConfig :> obj) }

// ----------------------------------------------------------------------------
// Bedrock Guardrail
// ----------------------------------------------------------------------------

/// <summary>Configuration for a Bedrock Guardrail resource.</summary>
type BedrockGuardrailConfig =
    { GuardrailName: string
      ConstructId: string option
      Description: string option
      BlockedInputMessaging: string option
      BlockedOutputsMessaging: string option
      ContentPolicyConfig: obj option
      ContextualGroundingPolicyConfig: obj option
      SensitiveInformationPolicyConfig: obj option
      TopicPolicyConfig: obj option
      WordPolicyConfig: obj option
      KmsKeyArn: string option
      Tags: (string * string) list }

/// <summary>Spec for a Bedrock Guardrail, holding the resolved props and mutable resource reference.</summary>
type BedrockGuardrailSpec =
    { GuardrailName: string
      ConstructId: string
      Props: CfnGuardrailProps
      mutable Guardrail: CfnGuardrail option }

type BedrockGuardrailBuilder(name: string) =
    member _.Yield(_: unit) : BedrockGuardrailConfig =
        { GuardrailName = name
          ConstructId = None
          Description = None
          BlockedInputMessaging = None
          BlockedOutputsMessaging = None
          ContentPolicyConfig = None
          ContextualGroundingPolicyConfig = None
          SensitiveInformationPolicyConfig = None
          TopicPolicyConfig = None
          WordPolicyConfig = None
          KmsKeyArn = None
          Tags = [] }

    member _.Zero() : BedrockGuardrailConfig =
        { GuardrailName = name
          ConstructId = None
          Description = None
          BlockedInputMessaging = None
          BlockedOutputsMessaging = None
          ContentPolicyConfig = None
          ContextualGroundingPolicyConfig = None
          SensitiveInformationPolicyConfig = None
          TopicPolicyConfig = None
          WordPolicyConfig = None
          KmsKeyArn = None
          Tags = [] }

    member inline _.Delay([<InlineIfLambda>] f: unit -> BedrockGuardrailConfig) : BedrockGuardrailConfig = f ()

    member _.Combine(state1: BedrockGuardrailConfig, state2: BedrockGuardrailConfig) : BedrockGuardrailConfig =
        { GuardrailName = state2.GuardrailName
          ConstructId = state2.ConstructId |> Option.orElse state1.ConstructId
          Description = state2.Description |> Option.orElse state1.Description
          BlockedInputMessaging = state2.BlockedInputMessaging |> Option.orElse state1.BlockedInputMessaging
          BlockedOutputsMessaging = state2.BlockedOutputsMessaging |> Option.orElse state1.BlockedOutputsMessaging
          ContentPolicyConfig = state2.ContentPolicyConfig |> Option.orElse state1.ContentPolicyConfig
          ContextualGroundingPolicyConfig =
            state2.ContextualGroundingPolicyConfig
            |> Option.orElse state1.ContextualGroundingPolicyConfig
          SensitiveInformationPolicyConfig =
            state2.SensitiveInformationPolicyConfig
            |> Option.orElse state1.SensitiveInformationPolicyConfig
          TopicPolicyConfig = state2.TopicPolicyConfig |> Option.orElse state1.TopicPolicyConfig
          WordPolicyConfig = state2.WordPolicyConfig |> Option.orElse state1.WordPolicyConfig
          KmsKeyArn = state2.KmsKeyArn |> Option.orElse state1.KmsKeyArn
          Tags =
            if state2.Tags.IsEmpty then
                state1.Tags
            else
                state2.Tags @ state1.Tags }

    member inline x.For
        (
            config: BedrockGuardrailConfig,
            [<InlineIfLambda>] f: unit -> BedrockGuardrailConfig
        ) : BedrockGuardrailConfig =
        let newConfig = f ()
        x.Combine(config, newConfig)

    member _.Run(config: BedrockGuardrailConfig) : BedrockGuardrailSpec =
        let constructId = config.ConstructId |> Option.defaultValue config.GuardrailName

        let props = CfnGuardrailProps()

        props.Name <- config.GuardrailName

        props.BlockedInputMessaging <-
            match config.BlockedInputMessaging with
            | Some m -> m
            | None -> failwith "Bedrock Guardrail blockedInputMessaging is required"

        props.BlockedOutputsMessaging <-
            match config.BlockedOutputsMessaging with
            | Some m -> m
            | None -> failwith "Bedrock Guardrail blockedOutputsMessaging is required"

        config.Description |> Option.iter (fun d -> props.Description <- d)

        config.ContentPolicyConfig
        |> Option.iter (fun c -> props.ContentPolicyConfig <- c)

        config.ContextualGroundingPolicyConfig
        |> Option.iter (fun c -> props.ContextualGroundingPolicyConfig <- c)

        config.SensitiveInformationPolicyConfig
        |> Option.iter (fun s -> props.SensitiveInformationPolicyConfig <- s)

        config.TopicPolicyConfig |> Option.iter (fun t -> props.TopicPolicyConfig <- t)
        config.WordPolicyConfig |> Option.iter (fun w -> props.WordPolicyConfig <- w)
        config.KmsKeyArn |> Option.iter (fun k -> props.KmsKeyArn <- k)

        if not (List.isEmpty config.Tags) then
            props.Tags <-
                config.Tags
                |> List.map (fun (k, v) -> CfnTag(Key = k, Value = v) :> ICfnTag)
                |> Array.ofList

        { GuardrailName = config.GuardrailName
          ConstructId = constructId
          Props = props
          Guardrail = None }

    /// <summary>Sets the construct ID for the guardrail.</summary>
    /// <param name="config">The guardrail configuration.</param>
    /// <param name="id">The construct ID.</param>
    /// <code lang="fsharp">
    /// bedrockGuardrail "MyGuardrail" {
    ///     constructId "MyGuardrailConstruct"
    /// }
    /// </code>
    [<CustomOperation("constructId")>]
    member _.ConstructId(config: BedrockGuardrailConfig, id: string) = { config with ConstructId = Some id }

    /// <summary>Sets the description for the guardrail.</summary>
    /// <param name="config">The guardrail configuration.</param>
    /// <param name="description">The description.</param>
    /// <code lang="fsharp">
    /// bedrockGuardrail "MyGuardrail" {
    ///     description "Content safety guardrail"
    /// }
    /// </code>
    [<CustomOperation("description")>]
    member _.Description(config: BedrockGuardrailConfig, description: string) =
        { config with
            Description = Some description }

    /// <summary>Sets the message shown when input is blocked.</summary>
    /// <param name="config">The guardrail configuration.</param>
    /// <param name="message">The blocked input message.</param>
    /// <code lang="fsharp">
    /// bedrockGuardrail "MyGuardrail" {
    ///     blockedInputMessaging "Your input was blocked by the guardrail."
    /// }
    /// </code>
    [<CustomOperation("blockedInputMessaging")>]
    member _.BlockedInputMessaging(config: BedrockGuardrailConfig, message: string) =
        { config with
            BlockedInputMessaging = Some message }

    /// <summary>Sets the message shown when output is blocked.</summary>
    /// <param name="config">The guardrail configuration.</param>
    /// <param name="message">The blocked output message.</param>
    /// <code lang="fsharp">
    /// bedrockGuardrail "MyGuardrail" {
    ///     blockedOutputsMessaging "The response was blocked by the guardrail."
    /// }
    /// </code>
    [<CustomOperation("blockedOutputsMessaging")>]
    member _.BlockedOutputsMessaging(config: BedrockGuardrailConfig, message: string) =
        { config with
            BlockedOutputsMessaging = Some message }

    /// <summary>Sets the content policy configuration (content filters).</summary>
    /// <param name="config">The guardrail configuration.</param>
    /// <param name="policyConfig">The CfnGuardrail.IContentPolicyConfigProperty.</param>
    /// <code lang="fsharp">
    /// bedrockGuardrail "MyGuardrail" {
    ///     contentPolicyConfig myContentPolicy
    /// }
    /// </code>
    [<CustomOperation("contentPolicyConfig")>]
    member _.ContentPolicyConfig
        (
            config: BedrockGuardrailConfig,
            policyConfig: CfnGuardrail.IContentPolicyConfigProperty
        ) =
        { config with
            ContentPolicyConfig = Some(policyConfig :> obj) }

    /// <summary>Sets the contextual grounding policy configuration.</summary>
    /// <param name="config">The guardrail configuration.</param>
    /// <param name="policyConfig">The CfnGuardrail.IContextualGroundingPolicyConfigProperty.</param>
    /// <code lang="fsharp">
    /// bedrockGuardrail "MyGuardrail" {
    ///     contextualGroundingPolicyConfig myGroundingPolicy
    /// }
    /// </code>
    [<CustomOperation("contextualGroundingPolicyConfig")>]
    member _.ContextualGroundingPolicyConfig
        (
            config: BedrockGuardrailConfig,
            policyConfig: CfnGuardrail.IContextualGroundingPolicyConfigProperty
        ) =
        { config with
            ContextualGroundingPolicyConfig = Some(policyConfig :> obj) }

    /// <summary>Sets the sensitive information policy configuration (PII filters).</summary>
    /// <param name="config">The guardrail configuration.</param>
    /// <param name="policyConfig">The CfnGuardrail.ISensitiveInformationPolicyConfigProperty.</param>
    /// <code lang="fsharp">
    /// bedrockGuardrail "MyGuardrail" {
    ///     sensitiveInformationPolicyConfig myPIIPolicy
    /// }
    /// </code>
    [<CustomOperation("sensitiveInformationPolicyConfig")>]
    member _.SensitiveInformationPolicyConfig
        (
            config: BedrockGuardrailConfig,
            policyConfig: CfnGuardrail.ISensitiveInformationPolicyConfigProperty
        ) =
        { config with
            SensitiveInformationPolicyConfig = Some(policyConfig :> obj) }

    /// <summary>Sets the topic policy configuration (denied topics).</summary>
    /// <param name="config">The guardrail configuration.</param>
    /// <param name="policyConfig">The CfnGuardrail.ITopicPolicyConfigProperty.</param>
    /// <code lang="fsharp">
    /// bedrockGuardrail "MyGuardrail" {
    ///     topicPolicyConfig myTopicPolicy
    /// }
    /// </code>
    [<CustomOperation("topicPolicyConfig")>]
    member _.TopicPolicyConfig(config: BedrockGuardrailConfig, policyConfig: CfnGuardrail.ITopicPolicyConfigProperty) =
        { config with
            TopicPolicyConfig = Some(policyConfig :> obj) }

    /// <summary>Sets the word policy configuration (blocked words/phrases).</summary>
    /// <param name="config">The guardrail configuration.</param>
    /// <param name="policyConfig">The CfnGuardrail.IWordPolicyConfigProperty.</param>
    /// <code lang="fsharp">
    /// bedrockGuardrail "MyGuardrail" {
    ///     wordPolicyConfig myWordPolicy
    /// }
    /// </code>
    [<CustomOperation("wordPolicyConfig")>]
    member _.WordPolicyConfig(config: BedrockGuardrailConfig, policyConfig: CfnGuardrail.IWordPolicyConfigProperty) =
        { config with
            WordPolicyConfig = Some(policyConfig :> obj) }

    /// <summary>Sets the KMS key ARN for encrypting the guardrail.</summary>
    /// <param name="config">The guardrail configuration.</param>
    /// <param name="keyArn">The KMS key ARN.</param>
    /// <code lang="fsharp">
    /// bedrockGuardrail "MyGuardrail" {
    ///     kmsKeyArn "arn:aws:kms:us-east-1:123456789012:key/my-key"
    /// }
    /// </code>
    [<CustomOperation("kmsKeyArn")>]
    member _.KmsKeyArn(config: BedrockGuardrailConfig, keyArn: string) = { config with KmsKeyArn = Some keyArn }

    /// <summary>Adds tags to the guardrail.</summary>
    /// <param name="config">The guardrail configuration.</param>
    /// <param name="tags">List of key-value tag pairs.</param>
    /// <code lang="fsharp">
    /// bedrockGuardrail "MyGuardrail" {
    ///     tags [ "Environment", "Production"; "Team", "AI" ]
    /// }
    /// </code>
    [<CustomOperation("tags")>]
    member _.Tags(config: BedrockGuardrailConfig, tags: (string * string) list) =
        { config with
            Tags = tags @ config.Tags }

// ============================================================================
// Builders
// ============================================================================

[<AutoOpen>]
module BedrockBuilders =
    /// <summary>
    /// Creates a new Bedrock Agent builder.
    /// Example: bedrockAgent "MyAgent" { foundationModel "anthropic.claude-3-sonnet-20240229-v1:0" }
    /// </summary>
    let bedrockAgent name = BedrockAgentBuilder name

    /// <summary>
    /// Creates a new Bedrock Knowledge Base builder.
    /// Example: bedrockKnowledgeBase "MyKB" { roleArn "..." }
    /// </summary>
    let bedrockKnowledgeBase name = BedrockKnowledgeBaseBuilder name

    /// <summary>
    /// Creates a new Bedrock Data Source builder.
    /// Example: bedrockDataSource "MyDS" { knowledgeBaseId "..." }
    /// </summary>
    let bedrockDataSource name = BedrockDataSourceBuilder name

    /// <summary>
    /// Creates a new Bedrock Guardrail builder.
    /// Example: bedrockGuardrail "MyGuardrail" { blockedInputMessaging "..." }
    /// </summary>
    let bedrockGuardrail name = BedrockGuardrailBuilder name
