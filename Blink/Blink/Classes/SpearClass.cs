﻿using System;

using System.IO;
using Blink.GUI;
using Blink;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Blink.Classes;

namespace Blink.Classes
{
    class SpearClass
    {
	    
        public Texture2D spearText;
        public Rectangle spear;
        public Vector2 pos, velocity, SCREENSIZE;
        public int spearOrientation, Width , Height;
        //  Spear orientation values
        //      1       2        3
        //      0     Player     4
        //      7       6        5
        public PlayerClass spearOwner; 
        public Map m;
        public readonly Keys THROW_KEY = Keys.Q, ATTACK_KEY = Keys.Space;
        public readonly Buttons THROW_BUTTON = Buttons.RightShoulder, ATTACK_BUTTON = Buttons.A;

        //Attacking or being thrown sets this to true
        public Boolean isInUse = false;

        //Constructor for new spear
        //Takes inputs (Player, ScreenSize, Map)
        public SpearClass(PlayerClass spearOwner, Texture2D spearText, Vector2 ScreenSize, /*necesary?*/ Map m)
        {
            this.spearText = spearText;
            spear.Width = spearOwner.getPlayerRect().Width/16;
            spear.Height = spearOwner.getPlayerRect().Height;
            Width = spear.Width;
            Height = spear.Height;
            this.spearOwner = spearOwner;
            spearOrientation = 0;
            this.SCREENSIZE = ScreenSize;
            this.m = m;
        }

        internal void setOwner(PlayerClass player)
        {
            spearOwner = player;
        }

        //Manage inputs, check for a spear throw.
        public void Update(KeyboardState input, GamePadState padState)
        {
            //Reset variables to defualt state
            spear.Width = Width;
            spear.Height = Height;
            isInUse = false; 
            //REMOVE LATER - Refresh mehtod (ctrl-r): restore all players to life for testing (dont do this while you're ontop of a dead character or things will break)
            if (input.IsKeyDown(Keys.LeftControl) && input.IsKeyDown(Keys.R))
            {
                PlayerClass[] players = spearOwner.getPlayers();
                foreach (PlayerClass player in players)
                {
                    player.setDead(false);
                }
            }

            //Spear throw
            if (input.IsKeyDown(THROW_KEY) || padState.IsButtonDown(THROW_BUTTON))
            {
                throwSpear();
            }

            //Holding spear attacks
            if (input.IsKeyDown(ATTACK_KEY) || padState.IsButtonDown(ATTACK_BUTTON))
            {
                isInUse = true;
                if (spearOwner.getDirectionFacing() == 0)
                {
                    spearOrientation = 0;
                }
                else if (spearOwner.getDirectionFacing() == 1)
                {
                    spearOrientation = 4;
                }
                if (input.IsKeyDown(Keys.Up) || padState.IsButtonDown(Buttons.DPadUp))
                {
                    spearOrientation = 2;
                }
                else if (input.IsKeyDown(Keys.Down) || padState.IsButtonDown(Buttons.DPadDown))
                {
                    spearOrientation = 6;
                }
            }

            //Set the spear rectangle to fit the current orientation if the spear;
            if (isInUse)
            {
                int temp;
                switch (spearOrientation)
                {
                    case 0:
                        spear.X = spearOwner.getPlayerRect().X - spearOwner.getPlayerRect().Width;
                        spear.Y = spearOwner.getPlayerRect().Y + spearOwner.getPlayerRect().Height / 2;
                        break;
                    case 2:
                        spear.X = spearOwner.getPlayerRect().X + spearOwner.getPlayerRect().Width / 2;
                        spear.Y = spearOwner.getPlayerRect().Y - 3*spearOwner.getPlayerRect().Height / 4;
                        temp = spear.Width;
                        spear.Width = spear.Height;
                        spear.Height = temp;
                        break;
                    case 4:
                        spear.X = spearOwner.getPlayerRect().X + spearOwner.getPlayerRect().Width;
                        spear.Y = spearOwner.getPlayerRect().Y + spearOwner.getPlayerRect().Height / 2;
                        break;
                    case 6:
                        spear.X = spearOwner.getPlayerRect().X + spearOwner.getPlayerRect().Width / 2;
                        spear.Y = spearOwner.getPlayerRect().Y + 3*spearOwner.getPlayerRect().Height / 4;
                        temp = spear.Width;
                        spear.Width = spear.Height;
                        spear.Height = temp;
                        break;
                }
                playerCollision();
            }
        }

