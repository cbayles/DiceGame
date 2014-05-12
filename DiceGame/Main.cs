using System;
using System.Collections.Generic;
using NSpec;

namespace DiceGame
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("How many players are going to play??????? (press 'q' at any time to stop the game)");
			var playerCount = Console.ReadLine();
			var game = new DiceGame(Convert.ToInt32 (playerCount));
			do
			{
				game.RollDie();
				game.PrintGameSummary();
				Console.WriteLine("It's {0}'s turn. Press a key to roll the die.", game.CurrentPlayer.Name);
			} while(Console.ReadKey().Key != ConsoleKey.Q);
		}
	}
	
	public class DiceGame
	{
		private IList<Player> _players;
		public IList<Player> Players 
		{
			get {return _players;}
		}
		
		private Player _currentPlayer;
		public Player CurrentPlayer
		{
			get {return _currentPlayer;}
		}
		
		private Player _nextPlayer;
		public Player NextPlayer
		{
			get {return _nextPlayer;}
		}
		
		private int _throughput;
		public int Throughput
		{
			get {return _throughput;}
		}
		
		private Random _random;
		private int _gameRound = -1;
		
		public DiceGame(int playerCount)
		{
			if (playerCount < 2)
				throw new ArgumentException("Player Count must be at least 2.");
			
			_random = new Random();
			_players = new List<Player>();
			for (int i = 0; i < playerCount; i++) {
				_players.Add(new Player("Player" + (i+1), i));
				//Console.WriteLine(_players[i].Name + "  " + _players[i].TurnOrder);
			}
			RotatePlayers();
		}

		
		void RotatePlayers ()
		{
			_currentPlayer = _nextPlayer;
			if (_currentPlayer == null)
			{
				_gameRound += 1;
				_currentPlayer = _players[0];
				_nextPlayer = _players[1];
				return;
			}
			
			if (_currentPlayer.TurnOrder == _players.Count-1)
			{
				_nextPlayer = null;
				return;
			}
			
			_nextPlayer = _players[_nextPlayer.TurnOrder+1];
		}
		
		public void RollDie()
		{
			var roll = _random.Next (1,6);
			RollDie(roll);
		}
		
		public void RollDie(int roll)
		{
			Console.WriteLine("{0} rolled a {1}.", CurrentPlayer.Name, roll);
			
			int available;
			if (_currentPlayer == _players[0]) available = roll;
			else available = Math.Min (roll, _currentPlayer.Inventory);
			
			_currentPlayer.Inventory -= available;
			if (_nextPlayer == null) _throughput += available;
			else _nextPlayer.Inventory += available;
			RotatePlayers ();
		}
		
		public void PrintGameSummary()
		{
			Console.WriteLine("{0} Player Game summary:", _players.Count);
			foreach (Player player in _players) {
				Console.WriteLine("\t{0} has {1} Inventory.", player.Name, player.Inventory);
			}
			Console.WriteLine("\tGame Throughput after {0} rounds: {1} (avg: {2}) \n", _gameRound, _throughput, _gameRound==0 ? "NaN" : Math.Round ((decimal)_throughput/_gameRound, 1).ToString());
		}
	}
	
	public class Player
	{
		private string _name;
		public string Name 
		{
			get{return _name;}
		}
		
		private int _inventory;
		public int Inventory 
		{
			get {return _inventory;}
			set {_inventory = value < 0 ? 0 : value;}
		}
		
		private int _turnOrder;
		public int TurnOrder 
		{
			get {return _turnOrder;}
		}
		public Player(string playerName, int turnOrder)
		{
			_name = playerName;
			_turnOrder = turnOrder;
		}
	}
}

namespace DiceGame.Tests
{
	public class Describe_DiceGame : nspec
	{
		DiceGame game;
		
