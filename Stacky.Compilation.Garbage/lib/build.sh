#!/usr/bin/env bash
set -Eeuo pipefail

gcc -c gc.c -Wall -Werror -fpic -o gc.o
gcc -shared -o gc.so gc.o