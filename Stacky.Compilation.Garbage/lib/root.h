#pragma once

#include <stdint.h>

struct root_t;
struct root_entry_t;

struct root_t* root_new();
void root_destroy(struct root_t* root);

void root_add(struct root_t* root, const void* value);
void root_remove(struct root_t* root, const void* value);

struct root_iterator_t {
    const struct root_t* root;
    int index;
    struct root_entry_t* entry; 
};

void root_iterate_init(const struct root_t* root, struct root_iterator_t* iterator);
const void* root_iterate_current(const struct root_iterator_t* iterator);
int root_iterate_next(struct root_iterator_t* iterator);