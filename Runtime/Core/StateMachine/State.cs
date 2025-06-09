using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class State 
    {
        public virtual void Enter()
        {
            
        }

        public virtual void LogicUpdate(float deltaTime)
        {

        }
    
        public virtual void PhysicsUpdate(float fixedDeltaTime)
        {
    
        }
    
        public virtual void LateLogicUpdate(float deltaTime)
        {
    
        }

        public virtual void Exit()
        {

        }
    }
}