        //Check to see if the spear is colliding with a player
        private void playerCollision()
        {
            if (isInUse)
            {
                PlayerClass[] players = spearOwner.getPlayers();
                foreach (PlayerClass player in players)
                {
                    if (!player.Equals(spearOwner))
                    {
                        if (spear.X <= player.getPlayerRect().X + player.getPlayerRect().Width && spear.X + spear.Height >= player.getPlayerRect().X &&
                            spear.Y <= player.getPlayerRect().Y + player.getPlayerRect().Width && spear.Y + spear.Width >= player.getPlayerRect().Y)
                        {
                            player.setDead(true);
                        }
                    }
                }
            }
        }

        //Handle throw physics
        private void throwSpear()
        {

        }

        public void Draw(SpriteBatch sB)
        {
            Vector2 origin = new Vector2(0, 0);
            Vector2 screenPos = new Vector2(spearOwner.getPlayerRect().X, spearOwner.getPlayerRect().Y);
            float RotationAngle = 0;
            Texture2D drawnText = spearText;
            if (!isInUse && spearOwner!=null)
            {
                RotationAngle = (float)(MathHelper.Pi * .5);
                //screenPos.Y += spearOwner.getPlayerRect().Height;
                if(spearOwner.getDirectionFacing() == 1)
                    screenPos.X += spearOwner.getPlayerRect().Width+3*spear.Width;
                else if (spearOwner.getDirectionFacing() == 0)
                    screenPos.X -= spear.Width;
                //Drawing when the player is looping over
                if (spearOwner.getDirectionFacing() == 1 && spearOwner.getPlayerRect().X + spearOwner.getPlayerRect().Width > SCREENSIZE.X)
                {
                    screenPos.X = (spearOwner.getPlayerRect().X + spearOwner.getPlayerRect().Width) - SCREENSIZE.X + 3*spear.Width;
                    sB.Draw(drawnText, screenPos, null, Color.White, RotationAngle, origin, 1.0f, SpriteEffects.None, 0f);
                }
                else
                    sB.Draw(drawnText, screenPos, null, Color.White, RotationAngle, origin, 1.0f, SpriteEffects.None, 0f);
            }
            else if (isInUse && spearOwner != null)
            {
                switch (spearOrientation) { 
                    case 0:
                        RotationAngle = 0;
                        screenPos.X -= spearOwner.getPlayerRect().Width;
                        screenPos.Y += spearOwner.getPlayerRect().Height/2;
                        break;
                    case 2:
                        RotationAngle = (float)(MathHelper.Pi/2);
                        screenPos.X += spearOwner.getPlayerRect().Width / 2;
                        screenPos.Y -= 3*spearOwner.getPlayerRect().Height / 4;
                        break;
                    case 4:
                        RotationAngle = (float)(MathHelper.Pi);
                        screenPos.X += spearOwner.getPlayerRect().Width*2;
                        screenPos.Y += spearOwner.getPlayerRect().Height/2;
                        break;
                    case 6:
                        RotationAngle = (float)(3*MathHelper.Pi/2);
                        screenPos.X += spearOwner.getPlayerRect().Width/2;
                        screenPos.Y += 7*spearOwner.getPlayerRect().Height/4;
                        break;
                }
                sB.Draw(drawnText, screenPos, null, Color.White, RotationAngle, origin, 1.0f, SpriteEffects.None, 0f);
                
            }
        }
    }
}