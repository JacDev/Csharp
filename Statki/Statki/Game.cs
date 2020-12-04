﻿using Battleship.LanguageServices;
using Battleship.Interfaces;
using System;
using System.IO;
using System.Threading;

namespace Battleship
{
	public enum Actions : int { BACK_TO_MENU = -1, MISSED, END_GAME, UNDO }

	public class Game
	{

		private int chosenOption;
		private Player leftPlayer = null, rightPlayer = null;
		private BoardSide whoseTurn;
		private readonly ChosenLanguageModel _chosenLanguage;
		private readonly IOutputDevice _outputDevice;
		private readonly IInputDevice _inputDevice;
		private bool _canLoadGame;
		public Game(ChosenLanguageModel chosenLanguage, IOutputDevice outputDevice, IInputDevice inputDevice)
		{
			_chosenLanguage = chosenLanguage;
			_outputDevice = outputDevice;
			_inputDevice = inputDevice;
			_canLoadGame = false;
			MakeGame();
		}
		private void MakeGame()
		{
			bool closeWindow = false;
			while (!closeWindow)
			{
				chosenOption = _outputDevice.ShowMenu(!_canLoadGame);
				bool endGame = false;
				switch (chosenOption)
				{
					case 0:
						{
							EndCurrentGame();
							leftPlayer = new Person(_outputDevice, BoardSide.Left, _inputDevice);
							rightPlayer = new Computer(_outputDevice, BoardSide.Right, leftPlayer);
							leftPlayer.Opponent = rightPlayer;
							_canLoadGame = true;
							whoseTurn = BoardSide.Left;
							endGame = Play();
							break;
						}
					case 1:
						{
							EndCurrentGame();
							leftPlayer = new Person(_outputDevice, BoardSide.Left, _inputDevice);
							rightPlayer = new Person(_outputDevice, BoardSide.Right, _inputDevice, leftPlayer);
							leftPlayer.Opponent = rightPlayer;
							_canLoadGame = true;
							whoseTurn = BoardSide.Left;
							endGame = Play();
							break;
						}
					case 2:
						{
							EndCurrentGame();
							LoadGame();
							endGame = Play();
							_canLoadGame = true;
							break;
						}
					case 3:
						{
							closeWindow = true;
							break;
						}
					case 6:
						{
							endGame = Play();
							break;
						}
					case 7:
						{
							SaveGame();
							break;
						}
				}
			}
		}
		private bool Play()
		{
			Player winer;
			while (true)
			{
				Player currentPlayer = whoseTurn == BoardSide.Left ? leftPlayer : rightPlayer;
				Actions currentPlayerAction = currentPlayer.Shoot();
				leftPlayer.SunkenShips = 9;
				if (currentPlayerAction == Actions.END_GAME)
				{
					winer = currentPlayer;
					break;
				}
				else if (currentPlayerAction == Actions.BACK_TO_MENU)
				{
					return false;
				}
				else
				{
					whoseTurn = whoseTurn == BoardSide.Left ? BoardSide.Right : BoardSide.Left;
				}
			}

			_outputDevice.PrintBoard(leftPlayer.Board, rightPlayer.Board);
			if (winer.WhichBoard == BoardSide.Left)
			{
				Console.WriteLine(("Wygrales").PadRight(40, ' '));
			}
			Console.Write(("").PadRight(40, ' '));
			ReadEnter();
			return true;
		}
		private void EndCurrentGame()
		{
			if (rightPlayer != null)
			{
				rightPlayer.Board.ClearBoard();
			}
			if (leftPlayer != null)
			{
				leftPlayer.Board.ClearBoard();
			}
			rightPlayer = leftPlayer = null;
		}
		public void ReadEnter()
		{
			Console.WriteLine("Press ENTER to continue\n");
			while (_inputDevice.ReadKey() != Keys.Enter) ;
			Thread.Sleep(200);
		}
		private void SaveGame()
		{
			string directory = Directory.GetCurrentDirectory();
			Console.WriteLine(directory);
			using (StreamWriter outputFile = File.CreateText(Path.Combine(directory, "savedGame.txt")))
			{
				outputFile.WriteLine(leftPlayer.IsPerson().ToString() + " " + rightPlayer.IsPerson().ToString() + " " + leftPlayer.SunkenShips + " " + rightPlayer.SunkenShips + " " + whoseTurn.ToString() + " ");
				outputFile.WriteLine(leftPlayer.GetShipsAsString() + rightPlayer.GetShipsAsString() + leftPlayer.Board.ToString() +"\n"+ rightPlayer.Board.ToString());
				outputFile.Close();
			}
		}
		private bool LoadGame()
		{
			string directory = Directory.GetCurrentDirectory();
			string inputFilePath = Path.Combine(directory, "savedGame.txt");
			if (File.Exists(inputFilePath))
			{
				StreamReader inputFile = File.OpenText(inputFilePath);
				string game = inputFile.ReadToEnd();
				inputFile.Close();
				StringReader reader = new StringReader(game);
				LoadPlayers(reader.ReadLine());

				for (int i = 0; i < 10; ++i)
				{
					leftPlayer.AddShipAfterLoadGame(reader.ReadLine(), i);
				}
				for (int i = 0; i < 10; ++i)
				{
					rightPlayer.AddShipAfterLoadGame(reader.ReadLine(), i);
				}

				(leftPlayer.Board as ILoadable).ReadFromString(reader.ReadLine());
				(rightPlayer.Board as ILoadable).ReadFromString(reader.ReadLine());
				return true;
			}
			return false;
		}
		private void LoadPlayers(string line)
		{
			char separator = ' ';
			string[] substrings = line.Split(separator);

			leftPlayer = MakePlayer(substrings[0], BoardSide.Left);
			rightPlayer = MakePlayer(substrings[1], BoardSide.Right);
			leftPlayer.Opponent = rightPlayer;
			rightPlayer.Opponent = leftPlayer;

			leftPlayer.SunkenShips = Convert.ToInt32(substrings[2]);
			rightPlayer.SunkenShips = Convert.ToInt32(substrings[3]);
			whoseTurn = substrings[4] == "Left"? BoardSide.Left : BoardSide.Right;
		}
		private Player MakePlayer(string isPerson, BoardSide side)
		{
			if(isPerson == "True")
			{
				return new Person(_outputDevice, side, _inputDevice, afterLoad: true);
			}
			else
			{
				return new Computer(_outputDevice, side, afterLoad: true);			
			}
		}
	}
}