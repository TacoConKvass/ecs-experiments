module ecs

pub struct World {
	mut:	
		systems		[]System
		components	[]IComponent
		entities	[]int
}

pub fn (mut w World) add_system(system System) bool{
	for exisiting_system in w.systems {
		if exisiting_system.name == system.name {
			println("A system with this name already exists")
			return false
		}
	}
	w.systems << system
	return true
}

pub fn (w World) run_system(name string) bool {
	for system in w.systems {
		if system.name == name {
			system.action()
			return true
		}
	}

	println("A system with that name was not found")
	return false
}

pub fn (mut w World) add_component(component IComponent) bool {
	if component in w.components {
		println("This component already exists")
		return false
	}

	w.components << component
	return true
}

pub fn (mut w World) get_component<T>() ?&T {
	for component in w.components {
		if component is T {
			return component
		}
	}

	println("Component of that type was not found")
	return none
}

pub fn (mut w World) add_entities(ids []int) bool {
	mut result := []int{}
	for id in ids {
		if id in w.entities {
			println("Entity with an id of ${id} already exists")
			return false
		}

		if id in result {
			println("You already used the id of ${id}. All ids should be unique")
			return false
		}
		result << id
	}

	w.entities << result
	return true
}

pub fn (mut w World) get_entities() []int {
	return w.entities
}

pub fn (mut w World) add_entity_to_component<T, V>(id int, value V) bool {
	if id in w.entities {
		mut comp := w.get_component<T>() or {
			return false
		}

		comp.add_entity(id, value)

		return true
	}
		
	println("An entity with id of ${id} does not exist")
	return false
}

pub fn (mut w World) add_entity_batch_to_component<T, V>(id []int, value []V) bool {
	mut comp := w.get_component<T>() or {
		return false
	}

	for i in id{
		if i in w.entities {

			if id.len != value.len {
				println("The list of ids must have the same length as the listo of values")
			}

			for ii in 0 .. id.len {
				comp.add_entity(id[ii], value[ii])
			}

			return true
		}
		println("An entity with the id of ${i} does not exist")
		return false
	}

	println("You should not be here. Report this case")
	return false
}

pub interface IComponent {
	is_component bool
}

pub struct Component {
	is_component bool
}

pub struct System {
	pub:
		name 	 string @[required]
		action 	 fn()	@[required]
}