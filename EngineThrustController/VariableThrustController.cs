using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace EngineThrustController
{
	public partial class ModuleSRBThrust : PartModule
	{
		[KSPField(isPersistant = false)]
		public FloatCurve thrustCurve = new FloatCurve();

		[KSPField(isPersistant = false)]
		public FloatCurve timeCurve = new FloatCurve();

		[KSPField(isPersistant = false)]
		public bool useTimeCurve = false;

		[KSPField(isPersistant = true)]
		private double ignitionStartTime = 0;

		[KSPField(isPersistant = false)]
		public string resourceName = "SolidFuel";

		[KSPField(isPersistant = false, guiName = "Accel", guiActive = true)]
		private string acceleration = "0.0G";

		public ModuleEngineThrustController parentController = null;
		public PartResource parentResource = null;

        [KSPField(isPersistant = true)]
		private float percentageFix = 1.0f;
		
		public override void OnStart(StartState state)
		{
            if (state == StartState.None || state == StartState.Editor)
                return;

			BindController();

			if (parentController != null)
			{
                percentageFix = parentController.GetPercentage();
			}

			base.OnStart(state);
		}

		private void BindController()
		{
			parentController = null;
			foreach (PartModule module in this.part.Modules)
			{
                var engineController = module as ModuleEngineThrustController;
				if (engineController != null)
				{
                    parentController = engineController;
					parentController.canAdjustOverride = true;
					break;
				}
			}

			if (part.Resources.Contains(resourceName))
			{
				parentResource = part.Resources[resourceName];
			}

			if (parentController.engine != null)
			{
				parentController.engine.useEngineResponseTime = false;
			}
			else if (parentController.engineFX != null)
			{
                parentController.engineFX.useEngineResponseTime = false;
			}
		}

		public override void OnFixedUpdate()
		{
            if (parentResource == null || parentController == null)
                return;
			
            if (useTimeCurve) {
                if (parentResource.amount < parentResource.maxAmount * 0.9999f) {
                    if (ignitionStartTime == 0)
                        ignitionStartTime = Planetarium.GetUniversalTime();

                    float timeElapsed = Convert.ToSingle(Planetarium.GetUniversalTime() - ignitionStartTime);

                    float thrustPercent = Mathf.Clamp01(timeCurve.Evaluate(timeElapsed)) * percentageFix;
                    //Debug.Log("VariableThrustController: timeElapsed = " + timeElapsed.ToString("F2") + " thrust: " + (thrustPercent * 100.0f).ToString("F2") + "%");
                    parentController.SetPercentage(thrustPercent);

                }
            } else {
                float fuelAmount = Convert.ToSingle(parentResource.amount / parentResource.maxAmount);
                float thrustPercent = Mathf.Clamp01(thrustCurve.Evaluate(fuelAmount)) * percentageFix;
                //Debug.Log("VariableThrustController: fuelAmount = " + fuelAmount.ToString("F2") + " thrust: " + (thrustPercent * 100.0f).ToString("F2") + "%");
                parentController.SetPercentage(thrustPercent);
            }

            float thrust = 0;
            if (parentController.engine != null)
                thrust = parentController.engine.finalThrust;
            else if (parentController.engineFX != null)
                thrust = parentController.engineFX.finalThrust;

            float acc = thrust / (part.mass + part.GetResourceMass()) / 9.82f;
            acceleration = acc.ToString("F1") + "G";
		}

	}
}
