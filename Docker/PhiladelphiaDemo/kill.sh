#!/bin/bash

TS=`date +%Y-%m-%d_%H-%M-%S`

CID=`cat CONTAINER_ID`

docker logs $CID > ../$TS

ISWINDOWS=`uname -o`

if [ "$ISWINDOWS" = "Msys" ];
then
	eval "$(docker-machine env --shell=bash default)"
fi

docker kill `cat CONTAINER_ID`
