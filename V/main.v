module main

import benchmark
import vectors { Vec2 }

// Available component data types 
type AllowedDataTypes = int | f64 | string | Vec2 

enum ComponentID {
	position
	velocity
	hitbox
}

struct ComponentData[T] {
	name string
	mut:
		data []T
		owner []int
}

// World struct for ECS
struct World {
	mut:
		entities []int
		components []ComponentData[AllowedDataTypes]
		systems map[string]fn(mut &World)
}

fn main() {
	mut wld := World{}

	/* ==== Add all entities to world ==== */
	wld.entities << []int{len: 20, init: index+1}	

	/* ==== Attach components to entities ==== */
	mut p_data := ComponentData[AllowedDataTypes]{"position", 
		[ Vec2{3, 1}, Vec2{2, 4} ].repeat(wld.entities.len/2),
		wld.entities
	}
	wld.components << p_data

	mut v_data := ComponentData[AllowedDataTypes]{"velocity", 
		[ Vec2{1, 0}, Vec2{0, 0} ].repeat(wld.entities.len/4),
		wld.entities.filter(it % 2 == 0)
	}
	wld.components << v_data

	/* ==== Add systems ==== */
	wld.systems = {
		...wld.systems
		"move": fn (mut wld &World) {
			mut position_data := &wld.components[ComponentID.position]
			velocity_data := &wld.components[ComponentID.velocity]
			
			for index in 0 .. wld.components[ComponentID.velocity].data.len {
				position_data.data[index] = (position_data.data[index] as Vec2).add(velocity_data.data[index] as Vec2)
			}
		}
	}

	/* ==== Tests ==== */
	move_test(mut wld, 1)
	move_test(mut wld, 10)
	move_test(mut wld, 100)
	move_test(mut wld, 1_000)
	move_test(mut wld, 10_000)
	move_test(mut wld, 100_000)
	move_test(mut wld, 1_000_000)
}

fn move_test(mut wld World, repetition int) {
	mut b := benchmark.start()
	for rep in 0 .. repetition {
		wld.systems["move"](mut &wld)
	}
	b.measure("move_test: ${repetition} repetitions")
}