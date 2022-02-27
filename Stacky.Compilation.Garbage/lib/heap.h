#pragma once

#include <stdint.h>

struct heap_t;

struct heap_t* heap_new();
void heap_destroy(struct heap_t* heap);
int64_t heap_count(const struct heap_t* heap);

void heap_add(struct heap_t* heap, const void* value);
void heap_remove(struct heap_t* heap,const void* value);

struct heap_iterator_t {
    const struct heap_t* heap;
    int64_t index;
};

void heap_iterate_init(const struct heap_t* heap, struct heap_iterator_t* iterator);
const void* heap_iterate_current(const struct heap_iterator_t* iterator);
int heap_iterate_next(struct heap_iterator_t* iterator);