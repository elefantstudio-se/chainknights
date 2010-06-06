using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Factories;

using Microsoft.Xna.Framework;
using Common;
using GLEED2D;

namespace Common
{
    public class Contraption
    {
        public Dictionary<string, Body> mapBody = new Dictionary<string, Body>();
        public Dictionary<string, Geom> mapGeom = new Dictionary<string, Geom>();
        public Dictionary<string, Joint> mapJoint = new Dictionary<string, Joint>();
        public Dictionary<string, Sprite> mapSprites = new Dictionary<string, Sprite>();
        public Dictionary<string, Texture2D> mapTextures = new Dictionary<string, Texture2D>();

        public Dictionary<string, Body> mapBlockBodies = new Dictionary<string, Body>();
        public Dictionary<string, Geom> mapBlockGeoms = new Dictionary<string, Geom>();
        public Dictionary<string, List<Joint>> mapBlockJoints = new Dictionary<string, List<Joint>>();

        PhysicsSimulator physicsSimulator;

        private Vector2 startPosition = new Vector2();

        private Body origin = null;

        public void Build(string filename, PhysicsSimulator physicsSimulator, ContentManager content, Vector2 position)
        {
            startPosition = position;

            Build(filename, physicsSimulator, content);


        }

        public void Build(string filename, PhysicsSimulator physicsSimulator, ContentManager content)
        {
            this.physicsSimulator = physicsSimulator;

            Level level = Level.FromFile(filename, content);

            Layer layerBody = level.getLayerByName("Body");
            Layer layerGeom = level.getLayerByName("Geom");
            Layer layerJoint = level.getLayerByName("Joints");
            Layer layerTexture = level.getLayerByName("Textures");
            Layer layerBlocks = level.getLayerByName("Blocks");

            foreach (Item item in layerBody.Items)
            {
                float mass = 1f;
                Body body = null;


                if (item.CustomProperties.ContainsKey("Mass") && item.CustomProperties["Mass"].value is string)
                {
                    string strMass = item.CustomProperties["Mass"].value as string;

                    if (!float.TryParse(strMass, out mass))
                    {
                        mass = 1f;
                    }
                }


                if (item.GetType() == typeof(CircleItem))
                {
                    CircleItem circle = item as CircleItem;
                    float radius = circle.Radius;

                    body = BodyFactory.Instance.CreateCircleBody(physicsSimulator, radius, mass);

                    body.Position = item.Position;
                }
                else if (item.GetType() == typeof(RectangleItem))
                {
                    RectangleItem rect = item as RectangleItem;

                    float width = rect.Width;
                    float height = rect.Height;

                    body = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, width, height, mass);

                    body.Position = item.Position + new Vector2(rect.Width / 2, rect.Height / 2);
                }

                if (body != null)
                {
                    if (item.CustomProperties.ContainsKey("origin"))
                    {
                        origin = body;
                    }

                    if (item.CustomProperties.ContainsKey("Static"))
                    {
                        body.IsStatic = true;
                    }

                    mapBody.Add(item.Name, body);
                }
            }

