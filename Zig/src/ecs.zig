const std = @import("std");
const assert = std.debug.assert;

const NameToType = std.meta.Tuple(&.{
    [:0]const u8,
    type,
});

const WorldError = error{
    EntityAlreadyExists,
};

pub fn World(comptime types: []const type, Entity: type) type {
    var fields: [types.len]NameToType = undefined;
    for (types, 0..) |T, index| {
        fields[index] = .{
            typeNameToFieldName(T),
            Component(T, Entity),
        };
    }

    const ComponentStore = BuildStruct(fields);
    const ComponentMask = std.meta.Int(.unsigned, types.len);

    return struct {
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
                var field = &@field(this.components, typeNameToFieldName(@TypeOf(component)));
                try field.add_entity(entity, component, this.allocator);

                // Add to bit mask of the entity
                const shift_amount = indexOfTypeInArray(@TypeOf(component), types);
                this.entities[entity] = this.entities[entity] | (@as(ComponentMask, 1) <<| shift_amount);
            }
        }
        pub fn entity_has(this: *@This(), entity: Entity, components: []const type) bool {
            const one: ComponentMask = 1;
            var components_to_check: ComponentMask = 0;
            inline for (components) |component| {
                _ = @field(this.components, typeNameToFieldName(component));
                const shift_amount = indexOfTypeInArray(component, types);
                components_to_check |= one <<| shift_amount;
            }
            return (this.entities[entity] & components_to_check) == components_to_check;
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
    };
}

pub fn BuildStruct(comptime declared_fields: anytype) type {
    var fields: [declared_fields.len]std.builtin.Type.StructField = undefined;
    for (declared_fields, 0..) |field, index| {
        fields[index] = .{
            .name = field[0][0..],
            .type = field[1],
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

fn typeNameToFieldName(T: type) [:0]const u8 {
    const base: [:0]const u8 = @typeName(T);
    var result = base;
    var dot: usize = 0;
    for (base, 0..) |char, i| {
        if (char == '.') dot = i;
    }
    return result[dot + 1 ..];
}

fn indexOfTypeInArray(comptime T: type, array: []const type) usize {
    var index: usize = 0;
    inline for (array, 0..) |Type, i| {
        if (Type == T) {
            index = i;
            break;
        }
    }
    return index;
}

const Position = struct { data: @Vector(2, f32) };
const Velocity = struct { data: @Vector(2, f32) };
const Hitbox = struct { data: @Vector(2, u16) };

test "World" {
    const types = &[_]type{ Hitbox, Position, Velocity };
    const ComponentMask = std.meta.Int(.unsigned, types.len);
    const MaxEntities = u4;
    const Struct = World(types, MaxEntities);

    var gpa = std.heap.GeneralPurposeAllocator(.{}){};
    const allocator = gpa.allocator();
    const world = Struct.init(allocator);

    assert(std.mem.eql(ComponentMask, &world.entities, &[_]ComponentMask{0} ** std.math.maxInt(MaxEntities)));
    std.debug.print("World creation: Passed\n", .{});
}

const TestWorld = World(&[_]type{
    Position,
    Velocity,
    Hitbox,
}, u4);

test "Entity has component check" {
    var gpa = std.heap.GeneralPurposeAllocator(.{}){};
    const allocator = gpa.allocator();
    var world = TestWorld.init(allocator);

    for (0..4) |i| {
        try world.add_entity(@truncate(i), .{
            Position{ .data = @Vector(2, f32){ 0, 4 } },
            Velocity{ .data = @Vector(2, f32){ 1, 0 } },
        });
    }

    std.debug.print("Entity 2 has Position & Velocity: {}\n", .{world.entity_has(2, &.{
        Velocity,
        Position,
    })});
    std.debug.print("Entity 3 has Position & Hitbox: {}\n", .{world.entity_has(2, &.{
        Hitbox,
        Position,
    })});
}
