FROM ubuntu:18.04

LABEL Description="Philadelphia Demo" Vendor="TODO IT Sp z o.o." Version="3.0.20190225"
RUN apt-get update && apt-get -y install tzdata dos2unix wget gpg apt-utils

# from https://www.mono-project.com/download/stable/#download-lin
RUN apt-get -y install gnupg ca-certificates
RUN apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
RUN echo "deb https://download.mono-project.com/repo/ubuntu stable-bionic main" | tee /etc/apt/sources.list.d/mono-official-stable.list
RUN apt-get -y update && apt-get -y install mono-complete

# add nuget from Mono
RUN apt-get -y install nuget

#from https://www.microsoft.com/net/learn/get-started/linux/ubuntu18-04
RUN wget -qO- https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.asc.gpg
RUN mv microsoft.asc.gpg /etc/apt/trusted.gpg.d/
RUN wget -q https://packages.microsoft.com/config/ubuntu/18.04/prod.list
RUN mv prod.list /etc/apt/sources.list.d/microsoft-prod.list
RUN chown root:root /etc/apt/trusted.gpg.d/microsoft.asc.gpg
RUN chown root:root /etc/apt/sources.list.d/microsoft-prod.list
RUN apt-get install apt-transport-https
RUN apt-get update && apt-get -y install dotnet-sdk-3.1


# needed for Build.csx version number fallback
RUN apt-get -y install git

#cleanup
RUN apt-get clean

ARG RUN_UID
ENV RUN_UID=$RUN_UID

ARG RUN_GID
ENV RUN_GID=$RUN_GID

RUN echo "using uid=$RUN_UID gid=$RUN_GID"
RUN groupadd --gid $RUN_GID buildergroup
RUN useradd --uid $RUN_UID --gid $RUN_GID builder

RUN touch /run.sh
RUN chown builder /run.sh
RUN chmod a+x /run.sh
RUN mkdir -p /home/builder
RUN chown builder /home/builder
USER builder

# from https://github.com/filipw/dotnet-script
RUN dotnet tool install -g dotnet-script

RUN echo "#!/bin/bash" >> /run.sh
RUN echo "echo \"starting dotnet-script\"" >> /run.sh
RUN echo "cd /build/Build/ && ~/.dotnet/tools/dotnet-script --no-cache --verbosity i Build.csx && ./build_demo_archive.sh && exec ./build_tests.sh" >> /run.sh

ENTRYPOINT ["/run.sh"]
