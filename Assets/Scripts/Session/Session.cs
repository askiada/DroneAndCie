using UnityEngine;
using System;

using System.Collections;
using System.IO;
using MathNet.Numerics.Random;
using System.Reflection;

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
        public abstract string GeneratePath(string task, bool withSeed = false);
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
                Type myType = this.GetType();
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                //Debug.Log("PropInfo : " + myPropInfo.ToString());
                return myPropInfo.GetValue(this, null);
            }
            set
            {
                Type myType = this.GetType();
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                //Debug.Log(value);
                myPropInfo.SetValue(this, value, null);

            }

        }

        void Awake()
        {
            SetParametersFromCommandLine();
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
