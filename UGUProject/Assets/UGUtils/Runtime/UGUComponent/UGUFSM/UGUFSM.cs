using System.Collections.Generic;
using UnityEngine;

namespace UGU.Runtime
{
    public class UGUFSM : MonoBehaviour
    {
        [SerializeField] private string m_curtStateName;
        private Dictionary<string, BaseUGUFSMState> m_dicState;
        private BaseUGUFSMState m_curtState;

        public void AddState(string stateKey, BaseUGUFSMState state)
        {
            m_dicState.Add(stateKey, state);
        }

        public void Init()
        {
            m_dicState = new Dictionary<string, BaseUGUFSMState>();
        }

        public void ChangeState(string stateKey)
        {
            if (m_dicState.ContainsKey(stateKey))
            {
                if (m_curtState != null)
                {
                    m_curtState.OnExit();
                }

                m_curtState = m_dicState[stateKey];
                m_curtStateName = stateKey;
                m_curtState.OnEnter();
            }
            else
            {
                Debug.LogError($"状态 {stateKey} 不存在!");
            }
        }

        private void Update()
        {
            if (m_curtState != null)
            {
                m_curtState.OnUpdate();
                m_curtState.CheckTransitions();
            }
        }

        private void FixedUpdate()
        {
            if (m_curtState != null)
            {
                m_curtState.OnFixedUpdate();
            }
        }
    }
}