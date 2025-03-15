const std = @import("std");

pub const World = struct {
    finalized: bool = true,
    id: u8,

    pub fn add(this: *@This(), T: type, allocator: std.mem.Allocator) !void {
        _ = try ComponentStorage(T).addTo(this.id, allocator);
    }

    pub fn get(this: *@This(), T: type) ?Component(T) {
        return ComponentStorage(T).get(this.id);
    }

    pub fn finalize(this: *@This()) void {
        this.finalized = true;
    }
};

pub fn ComponentStorage(T: type) type {
    return struct {
        pub var components: []?Component(T) = &[_]?Component(T){ };

        pub fn get(world_id: u8) ?Component(T) {
            if (world_id > components.len) {
                return null;
            }

            return components[world_id];
        }

        pub fn addTo(world_id: u8, allocator: std.mem.Allocator) !void {
            if (world_id >= components.len) {
                const old_length = components.len;
                components = try allocator.realloc(components, @max(world_id + 1, components.len * 2));
                @memset(components[old_length..], null);
            }

            components[world_id] = try Component(T).init(64, allocator);
        }
    };
}

pub fn Component(T : type) type {
    return struct {
        const Type = T;
        sparse: []i32,
        dense: []i32,
        data: []?T,

        pub fn init(capacity: u32, allocator: std.mem.Allocator) !@This() {
            const sparse = try allocator.alloc(i32, capacity);
            @memset(sparse, -1);
            
            return @This() {
                .sparse = sparse,
                .dense = &[_]i32{},
                .data = &[_]?T{},
            };
        }

        // pub fn add(id: u32, value: T) !void {}
    };
}
