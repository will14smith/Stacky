#include "gc.h"
#include "heap.h"
#include "root.h"

#include <stdlib.h>

struct gc_t
{
    struct heap_t* heap;
    struct root_t* root;
};

struct gc_header_t {
    struct {
        uint8_t marked : 1;
        uint8_t raw    : 1;
    };

    union {
        const struct type_t* type;
        uint64_t size;
    };       
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
    uint64_t header_size = sizeof(struct gc_header_t);
    uint64_t size = type_sizeof(type);
    void* pointer = malloc(header_size + size);
    
    // init header    
    struct gc_header_t* header = pointer;
    header->marked = 0;
    header->raw = 0;
    header->type = type;    

    // track the allocation
    heap_add(gc->heap, pointer);
    
    // return the offset to the actual data
    return pointer + header_size;
}
void* gc_allocate_raw(struct gc_t* gc, uint64_t size) { 
    uint64_t header_size = sizeof(struct gc_header_t);
    void* pointer = malloc(header_size + size);

    // init header    
    struct gc_header_t* header = pointer;
    header->marked = 0;
    header->raw = 1;
    header->size = size;    

    // track the allocation
    heap_add(gc->heap, pointer);
    
    // return the offset to the actual data
    return pointer + header_size; 
}

void gc_root_add(struct gc_t* gc, const void* ptr) {
    root_add(gc->root, ptr);
}
void gc_root_remove(struct gc_t* gc, const void* ptr) { 
    root_remove(gc->root, ptr);
}