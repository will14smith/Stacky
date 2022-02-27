#include "gc.h"
#include "heap.h"
#include "root.h"

#include <stdlib.h>

struct gc_t
{
    struct heap_t* heap;
    struct root_t* root;
};

struct gc_t* gc_new() { 
    struct gc_t* context = malloc(sizeof(struct gc_t));
    
    context->heap = heap_new();
    context->root = root_new();
    
    return context;
}
void gc_destroy(struct gc_t* gc) { 
    // TODO deallocate all items
    
    heap_destroy(gc->heap);
    root_destroy(gc->root);
    
    free(gc);
}

void* gc_allocate(struct gc_t* gc, const struct type_t* type) {
    uint64_t size = type_sizeof(type);
    void* pointer = malloc(size);

    // TODO add to heap tracking + add any metadata
    heap_add(gc->heap, pointer);

    return pointer;
}
void* gc_allocate_raw(struct gc_t* gc, uint64_t size) { 
    void* pointer = malloc(size);
    
    // TODO add to heap tracking + add any metadata
    heap_add(gc->heap, pointer);
    
    return pointer; 
}

void gc_root_add(struct gc_t* gc, const void* ptr) {
    root_add(gc->root, ptr);
}
void gc_root_remove(struct gc_t* gc, const void* ptr) { 
    root_remove(gc->root, ptr);
}