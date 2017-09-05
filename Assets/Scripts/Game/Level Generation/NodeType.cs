using UnityEngine;
using System.Collections;
using System;

[Flags]
public enum NodeType {

    Empty = 1,
    BasicGround = 2,
    Water = 4,
    Ice = 8,
    Lava = 16,
    Quicksand = 32,
    Bog = 64


    /*
	Empty = 1 << 2,
	BasicGround = 1 << 3,
	Water = 1 << 4,
	Ice = 1 << 5,
	Lava = 1 << 6,
	Quicksand = 1 << 7,
	Bog = 1 << 8*/
}
