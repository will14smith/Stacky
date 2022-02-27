#pragma once

#include <stdint.h>

struct heap_t;

struct heap_t* heap_new();
void heap_destroy(struct heap_t* heap);
int64_t heap_count(struct heap_t* heap);

void heap_add(struct heap_t* heap, void* value);
void heap_remove(struct heap_t* heap, void* value);

struct heap_iterator_t {
    struct heap_t* heap;
    int64_t index;
};

void heap_iterate_init(struct heap_t* heap, struct heap_iterator_t* iterator);
void* heap_iterate_current(struct heap_iterator_t* iterator);
int heap_iterate_next(struct heap_iterator_t* iterator);