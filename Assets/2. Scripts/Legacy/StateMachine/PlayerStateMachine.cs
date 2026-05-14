using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine : StateMachine
{
    private Dictionary<string, IState> _stateDict = new Dictionary<string, IState>();
    private PlayerContext _playerContext;

    private void Start()
    {
        _playerContext = GetComponent<PlayerContext>();

        _stateDict.Add("Idle", new PlayerIdleState(_playerContext));
        _stateDict.Add("Run", new PlayerRunState(_playerContext));

        _InitState();
    }

    private void _InitState()
    {
        ChangeState(_stateDict["Idle"]);
    }
}
