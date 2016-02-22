using FarseerPhysics.Dynamics;
using Kappa.entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kappa.world {
    abstract class MapModel : IDynamicObject {

        public List<Entity> entities { get; protected set; }

        public World World { get; protected set; }

        public MapModel() {
            entities = new List<Entity>();
        }
        
        public virtual T CreateEntity<T>(T ent) where T : Entity {
            entities.Add(ent);
            ent.Map = this;
            return ent;
        }

        public Entity GetEntity(Guid id) {
            foreach (Entity ent in entities) {
                //Console.WriteLine($"Checking {id} against {ent.id}");
                if(ent.id.Equals(id)) {
                    return ent;
                }
            }
            return null;
        }

        public void Update(float dt) {
            World.Step(dt);

            foreach (Entity ent in entities)
                ent.Update(dt);
        }
    }
}
