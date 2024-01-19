module vectors

import math

// Vector2 definition
pub struct Vec2 {
	x f64
	y f64
}

pub fn (v Vec2) add(v2 Vec2) Vec2 {
	result := Vec2 {v.x + v2.x, v.y + v2.y}
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