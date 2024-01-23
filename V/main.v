module main

import benchmark
import vectors { Vec2 }

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

struct ComponentList {
	mut:
		c_vec []ComponentData[Vec2]
		c_int []ComponentData[int]
		c_f64 []ComponentData[f64]
		c_str []ComponentData[string]
}

// World struct for ECS
struct World {
	mut:
		entities []int
		components ComponentList
		systems map[string]fn(mut &World)
}

fn main() {
	entity_count := 100
	println("All entities: ${entity_count}; Moving entities: ${entity_count/2}")

	mut wld := World{}

	/* ==== Add all entities to world ==== */
	wld.entities << []int{len: entity_count, init: index+1}	

	/* ==== Attach components to entities ==== */
	mut p_data := ComponentData[Vec2]{"position", 
		[ Vec2{3, 1}, Vec2{2, 4} ].repeat(wld.entities.len/2),
		wld.entities
	}
	wld.components.c_vec << p_data

	mut v_data := ComponentData[Vec2]{"velocity", 
		[ Vec2{1, 0}, Vec2{0, 0} ].repeat(wld.entities.len/4),
		wld.entities.filter(it % 2 == 0)
	}
	wld.components.c_vec << v_data

	/* ==== Add systems ==== */
	wld.systems = {
		...wld.systems
		"move": fn (mut wld &World) {
			mut position_data := &wld.components.c_vec[ComponentID.position]
			velocity_data := &wld.components.c_vec[ComponentID.velocity]
			
			for index in 0 .. velocity_data.data.len {
				position_data.data[index] = position_data.data[index].add(velocity_data.data[index])
			}
		}
	}

	/* ==== Tests ==== */
	println("==== ComponentList ====")
	move_test(mut wld, 1)
	move_test(mut wld, 10)
	move_test(mut wld, 100)
	move_test(mut wld, 1_000)
	move_test(mut wld, 10_000)
	move_test(mut wld, 100_000)
	move_test(mut wld, 1_000_000)
	println("\n"
}

fn move_test(mut wld World, repetition int) {
	mut b := benchmark.start()
	for rep in 0 .. repetition {
		wld.systems["move"](mut &wld)
	}
	b.measure("move_test: ${repetition} repetitions")
}