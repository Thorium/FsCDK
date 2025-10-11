namespace FsCDK

open Amazon.CDK.AWS.IAM

// ============================================================================
// IAM PolicyStatementProps Builder DSL
// ============================================================================

type PolicyStatementPropsConfig =
    { Actions: string list option
      Resources: string list option
      Effect: Effect option
      Principals: IPrincipal list option
      Sid: string option }

type PolicyStatementPropsBuilder() =
    member _.Yield _ : PolicyStatementPropsConfig =
        { Actions = None
          Resources = None
          Effect = None
          Principals = None
          Sid = None }

    member _.Zero() : PolicyStatementPropsConfig =
        { Actions = None
          Resources = None
          Effect = None
          Principals = None
          Sid = None }

    member _.Run(config: PolicyStatementPropsConfig) : PolicyStatementProps =
        let p = PolicyStatementProps()
        config.Actions |> Option.iter (fun a -> p.Actions <- (a |> List.toArray))
        config.Resources |> Option.iter (fun r -> p.Resources <- (r |> List.toArray))
        config.Effect |> Option.iter (fun e -> p.Effect <- e)

        config.Principals
        |> Option.iter (fun pr -> p.Principals <- (pr |> List.toArray))

        config.Sid |> Option.iter (fun sid -> p.Sid <- sid)
        p

    [<CustomOperation("actions")>]
    member _.Actions(config: PolicyStatementPropsConfig, actions: string list) = { config with Actions = Some actions }

    [<CustomOperation("resources")>]
    member _.Resources(config: PolicyStatementPropsConfig, resources: string list) =
        { config with
            Resources = Some resources }

    [<CustomOperation("effect")>]
    member _.Effect(config: PolicyStatementPropsConfig, effect: Effect) = { config with Effect = Some effect }

    [<CustomOperation("principals")>]
    member _.Principals(config: PolicyStatementPropsConfig, principals: IPrincipal list) =
        { config with
            Principals = Some principals }

    [<CustomOperation("sid")>]
    member _.Sid(config: PolicyStatementPropsConfig, sid: string) = { config with Sid = Some sid }

// ============================================================================
// IAM PolicyStatement Builder DSL
// ============================================================================

type PolicyStatementConfig =
    { Props: PolicyStatementProps option
      Actions: string list
      Resources: string list
      Effect: Effect option
      Principals: IPrincipal list
      Sid: string option }

type PolicyStatementBuilder() =
    member _.Yield _ : PolicyStatementConfig =
        { Props = None
          Actions = []
          Resources = []
          Effect = None
          Principals = []
          Sid = None }

    member _.Yield(props: PolicyStatementProps) : PolicyStatementConfig =
        { Props = Some props
          Actions = []
          Resources = []
          Effect = None
          Principals = []
          Sid = None }

    member _.Zero() : PolicyStatementConfig =
        { Props = None
          Actions = []
          Resources = []
          Effect = None
          Principals = []
          Sid = None }

    member inline _.Delay([<InlineIfLambda>] f: unit -> PolicyStatementConfig) : PolicyStatementConfig = f ()

    member inline x.For
        (
            config: PolicyStatementConfig,
            [<InlineIfLambda>] f: unit -> PolicyStatementConfig
        ) : PolicyStatementConfig =
        let newConfig = f ()
        x.Combine(config, newConfig)

    member _.Combine(state1: PolicyStatementConfig, state2: PolicyStatementConfig) : PolicyStatementConfig =
        { Props = if state1.Props.IsSome then state1.Props else state2.Props
          Actions = state1.Actions @ state2.Actions
          Resources = state1.Resources @ state2.Resources
          Effect =
            if state1.Effect.IsSome then
                state1.Effect
            else
                state2.Effect
          Principals = state1.Principals @ state2.Principals
          Sid = if state1.Sid.IsSome then state1.Sid else state2.Sid }

    member _.Run(config: PolicyStatementConfig) : PolicyStatement =
        match config.Props with
        | Some props ->
            let stmt = PolicyStatement(props)
            // Apply any additional properties
            if config.Actions.Length > 0 then
                stmt.AddActions([| for a in config.Actions -> a |])

            if config.Resources.Length > 0 then
                stmt.AddResources([| for r in config.Resources -> r |])

            stmt
        | None ->
            let props = PolicyStatementProps()

            if config.Actions.Length > 0 then
                props.Actions <- config.Actions |> List.toArray

            if config.Resources.Length > 0 then
                props.Resources <- config.Resources |> List.toArray

            config.Effect |> Option.iter (fun e -> props.Effect <- e)

            if config.Principals.Length > 0 then
                props.Principals <- config.Principals |> List.toArray

            config.Sid |> Option.iter (fun sid -> props.Sid <- sid)
            PolicyStatement(props)

    [<CustomOperation("withProps")>]
    member _.WithProps(config: PolicyStatementConfig, props: PolicyStatementProps) = { config with Props = Some props }

    [<CustomOperation("actions")>]
    member _.Actions(config: PolicyStatementConfig, actions: string list) = { config with Actions = actions }

    [<CustomOperation("resources")>]
    member _.Resources(config: PolicyStatementConfig, resources: string list) = { config with Resources = resources }

    [<CustomOperation("effect")>]
    member _.Effect(config: PolicyStatementConfig, effect: Effect) = { config with Effect = Some effect }

    [<CustomOperation("principals")>]
    member _.Principals(config: PolicyStatementConfig, principals: IPrincipal list) =
        { config with Principals = principals }

    [<CustomOperation("sid")>]
    member _.Sid(config: PolicyStatementConfig, sid: string) = { config with Sid = Some sid }

// ============================================================================
// Builders
// ============================================================================

[<AutoOpen>]
module PolicyStatementBuilders =
    let policyStatementProps = PolicyStatementPropsBuilder()
    let policyStatement = PolicyStatementBuilder()
