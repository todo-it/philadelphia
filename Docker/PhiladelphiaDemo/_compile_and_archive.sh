#!/bin/bash

FILE=philadelphia.tar.gz

if [ -f "$FILE" ];
then
    rm $FILE || exit 1
fi

CURDIR=`pwd`

cd ../..

cd Philadelphia.Demo.Server 
dotnet publish || exit 1

cd ../Philadelphia.Demo.Client
msbuild /target:clean Philadelphia.Demo.Client.csproj || exit 1
msbuild /p:Configuration=Debug /target:build Philadelphia.Demo.Client.csproj || exit 1

cd $CURDIR
./build_and_run.sh buildonly

tar cvfz $FILE *.sh binaries Dockerfile
