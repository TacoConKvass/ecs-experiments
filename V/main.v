module main

import gg
import gx
import vectors { Vec2 }
import ecs
import rand
import math

const color := [gx.red, gx.blue, gx.yellow, gx.green]

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
			width: 1280
			height: 720
			window_title: "ECS"
			swap_interval: 0
			frame_fn: frame
			user_data: app
	)

	app.world.register_component[Vec2]("position")!
	app.world.register_component[Vec2]("velocity")!

	mut pos_c := &app.world.components.c_vec[0]
	mut vel_c := &app.world.components.c_vec[1]

	pos_c.add_entity_batch([ Vec2{640, 360} ].repeat(12000), []int{len: 12000, init: index+1})
	vel_c.add_entity_batch([ Vec2{2.5, 0} ].repeat(12000) , []int{len: 12000, init: index+1})

	app.world.systems = {
		...app.world.systems
		"handle_movement": fn (mut wld &ecs.World) {
			mut position_c := wld.get_component[Vec2]("position") or { panic("Position not found") }
			mut velocity_c := wld.get_component[Vec2]("velocity") or { panic("Velocity not found") }

			for index in 0 .. velocity_c.owner.len {
				owner := velocity_c.owner[index] - 1
				position_c.data[owner] = position_c.data[owner].add(velocity_c.data[index])
			}
		}
		"handle_velocity": fn (mut wld &ecs.World) {
			mut position_c := wld.get_component[Vec2]("position") or { panic("Position not found") }
			mut velocity_c := wld.get_component[Vec2]("velocity") or { panic("Velocity not found") }

			for index in 0 ..  velocity_c.owner.len {
				owner := velocity_c.owner[index] - 1
				if position_c.data[owner].x > 1280 || position_c.data[owner].x < 0 || position_c.data[owner].y > 720 || position_c.data[owner].y < 0 {
					velocity_c.data[index] = Vec2{-velocity_c.data[index].x, -velocity_c.data[index].y}.rotated_by(math.radians(rand.f64_in_range(0, 360) or { 180 }))
					position_c.data[owner] = Vec2{640, 360}
				}
			}
		}
	}

	app.gg.run()
}

fn frame(mut app App) {
	app.world.systems["handle_movement"](mut app.world)
	app.world.systems["handle_velocity"](mut app.world)

	mut pos_c := app.world.get_component[Vec2]("position") or { panic("Position not found") }

	app.gg.begin()
	app.gg.end()

	for rep in 0 .. pos_c.data.len / 1000 {
		app.gg.begin()
		for entity in pos_c.data[rep*1000 .. rep*1000 + 1000] {
			app.gg.draw_circle_filled(f32(entity.x), f32(entity.y), 5, color[rep%4])		
		}
		app.gg.end(how: .passthru)
	}

	app.gg.begin()
	app.gg.show_fps()
	app.gg.end(how: .passthru)
}