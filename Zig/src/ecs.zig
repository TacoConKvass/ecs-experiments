const std = @import("std");
const meta = @import("meta.zig");
const assert = std.debug.assert;

const NameToType = std.meta.Tuple(&.{
    [:0]const u8,
    type,
});

/// Defines the place where all components and entities are held
/// Provided types are transformed into components and are accessible via world.components.TypeName
/// Can hold up to 4_294_967_295 entities
pub fn World(comptime types: []const type, comptime max_amount: u32) type {
    var fields: [types.len]NameToType = undefined;
    for (types, 0..) |T, index| {
        fields[index] = .{
            meta.typeNameToFieldName(T),
            Component(T, max_amount),
        };
    }

    const ComponentStore = meta.BuildStruct(fields);
    const ComponentMask = std.meta.Int(.unsigned, types.len);

    return struct {
        /// Possible to receive when calling add_entity
        const Error = error{
            OutOfEntitySlots,
        };

        const max_length = max_amount;
        var first_free: usize = 0;

        /// Allocator used for all entity related allocations
        allocator: std.mem.Allocator,

        /// Stores information on which components the entity has, as a bit mask
        /// The entities ID is its index in this array
        entities: []ComponentMask,

        /// Stores all generated components
        components: ComponentStore,

        /// Signifies that the amount of entities changed
        updated: bool = false,

        /// Initializes the object
        pub fn init(allocator: std.mem.Allocator) !@This() {
            var component: ComponentStore = undefined;

            inline for (std.meta.fields(ComponentStore)) |field| {
                @field(component, field.name) = try field.type.init(allocator);
            }

            const entities = try allocator.alloc(ComponentMask, max_length);
            @memset(entities, @as(ComponentMask, 0));

            return @This(){
                .allocator = allocator,
                .entities = entities,
                .components = component,
            };
        }

        /// Adds the specified components to the entity with the specified ID
        pub fn add_entity(this: *@This(), components: anytype) !u32 {
            var found = false;
            var entity: u32 = undefined;
            for (first_free..this.entities.len) |index| {
                if (this.entities[index] == 0) {
                    entity = @truncate(index);
                    found = true;
                    first_free = index;
                    break;
                }
            }
            if (!found) return Error.OutOfEntitySlots;

            inline for (components) |component| {
                // Add entity to the component set
                var field = &@field(this.components, meta.typeNameToFieldName(@TypeOf(component)));
                try field.add_entity(@truncate(entity), component, this.allocator);

                // Add to bit mask of the entity
                const shift_amount = meta.indexOfTypeInArray(@TypeOf(component), types);
                this.entities[entity] = this.entities[entity] | (@as(ComponentMask, 1) <<| shift_amount);
            }

            this.updated = true;
            return entity;
        }

        pub fn delete_entity(this: *@This(), entity: u32) !void {
            if (this.entities[entity] == 0) return;

            inline for (std.meta.fields(ComponentStore)) |component| {
                try @field(this.components, component.name).delete_entity(entity, this.allocator);
            }

            this.entities[entity] = 0;
            this.updated = true;
        }

        /// Checks if the specified entity has the queried components
        pub fn entity_has(this: *@This(), entity: u32, components: []const type) bool {
            const one: ComponentMask = 1;
            var components_to_check: ComponentMask = 0;
            inline for (components) |component| {
                _ = @field(this.components, meta.typeNameToFieldName(component));
                const shift_amount = meta.indexOfTypeInArray(component, types);
                components_to_check |= one <<| shift_amount;
            }
            return (this.entities[entity] & components_to_check) == components_to_check;
        }

        /// Returns all entities that have the specified components as a Query.
        /// Might throw an error on allocation.
        pub fn query(this: *@This(), comptime searched: []const type) !Query(searched, @This()) {
            var records = try this.allocator.alloc(Record(searched), max_length);
            var length: usize = 0;
            for (this.entities, 0..) |_, i| {
                if (this.entity_has(@truncate(i), searched)) {
                    var record: Record(searched) = undefined;
                    record.entity = @truncate(i);

                    inline for (searched) |T| {
                        const component = @field(this.components, meta.typeNameToFieldName(T));
                        @field(record, meta.typeNameToFieldName(T)) = &component.dense[component.sparse[i].?];
                    }

                    records[length] = record;
                    length += 1;
                }
            }

            records = try this.allocator.realloc(records, length);

            return Query(searched, @This()){
                .entities = records,
            };
        }
    };
}

/// Stores all entities that have components of the searched types
pub fn Query(comptime searched: []const type, ChosenWorld: type) type {
    return struct {
        pub var cache: @This() = undefined;
        var first: bool = true;
        entities: []Record(searched),

        /// Executes the query, caching the result.
        pub fn execute(world: *ChosenWorld) !@This() {
            if (world.updated or first) {
                if (!first) world.allocator.free(cache.entities) else first = false;
                cache = try world.query(searched);
            }
            return cache;
        }
    };
}

/// Stores an entities ID, and all data from its queried components
pub fn Record(comptime searched: []const type) type {
    var fields_tuple: [searched.len + 1]NameToType = undefined;
    fields_tuple[0] = .{
        "entity",
        u32,
    };
    for (searched, 0..) |T, index| {
        fields_tuple[index + 1] = .{
            meta.typeNameToFieldName(T),
            *T,
        };
    }
    return meta.BuildStruct(fields_tuple);
}

