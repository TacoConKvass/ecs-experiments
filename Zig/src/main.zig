const std = @import("std");
const rl = @import("raylib");
const ecs = @import("ecs.zig");

const Vec2 = @Vector(2, f32);
const Color = rl.Color;

const Position = struct { data: Vec2 };
const Velocity = struct { data: Vec2 };
const DisplayText = struct {
    text: [:0]const u8,
    color: rl.Color,
};

const colors = &[_]Color{ Color.red, Color.light_gray, Color.purple };
const components = &[_]type{ Position, DisplayText, Velocity };

const World = ecs.World(components, u12);

const RenderQuery = ecs.Query(&[_]type{ Position, DisplayText }, World);
const MovementQuery = ecs.Query(&[_]type{ Position, Velocity }, World);

const screenWidth = 1200;
const screenHeight = 720;

pub fn main() anyerror!void {
    // Screen parameters
    rl.initWindow(screenWidth, screenHeight, "");
    defer rl.closeWindow(); // Close window and OpenGL context at the end

    rl.setTargetFPS(60); // Set our game to run at 60 frames-per-second
    rl.setExitKey(.key_null); // Turn off "ESC to exit"

    var gpa = std.heap.GeneralPurposeAllocator(.{}){};
    const allocator = gpa.allocator();
    var world = World.init(allocator);

    var prng = std.rand.DefaultPrng.init(blk: {
        var seed: u64 = undefined;
        try std.posix.getrandom(std.mem.asBytes(&seed));
        break :blk seed;
    });
    const rand = prng.random();

    for (0..2_500) |i| {
        try world.add_entity(@truncate(i), .{
            Position{ .data = .{ screenWidth / 2, screenHeight / 2 } },
            Velocity{ .data = .{
                if (rand.boolean()) rand.floatNorm(f32) else -rand.floatNorm(f32),
                if (rand.boolean()) rand.floatNorm(f32) else -rand.floatNorm(f32),
            } },
            DisplayText{ .text = "!!", .color = colors[i % 3] },
        });
    }

    var movement_query: MovementQuery = undefined;
    var started = false;

    while (!rl.windowShouldClose()) {
        if (rl.isKeyDown(rl.KeyboardKey.key_enter)) {
            started = !started;
        }

        // Update
        if (started) {
            movement_query = try MovementQuery.execute(&world);
            handle_movement(&movement_query, rand);
        }

        // Render
        rl.beginDrawing();

        rl.clearBackground(rl.Color.black);
        render_text(try RenderQuery.execute(&world));
        rl.drawFPS(0, 0);

        rl.endDrawing();
    }
}

pub fn render_text(query: RenderQuery) void {
    for (query.entities) |entity| {
        rl.drawText(entity.DisplayText.text, @intFromFloat(entity.Position.data[0]), @intFromFloat(entity.Position.data[1]), 15, entity.DisplayText.color);
    }
}

pub fn handle_movement(query: *MovementQuery, rand: std.Random) void {
    for (query.entities) |entity| {
        entity.Position.data += entity.Velocity.data;
        if (entity.Position.data[0] < -3 or entity.Position.data[0] > screenWidth + 3 or entity.Position.data[1] < -3 or entity.Position.data[1] > screenHeight + 3) {
            entity.Position.data = .{ screenWidth / 2, screenHeight / 2 };
            entity.Velocity.data = .{
                if (rand.boolean()) rand.floatNorm(f32) else -rand.floatNorm(f32),
                if (rand.boolean()) rand.floatNorm(f32) else -rand.floatNorm(f32),
            };
        }
    }
}
