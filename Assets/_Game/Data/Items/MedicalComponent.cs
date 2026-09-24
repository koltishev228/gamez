using System;
using UnityEngine;

namespace Zomboid.Data.Items.Components
{
    [Serializable]
    public class MedicalComponent : ItemComponent
    {
        public float HealthRestore = 10f;
        public bool StopsBleeding = false;
        public bool CuresInfection = false;
    }
}
