const std = @import("std");
const meta = @import("meta.zig");
const assert = std.debug.assert;

const NameToType = std.meta.Tuple(&.{
    [:0]const u8,
    type,
});

pub fn World(comptime types: []const type, Entity: type) type {
    var fields: [types.len]NameToType = undefined;
    for (types, 0..) |T, index| {
        fields[index] = .{
            meta.typeNameToFieldName(T),
            Component(T, Entity),
        };
    }

    const ComponentStore = meta.BuildStruct(fields);
    const ComponentMask = std.meta.Int(.unsigned, types.len);

    return struct {
        const WorldError = error{
            EntityAlreadyExists,
        };

        allocator: std.mem.Allocator,

        entities: [std.math.maxInt(Entity)]ComponentMask,
        components: ComponentStore,

        pub fn init(allocator: std.mem.Allocator) @This() {
            const component: ComponentStore = undefined;

            inline for (std.meta.fields(ComponentStore)) |field| {
                var value = @field(component, field.name);
                value = field.type.init();
            }

            return @This(){
                .allocator = allocator,
                .entities = [_]ComponentMask{0} ** std.math.maxInt(Entity),
                .components = component,
            };
        }

        pub fn add_entity(this: *@This(), entity: Entity, components: anytype) !void {
            if (this.entities[entity] != 0) return WorldError.EntityAlreadyExists;
            inline for (components) |component| {
                // Add entity to the component set
                var field = &@field(this.components, meta.typeNameToFieldName(@TypeOf(component)));
                try field.add_entity(entity, component, this.allocator);

                // Add to bit mask of the entity
                const shift_amount = meta.indexOfTypeInArray(@TypeOf(component), types);
                this.entities[entity] = this.entities[entity] | (@as(ComponentMask, 1) <<| shift_amount);
            }
        }

        pub fn entity_has(this: *@This(), entity: Entity, components: []const type) bool {
            const one: ComponentMask = 1;
            var components_to_check: ComponentMask = 0;
            inline for (components) |component| {
                _ = @field(this.components, meta.typeNameToFieldName(component));
                const shift_amount = meta.indexOfTypeInArray(component, types);
                components_to_check |= one <<| shift_amount;
            }
            return (this.entities[entity] & components_to_check) == components_to_check;
        }

        const QueryType = struct {
            entities: []Entity,
            component_data: []*anyopaque,
        };

        pub fn query(this: *@This(), comptime searched: []const type) !QueryType {
            var aggregated_entities: [std.math.maxInt(Entity)]Entity = undefined;
            var length: usize = 0;
            for (this.entities, 0..) |_, index| {
                if (this.entity_has(@truncate(index), searched)) {
                    aggregated_entities[length] = @truncate(index);
                    length += 1;
                }
            }

            var components_stored: [types.len]*anyopaque = undefined;
            var index: usize = 0;
            inline for (searched) |component| {
                components_stored[index] = @ptrCast(&(@field(this.components, meta.typeNameToFieldName(component)).dense));
                index += 1;
            }

            return QueryType{
                .entities = aggregated_entities[0..length],
                .component_data = components_stored[0..searched.len],
            };
        }
    };
}

pub fn Component(comptime T: type, Entity: type) type {
    return struct {
        pub const Type = T;
        sparse: [std.math.maxInt(Entity)]?Entity,
        dense: []T,

        pub fn init() @This() {
            return @This(){
                .sparse = [_]?Entity{null} ** std.math.maxInt(Entity),
                .dense = &[_]T{},
            };
        }

        pub fn add_entity(this: *@This(), entity: Entity, value: T, allocator: std.mem.Allocator) !void {
            const old_length = this.dense.len;
            this.dense = try allocator.realloc(this.dense, old_length + 1);
            this.dense[old_length] = value;

            this.sparse[entity] = @truncate(old_length);
        }

        pub fn add_entity_batch(this: *@This(), entities: []Entity, values: []T, allocator: std.mem.Allocator) !void {
            const old_length = this.dense.len;
            this.dense = try allocator.realloc(this.dense, old_length + values.len);
            @memcpy(this.dense[old_length..], values);

            for (entities, 0..) |entity, index| {
                this.sparse[entity] = old_length + index;
            }
        }
    };
}

// Tests

test "World" {
    std.debug.print("World creation\n", .{});
    const Position = struct { data: @Vector(2, f32) };
    const Velocity = struct { data: @Vector(2, f32) };
    const Hitbox = struct { data: @Vector(2, u16) };

    const types = &[_]type{ Hitbox, Position, Velocity };
    const ComponentMask = std.meta.Int(.unsigned, types.len);

    const MaxEntities = u4;
    const Struct = World(types, MaxEntities);

    var gpa = std.heap.GeneralPurposeAllocator(.{}){};
    const allocator = gpa.allocator();
    const world = Struct.init(allocator);

    assert(std.mem.eql(ComponentMask, &world.entities, &[_]ComponentMask{0} ** std.math.maxInt(MaxEntities)));
    std.debug.print("\n", .{});
}

test "Entity has component check" {
    const Position = struct { data: @Vector(2, f32) };
    const Velocity = struct { data: @Vector(2, f32) };
    const Hitbox = struct { data: @Vector(2, u16) };

    const TestWorld = World(&[_]type{
        Position,
        Velocity,
        Hitbox,
    }, u4);

    std.debug.print("Entity has components check:\n", .{});

    var gpa = std.heap.GeneralPurposeAllocator(.{}){};
    const allocator = gpa.allocator();
    var world = TestWorld.init(allocator);

    for (0..4) |i| {
        try world.add_entity(@truncate(i), .{
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

    const TestWorld = World(&[_]type{
        Position,
        Velocity,
        Hitbox,
    }, u4);

    var gpa = std.heap.GeneralPurposeAllocator(.{}){};
    const allocator = gpa.allocator();
    var world = TestWorld.init(allocator);

    for (0..4) |i| {
        try world.add_entity(@truncate(i), .{
            Position{ .data = @Vector(2, f32){ 0, 4 } },
            Velocity{ .data = @Vector(2, f32){ 1, 0 } },
        });
    }

    const query = try world.query(&[_]type{Position});
    std.debug.print("{}\n", .{query});

    std.debug.print("\n", .{});
}