/// Holds data of the provided type of all entities in a Sparse-Dense set pair
pub fn Component(comptime T: type, comptime length: u32) type {
    return struct {
        const Type = T;
        sparse: []?u32,
        dense: []T,
        sparse_index: []u32,

        pub fn init(allocator: std.mem.Allocator) !@This() {
            const sparse = try allocator.alloc(?u32, length);
            @memset(sparse, null);
            return @This(){
                .sparse = sparse,
                .dense = &[_]T{},
                .sparse_index = &[_]u32{},
            };
        }

        /// Adds a single data entry to the dense set, and stores it's index at the index of the enrtities ID in the sparse set
        pub fn add_entity(this: *@This(), entity: u32, value: T, allocator: std.mem.Allocator) !void {
            const old_length = this.dense.len;
            this.dense = try allocator.realloc(this.dense, old_length + 1);
            this.sparse_index = try allocator.realloc(this.sparse_index, old_length + 1);
            this.dense[old_length] = value;
            this.sparse_index[old_length] = entity;

            this.sparse[entity] = @truncate(old_length);
        }

        /// Deletes the entity on the specified index
        pub fn delete_entity(this: *@This(), entity: u32, allocator: std.mem.Allocator) !void {
            if (this.sparse[entity] == null) return;
            const old_length = this.dense.len;
            this.sparse[this.sparse_index[old_length - 1]] = this.sparse_index[this.sparse[entity].?];
            this.sparse_index[this.sparse[entity].?] = this.sparse_index[old_length - 1];
            this.dense[this.sparse[entity].?] = this.dense[old_length - 1];
            this.sparse[entity] = null;

            this.dense = try allocator.realloc(this.dense, old_length - 1);
            this.sparse_index = try allocator.realloc(this.sparse_index, old_length - 1);
        }
    };
}

// Tests

test "Entity has component check" {
    const Position = struct { data: @Vector(2, f32) };
    const Velocity = struct { data: @Vector(2, f32) };
    const Hitbox = struct { data: @Vector(2, u16) };

    const TestWorld = World(&[_]type{
        Position,
        Velocity,
        Hitbox,
    }, 12);

    std.debug.print("Entity has components check:\n", .{});

    var gpa = std.heap.GeneralPurposeAllocator(.{}){};
    const allocator = gpa.allocator();
    var world = try TestWorld.init(allocator);

    for (0..4) |_| {
        _ = try world.add_entity(.{
            Position{ .data = @Vector(2, f32){ 0, 4 } },
            Velocity{ .data = @Vector(2, f32){ 1, 0 } },
        });
    }

    std.debug.print("Entity 5 has Position: {}\n", .{world.entity_has(5, &.{
        Position,
    })});

    std.debug.print("Entity 2 has Position & Velocity: {}\n", .{world.entity_has(2, &.{
        Velocity,
        Position,
    })});

    std.debug.print("Entity 3 has All: {}\n", .{world.entity_has(2, &.{
        Hitbox,
        Position,
        Velocity,
    })});
    std.debug.print("\n", .{});
}

test "Query" {
    const Position = struct { data: @Vector(2, f32) };
    const Velocity = struct { data: @Vector(2, f32) };
    const Hitbox = struct { data: @Vector(2, u16) };

    const types = &[_]type{
        Position,
        Velocity,
        Hitbox,
    };

    const TestWorld = World(types, 12);

    const A = struct {
        pub fn system(query: *Query(&[_]type{ Position, Velocity }, TestWorld)) void {
            for (query.entities) |entity| {
                entity.Position.data = entity.Position.data + entity.Velocity.data;
            }
        }
    };

    var gpa = std.heap.GeneralPurposeAllocator(.{}){};
    const allocator = gpa.allocator();
    var world = try TestWorld.init(allocator);

    for (0..4) |_| {
        _ = try world.add_entity(.{
            Position{ .data = @Vector(2, f32){ 0, 4 } },
            Velocity{ .data = @Vector(2, f32){ 1, 0 } },
        });
    }

    const PositionVelocityQuery = Query(&[_]type{ Position, Velocity }, TestWorld);

    for (5..10) |_| {
        for (0..4_000) |_| {
            var query_cached = try PositionVelocityQuery.execute(&world);
            world.updated = false;

            A.system(&query_cached);
        }

        _ = try world.add_entity(.{
            Position{ .data = @Vector(2, f32){ 0, 4 } },
            Velocity{ .data = @Vector(2, f32){ 1, 0 } },
        });
    }

    std.debug.print("{d}\n", .{world.entities.len});
    std.debug.print("\n", .{});
}

test "Entity deletion" {
    const Position = struct { data: @Vector(2, f32) };
    const Velocity = struct { data: @Vector(2, f32) };
    const Hitbox = struct { data: @Vector(2, u16) };

    const types = &[_]type{
        Position,
        Velocity,
        Hitbox,
    };

    const TestWorld = World(types, 12);

    var gpa = std.heap.GeneralPurposeAllocator(.{}){};
    const allocator = gpa.allocator();
    var world = try TestWorld.init(allocator);

    _ = try world.add_entity(.{
        Position{ .data = @Vector(2, f32){ 1, 1 } },
    });

    _ = try world.add_entity(.{
        Position{ .data = @Vector(2, f32){ 2, 2 } },
    });

    _ = try world.add_entity(.{
        Position{ .data = @Vector(2, f32){ 3, 3 } },
    });

    std.debug.print("{any}\n", .{world.components.Position.dense});
    std.debug.print("{any}\n", .{world.components.Position.sparse});
    std.debug.print("{any}\n", .{world.components.Position.sparse_index});
    std.debug.print("\n", .{});
    try world.delete_entity(1);
    std.debug.print("{any}\n", .{world.components.Position.dense});
    std.debug.print("{any}\n", .{world.components.Position.sparse});
    std.debug.print("{any}\n", .{world.components.Position.sparse_index});
    std.debug.print("\n", .{});
}
