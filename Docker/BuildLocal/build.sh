#!/bin/bash

echo "building image"
docker build -t="todoit/philadelphia-build" . || exit 1

if [ -f ./CONTAINER_ID ];
then
	echo "removing old container"
	CONTAINER_ID=`cat ./CONTAINER_ID`
	docker kill $CONTAINER_ID
	docker rm $CONTAINER_ID
fi

echo "building container"

CONTAINER_ID=$(docker create \
	-v `pwd`/../..:/build \
	--name philadelphia-build -t -i todoit/philadelphia-build) \
	|| exit 1

echo -n $CONTAINER_ID > CONTAINER_ID

echo "starting container"
exec docker start --interactive $CONTAINER_ID || exit 1
