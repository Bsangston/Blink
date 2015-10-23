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
        private int JUMP = 30, TILEWIDTH = 32, MARGIN = 0;
        public int spearOrientation, Width , Height;
        private float GRAVITY = 1.6f, TERMINAL_V = 30, SPEED = 1.2f, GROUNDSPEED = 1.2f, ICESPEED = 0.4f, ACC_CAP = 16;
        private float curFriction = 2.4f, airFriction = .2f, groundFriction = 2.4f, iceFriction = .2f;
        PlayerClass thrownBy = null;
        PlayerClass[] players;
        SpearClass[] spears;
        KeyboardState oldState;
        Vector2 staticPos = new Vector2(0, 0);
        //  Spear orientation values
        //      1       2        3
        //      0     Player     4
        //      7       6        5
        public PlayerClass spearOwner; 
        public Map m;
        public readonly Keys THROW_KEY = Keys.Q, ATTACK_KEY = Keys.Space;
        public readonly Buttons THROW_BUTTON = Buttons.RightShoulder, ATTACK_BUTTON = Buttons.A;

		public int inUseTimer = 10, coolDown = 10;
        //Attacking or being thrown sets this to true
        public Boolean isInUse = false, atRest = true;
        public Boolean attached = true, throwing = false, attackDown = false;
        //Constructor for new spear
        //Takes inputs (Player, ScreenSize, Map)
        public SpearClass(PlayerClass spearOwner, Texture2D spearText, Vector2 ScreenSize, /*necesary?*/ Map m, PlayerClass[] players, SpearClass[] spears)
        {
            this.spearText = spearText;
            this.players = players;
            this.spears = spears;
            spear.Width = spearText.Width;
            spear.Height = spearText.Height;
            Width = spear.Width;
            Height = spear.Height;
            velocity.X = 0;
            velocity.Y = 0;
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
            KeyboardState newState = input;
            spear.Width = Width;
            spear.Height = Height;
			if (isInUse)
			{
				if(inUseTimer > 0)
				{
					inUseTimer -= 1;
				}
				else
				{
            isInUse = false; 
					inUseTimer = 10;
                    coolDown = 20;
				}
			}
            if (!isInUse && coolDown > 0)
            {
                coolDown -= 1;
            }
            //REMOVE LATER - Refresh mehtod (ctrl-r): restore all players to life for testing (dont do this while you're ontop of a dead character or things will break)
            if (input.IsKeyDown(Keys.LeftControl) && input.IsKeyDown(Keys.R))
            {
                PlayerClass[] players = spearOwner.getPlayers();
                foreach (PlayerClass player in players)
                {
                    player.setDead(false,null,"wtf");
                }
            }

            //Spear throw
            if ((input.IsKeyDown(THROW_KEY) || padState.IsButtonDown(THROW_BUTTON)) && oldState != newState && attached && !spearOwner.dead && !isInUse && coolDown <= 0)
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
                thrownBy = spearOwner;
                throwSpear();
            }



            //Holding spear attacks
            if ((input.IsKeyDown(ATTACK_KEY) || padState.IsButtonDown(ATTACK_BUTTON)) && !attackDown && attached && !spearOwner.dead && !isInUse && coolDown <= 0)
            {
                attackDown = true;
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
            else if ((input.IsKeyUp(ATTACK_KEY) && padState.IsButtonUp(ATTACK_BUTTON)))
                attackDown = false;

            //Set the spear rectangle to fit the current orientation if the spear;
            if (isInUse)
            {
                int temp;
                switch (spearOrientation)
                {
                    case 0:
                        spear.X = spearOwner.getPlayerRect().X - spear.Width + spearOwner.getPlayerRect().Width;
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
                        spear.X = spearOwner.getPlayerRect().X;
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
            }
            playerCollision();
            if(!attached)
            {
                mapCollision();
            }
            if (!atRest)
            {
                throwUpdate();
            }
            if (!atRest && velocity.Y < TERMINAL_V)
            {
                spear.Y += (int)GRAVITY;
            }
            oldState = newState;
        }

        //Check to see if the spear is colliding with a player
        private void playerCollision()
        {
            if (isInUse && attached)
            {
                PlayerClass[] players = spearOwner.getPlayers();
                foreach (PlayerClass player in players)
                {
                    if (!player.Equals(spearOwner) && !player.dead)
                    {
                        if (player.getPlayerRect().Intersects(this.spear))
                        {
                            player.setDead(true, this.spearOwner, "SPEAR");
                        }
                        else if(this.spear.X <= 0)
                        {
                            Rectangle tempRect = new Rectangle((int)(spear.X + SCREENSIZE.X), (int)spear.Y, spear.Width, spear.Height);
                            if (player.getPlayerRect().Intersects(tempRect))
                            {
                                player.setDead(true, this.spearOwner, "SPEAR");
                            }
                        }
                        else if(this.spear.X >= SCREENSIZE.X - this.spear.Width)
                    {
                            Rectangle tempRect = new Rectangle((int)(spear.X - SCREENSIZE.X), (int)spear.Y, spear.Width, spear.Height);
                            if (player.getPlayerRect().Intersects(tempRect))
                        {
                                player.setDead(true, this.spearOwner, "SPEAR");
                            }
                        }
                    }
                }
            }
            if (!attached)
            {
                PlayerClass[] checkedPlayers = new PlayerClass[4];
                //PlayerClass[] players = spearOwner.getPlayers();
                //Make sure we're not checking a collision that wasn't already checked
                foreach (PlayerClass p in players)
                {
                    Boolean alreadyChecked = false;
                  /**  foreach (PlayerClass checkedPlayer in checkedPlayers)
                    {
                        if (checkedPlayer == p)
                            alreadyChecked = true;
                    } **/
                    if (!attached && !alreadyChecked && !throwing && atRest)
                    {
                        Rectangle inter = Rectangle.Intersect(p.getPlayerRect(), new Rectangle((int)spear.X,(int)spear.Y, spear.Width, spear.Height));
                        if (inter.Width > 0 && inter.Height > 0 && !p.hasSpear && !throwing)
                        {
                            attached = true;
                            setOwner(p);
                            p.hasSpear = true;
                        }
                    }
                    if (!atRest)
                    {
                        Rectangle inter = Rectangle.Intersect(p.getPlayerRect(), new Rectangle((int)spear.X, (int)spear.Y, spear.Width, spear.Height));
                        if (inter.Width > 0 && inter.Height > 0 && spearOwner != p && !p.dead)
                        {
                            p.setDead(true, thrownBy, "SPEAR");
                }
            }
                }
            }

        }

        private void mapCollision()
        {
            float testX = spear.X + velocity.X;
            float testY = spear.Y + velocity.Y;

            int d = 0, r = 0;
            if (velocity.X > 0)
                r = 1;
            else if (velocity.X < 0)
                r = -1;
            if (velocity.Y > 0)
                d = 1;
            else if (velocity.Y < 0)
                d = -1;

            Boolean[] collisions = m.collides(new Vector2(testX, testY), new Vector2(spear.X, spear.Y), d, r, new Vector2(spear.Width, spear.Height));
            if (collisions[0] || collisions[1] || collisions[2])
            {
                spear.X = (int)testX;
                spear.Y = (int)testY;
                velocity.X = 0;
                velocity.Y = 0;
                atRest = true;
                throwing = false;
        }
        }

        //Handle throw physics
        private void throwSpear()
        {
            attached = false;
            throwing = true;
            isInUse = false;
            spearOwner.hasSpear = false;
            velocity.X = spearOwner.velocity.X;
            spear.X = spearOwner.getPlayerRect().X;
            spear.Y = spearOwner.getPlayerRect().Y;
            atRest = false;
        }

        private void throwUpdate()
        {
            if(spear.X > spear.Height) 
            {
                spear.X -= (int)SCREENSIZE.X;
            }

            if (spear.X < spear.Height)
            {
                spear.X += (int)SCREENSIZE.X;
            }

            if (spear.Y > spear.Width)
            {
                spear.Y -= (int)SCREENSIZE.Y;
            }

            if (spear.Y < spear.Width)
            {
                spear.Y += (int)SCREENSIZE.Y;
            }
         
            switch (spearOrientation)
            {
                case 0:
                    velocity.X = -15;
                    spear.X += (int)velocity.X;
                    break;
                case 2:
                    velocity.Y = -15;
                    spear.Y += (int)velocity.Y;
                    break;
                case 4:
                    velocity.X = 15;
                    spear.X += (int)velocity.X;
                    break;
                case 6:
                    velocity.Y = 15;
                    spear.Y += (int)velocity.Y;
                    break;
            }
            if (spearOwner != null)
            {
                Rectangle inter = Rectangle.Intersect(spearOwner.getPlayerRect(), new Rectangle((int)spear.X, (int)spear.Y, spear.Width, spear.Height));
                if (inter.Width == 0 && inter.Height == 0)
                {
                    throwing = false;
            spearOwner.setSpear(null);
            this.setOwner(null);
                }
            }
        }

        public void Draw(SpriteBatch sB)
        {
            Vector2 origin = new Vector2(0, 8);
            float RotationAngle = 0;
            Texture2D drawnText = spearText;

            if (spearOwner == null)
            {
                Vector2 screenPos = new Vector2(spear.X, spear.Y);
                RotationAngle = (float)(MathHelper.Pi * .5);
                switch (spearOrientation)
                {
                    case 0:
                        RotationAngle = 0;
                        break;
                    case 2:
                        RotationAngle = (float)(MathHelper.Pi / 2);
                        break;
                    case 4:
                        RotationAngle = (float)(MathHelper.Pi);
                        break;
                    case 6:
                        RotationAngle = (float)(3 * MathHelper.Pi / 2);
                        break;
                }
                sB.Draw(drawnText, screenPos, null ,  Color.White, RotationAngle, origin, 1.0f, SpriteEffects.None, 0f);
            }

            else if (!isInUse && spearOwner!=null)
            {
                Vector2 screenPos = new Vector2(spearOwner.getPlayerRect().X, spearOwner.getPlayerRect().Y);
                RotationAngle = (float)(MathHelper.Pi * .5);
                //screenPos.Y += spearOwner.getPlayerRect().Height;
                if(spearOwner.getDirectionFacing() == 1)
                    screenPos.X += spearOwner.getPlayerRect().Width+spear.Width/13;
                else if (spearOwner.getDirectionFacing() == 0)
                    screenPos.X -= spear.Width/16;
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
                Vector2 screenPos = new Vector2(spearOwner.getPlayerRect().X, spearOwner.getPlayerRect().Y);
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

        public void reset(PlayerClass p)
        {
            setOwner(p);
            Width = spear.Width;
            Height = spear.Height;
            velocity.X = 0;
            velocity.Y = 0;
            spearOrientation = 0;
            isInUse = false;
            throwing = false;
            attached = true;
            atRest = true;
            spear = spearOwner.getPlayerRect();
        }
    }
}
