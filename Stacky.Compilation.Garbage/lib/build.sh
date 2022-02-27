#!/usr/bin/env bash
set -Eeuo pipefail

gcc -c gc.c -Wall -Werror -fpic -o gc.o
gcc -c heap.c -Wall -Werror -fpic -o heap.o
gcc -c root.c -Wall -Werror -fpic -o root.o
gcc -shared -o gc.so gc.o heap.o root.o