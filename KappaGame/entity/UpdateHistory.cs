using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kappa.entity {
    class UpdateHistory {

        private static readonly int MaxStates = 10;

        private Queue<EntityState> _history = null;

        private Queue<EntityState> History { get { return _history ?? (_history = new Queue<EntityState>()); } }

        public void AddState(EntityState state) {
            History.Enqueue(state);
            if (History.Count > MaxStates)
                History.Dequeue();
        }

        internal EntityState GetLastState() {
            return History.Peek();
        }
    }
}
