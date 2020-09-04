#!/bin/bash

#
# NOTE
#    Make sure within wsl you run dos2unix on both *.sh files in this directory. 
#    Failing to do so will get you a confusing errors:  
#       $'\r': command not found

#
# # README
#
# # What is this script's purpose?
#
# I grew tired of fighting against msbuild bugs due to multiple project files in the same folder manifestating as:
# https://stackoverflow.com/questions/52833741/your-project-does-not-reference-netframework-version-v4-6-2-framework-add-a
# https://stackoverflow.com/questions/36190601/your-project-is-not-referencing-the-netframework-version-v4-5-framework#41154960
#
# Cleaning obj and bin generally worked but it seems in 20202-08 with VS Version 16.7.1 that workaround stopped working.
#
# # What is it doing?
#
# This WSL script synchronizes AsBridgeDotNet csproj with its parent dotnet core project. You need to run it only if files are moved/added/removed from parent project



# exit on any error
set -e

SRC="Philadelphia.Common.AsBridgeDotNet.csproj"
ORG=$(cat $SRC)

ST_TAG="<!-- redirects start -->"
END_TAG="<!-- redirects end -->"

AFTER_ST_TAG=${ORG#*$ST_TAG}
AFTER_END_TAG=${ORG#*$END_TAG}

ST_AT=$((${#ORG} - ${#ST_TAG} - ${#AFTER_ST_TAG}))

(
    echo -n ${ORG:0:${ST_AT}}
    echo -n "$ST_TAG"
    echo -n "  <ItemGroup>"
    echo -ne "\r\n"
    find ../Philadelphia.Common -type f -print0 | xargs -0 -L 1 ./sync-filename2content.sh | paste
    echo -n "  </ItemGroup>"
    echo -ne "\r\n"
    echo -n "$END_TAG"
    echo -n "$AFTER_END_TAG"
) > tmp

mv tmp "$SRC"

echo "OK"
