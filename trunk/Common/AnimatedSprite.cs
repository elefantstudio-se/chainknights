using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using FarseerGames.FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Common
{
    public class AnimatedSprite : Sprite
    {
        public bool hide = false;

        public bool loop = true;
        public bool pause = false;

        public float frameTime = .05f;
        float elapsed = 0f;

        int numColumns = 1;
        int numRows = 1;
        public int frameWidth = 1;
        public int frameHeight = 1;
        public int numFrames = 0;
        int curFrame = 0;
        int numLoops = 0;

        List<Rectangle> frames = new List<Rectangle>();

        public delegate void AnimationDoneEventHandler(object sender, EventArgs e);

        public event AnimationDoneEventHandler AnimationDone;

        public AnimatedSprite(Texture2D texture, Body body, int frameWidth, int frameHeight) : base(texture, body)
        {
            this.frameWidth = frameWidth;
            this.frameHeight = frameHeight;

            Init();
        }

        public AnimatedSprite(Texture2D texture, Vector2 position, int frameWidth, int frameHeight)
            : base(texture, position)
        {
            this.frameWidth = frameWidth;
            this.frameHeight = frameHeight;

            Init();
        }

        public void Init()
        {
            frames.Clear();

            numColumns = Texture.Width / frameWidth;
            numRows = Texture.Height / frameHeight;

            numFrames = numColumns * numRows;

            int y = 0;
            for (int i = 0; i < numRows; i++)
            {
                int x = 0;

                for (int j = 0; j < numColumns; j++)
                {
                    Rectangle rect = new Rectangle(x, y, frameWidth, frameHeight);
                    frames.Add(rect);

                    x += frameWidth;
                }

                y += frameHeight;
            }
        }

        public void Reset()
        {
            hide = false;
            pause = false;

            elapsed = 0f;

            curFrame = 0;
            numLoops = 0;
        }

        public override void Update(float dtime)
        {
            if (pause || hide)
                return;

            elapsed += dtime;

            if (elapsed >= frameTime)
            {
                curFrame++;

                if (curFrame >= numFrames)
                {
                    numLoops++;

                    if (!loop && numLoops > 0)
                    {
                        pause = true;
                        curFrame--;

                        if (AnimationDone != null)
                            AnimationDone(this, new EventArgs());
                    }
                    else
                    {
                        curFrame = curFrame % numFrames;
                    }
                }
                else
                {
                    curFrame = curFrame % numFrames;
                }

                elapsed = 0;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (hide)
                return;

            Rectangle rect = frames[curFrame];
            if (Body != null)
            {
                spriteBatch.Draw(Texture, Body.Position, rect, Color.White, Body.Rotation,
                    new Vector2(frameWidth / 2, frameWidth / 2), 1f, SpriteEffects.None, 0);
            }
            else
            {
                spriteBatch.Draw(Texture, Position, rect, Color.White, Rotation,
                    new Vector2(frameWidth / 2, frameHeight / 2), 1f, SpriteEffects.None, 0);
            }
        }
    }
}
