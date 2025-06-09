using UnityEngine;

namespace Core
{
    /// <summary>  
    /// A simple and flexible state machine implementation.  
    /// </summary>  
    public class StateMachine
    {
        private State _currentState;
        private State _previousState;
        public State CurrentState => _currentState;
        public State PreviousState => _previousState;

        /// <summary>  
        /// Indicates whether the state machine is running any state.  
        /// </summary>  
        public bool IsActive => _currentState != null;

        /// <summary>  
        /// Initialize the state machine with a starting state.  
        /// </summary>  
        public virtual void Initialize(State startState)
        {
            _previousState = null;
            _currentState = startState;
            _currentState?.Enter();
        }

        /// <summary>  
        /// Change to a new state. Calls Exit on the old state and Enter on the new one.  
        /// </summary>  
        public virtual void ChangeState(State newState)
        {
            if (newState == null || newState == _currentState)
                return;

            _currentState?.Exit();
            _previousState = _currentState;
            _currentState = newState;
            _currentState.Enter();
        }

        /// <summary>  
        /// Call the current state's logic update. Should be called in MonoBehaviour's Update.  
        /// </summary>  
        public virtual void Update(float deltaTime)
        {
            _currentState?.LogicUpdate(deltaTime);
        }

        /// <summary>  
        /// Call the current state's physics update. Should be called in MonoBehaviour's FixedUpdate.  
        /// </summary>  
        public virtual void FixedUpdate(float fixedDeltaTime)
        {
            _currentState?.PhysicsUpdate(fixedDeltaTime);
        }

        /// <summary>  
        /// Call the current state's late logic update. Should be called in MonoBehaviour's LateUpdate.  
        /// </summary>  
        public virtual void LateUpdate(float deltaTime)
        {
            _currentState?.LateLogicUpdate(deltaTime);
        }

        /// <summary>  
        /// Force exit the current state without entering another (useful for cleanup).  
        /// </summary>  
        public virtual void ExitCurrentState()
        {
            _currentState?.Exit();
            _previousState = _currentState;
            _currentState = null;
        }
    }
}