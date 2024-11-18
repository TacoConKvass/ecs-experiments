const std = @import("std");

pub fn build(b: *std.Build) void {
    // Target architecture
    const target = b.standardTargetOptions(.{});

    // Optimization level
    const optimize = b.standardOptimizeOption(.{});

    // Declare RayLib dependency
    const raylib_dep = b.dependency("raylib-zig", .{
        .target = target,
        .optimize = optimize,
    });

    const raylib = raylib_dep.module("raylib");
    const raylib_artifact = raylib_dep.artifact("raylib");

    // Declare .exe
    const exe = b.addExecutable(.{
        .name = "ECS-Experiments",
        .root_source_file = b.path("src/main.zig"),
        .target = target,
        .optimize = optimize,
    });

    exe.linkLibrary(raylib_artifact);
    exe.root_module.addImport("raylib", raylib);

    // Build .exe
    b.installArtifact(exe);
}
