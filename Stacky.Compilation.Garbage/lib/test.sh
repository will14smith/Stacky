#!/usr/bin/env bash
set -Eeuo pipefail

export LD_LIBRARY_PATH=$(pwd)${LD_LIBRARY_PATH:+:$LD_LIBRARY_PATH}

cd ..
dotnet test