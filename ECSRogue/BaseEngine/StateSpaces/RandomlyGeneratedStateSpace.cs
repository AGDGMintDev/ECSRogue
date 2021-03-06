﻿using ECSRogue.BaseEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ECSRogue.ECS;
using ECSRogue.ProceduralGeneration;
using ECSRogue.ProceduralGeneration.Interfaces;
using ECSRogue.ECS.Systems;
using ECSRogue.ECS.Components;

namespace ECSRogue.BaseEngine.StateSpaces
{
    public class RandomlyGeneratedStateSpace : IStateSpace
    {
        #region Components
        private StateSpaceComponents stateSpaceComponents;
        #endregion

        #region Dungeon Environment Variables
        private Texture2D player;
        private Vector2 dungeonDimensions;
        private DungeonTile[,] dungeonGrid = null;
        private Random random = new Random();
        private IGenerationAlgorithm dungeonAlgorithm;
        #endregion

        public RandomlyGeneratedStateSpace(IGenerationAlgorithm dungeonGeneration, int worldMin, int worldMax)
        {
            stateSpaceComponents = new StateSpaceComponents();
            dungeonAlgorithm = dungeonGeneration;
            dungeonDimensions = dungeonGeneration.GenerateDungeon(ref dungeonGrid, worldMin, worldMax, random);
        }

        #region Load Logic
        public void LoadLevel(ContentManager content, GraphicsDeviceManager graphics, Camera camera)
        {
            player = content.Load<Texture2D>("Sprites/Ball");
            dungeonAlgorithm.LoadDungeonContent(content);
            CreatePlayer();
            camera.AttachedToPlayer = true;
        }

        private void CreatePlayer()
        {
            Guid id = stateSpaceComponents.CreateEntity();
            stateSpaceComponents.Entities.Where(x => x.Id == id).First().ComponentFlags = ComponentMasks.Player;
            //Set Position
            int X = 0;
            int Y = 0;
            do
            {
                X = random.Next(0, (int)dungeonDimensions.X);
                Y = random.Next(0, (int)dungeonDimensions.Y);
            } while (dungeonGrid[X, Y].Type != TileType.TILE_FLOOR);
            stateSpaceComponents.PositionComponents[id] = new PositionComponent() { Position = new Vector2(X, Y) };
            //Set Health
            stateSpaceComponents.HealthComponents[id] = new HealthComponent() { CurrentHealth = 50, MaxHealth = 100, MinHealth = 0 };
            //Set Display
            stateSpaceComponents.DisplayComponents[id] = new DisplayComponent() { Color = Color.Red, Texture = player };
            //Set Sightradius
            stateSpaceComponents.SightRadiusComponents[id] = new SightRadiusComponent() { Radius = 12 };
        }
        #endregion

        #region Update Logic
        public IStateSpace UpdateLevel(GameTime gameTime, ContentManager content, GraphicsDeviceManager graphics, KeyboardState prevKeyboardState, MouseState prevMouseState, GamePadState prevGamepadState, Camera camera)
        {
            if(stateSpaceComponents.EntitiesToDelete.Count > 0)
            {
                foreach(Guid entity in stateSpaceComponents.EntitiesToDelete)
                {
                    stateSpaceComponents.DestroyEntity(entity);
                }
            }
            IStateSpace nextStateSpace = this;
            #region Debug changing levels
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !prevKeyboardState.IsKeyDown(Keys.Enter))
            {
                nextStateSpace = new RandomlyGeneratedStateSpace(new CaveGeneration(), 75, 125);
            }
            if(Mouse.GetState().RightButton == ButtonState.Pressed)
            {
                camera.Target = Vector2.Transform(Mouse.GetState().Position.ToVector2(),camera.GetInverseMatrix());
                camera.AttachedToPlayer = false;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.R))
            {
                camera.AttachedToPlayer = true;
            }
            #endregion
            InputMovementSystem.HandleDungeonMovement(stateSpaceComponents, graphics, gameTime, prevKeyboardState, prevMouseState, prevGamepadState, camera, dungeonGrid);
            TileRevealSystem.RevealTiles(dungeonGrid, dungeonDimensions, stateSpaceComponents);
            UpdateCamera(camera, gameTime);
            stateSpaceComponents.EntitiesToDelete.Clear();
            return nextStateSpace;
        }

        private void UpdateCamera(Camera camera, GameTime gameTime)
        {
            if(camera.AttachedToPlayer)
            {
                Entity playerId = stateSpaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Player) == ComponentMasks.Player).FirstOrDefault();
                if(playerId != null)
                {
                    camera.Target = new Vector2((int)stateSpaceComponents.PositionComponents[playerId.Id].Position.X * dungeonAlgorithm.GetCellsize() + stateSpaceComponents.DisplayComponents[playerId.Id].Texture.Bounds.Center.X,
                    (int)stateSpaceComponents.PositionComponents[playerId.Id].Position.Y * dungeonAlgorithm.GetCellsize() + stateSpaceComponents.DisplayComponents[playerId.Id].Texture.Bounds.Center.Y);
                }
            }
            if (Vector2.Distance(camera.Position, camera.Target) > 0)
            {
                float distance = Vector2.Distance(camera.Position, camera.Target);
                Vector2 direction = Vector2.Normalize(camera.Target - camera.Position);
                float velocity = distance * 2.5f;
                camera.Position += direction * velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
        }
        #endregion

        #region Draw Logic
        public void DrawLevel(SpriteBatch spriteBatch, GraphicsDeviceManager graphics, Camera camera)
        {
            dungeonAlgorithm.DrawTiles(camera, spriteBatch, dungeonGrid, dungeonDimensions);
            DisplaySystem.DrawDungeonEntities(stateSpaceComponents, camera, spriteBatch, dungeonAlgorithm.GetCellsize());
        }
        #endregion
    }
}
