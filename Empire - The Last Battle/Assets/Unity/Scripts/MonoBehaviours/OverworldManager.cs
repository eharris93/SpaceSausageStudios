using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Linq;

public class OverworldManager : MonoBehaviour
{
	public OverworldUI _OverworldUI;
	public CardList _AvailableCaveCards;
	public CardSystem _CardSystem;
	public Board _Board;
	public Player _BattlebeardPlayer;
	public Player _StormshaperPlayer;
    public TurnManager _TurnManager;
	public GameStateHolder _GameStateHolder;
	public BattleData _BattleData;
	public string _BattleScene;

	//****TESTS ONLY****
	public CardList _StartCards;

	// Use this for initialization
	void Start() {
		//new game setup

		_Board.Initialise();
		_BattlebeardPlayer.Initialise();
		_StormshaperPlayer.Initialise();

		if (_BattleData._EndState == BattleEndState.None) {
			//try get the battleboard start tile
			if (_Board._BBStartTile != null) {
				_BattlebeardPlayer.CommanderPosition = _Board._BBStartTile;
			}
			else {
				Debug.LogError("Battlebeard start tile not set");
			}

			if (_Board._SSStartTile != null) {
				_StormshaperPlayer.CommanderPosition = _Board._SSStartTile;
			}
			else {
				Debug.LogError("Stormshaper start tile not set");
			}
		}

        //test by adding a scout card.

		_OverworldUI.Initialise(_BattlebeardPlayer, _StormshaperPlayer);

        _TurnManager.OnTurnStart += _TurnManager_OnTurnStart;
        _TurnManager.OnTurnEnd += _TurnManager_OnTurnEnd;
        _TurnManager.OnSwitchTurn += _TurnManager_OnSwitchTurn;


		//event listeners
		_OverworldUI.OnCommanderMove += _OverworldUI_OnCommanderMove;
		_OverworldUI.OnCommanderForceMove += _OverworldUI_OnCommanderForceMove;
        _OverworldUI.OnPause += _OverworldUI_OnPause;
        _OverworldUI.OnUnPause += _OverworldUI_OnUnPause;
        _OverworldUI.OnPlayerUseCard += _OverworldUI_OnPlayerUseCard;
		_BattlebeardPlayer.Currency.OnChange += _OverworldUI._ResourceUI.UpdateResources;
		_BattlebeardPlayer.OnCardAdded += _BattlebeardPlayer_OnCardAdded;
		_BattlebeardPlayer.OnCardRemoved += _BattlebeardPlayer_OnCardRemoved;
		_StormshaperPlayer.Currency.OnChange += _OverworldUI._ResourceUI.UpdateResources;
		_StormshaperPlayer.OnCardAdded += _StormshapersPlayer_OnCardAdded;
		_StormshaperPlayer.OnCardRemoved += _StormshapersPlayer_OnCardRemoved;

		_CardSystem.RequestUnitSelection +=_CardSystem_RequestUnitSelection;

		_BattlebeardPlayer.OnCastleProgress += _Board.SetCastleState;
		_StormshaperPlayer.OnCastleProgress += _Board.SetCastleState;
		

		_CardSystem.OnEffectApplied += _CardSystem_OnEffectApplied;

		_GameStateHolder._gameState = GameState.Overworld;

		if (_BattleData._EndState == BattleEndState.None) {

			//test by adding cards.
			_BattlebeardPlayer.SetCards(_StartCards);
			_StormshaperPlayer.SetCards(_StartCards);

			setPlayer(PlayerType.Battlebeard);
			_GameStateHolder._ActivePlayer.PlayerArmy.AddUnit(UnitType.Scout);

			_BattlebeardPlayer.CastleProgress = 4;
			_StormshaperPlayer.CastleProgress = 4;

			_TurnManager.StartTurn();    
		}
		else {
			setPlayer(_BattleData._InitialPlayer.Type);

			_OverworldUI.AllowPlayerMovement(_Board.GetReachableTiles(_GameStateHolder._ActivePlayer.Type, _GameStateHolder._ActivePlayer.CommanderPosition, 1));

			_BattlebeardPlayer.CastleProgress = _BattlebeardPlayer.CastleProgress;
			_StormshaperPlayer.CastleProgress = _StormshaperPlayer.CastleProgress;
			// run the battle end logic
			endBattle();
		}
	}

