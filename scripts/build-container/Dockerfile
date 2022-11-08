FROM ubuntu:20.04

USER root
RUN apt-get update
RUN apt-get install -y curl libicu66
RUN curl -sSL https://dot.net/v1/dotnet-install.sh | bash
ENV DOTNET_ROOT /root/.dotnet
ENV PATH $PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools

RUN dotnet tool install --global PowerShell

RUN apt-get install -y python3 python3-pip python3-venv