# Docs


## World<T>
Provide a type union of types used by your components as T.

### Members
 - systems 		=> `[]System`
 - components	=> `[]Componnent<T>`
 - entities 	=> `[]int`

### Methods

#### add_system(system System) bool
Only one system with a specific name is allowed.
Returns a boolean value showing wether adding the system ended in a success.

#### run_system(name string) bool
Runs the system with the specified name.
Returns a boolean value dependent of wether a system with the provided name was found.
