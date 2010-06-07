using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;

using Common;



namespace chainknights
{
    public class Knight
    {
        PhysicsSimulator physicsSimulator;

        Contraption ContraptionKnight = new Contraption();

        Texture2D TextureLeaping;
        Texture2D TextureStanding;
        Texture2D TextureSquatting;
        Texture2D TextureWalking1;
        Texture2D TextureWalking2;
        Texture2D TextureWalking3;

        Body BodyTorso;

        Geom GeomLegsStanding;
        Geom GeomLegsSquatting;
        
        Sprite SpriteLeg;

        enum LegStates
        {
            Standing,
            Squatting,
            Leaping,
            Walking1,
            Walking2,
            Walking3
        }
        LegStates LegStateOld = LegStates.Standing;
        LegStates LegState = LegStates.Standing;

        bool IsGrounded = false;
        bool IsLandable = true;
        bool IsWalking = false;

        Timer timerLandable;
        Timer timerWalking;

        public Keys KeyUp = Keys.W;
        public Keys KeyLeft = Keys.A;
        public Keys KeyRight = Keys.D;
        public Keys KeyDown = Keys.S;

        public Vector2 Tangent = new Vector2();
        public Vector2 Normal = new Vector2();

        public float JumpForce = 150000f;
        public float WalkForce = 5000f;
        public float GlideForce = 5000f;
        public float HopForce = 25000f;


        public Knight()
        {
            timerLandable = new Timer();
            timerLandable.Interval = 50;
            timerLandable.Elapsed += new ElapsedEventHandler(timerLandable_Elapsed);

            timerWalking = new Timer();
            timerWalking.Interval = 150;
            timerWalking.Elapsed += new ElapsedEventHandler(timerWalking_Elapsed);
        }

        public void LoadContent(PhysicsSimulator physicsSimulator, ContentManager content)
        {
            this.physicsSimulator = physicsSimulator;

            ContraptionKnight.Build("knight.xml", physicsSimulator, content);

            SpriteLeg = ContraptionKnight.mapSprites["t_legs"];

            BodyTorso = ContraptionKnight.mapBody["b_body"];
            JointFactory.Instance.CreateFixedAngleLimitJoint(physicsSimulator, BodyTorso, 0f, 0f);

            GeomLegsStanding = ContraptionKnight.mapGeom["g_legs_standing"];
            GeomLegsSquatting = ContraptionKnight.mapGeom["g_legs_squatting"];

            GeomLegsStanding.OnCollision += OnCollision;
            GeomLegsStanding.OnSeparation += OnSeperation;

            GeomLegsSquatting.OnCollision += OnCollision;
            GeomLegsSquatting.OnSeparation += OnSeperation;

            TextureLeaping = content.Load<Texture2D>("Media/ChainKnights_LegsLeaping_Full");
            TextureStanding = content.Load<Texture2D>("Media/ChainKnights_LegsStanding_Full");
            TextureSquatting = content.Load<Texture2D>("Media/ChainKnights_LegsSquatting_Full");
            TextureWalking1 = content.Load<Texture2D>("Media/ChainKnights_LegsWalking01_Full");
            TextureWalking2 = content.Load<Texture2D>("Media/ChainKnights_LegsWalking02_Full");
            TextureWalking3 = content.Load<Texture2D>("Media/ChainKnights_LegsWalking03_Full");
        }

        void timerWalking_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (IsGrounded)
            {
                if (LegState == LegStates.Walking1) LegState = LegStates.Walking2;
                else if (LegState == LegStates.Walking2) LegState = LegStates.Walking3;
                else if (LegState == LegStates.Walking3) LegState = LegStates.Walking1;
                else LegState = LegStates.Walking1;
            }
        }

        void timerLandable_Elapsed(object sender, ElapsedEventArgs e)
        {
            IsLandable = true;
            timerLandable.Stop();
        }

