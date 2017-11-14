using UnityEngine;
using System.Collections;
using Lexmou.Environment.Wind;

namespace Lexmou.Manager
{
    public class WindManager : MonoBehaviour
    {
        public void SetWind(float val)
        {
            GameObject.Find("WindArea").GetComponent<WindArea>().windStrength = val;
        }
    }
}
