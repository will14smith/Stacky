#include "gc.h"
#include "heap.h"
#include "root.h"

#include <stdlib.h>
#include <stdio.h>

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

struct gc_header_t* gc_data_to_header(void* data) { return data - sizeof(struct gc_header_t); }
void* gc_header_to_data(struct gc_header_t* header) { return (void*)header + sizeof(struct gc_header_t); }

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
    void* pointer = calloc(1, header_size + size);
        
    // init header    
    struct gc_header_t* header = pointer;
    header->marked = 0;
    header->raw = 0;
    header->type = type;

    // track the allocation
    heap_add(gc->heap, pointer);
    
    return gc_header_to_data(pointer);
}
void* gc_allocate_raw(struct gc_t* gc, uint64_t size) { 
    uint64_t header_size = sizeof(struct gc_header_t);
    void* pointer = calloc(1, header_size + size);

    // init header    
    struct gc_header_t* header = pointer;
    header->marked = 0;
    header->raw = 1;
    header->size = size;    

    // track the allocation
    heap_add(gc->heap, pointer);
    
    return gc_header_to_data(pointer);
}

void gc_root_add(struct gc_t* gc, const void* ptr) {
    root_add(gc->root, ptr);
}
void gc_root_remove(struct gc_t* gc, const void* ptr) { 
    root_remove(gc->root, ptr);
}

void gc_mark_entry(struct gc_header_t* entry) {
    if(entry->marked) {
        return;
    }
        
    entry->marked = 1;
        
    if(entry->raw) {
        return;
    }
        
    const struct type_t* type = entry->type;
    if(type->kind == TK_PRIMITIVE) {
        return;
    }
    
    const struct type_field_t** fields = type->data.reference.fields;
       
    uint64_t offset = 0;
    while(*fields != NULL) {
        const struct type_field_t* field = *fields++;
        
        if(field->type->kind == TK_REFERENCE) {        
            void** dataPtr = gc_header_to_data(entry) + offset;
            if(*dataPtr) {
                struct gc_header_t* dataHeader = gc_data_to_header(*dataPtr);
                gc_mark_entry(dataHeader);
            }
        }
        
        offset += type_field_sizeof(field);
    }
}
void gc_mark(const struct gc_t* gc) {
    struct root_iterator_t it;
    root_iterate_init(gc->root, &it);
    while(root_iterate_next(&it)) {
        struct gc_header_t* header = gc_data_to_header((void*)root_iterate_current(&it));       
        gc_mark_entry(header);
    }
}
void gc_unmark(const struct gc_t* gc) {
    struct heap_iterator_t it;
    heap_iterate_init(gc->heap, &it);
    while(heap_iterate_next(&it)) {
        struct gc_header_t* header = (void*)heap_iterate_current(&it);
        header->marked = 0;
    }
}
void gc_sweep(struct gc_t* gc) {
    struct heap_t* newHeap = heap_new();

    struct heap_iterator_t it;
    heap_iterate_init(gc->heap, &it);
    while(heap_iterate_next(&it)) {
        struct gc_header_t* header = (void*)heap_iterate_current(&it);
        if(header->marked) {
            header->marked = 0;
            heap_add(newHeap, header);
        } else {
            free(header);
        }
    }
    
    heap_destroy(gc->heap);
    gc->heap = newHeap;    
}

void gc_collect(struct gc_t* gc) {
    gc_mark(gc);
    gc_sweep(gc);
}
struct gc_stats_t gc_stats(const struct gc_t* gc) {
    struct gc_stats_t stats = {0};
    
    stats.allocated_items = heap_count(gc->heap);
    
    gc_mark(gc);
    
    struct heap_iterator_t heap_iterator;
    heap_iterate_init(gc->heap, &heap_iterator);
    while(heap_iterate_next(&heap_iterator)) {
        const struct gc_header_t* header = heap_iterate_current(&heap_iterator);
        if(header->raw) {
            stats.allocated_items_size += header->size;
        } else {
            stats.allocated_items_size += type_sizeof(header->type);
        }
        
        if(header->marked) {
            stats.reachable_items++;
        }
    }

    gc_unmark(gc);
    
    struct root_iterator_t root_iterator;
    root_iterate_init(gc->root, &root_iterator);
    while(root_iterate_next(&root_iterator)) {
        stats.rooted_items++;
    }
    
    return stats;
}

void gc_dump_entry(struct gc_header_t* entry, int depth) {
    printf("%*s- %p ", depth, "", gc_header_to_data(entry));
   
    if(entry->raw) {
        printf("raw data - %ld bytes", entry->size);
    } else {
        printf("%s - %ld bytes", entry->type->name, type_sizeof(entry->type));
    }
    
    printf("\n");

    if(entry->marked) {
        return;
    }
    
    if(entry->raw) {
        return;
    }
        
    entry->marked = 1;
                
    const struct type_t* type = entry->type;
    if(type->kind == TK_PRIMITIVE) {
        return;
    }
    
    const struct type_field_t** fields = type->data.reference.fields;
              
    uint64_t offset = 0;
    while(*fields != NULL) {
        const struct type_field_t* field = *fields++;
                
        if(field->type->kind == TK_REFERENCE) {
            void** dataPtr = gc_header_to_data(entry) + offset;
            if(*dataPtr) {
                struct gc_header_t* dataHeader = gc_data_to_header(*dataPtr);
                gc_dump_entry(dataHeader, depth + 2);
            }
        }
        
        offset += type_field_sizeof(field);
    }
}

void gc_dump(struct gc_t* gc) {
    printf("-- dumping root --\n");

    struct root_iterator_t it;
    root_iterate_init(gc->root, &it);
    while(root_iterate_next(&it)) {
        struct gc_header_t* header = gc_data_to_header((void*)root_iterate_current(&it));       
        gc_dump_entry(header, 0);
    }
    
    gc_unmark(gc);
    
    printf("-- finished dumping root --\n");
}
