#include "types.h"

#include <stddef.h>

const struct type_t native_bool = {
    .name = "bool",
    .kind = TK_PRIMITIVE,
    .data = { .primitive = { .size = sizeof(uint8_t) } }
};

const struct type_t native_u8 = { 
    .name = "u8",
    .kind = TK_PRIMITIVE,
    .data = { .primitive = { .size = sizeof(uint8_t) } }
};

const struct type_t native_i64 = { 
    .name = "i64",
    .kind = TK_PRIMITIVE,
    .data = { .primitive = { .size = sizeof(uint64_t) } }
};

uint64_t type_field_sizeof(const struct type_field_t* field) {
    if(field->type->kind == TK_PRIMITIVE) {
        return field->type->data.primitive.size;
    }
    
    return sizeof(void*);
}

uint64_t type_sizeof(const struct type_t* type) {
    if(type->kind == TK_PRIMITIVE) {
        return type->data.primitive.size;
    }
    
    const struct type_field_t** fields = type->data.reference.fields;
    uint64_t size = 0;
       
    while(*fields != NULL) {
        const struct type_field_t* field = *fields++;
        size += type_field_sizeof(field);
    }
        
    return size;
}