        private bool OnCollision(Geom geom1, Geom geom2, ContactList contactList)
        {
            bool result = true;

            IsGrounded = true;

            if (geom1 == GeomLegsStanding && IsLandable && !IsWalking)
            {
                LegState = LegStates.Standing;
            }

            return result;
        }

        private void OnSeperation(Geom geom1, Geom geom2)
        {
            IsGrounded = false;
        }

        public void Squat()
        {
            LegState = LegStates.Squatting;
            physicsSimulator.Remove(GeomLegsStanding);
        }

        public void Stand()
        {
            LegState = LegStates.Standing;
            physicsSimulator.Add(GeomLegsStanding);
        }

        public void Jump()
        {
            LegState = LegStates.Leaping;
            BodyTorso.ApplyForce(Normal * JumpForce * -1);
            IsLandable = false;
            timerLandable.Start();
        }

        public void Hop()
        {
            LegState = LegStates.Leaping;
            BodyTorso.ApplyForce(Normal * HopForce * -1);
            IsLandable = false;
            timerLandable.Start();
        }

        public void HandleInput(InputHelper input)
        {
            if (IsGrounded)
            {
                if (input.CurrentKeyboardState.IsKeyDown(KeyUp) && IsLandable)
                {
                    Jump();
                }
                else if (input.CurrentKeyboardState.IsKeyDown(KeyDown))
                {
                    Squat();
                }
                else if(input.LastKeyboardState.IsKeyDown(KeyDown) && input.CurrentKeyboardState.IsKeyUp(KeyDown))
                {
                    Stand();
                }

                if (input.CurrentKeyboardState.IsKeyDown(KeyLeft))
                {
                    IsWalking = true;
                    timerWalking.Start();
                    BodyTorso.ApplyForce(Tangent * WalkForce * -1);
                }
                else if (input.CurrentKeyboardState.IsKeyDown(KeyRight))
                {
                    IsWalking = true;
                    timerWalking.Start();
                    BodyTorso.ApplyForce(Tangent * WalkForce);
                }
                else
                {
                    IsWalking = false;
                    timerWalking.Stop();
                }

            }
            else
            {
                if (input.CurrentKeyboardState.IsKeyDown(KeyLeft))
                {
                    BodyTorso.ApplyForce(Tangent * GlideForce * -1);
                }
                if (input.CurrentKeyboardState.IsKeyDown(KeyRight))
                {
                    BodyTorso.ApplyForce(Tangent * GlideForce);
                }
            }
        }

        public void Update(float dtime, InputHelper input)
        {
            HandleInput(input);

            Tangent.X = (float)Math.Cos(BodyTorso.Rotation);
            Tangent.Y = (float)Math.Sin(BodyTorso.Rotation);
            Normal.X = -Tangent.Y;
            Normal.Y = Tangent.X;

            if (LegState == LegStates.Leaping && BodyTorso.LinearVelocity.Y > 0 && IsLandable)
            {
                LegState = LegStates.Squatting;
            }

            if (LegState != LegStateOld)
            {
                switch (LegState)
                {
                    case LegStates.Standing:
                        SpriteLeg.Texture = TextureStanding;
                        break;
                    case LegStates.Squatting:
                        SpriteLeg.Texture = TextureSquatting;
                        break;
                    case LegStates.Leaping:
                        SpriteLeg.Texture = TextureLeaping;
                        break;
                    case LegStates.Walking1:
                        SpriteLeg.Texture = TextureWalking1;
                        break;
                    case LegStates.Walking2:
                        SpriteLeg.Texture = TextureWalking2;
                        break;
                    case LegStates.Walking3:
                        SpriteLeg.Texture = TextureWalking3;
                        break;
                    default:
                        //SpriteLeg.Texture = TextureStanding;
                        break;
                }
            }
            LegStateOld = LegState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            ContraptionKnight.Draw(spriteBatch);
        }
    }
}
