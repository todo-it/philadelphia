FROM ubuntu:18.04

LABEL Description="Philadelphia Demo" Vendor="TODO IT Sp z o.o." Version="3.0.20181104"
RUN apt-get update && \
	apt-get -y upgrade && \
	apt-get -y install tzdata dos2unix && \
	apt-get clean
RUN apt-get -y install wget gpg apt-utils

#from https://www.microsoft.com/net/learn/get-started/linux/ubuntu18-04
RUN wget -qO- https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.asc.gpg
RUN mv microsoft.asc.gpg /etc/apt/trusted.gpg.d/
RUN wget -q https://packages.microsoft.com/config/ubuntu/18.04/prod.list 
RUN mv prod.list /etc/apt/sources.list.d/microsoft-prod.list
RUN chown root:root /etc/apt/trusted.gpg.d/microsoft.asc.gpg
RUN chown root:root /etc/apt/sources.list.d/microsoft-prod.list
RUN apt-get install apt-transport-https
RUN apt-get update
RUN apt-get -y install dotnet-sdk-2.2

RUN apt-get clean
ADD binaries /philadelphia/
EXPOSE 8090
ADD run.sh /
RUN chmod a+x /run.sh && dos2unix /run.sh
RUN rm /etc/localtime && ln -s /usr/share/zoneinfo/Europe/Warsaw /etc/localtime
RUN useradd philadelphia
USER philadelphia
ENTRYPOINT ["/run.sh"]
