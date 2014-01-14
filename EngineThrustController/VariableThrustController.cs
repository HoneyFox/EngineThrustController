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

		public ModuleEngineThrustController parentController = null;
		public PartResource parentResource = null;

		private float percentageFix = 1.0f;
		
		private StartState m_startState = StartState.None;

		public override void OnStart(StartState state)
		{
			m_startState = state;
			if (state == StartState.None || state == StartState.Editor) return;

			BindController();

			if (parentController != null)
			{
				if (parentController.engine != null)
				{
					percentageFix = parentController.engine.thrustPercentage / 100.0f;
				}
				else if (parentController.engineFX != null)
				{
					percentageFix = parentController.engineFX.thrustPercentage / 100.0f;
				}
			}

			base.OnStart(state);
		}

		private void BindController()
		{
			parentController = null;
			foreach (PartModule module in this.part.Modules)
			{
				if (module is ModuleEngineThrustController)
				{
					parentController = module as ModuleEngineThrustController;
					parentController.canAdjustOverride = true;
					break;
				}
			}

			if (part.Resources.Contains(resourceName))
			{
				parentResource = part.Resources[resourceName];
			}
		}

		public override void OnFixedUpdate()
		{
			if(parentResource != null && parentController != null)
			{
				if (useTimeCurve)
				{
					if (parentResource.amount < parentResource.maxAmount * 0.9999f)
					{
						if (ignitionStartTime == 0)
							ignitionStartTime = Planetarium.GetUniversalTime();

						float timeElapsed = Convert.ToSingle(Planetarium.GetUniversalTime() - ignitionStartTime);

						float thrustPercent = timeCurve.Evaluate(timeElapsed) * percentageFix;
						//Debug.Log("VariableThrustController: timeElapsed = " + timeElapsed.ToString("F2") + " thrust: " + (thrustPercent * 100.0f).ToString("F2") + "%");
						parentController.SetPercentage(thrustPercent);
					}
				}
				else
				{
					float fuelAmount = Convert.ToSingle(parentResource.amount / parentResource.maxAmount);
					float thrustPercent = thrustCurve.Evaluate(fuelAmount) * percentageFix;
					//Debug.Log("VariableThrustController: fuelAmount = " + fuelAmount.ToString("F2") + " thrust: " + (thrustPercent * 100.0f).ToString("F2") + "%");
					parentController.SetPercentage(thrustPercent);
				}
			}
		}

	}
}
