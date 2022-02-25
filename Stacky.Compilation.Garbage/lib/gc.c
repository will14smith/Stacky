#include "gc.h"
#include <stdio.h>
#include <stdlib.h>

struct gc_t
{
    // heap - all allocated items
    // root - all root items
};

struct gc_t* gc_new() { 
    struct gc_t* context = malloc(sizeof(struct gc_t));
    
    // TODO init context
    
    return context;
}
void gc_destroy(struct gc_t* gc) { 
    // TODO deallocate all items
    // TODO cleanup context
    free(gc);
}

void* gc_allocate(struct gc_t* gc, struct type_t* type) {
    // TODO calculate type size
    uint64_t size = 1; 
    void* pointer = malloc(size);

    // TODO add to heap tracking + add any metadata

    return pointer;
}
void* gc_allocate_raw(struct gc_t* gc, uint64_t size) { 
    void* pointer = malloc(size);
    
    // TODO add to heap tracking + add any metadata
    
    return pointer; 
}

void gc_root_add(struct gc_t* gc, void* ptr) {
    // TODO add to root tracking 
}
void gc_root_remove(struct gc_t* gc, void* ptr) { 
    // TODO remove from root tracking 
}