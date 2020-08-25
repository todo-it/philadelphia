#!/bin/bash

MASTERLOC="../../_output/philadelphia.tar.gz"
LOCALLOC="./philadelphia.tar.gz"

# update if possible
if [ -f "${LOCALLOC}" ] && [ -f "${MASTERLOC}" ];
then
    rm "${LOCALLOC}"
    cp "${MASTERLOC}" "${LOCALLOC}" || exit 1
fi

# initialize if needed
if [ ! -f "${LOCALLOC}" ] && [ -f "${MASTERLOC}" ];
then
    cp "${MASTERLOC}" "${LOCALLOC}" || exit 1
fi

# it builds and runs Docker container from already built binaries

DOTNETCOREVERSION="3.1"

CURDIR=`pwd`

echo "building image"
docker build -t="todoit/philadelphia-demo-server" . || exit 1

if [ -f ./CONTAINER_ID ];
then
	echo "removing old container"
	CONTAINER_ID=`cat ./CONTAINER_ID`
	docker kill $CONTAINER_ID
	docker rm $CONTAINER_ID
fi

#	-v ${CURDIR}/log:/philadelphia/Philadelphia.Demo.Server/bin/Debug/netcoreapp${DOTNETCOREVERSION}/log 

echo "building container"
CONTAINER_ID=$(docker create \
	--cap-drop=all \
	-v /tmp:/data \
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
