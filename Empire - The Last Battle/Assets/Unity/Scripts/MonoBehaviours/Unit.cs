﻿using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour {

	public UnitBaseData UnitBaseData;
	//TODO: Decide if this is the best way to handle an upgrade. Is it better to have a separate Upgrade class?
	public UnitBaseData Upgrade;
	public TileData Position;

	void Start () {
	
	}
}
