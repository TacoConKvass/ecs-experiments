const std = @import("std");

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

pub fn typeNameToFieldName(T: type) [:0]const u8 {
    const base: [:0]const u8 = @typeName(T);
    var result = base;
    var dot: usize = 0;
    for (base, 0..) |char, i| {
        if (char == '.') dot = i;
    }
    return result[dot + 1 ..];
}

pub fn indexOfTypeInArray(comptime T: type, array: []const type) usize {
    var index: usize = 0;
    inline for (array, 0..) |Type, i| {
        if (Type == T) {
            index = i;
            break;
        }
    }
    return index;
}
