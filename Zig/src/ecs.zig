const std = @import("std");

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
                var shift_amount: ComponentMask = 0;
                inline for (types) |Type| {
                    if (Type == @TypeOf(component)) {
                        break;
                    }
                    shift_amount += 1;
                }
                this.entities[entity] = this.entities[entity] | (@as(ComponentMask, 1) <<| shift_amount);
            }
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

test "Test component" {
    const Test = Component(f32, u4);
    _ = Test.init();
    std.debug.print("{}\n", .{Test.Type});
}

const Position = struct { data: @Vector(2, f32) };
const Velocity = struct { data: @Vector(2, f32) };
const Hitbox = struct { data: @Vector(2, u16) };

test "World" {
    const Struct = World(&[_]type{
        Hitbox,
        Position,
        Velocity,
    }, u4);

    var gpa = std.heap.GeneralPurposeAllocator(.{}){};
    const allocator = gpa.allocator();
    var world = Struct.init(allocator);
    for (0..4) |i| {
        try world.add_entity(@truncate(i), .{
            Position{ .data = @Vector(2, f32){ 0, 4 } },
            Velocity{ .data = @Vector(2, f32){ 1, 0 } },
        });
    }
    std.debug.print("{any}\n", .{world});
    // std.debug.print("{}\n", .{world.components.Position});
}