	void _OverworldUI_OnPlayerUseCard(CardData card)
    {
        UseCard(card);
    }

	void _BattlebeardPlayer_OnCardAdded(CardData card)
	{
		//inform ui
		_OverworldUI.AddPlayerCard (PlayerType.Battlebeard, card);
	}

	void _BattlebeardPlayer_OnCardRemoved(CardData card)
	{
		//inform ui
		_OverworldUI.RemovePlayerCard (PlayerType.Battlebeard, card);
	}

	void _StormshapersPlayer_OnCardAdded(CardData card)
	{
		//inform ui
		_OverworldUI.AddPlayerCard (PlayerType.Stormshaper, card);
	}

	void _StormshapersPlayer_OnCardRemoved(CardData card)
	{
		//inform ui
		_OverworldUI.RemovePlayerCard (PlayerType.Stormshaper, card);
	}

	// get rid of this shit later
	void cardNotUsedModal () {
		ModalPanel.Instance().ShowOK("Oh No!", "Card could not be used", null);
	}

	void _CardSystem_OnEffectApplied(bool success, CardData card, Player player, Unit u) {
		ModalPanel p = ModalPanel.Instance();
		if (!success) {
			// we wont bother with this in the final but for now it hides the previous modal after it shows this one.
			Invoke("cardNotUsedModal", 1);
			return;
		}
		if (card.Type == CardType.Scout_Card) {
            _OverworldUI.AllowPlayerMovement(_Board.GetReachableTiles(player.Type, player.CommanderPosition, card.Value));
		}
		ModalPanel.Instance().ShowOK("Yeah!", "Used " + card.Type, null);
		_GameStateHolder._ActivePlayer.RemoveCard(card);
	}

	void UseCard(CardData card) {

		ModalPanel p = ModalPanel.Instance();
		p.ShowOK("Card", "Using " + card.Type, () => {
			_CardSystem.UseCard(card, _GameStateHolder._ActivePlayer, _GameStateHolder._InactivePlayer);
		});


		//use the card
		//if (_CardSystem.CanUseCard (card, _GameStateHolder._gameState))
		//{
		//	_CardSystem.ApplyEffect (card, _GameStateHolder._ActivePlayer);	
		//}

	}

    void _OverworldUI_OnUnPause()
    {
        _OverworldUI._Paused = false;
    }

    void _OverworldUI_OnPause()
    {
        _OverworldUI._Paused = true;
    }

	void _OverworldUI_OnCommanderForceMove(TileData tile) {
		_GameStateHolder._ActivePlayer.CommanderPosition = tile;
		//****JUST FOR TESTING**** set new reachable tiles
		_OverworldUI.AllowPlayerMovement(_Board.GetReachableTiles(_GameStateHolder._ActivePlayer.Type, _GameStateHolder._ActivePlayer.CommanderPosition, 1));
	}

