namespace UGU.Runtime
{
    public class BaseUGUFSMState
    {
        protected UGUFSM m_fsm;

        public BaseUGUFSMState(UGUFSM fsm)
        {
            m_fsm = fsm;
        }

        public virtual void OnEnter()
        {
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void OnFixedUpdate()
        {
        }

        public virtual void OnExit()
        {
        }

        // 状态转换检查
        public virtual void CheckTransitions()
        {
        }
    }
}