            foreach (Item item in layerGeom.Items)
            {
                Geom geom = null;
                Body body = null;
                int numEdges = 25;

                if (item.CustomProperties.ContainsKey("Body") && item.CustomProperties["Body"].value is Item)
                {
                    Item bodyItem = item.CustomProperties["Body"].value as Item;

                    if (mapBody.ContainsKey(bodyItem.Name))
                    {
                        body = mapBody[bodyItem.Name];

                        Vector2 offset;

                        if (item.GetType() == typeof(CircleItem))
                        {
                            CircleItem circle = item as CircleItem;

                            float radius = circle.Radius;

                            offset = item.Position - body.Position;

                            //geom = GeomFactory.Instance.CreateCircleGeom(physicsSimulator, body, radius, numEdges);
                            geom = GeomFactory.Instance.CreateCircleGeom(physicsSimulator, body, radius, numEdges, offset, 0f);
                        }
                        else if (item.GetType() == typeof(RectangleItem))
                        {
                            RectangleItem rect = item as RectangleItem;

                            float width = rect.Width;
                            float height = rect.Height;

                            offset = (item.Position + new Vector2(rect.Width / 2, rect.Height / 2)) - body.Position;

                            //geom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, body, width, height, numEdges);
                            geom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, body, width, height, offset, 0f);
                        }

                        int collisionGroup = 10;
                        
                        if (item.CustomProperties.ContainsKey("CollisionGroup") && item.CustomProperties["CollisionGroup"].value is string)
                        {
                            string strCollisionGroup = item.CustomProperties["CollisionGroup"].value as string;
                            
                            int.TryParse(strCollisionGroup, out collisionGroup);
                        }

                        if (geom != null)
                        {
                            geom.CollisionGroup = collisionGroup;

                            geom.RestitutionCoefficient = 0f;
                            geom.FrictionCoefficient = 1f;

                            mapGeom.Add(item.Name, geom);
                        }
                    }
                }
                else
                {
                    if (item.GetType() == typeof(PathItem))
                    {
                        PathItem path = item as PathItem;

                        Vertices vertices = new Vertices(path.LocalPoints);
                        Body staticBody = new Body();
                        staticBody.IsStatic = true;
                        geom = GeomFactory.Instance.CreatePolygonGeom(physicsSimulator, staticBody, vertices, 0);
                    }
                }
            }

            foreach (Item item in layerJoint.Items)
            {
                Body body1 = null;
                Body body2 = null;
                Joint joint = null;

                if (item.CustomProperties.ContainsKey("Body1") && item.CustomProperties["Body1"].value is Item)
                {
                    Item body1Item = item.CustomProperties["Body1"].value as Item;
                        
                    if(mapBody.ContainsKey(body1Item.Name))
                    {
                        body1 = mapBody[body1Item.Name];
                    }
                }

                if (item.CustomProperties.ContainsKey("Body2") && item.CustomProperties["Body2"].value is Item)
                {
                    Item body2Item = item.CustomProperties["Body2"].value as Item;

                    if (mapBody.ContainsKey(body2Item.Name))
                    {
                        body2 = mapBody[body2Item.Name];
                    }
                }

               

                if (body1 != null && body2 != null)
                {
                    if (item.CustomProperties.ContainsKey("Type") && item.CustomProperties["Type"].value is string)
                    {
                        string strType = item.CustomProperties["Type"].value as string;

                        switch (strType)
                        {
                            case "Static":
                                joint = JointFactory.Instance.CreateAngleJoint(physicsSimulator, body1, body2);
                                JointFactory.Instance.CreateRevoluteJoint(physicsSimulator, body1, body2, item.Position);
                                break;
                            case "Slider":
                                joint = JointFactory.Instance.CreateAngleJoint(physicsSimulator, body1, body2);
                                JointFactory.Instance.CreateRevoluteJoint(physicsSimulator, body1, body2, item.Position);
                                //joint = JointFactory.Instance.CreateSliderJoint(physicsSimulator, body1, item.Position, body2, item.Position, 0f, 10f);
                                //JointFactory.Instance.CreateSliderJoint(physicsSimulator, body2, item.Position, body2, item.Position, 0, 0);
                                break;
                            case "AngleLimit":
                                float min = 0;
                                float max = 0;

                                if (item.CustomProperties.ContainsKey("Min") && item.CustomProperties["Min"].value is string)
                                {
                                    string strMass = item.CustomProperties["Min"].value as string;

                                    if (!float.TryParse(strMass, out min))
                                    {
                                        min = 0f;
                                    }
                                }

                                if (item.CustomProperties.ContainsKey("Max") && item.CustomProperties["Max"].value is string)
                                {
                                    string strMass = item.CustomProperties["Max"].value as string;

                                    if (!float.TryParse(strMass, out max))
                                    {
                                        max = 0f;
                                    }
                                }

                                joint = JointFactory.Instance.CreateAngleLimitJoint(physicsSimulator, body1, body2, min, max);

                                JointFactory.Instance.CreateRevoluteJoint(physicsSimulator, body1, body2, item.Position);
                                break;
                            default:
                                joint = JointFactory.Instance.CreateRevoluteJoint(physicsSimulator, body1, body2, item.Position);
                                break;
                        }

                    }
                    else
                    {
                        joint = JointFactory.Instance.CreateRevoluteJoint(physicsSimulator, body1, body2, item.Position);
                    }


                }
                else
                {
                    if (item.CustomProperties.ContainsKey("Body") && item.CustomProperties["Body"].value is Item)
                    {
                        Item body1Item = item.CustomProperties["Body"].value as Item;

                        if (mapBody.ContainsKey(body1Item.Name))
                        {
                            body1 = mapBody[body1Item.Name];

                            joint = JointFactory.Instance.CreateFixedRevoluteJoint(physicsSimulator, body1, item.Position);
                        }
                    }
                }

                if (joint != null)
                {
                    joint.BiasFactor = .01f;
                    mapJoint.Add(item.Name, joint);
                }
            }

