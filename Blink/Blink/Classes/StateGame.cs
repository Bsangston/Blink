﻿using System;
using System.IO;
using Blink.GUI;
using Blink.Classes;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Blink
{
    enum PlayerKeys
    {
        Player1,
        Player2,
        Player3,
        Player4,
        allPlayers

    }

	public class StateGame : GameState
	{
		Vector2 screenSize;
		PlayerClass player1;
		PlayerClass player2;
		PlayerClass player3;
		PlayerClass player4;
        PlayerClass[] players = new PlayerClass[4];
		PlayerKeys currPlayer;
		KeyboardState oldState;
		KeyboardState player1State;
		KeyboardState player2State;
		KeyboardState player3State;
		KeyboardState player4State;
        string mapName = "map1";
        float roundReset = -1;
        float timeElapsed;
        GameTime gameTime = new GameTime();
		Map map1;

        public void setMap(string map)
        {
            mapName = map;
        }

		public StateGame(Vector2 screenSize)
		{
			this.screenSize = screenSize;
		}

		public void Initialize()
		{
			player1 = new PlayerClass();
			player2 = new PlayerClass();
			player3 = new PlayerClass();
			player4 = new PlayerClass();

            player1.title = "p1";
            player2.title = "p2";
            player3.title = "p3";
            player4.title = "p4";

            player1.onPlayerKilled += new PlayerClass.PlayerKilledHandler(playerKilled);
            player2.onPlayerKilled += new PlayerClass.PlayerKilledHandler(playerKilled);
            player3.onPlayerKilled += new PlayerClass.PlayerKilledHandler(playerKilled);
            player4.onPlayerKilled += new PlayerClass.PlayerKilledHandler(playerKilled);

            map1 = new Map();
			currPlayer = PlayerKeys.Player1;
		}

		public void LoadContent(ContentManager Content)
		{
			Vector2 player1Pos = new Vector2(96, 96);
			Vector2 player2Pos = new Vector2(1400, 96);
			Vector2 player3Pos = new Vector2(400, 96);
			Vector2 player4Pos = new Vector2(1120, 96);
            
            players[0] = player1;
            players[1] = player2;
            players[2] = player3;
            players[3] = player4;

            player1.Initialize(Content.Load<Texture2D>("sprite"), player1Pos, screenSize, map1, players);
            player2.Initialize(Content.Load<Texture2D>("sprite"), player2Pos, screenSize, map1, players);
            player3.Initialize(Content.Load<Texture2D>("sprite"), player3Pos, screenSize, map1, players);
            player4.Initialize(Content.Load<Texture2D>("sprite"), player4Pos, screenSize, map1, players);

            player1.deadText = Content.Load<Texture2D>("spriteDead");
            player2.deadText = Content.Load<Texture2D>("spriteDead");
            player3.deadText = Content.Load<Texture2D>("spriteDead");
            player4.deadText = Content.Load<Texture2D>("spriteDead");

            StreamReader mapData;
            mapData = File.OpenText("Content/MapData/"+mapName+".map");
            map1.Initialize(Content.Load<Texture2D>("MapData/"+mapName+"Color"), mapData.ReadToEnd(), 32, 50, 30, players);
        }

		public void UnloadContent()
		{
			
		}

		public void Update(GameTime gameTime)
		{
			KeyboardState currState = Keyboard.GetState();

			/* Press TAB to change player if using keyboard. *** For Testing Purposes Only ***
                If you hold down a key while pressing TAB, the previous player will continue to do that same action
                over and over again until you tab to that player again. 
                (It is kinda amusing, but could be useful for collison testing) */
			if (currState.IsKeyDown(Keys.Tab) && oldState != currState)
			{
				switch ((int)currPlayer)
				{
					case 0:
						currPlayer = PlayerKeys.Player2;
						break;

					case 1:
						currPlayer = PlayerKeys.Player3;
						break;

					case 2:
						currPlayer = PlayerKeys.Player4;
						break;

					case 3:
						currPlayer = PlayerKeys.allPlayers;
						break;

					case 4:
						currPlayer = PlayerKeys.Player1;
						break;
				}
				//Switches to the next player
			}
			if (currPlayer == PlayerKeys.Player1)
			{
				player1State = Keyboard.GetState();
			}

			if (currPlayer == PlayerKeys.Player2)
			{
				player2State = Keyboard.GetState();
			}

			if (currPlayer == PlayerKeys.Player3)
			{
				player3State = Keyboard.GetState();
			}
			if (currPlayer == PlayerKeys.Player4)
			{
				player4State = Keyboard.GetState();
			}

			if (currPlayer == PlayerKeys.allPlayers)
			{
				player1State = Keyboard.GetState();
				player2State = Keyboard.GetState();
				player3State = Keyboard.GetState();
				player4State = Keyboard.GetState();
			}
			//End of TAB code. Can now only control one player at a time using keyboard.

			player1.Update(player1State, GamePad.GetState(PlayerIndex.One));
			player2.Update(player2State, GamePad.GetState(PlayerIndex.Two));
			player3.Update(player3State, GamePad.GetState(PlayerIndex.Three));
			player4.Update(player4State, GamePad.GetState(PlayerIndex.Four));
			oldState = currState;


            //If a timer is running, decrement here

            timeElapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if(roundReset > 0)
            {
                roundReset -= timeElapsed;
                if(roundReset < 0)
                {
                    resetMap();
                }
            }
        }

		public void Draw(SpriteBatch sb)
		{
			map1.Draw(sb);
			player1.Draw(sb);
			player2.Draw(sb);
			player3.Draw(sb);
			player4.Draw(sb);
		}

		public GameState GetTransition() 
		{
			return null;
		}

        private void playerKilled(Object sender, DeathEventArgs args)
        {
            //Do things like announce death/method of death

            detectWinner();
        }

        public void detectWinner()
        {
            Boolean survivor = false;
            PlayerClass victor = null;
            foreach (PlayerClass p in players)
            {
                if (victor == null && !p.isDead())
                {
                    victor = p;
                    survivor = true;
                }
                else if (victor != null && !p.isDead())
                {
                    victor = null;
                    break;
                }
            }

            if (victor != null || (victor == null && !survivor))
            {
                declareVictor(victor);
            }
        }

        private void declareVictor(PlayerClass victor)
        {
            victor.winner();
            roundReset = 3;
        }

        private void resetMap()
        {
            map1.reset();
            foreach(PlayerClass p in players)
            {
                p.reset();
            }
        }
    }
}

