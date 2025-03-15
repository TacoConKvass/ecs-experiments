const std = @import("std");
const ecs = @import("ecs.zig");


pub fn main() !void {
    var gpa = std.heap.GeneralPurposeAllocator(.{}){};
    var world = ecs.World{ .id = 3 };
    try world.add(Position, gpa.allocator());
    try world.add(Velocity, gpa.allocator());
    
    std.debug.print("{any}", .{ecs.ComponentStorage(Position).components});
}

// Components
const Vec2 = @Vector(2, f32);

const Position = struct { 
    value: Vec2,
};

const Velocity = struct {
    value: Vec2,
};
