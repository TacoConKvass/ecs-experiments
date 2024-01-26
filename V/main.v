module main

import gg
import gx
import vectors { Vec2 }
import ecs
import rand
import math
import benchmark

const color := [gx.red, gx.blue, gx.yellow, gx.green]
const window_width = 1280
const window_height = 720
const entity_count = 5000
const entity_size = 3

struct App {
	mut:
		gg &gg.Context
		world ecs.World
}

fn main() {
	mut app := &App {
		unsafe { nil },
		ecs.World{}
	}	

	app.gg = gg.new_context(
			width: window_width
			height: window_height
			window_title: "ECS"
			swap_interval: 1
			frame_fn: frame
			user_data: app
	)

	app.world.register_component[Vec2]("position")!
	app.world.register_component[Vec2]("velocity")!

	mut pos_c := &app.world.components.c_vec[0]
	mut vel_c := &app.world.components.c_vec[1]

	pos_c.add_entity_batch( []Vec2{len:entity_count, init: Vec2{window_width/2, window_height/2}}, []int{len: entity_count, init: index+1})
	vel_c.add_entity_batch( []Vec2{len:entity_count, init: Vec2{4, 0} }, []int{len: entity_count, init: index+1})

	app.world.systems = {
		...app.world.systems
		"handle_movement": fn (mut wld &ecs.World) {
			mut position_c := wld.get_component[Vec2]("position") or { panic("Position not found") }
			mut velocity_c := wld.get_component[Vec2]("velocity") or { panic("Velocity not found") }

			for index in 0 .. velocity_c.owner.len {
				owner := velocity_c.owner[index] - 1
				position_c.data[owner] = position_c.data[owner].add(velocity_c.data[index])
				
				if position_c.data[owner].x > window_width || position_c.data[owner].x < 0 || position_c.data[owner].y > window_height || position_c.data[owner].y < 0 {
					velocity_c.data[index] = velocity_c.data[index].rotated_by(math.radians(rand.f64_in_range(-225, -145) or { 90 }))

				}

				/*
				if (position_c.data[owner].x > window_width && velocity_c.data[index].x > 0) || (position_c.data[owner].x < 0 && velocity_c.data[index].x < 0) {
					velocity_c.data[index] = Vec2{-velocity_c.data[index].x, velocity_c.data[index].y}
				}

				if (position_c.data[owner].y > window_height && velocity_c.data[index].y > 0) || (position_c.data[owner].y < 0 && velocity_c.data[index].y > 0) {
					velocity_c.data[index] = Vec2{-velocity_c.data[index].x, velocity_c.data[index].y}
				}
				*/
			}
		}
	}

	app.gg.run()
}

fn frame(mut app App) {
	mut b := benchmark.start()
	app.world.systems["handle_movement"](mut app.world)

	mut pos_c := app.world.get_component[Vec2]("position") or { panic("Position not found") }

	app.gg.begin()
	app.gg.end()

	for rep in 0 .. pos_c.data.len / 100 {
		app.gg.begin()
		for entity in pos_c.data[rep*100 .. rep*100 + 100] {
			app.gg.draw_circle_filled(f32(entity.x), f32(entity.y), entity_size, color[rep%4])		
		}
		app.gg.end(how: .passthru)
	}

	app.gg.begin()
	app.gg.show_fps()
	app.gg.end(how: .passthru)
	b.measure("frame")
}