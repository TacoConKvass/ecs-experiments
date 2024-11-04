const std = @import("std");
const rl = @import("raylib");
const ecs = @import("ecs.zig");

const Vec2 = @Vector(2, f32);

fn scale_vector(this: Vec2, value: f32) Vec2 {
    return Vec2{ this[0] * value, this[1] * value };
}

inline fn vector_len(this: Vec2) f32 {
    return @sqrt(this[0] * this[0] + this[1] * this[1]);
}

fn normalize_vector(this: Vec2) Vec2 {
    const len = vector_len(this);
    return Vec2{
        this[0] / len,
        this[1] / len,
    };
}

const Color = rl.Color;

const Position = struct { data: Vec2 };
const Velocity = struct { data: Vec2 };
const Renderable = struct { color: Color };

const colors = &[_]Color{ Color.green, Color.blue, Color.purple };
const components = &[_]type{ Position, Velocity, Renderable };

const max_entities = 1_000_000;

const World = ecs.World(components, max_entities);

const RenderQuery = ecs.Query(&[_]type{ Position, Renderable }, World);
const MovementQuery = ecs.Query(&[_]type{ Position, Velocity }, World);

const screenWidth = 1280;
const screenHeight = 720;

const seconds_per_frame: f32 = 0.016667;

var delta_time: f32 = 0;

pub fn main() anyerror!void {
    // Screen parameters
    rl.initWindow(screenWidth, screenHeight, "");
    defer rl.closeWindow(); // Close window and OpenGL context at the end

    rl.setTargetFPS(60); // Set our game to run at 60 frames-per-second
    rl.setExitKey(.key_null); // Turn off "ESC to exit"

    var gpa = std.heap.GeneralPurposeAllocator(.{}){};
    const allocator = gpa.allocator();
    var world = try World.init(allocator);

    var prng = std.rand.DefaultPrng.init(blk: {
        var seed: u64 = undefined;
        try std.posix.getrandom(std.mem.asBytes(&seed));
        break :blk seed;
    });
    const rand = prng.random();

    for (0..max_entities) |i| {
        _ = try world.add_entity(.{
            Position{ .data = .{ screenWidth / 2, screenHeight / 2 } },
            Velocity{
                .data = //scale_vector(normalize_vector(
                .{
                    if (rand.boolean()) rand.floatNorm(f32) else -rand.floatNorm(f32),
                    if (rand.boolean()) rand.floatNorm(f32) else -rand.floatNorm(f32),
                    //}), 3)
                },
            },
            Renderable{ .color = colors[i % 3] },
        });
    }

    var movement_query: MovementQuery = undefined;

    var updated_x_frames_ago: u3 = 0;

    while (!rl.windowShouldClose()) {
        // Recalc delta time
        delta_time = @floatCast(rl.getFrameTime() / seconds_per_frame);

        // Update
        movement_query = try MovementQuery.execute(&world);
        handle_movement(&movement_query);

        // Render
        rl.beginDrawing();
        rl.clearBackground(rl.Color.black);

        render_text(try RenderQuery.execute(&world));
        rl.drawFPS(0, 0);

        rl.endDrawing();

        if (world.updated and updated_x_frames_ago == 0) updated_x_frames_ago = 2;
        updated_x_frames_ago -|= 1;
        world.updated = updated_x_frames_ago != 0;
    }
}

pub fn render_text(query: RenderQuery) void {
    for (query.entities) |entity| rl.drawLine(@intFromFloat(entity.Position.data[0] - 1), @intFromFloat(entity.Position.data[1] - 1), @intFromFloat(entity.Position.data[0]), @intFromFloat(entity.Position.data[1] - 1), entity.Renderable.color);
}

pub fn handle_movement(query: *MovementQuery) void {
    for (query.entities) |entity| {
        entity.Position.data += scale_vector(entity.Velocity.data, delta_time);
        const position = entity.Position.data;
        if (position[0] < 0 and entity.Velocity.data[0] < 0) entity.Velocity.data[0] = -entity.Velocity.data[0];
        if (position[0] > screenWidth and entity.Velocity.data[0] > 0) entity.Velocity.data[0] = -entity.Velocity.data[0];
        if (position[1] < 0 and entity.Velocity.data[1] < 0) entity.Velocity.data[1] = -entity.Velocity.data[1];
        if (position[1] > screenHeight and entity.Velocity.data[1] > 0) entity.Velocity.data[1] = -entity.Velocity.data[1];
    }
}