		public void describe_a_3_player_game()
		{
			before = () => game = new DiceGame(3);
				
			context["when the first round begins"] = () => 
			{
				it["Player count should be 3"] = () => game.Players.Count.should_be(3);
				it["and the current player should be Player1"] = () => game.CurrentPlayer.Name.should_be("Player1");
				it["and the next player should be Player2"] = () => game.NextPlayer.Name.should_be("Player2");
			
				context["when the first player rolls a 4"] = () =>
				{
					before = () => game.RollDie(4);
			
					it["the second player should have 4 inventory"] = () => game.Players[1].Inventory.should_be(4);
					it["and the current player should be Player2"] = () => game.CurrentPlayer.Name.should_be("Player2");
					it["and the next player should be Player3"] = () => game.NextPlayer.Name.should_be("Player3");
				
			
					context["when the second player rolls a 2"] = () =>
					{
						before = () => game.RollDie(2);
				
						it["the second player should have 2 inventory"] = () => game.Players[1].Inventory.should_be(2);
						it["the third player should have 2 inventory"] = () => game.Players[2].Inventory.should_be(2);
						it["and the current player should be Player3"] = () => game.CurrentPlayer.Name.should_be("Player3");
						it["and the next player should be null"] = () => game.NextPlayer.should_be(null);
					
			
						context["when the third player rolls a 2"] = () =>
						{
							before = () => game.RollDie(2);
					
							it["the second player should have 2 inventory"] = () => game.Players[1].Inventory.should_be(2);
							it["the third player should have 0 inventory"] = () => game.Players[2].Inventory.should_be(0);
							it["the game Throughput should be 2"] = () => game.Throughput.should_be(2);
							it["and the current player should be Player1"] = () => game.CurrentPlayer.Name.should_be("Player1");
							it["and the next player should be Player2"] = () => game.NextPlayer.Name.should_be("Player2");
						};
					};	
				};
			};
			
			context["when the second round begins"] = () => 
			{
				before = () => {
					game.RollDie(4);
					game.RollDie(2);
					game.RollDie(2);
				};
				
				it["Player count should be 3"] = () => game.Players.Count.should_be(3);
				it["and the current player should be Player1"] = () => game.CurrentPlayer.Name.should_be("Player1");
				it["and the next player should be Player2"] = () => game.NextPlayer.Name.should_be("Player2");
				it["the second player should have 2 inventory"] = () => game.Players[1].Inventory.should_be(2);
				it["the third player should have 0 inventory"] = () => game.Players[2].Inventory.should_be(0);
				it["the game Throughput should be 2"] = () => game.Throughput.should_be(2);
				
				context["when the first player rolls a 2"] = () => 
				{
					before = () => game.RollDie(2);
					it["the second player should have 2 more inventory, totaling 4"] = () => game.Players[1].Inventory.should_be(4);
					it["and the current player should be Player2"] = () => game.CurrentPlayer.Name.should_be("Player2");
					it["and the next player should be Player3"] = () => game.NextPlayer.Name.should_be("Player3");
					
					context["when the second player rolls a 6"] = () => 
					{
						before = () => game.RollDie(6);
						it["the second player should have 0 inventory"] = () => game.Players[1].Inventory.should_be(0);
						it["the third player should receive only the 4 inventory that Player2 had, totaling 4"] = () => game.Players[2].Inventory.should_be(4);
						it["and the current player should be Player3"] = () => game.CurrentPlayer.Name.should_be("Player3");
						it["and the next player should be null"] = () => game.NextPlayer.should_be(null);
						
						context["when the third player rolls a 6"] = () => 
						{
							before = () => game.RollDie(6);
							it["the second player should have 0 inventory"] = () => game.Players[1].Inventory.should_be(0);
							it["the third player should have 0 inventory"] = () => game.Players[2].Inventory.should_be(0);
							it["the game Throughput receive only the 4 inventory that Player3 had, totaling 6"] = () => game.Throughput.should_be(6);
							it["and the current player should be Player1"] = () => game.CurrentPlayer.Name.should_be("Player1");
							it["and the next player should be Player2"] = () => game.NextPlayer.Name.should_be("Player2");
						};
					};
				};
			};
		}
	}
}