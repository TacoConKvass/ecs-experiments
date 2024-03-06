module main

//import gg
//import benchmark
import vectors { Vec2 }
import ecs { World, Component, System	 }

struct Position {
	Component
	mut:
		data 	[]Vec2 	@[skip]
		owners 	[]int 	@[skip]
}

pub fn (mut p Position) add_entity(id int, value Vec2) {
	p.data << value
	p.owners << id
}

struct Velocity {
	Component
	mut:
		data 	[]Vec2
		owners 	[]int
}

pub fn (mut v Velocity) add_entity(id int, value Vec2) {
	v.data << value
	v.owners << id
}

fn main() {
	mut w := World{}
	w.add_component(Position{})
	w.add_component(Velocity{})
	w.add_entities([1, 2, 3, 4])
	w.add_entity_batch_to_component<Position, Vec2>([1, 2, 3 ], [Vec2{1, 1}, Vec2{1, 1}, Vec2{1, 1}] )
	pos := w.get_component<Position>() or { panic("WAH") }
	println(pos.data)
}