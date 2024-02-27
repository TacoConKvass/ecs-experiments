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
const entity_count = 2000
const draw_per_call = 100

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
			frame_fn: frame
			user_data: app
	)

	app.world.register_component[Vec2]("position")!
	app.world.register_component[Vec2]("velocity")!

	mut pos_c := &app.world.components.c_vec[0]
	mut vel_c := &app.world.components.c_vec[1]

	pos_c.add_entity_batch( []Vec2{len:entity_count, init: Vec2{window_width/2, window_height/2}}, []int{len: entity_count, init: index+1})
	vel_c.add_entity_batch( []Vec2{len:entity_count, init: Vec2{3, 0}.rotated_by(index) }, []int{len: entity_count, init: index+1})

	handle_mov := fn (mut wld &ecs.World) {
			mut position_c := wld.get_component[Vec2]("position") or { panic("Position not found") }
			mut velocity_c := wld.get_component[Vec2]("velocity") or { panic("Velocity not found") }

			for index in 0 .. velocity_c.owner.len {
				owner := velocity_c.owner[index] - 1
				position_c.data[owner] = position_c.data[owner].add(velocity_c.data[index])
				
				if (position_c.data[owner].x > window_width && velocity_c.data[index].x > 0) || (position_c.data[owner].x < 0 && velocity_c.data[index].x < 0) {
					velocity_c.data[index] = Vec2{-velocity_c.data[index].x, velocity_c.data[index].y}.rotated_by(math.radians(rand.f64_in_range(-10, 10) or { 0 }))
				}

				if (position_c.data[owner].y > window_height && velocity_c.data[index].y > 0) || (position_c.data[owner].y < 0 && velocity_c.data[index].y < 0) {
					velocity_c.data[index] = Vec2{velocity_c.data[index].x, -velocity_c.data[index].y}.rotated_by(math.radians(rand.f64_in_range(-10, 10) or { 0 }))
			}
		}
	}
	
	app.world.register_system("handle_movement", handle_mov)

	handle_collision := fn (mut wld &ecs.World) {
		mut position_c := wld.get_component[Vec2]("position") or { panic("Position not found") }
		mut velocity_c := wld.get_component[Vec2]("velocity") or { panic("Velocity not found") }

		mut position_data_x := position_c.data.clone()
		position_data_x.sort_with_compare(fn (a &Vec2, b &Vec2) int {
			if a.x > b.x {
				return 1
			}
			if a.x < b.x {
				return -1
			}

			return 0
		})

		for rep in 0 .. position_data_x.len / 10 {
			mut position_data_y := position_data_x[rep * 10 .. (rep+1) * 10].clone()
			position_data_y.sort_with_compare(fn (a &Vec2, b &Vec2) int {
				if a.y > b.y {
					return 1
				}
				if a.y < b.y {
					return -1
				}

				return 0
			})

			for entity in 0 .. position_data_y.len - 1 {
				if position_data_y[entity].distance_to(position_data_y[entity + 1]) < 8 {
					owner_id := position_c.owner[position_c.data.index(position_data_y[entity])]
					owner2_id := position_c.owner[position_c.data.index(position_data_y[entity + 1])]
					if owner_id in velocity_c.owner {
						owner_index := velocity_c.owner.index(owner_id)
						owner2_index := velocity_c.owner.index(owner2_id)
						mut alias := &velocity_c.data[owner_index]
						mut alias2 := &velocity_c.data[owner2_index]
						velocity_c.data[owner_index] = position_data_y[entity].substract(position_data_y[entity + 1]).scale(alias.get_dot(alias2) / math.pow(position_data_y[entity].distance_to(position_data_y[entity + 1]), 2)).normalize().scale(4)
					}
				}
			}
		}
	}

	app.world.register_system("handle_collision", handle_collision)
	
	app.gg.run()
}

fn frame(mut app App) {
	mut bg := benchmark.start()
	mut b := benchmark.start()
	app.world.systems["handle_movement"](mut app.world)
	if app.gg.frame > 800 {
		app.world.systems["handle_collision"](mut app.world)
	}
	b.measure("Handle movement")
	
	mut pos_c := app.world.get_component[Vec2]("position") or { panic("Position not found") }

	app.gg.begin()
	app.gg.end()

	mut b2 := benchmark.start()
	for rep in 0 .. pos_c.data.len / draw_per_call {
		app.gg.begin()
			
		mut temp := Vec2{}
		for entity in pos_c.data[rep*draw_per_call .. (rep + 1)*draw_per_call] {
			if entity.distance_to(temp) > 2 {
				app.gg.draw_circle_filled(f32(entity.x), f32(entity.y), 2, color[rep%4])
			}
			temp = entity
		}
		app.gg.end(how: .passthru)
	} 
	b2.measure("Draw")

	app.gg.begin()
	app.gg.show_fps()
	app.gg.end(how: .passthru)
	bg.measure("Frame")
}