            foreach (Item item in layerTexture.Items)
            {
                if (item.GetType() == typeof(TextureItem))
                {
                    TextureItem textureItem = item as TextureItem;

                    if (item.CustomProperties.ContainsKey("Body") && item.CustomProperties["Body"].value is Item)
                    {
                        Item bodyItem = item.CustomProperties["Body"].value as Item;

                        if (mapBody.ContainsKey(bodyItem.Name))
                        {
                            Body body = mapBody[bodyItem.Name];

                            Texture2D texture = content.Load<Texture2D>(textureItem.asset_name);

                            Sprite sprite = new Sprite(texture, body);
                            sprite.Origin = body.Position - item.Position + new Vector2(texture.Width/2.0f, texture.Height/2.0f);

                            mapSprites.Add(item.Name, sprite);
                        }

                    }
                    else
                    {
                        Texture2D texture = content.Load<Texture2D>(textureItem.asset_name);

                        Sprite sprite = new Sprite(texture, item.Position);

                        mapSprites.Add(item.Name, sprite);
                    }
                }
            }

            if (layerBlocks != null)
            {
                foreach (Item item in layerBlocks.Items)
                {
                    if (item.GetType() == typeof(TextureItem))
                    {
                        TextureItem textureItem = item as TextureItem;
                        
                        Texture2D texture = content.Load<Texture2D>(textureItem.asset_name);

                        Body body = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, texture.Width, texture.Height, 1f);
                        body.Position = item.Position;
                        mapBlockBodies.Add(item.Name, body);

                        Geom geom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, body, texture.Width, texture.Height);
                        geom.CollisionGroup = 0;
                        mapBlockGeoms.Add(item.Name, geom);

                        Joint revJoint = JointFactory.Instance.CreateFixedRevoluteJoint(physicsSimulator, body, item.Position);
                        Joint angJoint = JointFactory.Instance.CreateFixedAngleLimitJoint(physicsSimulator, body, 0, 0);

                        List<Joint> joints = new List<Joint>();
                        joints.Add(revJoint);
                        joints.Add(angJoint);
                        mapBlockJoints.Add(item.Name, joints);

                        geom.Tag = joints;

                        Sprite sprite = new Sprite(texture, body);
                        
                        mapSprites.Add(item.Name, sprite);
                    }
                }
            }

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (KeyValuePair<string, Sprite> kvp in mapSprites)
            {
                kvp.Value.Draw(spriteBatch);
            }
        }

        public void ReleaseJoints()
        {
            foreach (KeyValuePair<string, List<Joint>> kvp in mapBlockJoints)
            {
                foreach (Joint joint in kvp.Value)
                {
                    physicsSimulator.Remove(joint);
                }
            }
        }
    }
}
