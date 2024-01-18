module main

import benchmark

const element_num = 20

// Vector2 definition
struct Vec2 {
	x  f64
	y  f64
}

fn (v Vec2) add(v2 Vec2) Vec2 {
	result := Vec2 {v.x + v2.x, v.y + v2.y}
	return result
}

// Available component types 
type Component = int | i64 | f64 | string | Vec2

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

// MapWorld struct for the ECS
struct MapWorld {
	mut:
		entities []int
		components map[string]map[int]Component
		systems map[string]fn(mut &MapWorld)
}

struct World {
	mut:
		entities []int
		components []ComponentData[Component]
		systems map[string]fn(mut &World)
}

pub fn (mut mwld MapWorld) unload_entity(id int) {
	mwld.entities = mwld.entities.filter(it != id)
	for key in mwld.components.keys() {
		mwld.components[key].delete(id)
	}
}

fn main() {
	println("${'*':10r} Map World setup ${'*':10r}")
	mut mwld := MapWorld{}
	mwld.entities << [1, 2]

	mwld.components["position"] = {
		...mwld.components["position"]
		1: Vec2{3, 1}
		2: Vec2{3, 1}
		3: Vec2{3, 1}
		4: Vec2{3, 1}
		5: Vec2{3, 1}
		6: Vec2{3, 1}
		7: Vec2{3, 1}
		8: Vec2{3, 1}
		9: Vec2{3, 1}
		10: Vec2{3, 1}
		11: Vec2{3, 1}
		12: Vec2{3, 1}
		13: Vec2{3, 1}
		14: Vec2{3, 1}
		15: Vec2{3, 1}
		16: Vec2{3, 1}
		17: Vec2{3, 1}
		18: Vec2{3, 1}
		19: Vec2{3, 1}
		20: Vec2{3, 1}
	}

	mwld.components["velocity"] = {
		...mwld.components["velocity"]
		1: Vec2{0, 1}
		2: Vec2{0, 1}
		3: Vec2{0, 1}
		4: Vec2{0, 1}
		5: Vec2{0, 1}
		6: Vec2{0, 1}
		7: Vec2{0, 1}
		8: Vec2{0, 1}
		9: Vec2{0, 1}
		10: Vec2{0, 1}
	}

	mwld.systems = {
		...mwld.systems
		"move": fn (mut mwld &MapWorld) {
			for key, value in mwld.components["velocity"] {
				mwld.components["position"][key] = (mwld.components["position"][key] as Vec2).add(value as Vec2)
			}
		}
	}

	mut b := benchmark.start()
	for rep in 0 .. 10_000 {
		move_test(mut &mwld)
	}
	b.measure("Map World")

	println("\n\n\n${'*':10r} Non-map World setup ${'*':10r}")
	mut wld := World{}
	wld.entities << [1, 2]

	mut p_data := ComponentData[Component]{"position", 
		[ Vec2{3, 1}].repeat(element_num),
		[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20]
	}
	wld.components << p_data

	mut v_data := ComponentData[Component]{"velocity", 
		[ Vec2{1, 0} ].repeat(element_num/2),
		[1, 2, 3, 4, 5, 6, 7, 8, 9, 10] }
	wld.components << v_data

	wld.systems = {
		...wld.systems
		"move2": fn (mut wld &World) {
			for index in 0 .. wld.components[ComponentID.velocity].data.len {
				wld.components[ComponentID.position].data[index] = (wld.components[ComponentID.position].data[index] as Vec2).add(wld.components[ComponentID.velocity].data[index] as Vec2)
			} 
		}
	}

	mut b2 := benchmark.start()
	for rep in 0 .. 10_000 {
		move2_test(mut &wld)
	}
	b2.measure("Non-Map World")

	println("\n\n\n${mwld.components}")
	println("\n\n\n${wld.components}")
}

fn unload_test(mut mwld &MapWorld) {
	mwld.entities << 3
	mwld.components["position"] = {
		...mwld.components["position"]
		3: Vec2{10, 2}
	}

	mwld.components["hitbox"] = {
		...mwld.components["hitbox"]
		3: Vec2{3, 3}
	}

	println("${mwld.entities}\n${mwld.components}\n\n\n")
	mwld.unload_entity(3)
	println("${mwld.entities}\n${mwld.components}\n\n\n")
}

fn move_test(mut mwld &MapWorld) {
	//println("${mwld.components}\n\n\n")
	mwld.systems["move"](mut &mwld)
	//println("${mwld.components}")
}

fn move2_test(mut wld &World) {
	//println("${wld.components}\n\n\n")
	wld.systems["move2"](mut &wld)
	//println("${wld.components}")
}