	void _OverworldUI_OnCommanderMove(TileData tile) {
		//set new position for the player (should depend on whose players turn it is)
		_GameStateHolder._ActivePlayer.CommanderPosition = tile;

		//****JUST FOR TESTING**** set new reachable tiles
		_OverworldUI.AllowPlayerMovement(_Board.GetReachableTiles(_GameStateHolder._ActivePlayer.Type, _GameStateHolder._ActivePlayer.CommanderPosition, 1));

		//Handles events that happen when player lands on that tile
		HandleTileEvent(tile);
	}
    //This is temporary until we actually have things that happen after the move
    IEnumerator SwitchPlayer() {
        yield return new WaitForSeconds(1);
        _TurnManager.SwitchTurn();
        _OverworldUI.Enable();
    }
	void HandleTileEvent(TileData tile) {
        _OverworldUI.Disable();
		if (_GameStateHolder._ActivePlayer.IsScouting) {
			_GameStateHolder._ActivePlayer.IsScouting = false;
			endTurn ();
		} else {
			ModalPanel p = ModalPanel.Instance ();
			switch (tile.Building) {
			case BuildingType.Armoury:
				p.ShowOK ("Armoury", "You landed on the Armoury.", endTurn);
				break;
			case BuildingType.Camp:
				if (tile.Owner != _GameStateHolder._ActivePlayer.Type) {
					if (tile.Owner == PlayerType.None) {
						// MONSTER BATTLE
						p.ShowOK("Camp", "You landed on a camp full of monsters.", () => {
							startBattle(BattleType.Monster);
						});
					} else {
						// PVP BATTLE
						p.ShowOK("Camp", "You landed on the other player's camp.", () => {
							startBattle(BattleType.PvP);
						});
					}
					break;
				}
				p.ShowOK("Camp", "Nothing happened.", endTurn);
				break;
			case BuildingType.Cave:
				if (tile.Owner != _GameStateHolder._ActivePlayer.Type) {
					if (tile.Owner != PlayerType.None) {
						p.ShowOK("Cave", "You landed on the other player's cave.", () => {
							startBattle(BattleType.PvP);
						});
					} else {
						CardData c = GenerateRandomCard(_AvailableCaveCards.cards);
						_GameStateHolder._ActivePlayer.AddCard(c);
						_Board.SetTileOwner(tile, _GameStateHolder._ActivePlayer.Type);
						p.ShowOK("Card Recieved!", "You recieved a " + c.Type + " card.", endTurn);
					}
					break;
				}
				endTurn ();
				break;
			case BuildingType.Fortress:
					// Make sure that the fortress type matches the player type
				if (tile.Owner == _GameStateHolder._ActivePlayer.Type) {
					// make sure the player owns at least 3 surrounding tiles
					if (tile.GetConnectedTiles ().FindAll (t => t.Owner == _GameStateHolder._ActivePlayer.Type).Count >= 3) {
						// Battle lost immortal
						p.ShowOK("Fortress", "A bloody Lost Immortal just showed up innit blud!", () => {
							startBattle(BattleType.LostImmortal);
						});
					} else {
						p.ShowOK ("Fortress", "You felt a chill in the air, but nothing appeared.", endTurn);
					}
					break;
				}
				p.ShowOK ("Fortress", "Nothing appeared.", endTurn);
					// end turn
				break;
			case BuildingType.Inn:
				if (_GameStateHolder._InactivePlayer.CastleProgress >= 4) {
					p.ShowOK ("Oh No!", "The inn won't accept you!", endTurn);
					break;
				}

				var rnd = UnityEngine.Random.Range (0, 3);
				List<Unit> units;

				units = _GameStateHolder._ActivePlayer.PlayerArmy.GetRandomUnits (rnd, true);

				foreach (var unit in units) {
					unit.Heal ();
				}

				string content, title;

				if (_GameStateHolder._ActivePlayer.PlayerArmy.GetKOUnits ().Count == 0) {
					title = "The Inn Welcomes You";
					content = "You are well rested.";
				} else {
					title = units.Count + " Units Healed";
					content = rnd == 0 ?
						"Their wounds were too great. Looks like they'll need some more time." :
						"Your army is well rested.";
				}
				p.ShowOK (title, content, endTurn);
				break;
			default:
				endTurn ();
				break;
			}
		}
	}

	void startBattle(BattleType type) {
		_BattleData._BattleType = type;
		_BattleData._InitialPlayer = _GameStateHolder._ActivePlayer;
		tearDownScene();
		Application.LoadLevel(_BattleScene);
	}

