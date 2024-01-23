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

fn (mut w World) get_component[T](name string) !ComponentData[T]{
	mut temp := []ComponentData[T]
	mut list := &temp

	match T.name {
		"str" { list = &w.components.c_str }
		"int" { list = &w.components.c_int }
		"f64" { list = &w.components.c_f64 }
		"vectors.Vec2" { list = &w.components.c_vec }
		else { return error("No component can have the type of ${T.name}") }
	}

	for component in list {
		if component.name == name {
			return component
		}
	}

	return error("No component found of name ${name}")
}

fn main() {
	entity_count := 100
	println("All entities: ${entity_count}; Moving entities: ${entity_count/2}")

	mut wld := &World{}

	/* ==== Add all entities to world ==== */
	wld.entities << []int{len: entity_count, init: index+1}	

	/* ==== Attach components to entities ==== */
	mut p_data := ComponentData[Vec2]{"position", 
		[ Vec2{3, 1} ].repeat(wld.entities.len),
		wld.entities
	}
	wld.components.c_vec << p_data

	mut v_data := ComponentData[Vec2]{"velocity", 
		[ Vec2{1, 0}, Vec2{0, 1} ].repeat(wld.entities.len/4),
		wld.entities.filter(it % 2 == 0)
	}
	wld.components.c_vec << v_data

	/* ==== Add systems ==== */
	wld.systems = {
		...wld.systems
		"move": fn (mut wld &World) {
			mut position_c := wld.components.c_vec[ComponentID.position]
			velocity_c := wld.components.c_vec[ComponentID.velocity]

			/* This approach is slower, not sure abt safety tho */
			// wld.get_component[Vec2]("position") or { panic(err) }
			// wld.get_component[Vec2]("velocity") or { panic(err) }

			for index in 0 .. velocity_c.owner.len {
				owner := velocity_c.owner[index]
				position_c.data[owner - 1] = position_c.data[owner - 1].add(velocity_c.data[index])
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
	println("\n")
}

fn move_test(mut wld World, repetition int) {
	mut b := benchmark.start()
	for rep in 0 .. repetition {
		wld.systems["move"](mut &wld)
	}
	b.measure("move_test: ${repetition} repetitions")
}