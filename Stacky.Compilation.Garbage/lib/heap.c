#include "heap.h"

#include <stdlib.h>

struct heap_t {
    const void** ptr;
    int64_t capacity;
    int64_t count;
};

struct heap_t* heap_new() {
    struct heap_t* heap = malloc(sizeof(struct heap_t));
        
    heap->capacity = 4;
    heap->count = 0;
    heap->ptr = malloc(sizeof(void*) * heap->capacity);
    
    return heap;
}

void heap_destroy(struct heap_t* heap) {
    free(heap->ptr);
    free(heap);
}

int64_t heap_count(const struct heap_t* heap) { 
    return heap->count;
}

void heap_add(struct heap_t* heap, const void* value) {
    if(heap->count == heap->capacity) {
        heap->capacity <<= 1;
        heap->ptr = realloc(heap->ptr, sizeof(void*) * heap->capacity);
    }
    
    heap->ptr[heap->count++] = value;
}
void heap_remove(struct heap_t* heap, const void* value) {
    int64_t removed = 0;
    
    int64_t read_index = 0;
    int64_t write_index = 0;
    for(; read_index < heap->count; read_index++) {
        if(heap->ptr[read_index] == value) {
            removed++;
        } else {
            if(read_index != write_index) {
                heap->ptr[write_index] = heap->ptr[read_index];
            }
        
            write_index++;
        }
    }
    
    heap->count -= removed;
}

void heap_iterate_init(const struct heap_t* heap, struct heap_iterator_t* iterator) {
    iterator->heap = heap;
    iterator->index = -1;
}
const void* heap_iterate_current(const struct heap_iterator_t* iterator) {
    if(iterator->index < 0 || iterator->index >= iterator->heap->count) { return 0; }
   
    return iterator->heap->ptr[iterator->index];
}
int heap_iterate_next(struct heap_iterator_t* iterator) {
    return ++iterator->index < iterator->heap->count;    
}