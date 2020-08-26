#!/bin/bash

PTH="$(realpath $0)"
DIR=$(dirname $PTH)

echo "dir $DIR"
cd $DIR

LINES=$(docker ps | grep [p]hiladelphia-demo-server | wc -l)

if [ $LINES -eq 1 ];
then
    echo "`date` doesn't need restart"
else
    echo "`date` needs restart"
    ./build_and_run.sh
fi
