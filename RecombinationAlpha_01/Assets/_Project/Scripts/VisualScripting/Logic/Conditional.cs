using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.VisualScripting
{
    public class Conditional : ProcessBase
    {
        [SerializeField] private ProcessData inputData;
        [SerializeField] private List<ProcessData> outputData;

        private void Update()
        {
            if (CheckInputProcessStatus(inputData))
            {
                IsOn = true;
                Execute();
            }
            else
            {
                IsOn = false;
            }
        }
        
        public override void Execute()
        {
            foreach (var output in outputData)
                output.process.Execute();
        }
    }
}