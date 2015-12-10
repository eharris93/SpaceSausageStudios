﻿using System;
using UnityEngine;

public class OverworldManager : MonoBehaviour
{
    public OverworldUI _OverworldUI;
    public CardSystem _CardSystem;
    public Board _Board;
    public Player _Player1;
    public int _Player1StartX;
    public int _Player1StartY;

    // Use this for initialization
    void Start()
    {
        //new game setup
        _Board.Initialise();
        _OverworldUI.Initialise();

        //try get the player start tile
        TileData startTile1 = _Board.GetTileAt(_Player1StartX, _Player1StartY);
        if (startTile1 != null)
        {
            _Player1.CommanderPosition = _Board.GetTileAt(_Player1StartX, _Player1StartY);
        }
        else
            Debug.LogError("Player start tile indexes are out of bounds");

        //snap player to start position
        _OverworldUI.UpdateCommanderPosition();

        //event listeners
        _OverworldUI.OnCommanderMove += _OverworldUI_OnCommanderMove;

        //allow player movement for the start ****JUST FOR TESTING****
        _OverworldUI.AllowPlayerMovement(_Board.GetReachableTiles(_Player1.CommanderPosition, 1));
    }

    void _OverworldUI_OnCommanderMove(TileData tile)
    {
        //set new position for the player (should depend on whose players turn it is)
        _Player1.CommanderPosition = tile;

        //****JUST FOR TESTING**** set new reachable tiles
        _OverworldUI.AllowPlayerMovement(_Board.GetReachableTiles(_Player1.CommanderPosition, 1));

        //Handles events that happen when player lands on that tile
        HandleTileEvent(tile);
    }

    void HandleTileEvent(TileData tile)
    {
        switch (tile.Building)
        {
            case BuildingType.None:
                break;
            case BuildingType.Armoury:
                break;
            case BuildingType.Camp:
                break;
            case BuildingType.CastleBattlebeard:
                break;
            case BuildingType.CastleStormshaper:
                break;
            case BuildingType.Cave:
                CaveGiveCard();
                break;
            case BuildingType.Fortress:
                break;
            case BuildingType.Inn:
                break;
            case BuildingType.StartTileBattlebeard:
                break;
            case BuildingType.StartTileStormshaper:
                break;
            default:
                break;
        }

        switch (tile.Terrain)
        {
            case TerrainType.CastleCorner00:
                break;
            case TerrainType.CastleCorner01:
                break;
            case TerrainType.CastleCorner10:
                break;
            case TerrainType.CastleCorner11:
                break;
            case TerrainType.Forest:
                break;
            case TerrainType.Grass:
                break;
            case TerrainType.Mountain:
                break;
            case TerrainType.Swamp:
                break;
            case TerrainType.Tundra:
                break;
            default:
                break;
        }
    }

    public void CaveGiveCard()
    {
        //Generate a random card (Warning: This is weighted heavily towards resource cards because 
        //there are more of them in the enum, change this later?)
        short randomCard = (short)UnityEngine.Random.Range(0, Enum.GetNames(typeof(Cards)).Length - 1);

        //Add the card to the players hand
        _Player1.hand.cardId.Add(randomCard);
    }

    // Update is called once per frame
    void Update()
    {

    }


}
