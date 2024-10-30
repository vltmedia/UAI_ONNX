using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UAI.ConsoleApp;

namespace UAI.Common
{
    [System.Serializable]
    public class UAIComponent
    {
        public static List<UAIComponent> instantiatedComponents = new List<UAIComponent>();

        public static void UpdateAll()
        {
            foreach (var component in instantiatedComponents)
            {
                component.Update();
            }
        }

        public static void DisableAll()
        {
            foreach (var component in instantiatedComponents)
            {
                component.OnDisable();
            }
        }


        public UAIComponent()
        {
            instantiatedComponents.Add(this);
            RuntimeBase.Instance.OnUpdate += Update;

            Awake();
            Start();
        }

        public virtual void Awake()
        {

        }
        
        public virtual void OnEnable()
        {

        }
        public virtual void OnDisable()
        {

        }

        public virtual void Start()
        {

        }
        float deltaTime = 0;
        public virtual void Update(float deltaTime)
        {
            this.deltaTime = deltaTime;
            Update();
        }
        public virtual void Update()
        {


        }


    }
}
