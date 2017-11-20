using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.IO;
using MathNet.Numerics.Random;

namespace Lexmou.MachineLearning
{

    public abstract class Session : MonoBehaviour
    {

        private float nextUpdate;
        public float intervalUpdate = 5.0f;

        public SystemRandomSource rndGenerator;
        public int _seed;
        public int seed { get { return _seed; } set { _seed = value; } }

        public float _timeScale;
        public float timeScale { get { return _timeScale; } set { _timeScale = value; } }

        public abstract void Build();
        public abstract void BuildHUD();
        public abstract void BuildSessionWriter();
        public abstract void CloseSessionWriter();
        public abstract string GeneratePath(bool withSeed = false);
        public abstract void SetParametersFromCommandLine();
        public abstract void OnDestroy();
        public abstract void Reset();
        public abstract void Destroy();
        public abstract void RunEachFixedUpdate();
        public abstract void RunEachIntervalUpdate();

        public object this[string propertyName]
        {
            get
            {
                Type myType = typeof(Session);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                return myPropInfo.GetValue(this, null);
            }
            set
            {
                Type myType = typeof(Session);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                Debug.Log(value);
                myPropInfo.SetValue(this, value, null);

            }

        }

        public Session()
        {
            Debug.Log("Constructor Session");
            SetParametersFromCommandLine();
            //Debug.Log(seed);

        }

        void Awake()
        {
            Debug.Log("Awake Session");
            Build();
            Time.timeScale = timeScale;
            BuildHUD();
            BuildSessionWriter();
            nextUpdate = intervalUpdate;
            rndGenerator = new SystemRandomSource(seed);
            Reset();
            CloseSessionWriter();
        }

        void CloseApp()
        {
            if (Directory.Exists("Quit/"))
            {
                OnDestroy();
                Application.Quit();
            }

        }

        void FixedUpdate()
        {
            CloseApp();
            RunEachFixedUpdate();
            if (Time.time >= nextUpdate)
            {
                nextUpdate = Mathf.FloorToInt(Time.time) + intervalUpdate;
                RunEachIntervalUpdate();
                Destroy();
                Reset();
            }
        }
    }
}
