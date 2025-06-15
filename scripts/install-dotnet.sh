#!/bin/bash
set -e

# Installs .NET 8 SDK on Ubuntu-based systems

if command -v dotnet &>/dev/null; then
  echo "Dotnet already installed: $(dotnet --version)"
  exit 0
fi

apt-get update
apt-get install -y dotnet-sdk-8.0
