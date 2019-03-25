#!/bin/bash

export DOTNET_CLI_TELEMETRY_OPTOUT=1

cd /philadelphia/Philadelphia.Demo.Server/bin/Debug/netcoreapp2.1
exec dotnet Philadelphia.Demo.Server.dll
