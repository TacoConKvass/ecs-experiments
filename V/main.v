module main

import gg
import gx
import benchmark
import vectors { Vec2 }
import ecs { World, Component, System, IComponent }
import example_ecs_data { Position, Velocity }

struct App {
	mut:
		g 	&gg.Context
		wld World
}

const window_width 		= 1280
const window_height 	= 720
const entity_amount 	= 2500
const entity_size 		= 2
const draw_batch_size 	= 100

fn main() {
	mut app := &App {
		unsafe {nil},
		World{}
	}

	app.g = gg.new_context(
			width: window_width
			height: window_height
			window_title: "ECS"
			frame_fn: frame
			user_data: app
	)

	mut wld := &app.wld

	wld.add_component(Position{})
	wld.add_component(Velocity{})

	wld.add_entities([]int{len: entity_amount, init: index + 1})
	wld.add_entity_batch_to_component<Position, Vec2>([]int{len: entity_amount, init: index + 1}, []Vec2{len: entity_amount, init: Vec2{window_width/2, window_height/2}})
	wld.add_entity_batch_to_component<Velocity, Vec2>([]int{len: entity_amount, init: index + 1}, []Vec2{len: entity_amount, init: Vec2{1, 1}.rotated_by(index)})

	wld.add_system(System{
		name: "handle_movement",
		action: fn (mut components []IComponent) {
			mut pos := components[0]
			mut vel := components[1]
				
			if mut pos is Position && mut vel is Velocity {
				for entity in vel.owners {
					id := pos.owners.index(entity)
					if id != -1 {
						pos.data[id] = pos.data[id].add(vel.data[vel.owners.index(entity)])
					}
				}

				return
			}

			println("The method \"handle_movement\" requires 2 arguments: Position and Velocity")
			return
		}
	})

	wld.add_system(System{
		name: "handle_collision",
		action: fn (mut components []IComponent) {
			mut pos := components[0]
			mut vel := components[1]
				
			if mut pos is Position && mut vel is Velocity {
				for entity_id in 0 .. vel.owners.len - 1 {
					id := pos.owners.index(vel.owners[entity_id])
					if id != -1 {
						if (pos.data[id].x > window_width && vel.data[entity_id].x > 0) || (pos.data[id].x < 0 && vel.data[entity_id].x < 0)  {
							vel.data[entity_id].x *= -1
						}

						if (pos.data[id].y > window_height && vel.data[entity_id].y > 0) || (pos.data[id].y < 0 && vel.data[entity_id].y < 0){
							vel.data[entity_id].y *= -1
						}
					}
				}
				return
			}

			println("The method \"handle_collision\" requires 2 arguments: Position and Velocity")
			return
		}
	})

	app.g.run()
}

fn frame(mut app &App) {
	mut wld := &app.wld

	mut pos := wld.get_component<Position>() or { &Position{} }
	mut vel := wld.get_component<Velocity>() or { &Velocity{} }

	mut move_args := &[IComponent(pos), IComponent(vel)]
	wld.run_system("handle_movement", mut move_args)
	wld.run_system("handle_collision", mut move_args)
	
	// Clear the screen
	app.g.begin()
	app.g.end()

	for rep in 0 .. pos.data.len / draw_batch_size {
		app.g.begin()
		for entity in pos.data[rep*draw_batch_size .. (rep + 1)*draw_batch_size] {
			app.g.draw_circle_filled(f32(entity.x), f32(entity.y), entity_size, gx.red)
		}
		app.g.end(how: .passthru)
	}

	app.g.begin()
	app.g.show_fps()
	app.g.end(how: .passthru)
}