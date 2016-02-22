using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kappa.entity {
    class EntityState {
        public long TimeStamp = 0; // Uh oh...
        public Guid id { get; set; }
        public Vector2 Velocity { get; set; } = Vector2.Zero;
        // Add angular velocity
        public Vector2 Position { get; set; } = Vector2.Zero;
    }
}
