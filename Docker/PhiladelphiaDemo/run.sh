#!/bin/bash

export DOTNET_CLI_TELEMETRY_OPTOUT=1
export USE_FANCY_CONNECTION_ID=false
export ALLOW_SERVER_SIDE_MUTATION=false

cd /philadelphia/Philadelphia.Demo.Server/bin/Debug/netcoreapp2.2
exec dotnet Philadelphia.Demo.Server.dll
