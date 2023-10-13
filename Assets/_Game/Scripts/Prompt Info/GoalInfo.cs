using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Unity.Collections;
using UnityEngine;

namespace ChatGPT_Detective
{
    [System.Serializable]
    public class GoalInfo 
    {
        [SerializeField] private int _id;

        [SerializeField] private List<int> _prerequisiteIds;

        [SerializeField] [TextArea(3, 10)] private string _goal;

        public int Id => _id;

        public IReadOnlyList<int> PrerequisiteIds => _prerequisiteIds;

        public string Goal => _goal;

        public GoalInfo(int id, GoalInfo goalInfo)
        {
            _id = id;

            _prerequisiteIds = (List<int>)goalInfo.PrerequisiteIds;
            _goal = goalInfo.Goal;
        }
    }
}