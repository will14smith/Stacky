#pragma once

#include <stdint.h>

struct type_t;

enum type_kind_t {
    TK_PRIMITIVE = 0,
    TK_REFERENCE = 1,
};

struct type_primitive_t {
    uint64_t size;
};

struct type_field_t {
    const char* name;
    const struct type_t* type; 
};

struct type_reference_t {
    // null terminated array
    const struct type_field_t** fields;
};

struct type_t
{
    const char* name;
    enum type_kind_t kind;
    union {
        struct type_primitive_t primitive;
        struct type_reference_t reference;
    } data;
};

extern const struct type_t native_bool;
extern const struct type_t native_u8;
extern const struct type_t native_i64;

// the size of a field of that type 
uint64_t type_field_sizeof(const struct type_field_t* field);
// just the size of the data, any metadata/headers are separate
uint64_t type_sizeof(const struct type_t* type);