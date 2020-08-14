#!/bin/bash

# case insensitive regexp match
shopt -s nocasematch

if [[ $1 =~ /(bin|obj|obj_netfx|output)/ ]] || [[ $1 =~ \.csproj$ ]];
then
#    echo "skipping $1"
    echo -n ""
else
    PTH_REL=${1//\//\\}

#left trims ..\Philadelphia.Common\
    PTH_WO_DOTS=${PTH_REL:23}

# uses separate "-ne" because "something\bridge" would brake on "\b" and similar false escapes

    if [[ $1 =~ \.[cC][sS]$ ]]; then
        echo -n "    <Compile Include=\"${PTH_REL}\">"
        echo -ne "\r\n"
        echo -n "      <Link>${PTH_WO_DOTS}</Link>"
        echo -ne "\r\n"
        echo -n "    </Compile>"
        echo -ne "\r\n"
    else
        echo -n "    <None Include=\"${PTH_REL}\">"
        echo -ne "\r\n"
        echo -n "      <Link>${PTH_WO_DOTS}</Link>"
        echo -ne "\r\n"
        echo -n "    </None>"
        echo -ne "\r\n"
    fi
fi
