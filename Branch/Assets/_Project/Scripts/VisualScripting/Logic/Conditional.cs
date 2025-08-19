using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.VisualScripting
{
    public class Conditional : ProcessBase
    {
        [SerializeField] private ProcessData inputData;
        [SerializeField] private List<ProcessData> outputData;

        private bool _isRunning;


        // TODO: inputData �� ture �϶� �ѹ��� ����ȴ�.
        // Input Data�� True ��ȣ�� ���� �� ���� �ѹ� �����Ѵ�.
        private void Update()
        {
            IsOn = CheckInputProcessStatus(inputData);

            if (IsOn)
            {
                if (_isRunning) return;

                Execute();
                _isRunning = true;
            }
            else
            {
                _isRunning = false;
            }
        }

        public override void Execute()
        {
            foreach (var output in outputData)
                output.process.Execute();
        }
    }
}