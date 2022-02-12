#include <stdint.h>

extern int8_t *stackPointer;

typedef struct
{
    // heap - all allocated items
    // root - all root items
} gc_t;

typedef struct
{
    const char *name;
    int numFields;
    const fielddef_t *fields;
} typedef_t;

typedef struct
{
    const char *name;
    typekind_t kind;
} field_t;

enum typekind_t
{
    TK_INT,
    TK_STRING,
    TK_REF,
}

void
gc_init(gc_t *gc)
{
    // TODO handle initializing gc
}

char *gc_allocate(gc_t *gc, typedef_t *type)
{
    // TODO handle allocating header + object
    // TODO ???
    return gc_allocate_raw(0);
}

char *gc_allocate_raw(gc_t *gc, uint32_t size)
{
    // TODO handle mark/sweep
    // TODO handle allocating memory
    return 0;
}

void gc_root_add(gc_t *gc, char *ptr)
{
    // TODO add item to root
}

void gc_root_remove(gc_t *gc, char *ptr)
{
    // TODO remove item from root
}