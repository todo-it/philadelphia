#!/bin/bash

SSHUSER=$1
SSHHOST=$2

echo "removing old container if present"
PRES=$(ssh ${SSHUSER}@${SSHHOST} "docker ps -a | grep [p]hiladelphia-demo-server | wc -l")

if [ ! "$PRES" -eq 0 ];
then
    ssh ${SSHUSER}@${SSHHOST} "docker rm --force philadelphia-demo-server" || exit 1
    echo "removed old instance"
fi

echo "deploying to ${SSHHOST}"
scp *.sh Dockerfile ${SSHUSER}@${SSHHOST}:~/ && \
    scp ../../_output/philadelphia.tar.gz ${SSHUSER}@${SSHHOST}:~/ && \
    ssh ${SSHUSER}@${SSHHOST} "./build_and_run.sh" || \
    exit 1

echo "waiting for start (cheap vm safety)"
sleep 20

echo "validating if server started"
wget -O/dev/null -q http://${SSHHOST} || exit 1

echo "running OK"
