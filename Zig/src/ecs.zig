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

        const MaxLengthType: type = Entity;
        comptime max_length: Entity = std.math.maxInt(Entity),

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

        pub fn Query(comptime searched: []const type) type {
            return struct {
                entities: []Record(searched, Entity),
            };
        }

        pub fn query(this: *@This(), comptime searched: []const type) Query(searched) {
            var records: [this.max_length]Record(searched, Entity) = undefined;
            var length: usize = 0;
            for (this.entities, 0..) |_, i| {
                if (this.entity_has(@truncate(i), searched)) {
                    var record: Record(searched, Entity) = undefined;
                    record.entity = @truncate(i);

                    inline for (searched) |T| {
                        var record_field = @field(record, meta.typeNameToFieldName(T));
                        var component = @field(this.components, meta.typeNameToFieldName(T));
                        record_field = &component.dense[component.sparse[i].?];
                    }

                    records[length] = record;
                    length += 1;
                }
            }

            return Query(searched){
                .entities = records[0..length],
            };
        }
    };
}

pub fn Record(comptime searched: []const type, Entity: type) type {
    var fields_tuple: [searched.len + 1]NameToType = undefined;
    fields_tuple[0] = .{
        "entity",
        Entity,
    };
    for (searched, 0..) |T, index| {
        fields_tuple[index + 1] = .{
            meta.typeNameToFieldName(T),
            *T,
        };
    }
    return meta.BuildStruct(fields_tuple);
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

    const query = world.query(&[_]type{Position});
    std.debug.print("{any}\n", .{query});

    std.debug.print("{any}\n", .{query.entities});

    std.debug.print("{}\n", .{world.components.Position});
}
