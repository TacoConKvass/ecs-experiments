module ecs

import vectors { Vec2 }

pub struct ComponentData[T] {
	pub:
		name string
	pub mut:
		data []T
		owner []int
}

pub fn (mut c ComponentData[T]) add_entity(data T, entity_id int) {
	c.data << data
	c.owner << entity_id
}

pub fn (mut c ComponentData[T]) add_entity_batch(data []T, entity_id []int) {
	if data.len != entity_id.len {
		panic("Amount of data points does not equal the amount of entities")
	}
	c.data << data
	c.owner << entity_id
}

pub struct ComponentList {
	pub mut:
		c_vec []ComponentData[Vec2]
		c_int []ComponentData[int]
		c_f64 []ComponentData[f64]
		c_str []ComponentData[string]
}

// World struct for ECS
pub struct World {
	pub mut:	
			entities []int
			components ComponentList
			systems map[string]fn(mut &World)
}

pub fn (mut w World) get_component[T](name string) !ComponentData[T]{
	mut temp := []ComponentData[T]
	mut list := &temp

	match T.name {
		"int" { list = &w.components.c_int }
		"f64" { list = &w.components.c_f64 }
		"string" { list = &w.components.c_str }
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

pub fn (mut w World) register_component[T](name string) !{
	match T.name {
		"int" { w.components.c_int << ComponentData[int]{ name, []int, []int } }
		"f64" { w.components.c_f64 << ComponentData[f64]{ name, []f64, []int } }
		"string" { w.components.c_str << ComponentData[string]{ name, []string, []int } }
		"vectors.Vec2" {  w.components.c_vec << ComponentData[Vec2]{ name, []Vec2, []int } }
		else { return error("This data type is not allowed")}
	}
}

pub fn (mut w World) register_system(name string, func fn (mut wld &World)) {
	w.systems = {
		...w.systems
		name: func
	}
}