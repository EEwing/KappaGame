using FarseerPhysics.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kappa.world {
    abstract class MapModel : IDynamicObject {

        public World World { get; protected set; }

        public abstract void Update(float dt);
    }
}
