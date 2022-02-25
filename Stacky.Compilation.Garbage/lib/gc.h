#pragma once

#include <stdint.h>

struct gc_t;
struct field_t;

enum kind_t
{
    TK_PRIMITIVE = 0,
    TK_REFERENCE = 1,
};

struct type_t
{
    const char* name;
    int num_fields;
    const struct field_t** fields;
};

struct field_t
{
    const char* name;
    enum kind_t kind;
    const struct type_t* type; 
};

struct gc_t* gc_new();
void gc_destroy(struct gc_t* gc);

void* gc_allocate(struct gc_t* gc, struct type_t* type);
void* gc_allocate_raw(struct gc_t* gc, uint64_t size);

void gc_root_add(struct gc_t* gc, void* ptr);
void gc_root_remove(struct gc_t* gc, void* ptr);