	void tearDownScene() {
		_BattlebeardPlayer.RemoveListeners();
		_StormshaperPlayer.RemoveListeners();
		_OverworldUI.RemoveListeners();
		_CardSystem.RemoveListeners();
	}


	void endBattle() {
		ModalPanel p = ModalPanel.Instance ();
		_OverworldUI.Disable ();
		TileData tile = _GameStateHolder._ActivePlayer.CommanderPosition;
		if (_BattleData._BattleType == BattleType.Monster) {
			Debug.Log(_BattleData._EndState);
			if (_BattleData._EndState == BattleEndState.Win) {
				p.ShowOK ("Battle", "You beat the monster!", endTurn);
				if (tile.Building == BuildingType.Camp) {
					_Board.SetTileOwner(tile, _GameStateHolder._ActivePlayer.Type);
				}
			} else {
				_OverworldUI.Enable();
				_OverworldUI.ForceMoveCommander(_GameStateHolder._ActivePlayer.PreviousCommanderPosition);
				p.ShowOK("Battle", "You lost against the monster!", endTurn);
			}
		}
		else if (_BattleData._BattleType == BattleType.PvP) {
			if (_BattleData._EndState == BattleEndState.Win) {
				_Board.SetTileOwner(tile, _GameStateHolder._ActivePlayer.Type);

				if (tile.Building == BuildingType.Cave) {
					CardData c = GenerateRandomCard(_AvailableCaveCards.cards);
					_GameStateHolder._ActivePlayer.AddCard(c);
					Debug.Log("You beat the other player");
					p.ShowOK("Card Recieved!", "You recieved a " + c.Type + " card.", endTurn);
				}
				else {
					p.ShowOK("Battle", "You beat the other player!", endTurn);
				}
				
			} else {
				_OverworldUI.Enable();
				_OverworldUI.ForceMoveCommander(_GameStateHolder._ActivePlayer.PreviousCommanderPosition);
				p.ShowOK("Battle", "You lost against the other player!", endTurn);
			}
		}
		else if (_BattleData._BattleType == BattleType.LostImmortal) {
			if (_BattleData._EndState == BattleEndState.Win && tile.Building == BuildingType.Fortress) {
				tile.Owner = PlayerType.None;
				_GameStateHolder._ActivePlayer.LostImmortalKillCount++;
				_GameStateHolder._ActivePlayer.CastleProgress++;
				p.ShowOK ("Battle", "You beat the lost immortal!", endTurn);
			} else {
				_OverworldUI.Enable();
				_OverworldUI.ForceMoveCommander(_GameStateHolder._ActivePlayer.PreviousCommanderPosition);
				p.ShowOK("Battle", "You lost against the lost immortal!", endTurn);
			}
		}
	}

	void endTurn() {
		_TurnManager.EndTurn ();
		_BattleData._EndState = BattleEndState.None;
		StartCoroutine (SwitchPlayer ());
	}

	public CardData GenerateRandomCard(List<CardData> availableCards) {
		//Generate a random card
		List<CardType> uniqueTypes = availableCards.Select(x => x.Type).Distinct().ToList();
		int randomTypeIndex = Random.Range(0, uniqueTypes.Count - 1);
		List<CardData> cardsOfType = availableCards.FindAll(x => x.Type == uniqueTypes[randomTypeIndex]);
		int randomCardIndex = (short)Random.Range(0, cardsOfType.Count - 1);
		return cardsOfType[randomCardIndex];
	}

