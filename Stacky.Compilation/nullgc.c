#include <stdint.h>
#include <stdlib.h>

char* gc_allocate_raw(uint64_t count) { return malloc(count); }
void gc_root_add(char* ptr) { }
void gc_root_remove(char* ptr) { }