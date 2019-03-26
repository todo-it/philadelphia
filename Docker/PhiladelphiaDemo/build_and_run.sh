#!/bin/bash

# it builds and runs Docker container from already built binaries

CURDIR=`pwd`

ISWINDOWS=`uname -o`

if [ "$ISWINDOWS" = "Msys" ];
then
	eval "$(docker-machine env --shell=bash default)"
fi

if [ -d "../../packages" ]; then
    echo "preparing binaries"

    rm -rf binaries || exit 1

    mkdir -p binaries/Philadelphia.Demo.Server/bin/Debug/netcoreapp2.1 || exit 1
    cp -r ../../Philadelphia.Demo.Server/bin/Debug/netcoreapp2.1/publish/* binaries/Philadelphia.Demo.Server/bin/Debug/netcoreapp2.1/ || exit 1
    cp ../../Philadelphia.Demo.Server/*.json binaries/Philadelphia.Demo.Server/ || exit 1

    mkdir -p binaries/Philadelphia.StaticResources || exit 1
    cp ../../Philadelphia.StaticResources/*.png binaries/Philadelphia.StaticResources/ || exit 1
    cp ../../Philadelphia.StaticResources/*.gif binaries/Philadelphia.StaticResources/ || exit 1
    cp ../../Philadelphia.StaticResources/*.css binaries/Philadelphia.StaticResources/ || exit 1
    cp ../../Philadelphia.StaticResources/*.woff binaries/Philadelphia.StaticResources/ || exit 1

    mkdir -p binaries/Philadelphia.Demo.Client/Bridge/output || exit 1
    cp ../../Philadelphia.Demo.Client/Bridge/output/*.js binaries/Philadelphia.Demo.Client/Bridge/output/ || exit 1
    cp ../../Philadelphia.Demo.Client/*.css binaries/Philadelphia.Demo.Client/ || exit 1
    cp ../../Philadelphia.Demo.Client/*.html binaries/Philadelphia.Demo.Client/ || exit 1
    cp ../../Philadelphia.Demo.Client/*.ico binaries/Philadelphia.Demo.Client/ || exit 1
    
    mkdir -p binaries/Philadelphia.Demo.Client/ImagesForUploadDemo/Full || exit 1
    cp ../../Philadelphia.Demo.Client/ImagesForUploadDemo/Full/* binaries/Philadelphia.Demo.Client/ImagesForUploadDemo/Full/ || exit 1
    
    mkdir -p binaries/Philadelphia.Demo.Client/ImagesForUploadDemo/Thumb || exit 1
    cp ../../Philadelphia.Demo.Client/ImagesForUploadDemo/Thumb/* binaries/Philadelphia.Demo.Client/ImagesForUploadDemo/Thumb/ || exit 1
        
fi

if [ "$1" == "buildonly" ]; then
    exit
fi


echo "building image"
docker build -t="todoit/philadelphia-demo-server" . || exit 1

if [ -f ./CONTAINER_ID ];
then
	echo "removing old container"
	CONTAINER_ID=`cat ./CONTAINER_ID`
	docker kill $CONTAINER_ID
	docker rm $CONTAINER_ID
fi

echo "building container"
CONTAINER_ID=$(docker create \
	--cap-drop=all \
	-v /tmp:/data \
	-v ${CURDIR}/log:/philadelphia/Philadelphia.Demo.Server/bin/Debug/netcoreapp2.1/log \
	-e TRANSLATION_FILE="../../../translation_pl-PL.json" \
	-e LOCAL_TIMEZONE_ID="Europe/Warsaw" \
	-e TOKENS_DIRECTORY="/data" \
	-e DEPLOYMENT_NAME="dev" \
	-e ALLOW_SERVER_SIDE_MUTATION="false" \
	-p 80:8090 --name philadelphia-demo-server -t -i todoit/philadelphia-demo-server) \
	|| exit 1

echo -n $CONTAINER_ID > CONTAINER_ID

echo "starting container"
exec docker start $1 $CONTAINER_ID || exit 1

