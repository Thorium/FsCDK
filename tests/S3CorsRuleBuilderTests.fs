module FsCDK.Tests.S3CorsRuleBuilderTests

open Amazon.CDK
open Expecto
open FsCDK
open Amazon.CDK.AWS.S3

[<Tests>]
let s3_cors_rule_builder_tests =
    testList
        "S3 CorsRule builder"
        [ test "app synth succeeds with cors via builder" {
              let app = App()

              stack "S3StackCorsBuilder" {
                  app

                  bucket "my-bucket-cors-builder" {
                      constructId "MyBucketCorsBuilder"

                      corsRule {
                          allowedOrigins [ "*" ]
                          allowedMethods [ HttpMethods.GET; HttpMethods.HEAD ]
                          allowedHeaders [ "*" ]
                          id "default"
                          maxAgeSeconds 300
                      }
                  }
              }

              let cloudAssembly = app.Synth()
              Expect.equal cloudAssembly.Stacks.Length 1 "App should synthesize one stack"
          } ]
    |> testSequenced
