using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using Common;
using FarseerGames.FarseerPhysics;


namespace chainknights
{
    public class Knight
    {
        Contraption contraption = new Contraption();

        public Knight()
        {

        }

        public void LoadContent(PhysicsSimulator physicsSimulator, ContentManager content)
        {
            contraption.Build("knight.xml", physicsSimulator, content);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            contraption.Draw(spriteBatch);
        }
    }
}
