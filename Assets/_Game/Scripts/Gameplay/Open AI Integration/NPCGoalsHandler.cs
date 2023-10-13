using ChatGPT_Detective;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class NpcGoalsHandler : MonoBehaviour
{
    private GoalInfo _currentGoal;

    private IReadOnlyList<GoalInfo> _goalsList = new List<GoalInfo>();

    private Dictionary<int, bool> _prereqTracker = new Dictionary<int, bool>();

    private int _currentGoalIndex = 0;

    public GoalInfo CurrentGoal => _currentGoal;

    public void SetupGoalHandling(IReadOnlyList<GoalInfo> charGoals)
    {
        _goalsList = charGoals;

        _currentGoal = _goalsList[0];

        foreach (GoalInfo goal in _goalsList)
        {
            foreach (int prereqId in goal.PrerequisiteIds)
            {
                _prereqTracker.Add(prereqId, false);
            }
        }
    }

    public void UpdateGoals(int prereqId)
    {
        if (_prereqTracker.TryGetValue(prereqId, out bool value))
        {
            _prereqTracker[prereqId] = true;

            bool cleared = true;

            foreach (int id in _currentGoal.PrerequisiteIds)
            {
                cleared &= _prereqTracker[id];
            }

            if (cleared)
            {
                _currentGoalIndex++;

                if(_currentGoalIndex< _goalsList.Count)
                {
                    _currentGoal = _goalsList[_currentGoalIndex];
                }
            }
        }
    }
}
