#!/bin/bash

FILE="../_output/philadelphia.tar.gz"
DOTNETCOREVERSION="3.1"
BINARIESDIR="../_output/binaries"


if [ -f "$FILE" ];
then
    rm $FILE || exit 1
fi

CURDIR=`pwd`

cd ..

echo "nuget restore-ing"
nuget restore Philadelphia.Toolkit.And.Demo.sln || exit 1

echo "building"
cd Philadelphia.Demo.Server
dotnet publish || exit 1
cd ..

cd Philadelphia.Demo.Client
msbuild /target:clean Philadelphia.Demo.Client.csproj || exit 1
msbuild /p:Configuration=Debug /target:build Philadelphia.Demo.Client.csproj || exit 1
cd ..



cd $CURDIR

if [ -d "${BINARIESDIR}" ];
then
    echo "cleaning old binaries"
    rm -rf "$BINARIESDIR" || exit 1
fi

echo "preparing binaries ${BINARIESDIR}"

mkdir -p ${BINARIESDIR}/Philadelphia.Demo.Server/bin/Debug/netcoreapp${DOTNETCOREVERSION} || exit 1
cp -r ../Philadelphia.Demo.Server/bin/Debug/netcoreapp${DOTNETCOREVERSION}/publish/* ${BINARIESDIR}/Philadelphia.Demo.Server/bin/Debug/netcoreapp${DOTNETCOREVERSION}/ || exit 1
cp ../Philadelphia.Demo.Server/*.json ${BINARIESDIR}/Philadelphia.Demo.Server/ || exit 1

mkdir -p ${BINARIESDIR}/Philadelphia.StaticResources || exit 1
cp ../Philadelphia.StaticResources/*.png ${BINARIESDIR}/Philadelphia.StaticResources/ || exit 1
cp ../Philadelphia.StaticResources/*.gif ${BINARIESDIR}/Philadelphia.StaticResources/ || exit 1
cp ../Philadelphia.StaticResources/*.css ${BINARIESDIR}/Philadelphia.StaticResources/ || exit 1
cp ../Philadelphia.StaticResources/*.woff2 ${BINARIESDIR}/Philadelphia.StaticResources/ || exit 1

mkdir -p ${BINARIESDIR}/Philadelphia.Demo.Client/Bridge/output || exit 1
cp ../Philadelphia.Demo.Client/Bridge/output/*.js ${BINARIESDIR}/Philadelphia.Demo.Client/Bridge/output/ || exit 1
cp ../Philadelphia.Demo.Client/*.css ${BINARIESDIR}/Philadelphia.Demo.Client/ || exit 1
cp ../Philadelphia.Demo.Client/*.html ${BINARIESDIR}/Philadelphia.Demo.Client/ || exit 1
cp ../Philadelphia.Demo.Client/*.ico ${BINARIESDIR}/Philadelphia.Demo.Client/ || exit 1

mkdir -p ${BINARIESDIR}/Philadelphia.Demo.Client/ImagesForUploadDemo/Full || exit 1
cp ../Philadelphia.Demo.Client/ImagesForUploadDemo/Full/* ${BINARIESDIR}/Philadelphia.Demo.Client/ImagesForUploadDemo/Full/ || exit 1

mkdir -p ${BINARIESDIR}/Philadelphia.Demo.Client/ImagesForUploadDemo/Thumb || exit 1
cp ../Philadelphia.Demo.Client/ImagesForUploadDemo/Thumb/* ${BINARIESDIR}/Philadelphia.Demo.Client/ImagesForUploadDemo/Thumb/ || exit 1

echo "{\"sha\":\"$(git rev-parse HEAD)\",\"committedAt\":\"$(git show -s --format=%ci)\",\"compiledAt\":\"$(date '+%F %T %z' )\"}" > ${BINARIESDIR}/version.json

echo "binaries ready for archiving"

tar cvfz $FILE -C "${BINARIESDIR}" . || exit 1
echo "archive ready for deployment"
