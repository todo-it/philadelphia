#!/bin/bash

DOTNETCOREVERSION="3.1"

cd ..

echo "nuget restore-ing"
nuget restore Philadelphia.Toolkit.And.Demo.sln || exit 1



echo "building"
cd Tests/Heavy/ControlledByTests.Client

msbuild /target:clean ControlledByTests.Client.csproj || exit 1
msbuild /p:Configuration=Debug /target:build ControlledByTests.Client.csproj || exit 1

echo "OK"
