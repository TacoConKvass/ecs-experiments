const std = @import("std");
const ecs = @import("ecs.zig");

const String = []u8;

const World = ecs.World(.{
    .{ "position", @Vector(2, f32) },
    .{ "velocity", @Vector(2, f32) },
    .{ "scale", f32 },
    .{ "ai_store", f32 },
    .{ "texture", String },
});

pub fn main() !void {
    var gpa = std.heap.GeneralPurposeAllocator(.{}){};
    var alloc = gpa.allocator();

    var world = World.init(&alloc);

    try world.components.position.add_entity(1, @Vector(2, f32){ 0, 0 }, world.allocator);
    try world.components.position.add_entity(2, @Vector(2, f32){ 1, 0 }, world.allocator);
    try world.components.velocity.add_entity(3, @Vector(2, f32){ 0, 1 }, world.allocator);

    std.debug.print("{any}\n", .{world.components.position});
    std.debug.print("{any}\n", .{world.components.velocity});
    std.debug.print("{any}\n", .{world.components.scale});
    std.debug.print("{any}\n", .{world.components.ai_store});
    std.debug.print("{any}\n", .{world.components.texture});
}
