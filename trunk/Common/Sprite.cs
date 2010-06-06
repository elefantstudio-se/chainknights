using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Factories;

namespace Common
{
    public class Sprite
    {
        public Body Body
        {
            get;
            set;
        }

        public Texture2D Texture
        {
            get;
            set;
        }

        public Vector2 Origin
        {
            get;
            set;
        }

        public Vector2 Position
        {
            get;
            set;
        }

        public float Rotation
        {
            get;
            set;
        }

        public float Scale
        {
            get;
            set;
        }

        public Vector2 Offset
        {
            get;
            set;
        }

        public Color Color
        {
            get;
            set;
        }

        public byte Alpha
        {
            get;
            set;
        }

        public Sprite(Texture2D texture, Vector2 position)
        {
            this.Texture = texture;
            this.Position = position;
            this.Origin = new Vector2(texture.Width / 2, texture.Height / 2);
            this.Rotation = 0.0f;
            this.Body = null;
            this.Scale = 1.0f;
            this.Offset = Vector2.Zero;
            this.Color = Color.White;
            this.Alpha = 255;
        }

        public Sprite(Texture2D texture, Vector2 position, Vector2 origin)
        {
            this.Texture = texture;
            this.Position = position;
            this.Origin = origin;
            this.Rotation = 0.0f;
            this.Body = null;
            this.Scale = 1.0f;
            this.Color = Color.White;
            this.Alpha = 255;
        }

        public Sprite(Texture2D texture, Body body)
        {
           this.Texture = texture;
           this.Body = body;
           //this.Position = body.Position;
           this.Origin = new Vector2(texture.Width / 2, texture.Height / 2);
           this.Rotation = 0.0f;
           this.Scale = 1.0f;
           this.Color = Color.White;
           this.Alpha = 255;
        }

        public virtual void Update(float dtime)
        {

        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {

            if (Body != null)
            {
                spriteBatch.Draw(Texture, Body.Position, null, Color, Body.Rotation,
                    Origin, 1f, SpriteEffects.None, 0);
            }
            else
            {
                //TCC: The orgin is the offset retard
                spriteBatch.Draw(Texture, Position, null, Color, Rotation, 
                    Origin, Scale, SpriteEffects.None, 0);
            }
        }

        public void RotateToMouse(Vector2 mouse)
        {
            
            Vector2 v1 = Vector2.Normalize(Position);
            Vector2 v2 = Vector2.Normalize(mouse);

            Rotation = (float)Math.Acos(Vector2.Dot(v1, v2));
            if (Vector3.Cross(new Vector3(v1.X, v1.Y, 0f), new Vector3(v2.X, v2.Y, 0f)).Z < 0)
            {
                Rotation *= -1;
            }
            
            //Vector2 v1 = new Vector2(0, -1);
            //Vector2 v2 = mouse - position;
            //rotation = SignedAngle(v1, v2);
            //rotation = (float)Math.Atan(mouse.Y / mouse.X);
        }

        protected float SignedAngle(Vector2 v1, Vector2 v2)
        {
            float perpDot = v1.X * v2.Y - v1.Y * v2.X;
            return (float)Math.Atan2(perpDot, Vector2.Dot(v1, v2));
        }

        public Rectangle Rect
        {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height); 
            }
        }
    }
}
