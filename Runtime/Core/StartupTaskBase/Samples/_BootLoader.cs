using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Core
{
    public class _BootLoader : MonoBehaviour
    {
        [SerializeField] private Core.StartupTaskRunner runner;

        private void Start()
        {
            runner.OnProgressChanged += p => Debug.Log($"Init progress: {p:P}");
            runner.OnInitializeCompleted += OnInitComplete;
            runner.Init();
        }

        private void OnInitComplete()
        {
            Debug.Log("Startup initialization done. Proceed to next scene...");
            // e.g., SceneManager.LoadScene("MainMenu");  
        }

        private void OnDisable()
        {
            if (runner)
            {
                runner.OnProgressChanged -= null;
                runner.OnInitializeCompleted -= null;
            }
        }
    }
}
