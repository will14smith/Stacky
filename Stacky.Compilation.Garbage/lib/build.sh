#!/usr/bin/env bash
set -Eeuo pipefail

gcc -g -c gc.c -Wall -Werror -fpic -o gc.o
gcc -g -c heap.c -Wall -Werror -fpic -o heap.o
gcc -g -c root.c -Wall -Werror -fpic -o root.o
gcc -g -c types.c -Wall -Werror -fpic -o types.o
gcc -g -shared -o gc.so gc.o heap.o root.o types.o