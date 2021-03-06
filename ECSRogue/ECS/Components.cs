﻿using ECSRogue.ECS.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS
{
    [Flags]
    public enum Component : ulong
    {
        NONE = 0,
        COMPONENT_POSITION = 1 << 0,
        COMPONENT_VELOCITY = 1 << 1,
        COMPONENT_DISPLAY = 1 << 2,
        COMPONENT_LABEL = 1 << 3,
        COMPONENT_ANIMATION = 1 << 4,
        COMPONENT_HEALTH = 1 << 5,
        COMPONENT_INPUTMOVEMENT = 1 << 6,
        COMPONENT_SIGHTRADIUS = 1 << 7
    }

    public struct ComponentMasks
    {
        public const Component Player = Component.COMPONENT_POSITION | Component.COMPONENT_HEALTH | Component.COMPONENT_DISPLAY | Component.COMPONENT_SIGHTRADIUS | Component.COMPONENT_INPUTMOVEMENT;
        public const Component InputMoveable = Component.COMPONENT_POSITION | Component.COMPONENT_INPUTMOVEMENT;
        public const Component Drawable = Component.COMPONENT_DISPLAY | Component.COMPONENT_POSITION;
    }

    public class GameComponents
    {

    }

    public class StateSpaceComponents
    {
        public List<Entity> Entities { get; private set; }
        public List<Guid> EntitiesToDelete { get; private set; }
        public Dictionary<Guid, PositionComponent> PositionComponents { get; private set; }
        public Dictionary<Guid, VelocityComponent> VelocityComponents { get; private set; }
        public Dictionary<Guid, DisplayComponent> DisplayComponents { get; private set; }
        public Dictionary<Guid, HealthComponent> HealthComponents { get; private set; }
        public Dictionary<Guid, AnimationComponent> AnimationComponents { get; private set; }
        public Dictionary<Guid, SightRadiusComponent> SightRadiusComponents { get; private set; }
        public Dictionary<Guid, LabelComponent> LabelComponents { get; private set; }

        public StateSpaceComponents()
        {
            EntitiesToDelete = new List<Guid>();
            Entities = new List<Entity>();
            PositionComponents = new Dictionary<Guid, PositionComponent>();
            VelocityComponents = new Dictionary<Guid, VelocityComponent>();
            DisplayComponents = new Dictionary<Guid, DisplayComponent>();
            HealthComponents = new Dictionary<Guid, HealthComponent>();
            AnimationComponents = new Dictionary<Guid, AnimationComponent>();
            LabelComponents = new Dictionary<Guid, LabelComponent>();
            SightRadiusComponents = new Dictionary<Guid, SightRadiusComponent>();
        }

        public Guid CreateEntity()
        {
            Entity newEntity = new Entity();
            this.Entities.Add(newEntity);
            return newEntity.Id;
        }

        public void DestroyEntity(Guid id)
        {
            Entity removal = this.Entities.Where(x => x.Id == id).FirstOrDefault();
            if (removal != null)
            {
                Entities.Remove(removal);
                PositionComponents.Remove(id);
                VelocityComponents.Remove(id);
                DisplayComponents.Remove(id);
                HealthComponents.Remove(id);
                AnimationComponents.Remove(id);
                SightRadiusComponents.Remove(id);
                LabelComponents.Remove(id);
            }
        }
    }
}
