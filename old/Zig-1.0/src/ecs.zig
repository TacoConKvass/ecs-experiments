const std = @import("std");

const max_entities = 0b1111;
const EntityID = u4;

const NameTypeTuple = std.meta.Tuple(&.{
    [:0]const u8,
    type,
});

pub fn World(comptime field_declarations: anytype) type {
    var fields: [field_declarations.len]NameTypeTuple = undefined;
    for (fields, 0..) |_, index| {
        fields[index][0] = field_declarations[index][0];
        fields[index][1] = Component(field_declarations[index][1]);
    }

    const ComponentStore = BuildStruct(fields);

    return struct {
        allocator: std.mem.Allocator,
        components: ComponentStore,

        pub fn init(allocator: *std.mem.Allocator) @This() {
            const component_store: ComponentStore = undefined;

            inline for (std.meta.fields(ComponentStore)) |t| {
                var field = @field(component_store, t.name);
                field = t.type.init();
            }

            return @This(){
                .allocator = allocator.*,
                .components = component_store,
            };
        }
    };
}

pub fn BuildStruct(comptime field_declarations: anytype) type {
    var fields: [field_declarations.len]std.builtin.Type.StructField = undefined;
    for (field_declarations, 0..) |declaration, index| {
        fields[index] = .{
            .name = declaration[0][0..],
            .type = declaration[1],
            .default_value = null,
            .is_comptime = false,
            .alignment = 0,
        };
    }
    return @Type(.{
        .Struct = .{
            .layout = .auto,
            .fields = &fields,
            .decls = &[_]std.builtin.Type.Declaration{},
            .is_tuple = false,
        },
    });
}

pub fn Component(comptime T: type) type {
    return struct {
        // Sparse Set
        entities: [max_entities]?EntityID,
        values: []T,

        pub fn init() @This() {
            return @This(){
                .entities = [_]?EntityID{null} ** max_entities,
                .values = &[_]T{},
            };
        }

        pub fn add_entity(this: *@This(), id: EntityID, value: T, allocator: std.mem.Allocator) !void {
            const old_length = this.values.len;
            this.values = try allocator.realloc(this.values, old_length + 1);
            this.values[old_length] = value;

            this.entities[id] = @truncate(old_length);
        }

        pub fn add_entity_batch(this: *@This(), ids: []EntityID, values: []T, allocator: std.mem.Allocator) !void {
            std.debug.assert(ids.len == values.len);

            const old_length: EntityID = this.entities.len;
            this.values = try allocator.realloc(this.values, old_length + values.len);
            @memcpy(this.values[old_length..], values);

            for (ids, 0..) |entity, index| {
                this.entities[entity] = @truncate(old_length + index);
            }
        }
    };
}
