module vectors

import math

// Vector2 definition
pub struct Vec2 {
	pub:
		x f64
		y f64
}

pub fn (v Vec2) add(v2 Vec2) Vec2 {
	result := Vec2 {v.x + v2.x, v.y + v2.y}
	return result
}

pub fn (v Vec2) substract(v2 Vec2) Vec2 {
	result := Vec2 {v.x - v2.x, v.y - v2.y}
	return result
}

pub fn (v Vec2) length() f64 {
	len := math.sqrt(v.x * v.x + v.y * v.y)
	return len
}

pub fn (v Vec2) normalize() Vec2 {
	result := Vec2{ v.x / v.length(), v.y / v.length() }
	return result
}

pub fn (v Vec2) distance_to(v2 Vec2) f64 {
	result := Vec2{math.abs(v.x - v2.x), math.abs(v.y - v2.y)}.length()
	return result
}

pub fn (v Vec2) get_angle() f64 {
	result := if v.y < 0 {
		2 * math.pi - math.acos(v.x / v.length())
	} else {
		math.acos(v.x / v.length())
	}
	return result
} 

pub fn (v Vec2) rotated_towards(angleInRadians f64) Vec2 {
	result := match angleInRadians {
		math.pi { Vec2{ -v.length(), 0 } }
		math.pi / 2 { Vec2{ 0, v.length() } }
		math.pi * 3 / 2 { Vec2{ 0, -v.length() } }
		else { Vec2{ math.cos(angleInRadians) * v.length(), math.sin(angleInRadians) * v.length() } }
	}
	return result
}

pub fn (v Vec2) rotated_by(angleInRadians f64) Vec2	{
	result := v.rotated_towards(v.get_angle() + angleInRadians)
	return result
}

pub fn (v Vec2) direction_to(v2 Vec2) Vec2 {
	result := Vec2{-(v.x - v2.x), -(v.y-v2.y)}
	return result
}

pub fn (v Vec2) direction_from(v2 Vec2) Vec2 {
	result := Vec2{v.x - v2.x, v.y-v2.y}
	return result
}

pub fn (v Vec2) scale(scalar f64) Vec2 {
	result := Vec2{v.x * scalar, v.y * scalar}
	return result
} 

pub fn (v Vec2) get_dot(v2 Vec2) f64 {
	result := (v.x * v2.x) - (v.y * v2.y)
	return result
}

pub fn (v Vec2) copy() Vec2 {
	result := Vec2{v.x, v.y}
	return result
}