	void _CardSystem_RequestUnitSelection(CardData c, int numSelection, Player p, CardAction action, EndCardAction done) {
		// assume P is going to be the current player

		UnitSelection flags = UnitSelection.None;
		if (c.Type == CardType.Healing_Card) {
			flags = flags | UnitSelection.Inactive;
		}
		if (c.Type == CardType.Upgrade_Card) {
			flags = flags | UnitSelection.Active;
			flags = flags | UnitSelection.NotUpgraded;
		}
		if (c.Type == CardType.Tactic_Card) {
			flags = flags | UnitSelection.Active;
			flags = flags | UnitSelection.NotTempUpgraded;
		}

		_OverworldUI.ShowUnitSelectionUI(flags);

		int selectedUnits = 0;
		UIPlayerUnitTypeIndexCallback selectUnit = null;
		selectUnit = (PlayerType pt, UnitType u, int i) => {
			Unit unit = _GameStateHolder._ActivePlayer.PlayerArmy.GetUnits(u)[i];
			selectedUnits++;
			// perform the action each time something is selected. This will only effect healing.
			// we don't want the player to be stuck with no units to select
			action(c, p, unit);
			// we reached the total?
			if (selectedUnits == numSelection) {
				// don't listen for namy more and hide the UI
				_OverworldUI._ArmyUI.OnClickUnit -= selectUnit;
				_OverworldUI.HideUnitSelectionUI();
				done(true, c, p, unit);
			}

		};
		_OverworldUI._ArmyUI.OnClickUnit += selectUnit;
	}


    public void Pause() {
        _OverworldUI.Disable();
    }

    public void UnPause()
    {
        _OverworldUI.Enable();
    }

    void _TurnManager_OnTurnStart() {
		_OverworldUI.Show();
		_OverworldUI.AllowPlayerMovement(_Board.GetReachableTiles(_GameStateHolder._ActivePlayer.Type, _GameStateHolder._ActivePlayer.CommanderPosition, 1));
    }

    void _TurnManager_OnTurnEnd() {
		_OverworldUI.Hide();
        _OverworldUI.DisablePlayerMovement();
    }

	void setPlayer(PlayerType p) {
		_GameStateHolder._ActivePlayer = p == PlayerType.Battlebeard ? _BattlebeardPlayer : _StormshaperPlayer;    
		_GameStateHolder._InactivePlayer = p == PlayerType.Battlebeard ? _StormshaperPlayer : _BattlebeardPlayer;    
		_OverworldUI.SetPlayer(_GameStateHolder._ActivePlayer);    
	}  


    void _TurnManager_OnSwitchTurn() {    
		setPlayer (_GameStateHolder._ActivePlayer.Type == PlayerType.Battlebeard ? PlayerType.Stormshaper : PlayerType.Battlebeard);
		_TurnManager.StartTurn ();
    }

	// Update is called once per frame
	void Update() {
		if (Input.GetKeyDown(KeyCode.Return)) {
			StartCoroutine(SwitchPlayer());
		}

		if (Input.GetKeyDown (KeyCode.M)) {
			if (_GameStateHolder._ActivePlayer.PreviousCommanderPosition && _GameStateHolder._ActivePlayer.PreviousCommanderPosition != _GameStateHolder._ActivePlayer.CommanderPosition) {
				_OverworldUI.ForceMoveCommander(_GameStateHolder._ActivePlayer.PreviousCommanderPosition);
			}
		}
		if (Input.GetKeyDown(KeyCode.N)) {
			// move back using up a turn
			if (_GameStateHolder._ActivePlayer.PreviousCommanderPosition && _GameStateHolder._ActivePlayer.PreviousCommanderPosition != _GameStateHolder._ActivePlayer.CommanderPosition) {
				_OverworldUI.MoveCommander(_GameStateHolder._ActivePlayer.PreviousCommanderPosition);
			}
		}
		if (Input.GetKeyDown(KeyCode.C)) {
			CardData c = GenerateRandomCard(_AvailableCaveCards.cards);
			_GameStateHolder._ActivePlayer.AddCard(c);
		}
		//Delete in final build. Used for testing, an example of how to call debug message class
		if (Input.GetKeyDown (KeyCode.Alpha8)) {
			DebugUI.getUI ().SetMessage ("Test", 22, Color.green);
		}
	}
}
