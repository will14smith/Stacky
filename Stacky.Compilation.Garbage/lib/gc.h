#pragma once

#include "types.h"
#include <stdint.h>

struct gc_t;

struct gc_t* gc_new();
void gc_destroy(struct gc_t* gc);

void* gc_allocate(struct gc_t* gc, const struct type_t* type);
void* gc_allocate_raw(struct gc_t* gc, uint64_t size);

void gc_root_add(struct gc_t* gc, const void* ptr);
void gc_root_remove(struct gc_t* gc, const void* ptr);