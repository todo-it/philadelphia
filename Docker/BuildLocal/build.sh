#!/bin/bash

echo "building image"
docker build \
	--build-arg "RUN_UID=$(id -u)" \
	--build-arg "RUN_GID=$(id -g)" \
	-t="todoit/philadelphia-build" . || exit 1

if [ -f ./CONTAINER_ID ];
then
	echo "removing old container"
	CONTAINER_ID=`cat ./CONTAINER_ID`
	docker kill $CONTAINER_ID
	docker rm $CONTAINER_ID
fi

echo "building container"

CONTAINER_ID=$(docker create \
	-e "RUN_UID=$(id -u)" \
	-e "RUN_GID=$(id -g)" \
	--user "$(id -u):$(id -g)" \
	-v `pwd`/../..:/build \
	--name philadelphia-build -t -i todoit/philadelphia-build) \
	|| exit 1

echo -n $CONTAINER_ID > CONTAINER_ID

echo "starting container"

# no idea why stdout is not visible in 'Console Out' in Jenkins (it is visible in regular shell). Hence workaround is applied
docker start --interactive --attach $CONTAINER_ID
ERR=$?
docker logs $CONTAINER_ID
exit $ERR
