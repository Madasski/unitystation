﻿using System;
using System.Collections.Generic;
using UnityEngine;
using ScriptableObjects;
using Systems.ObjectConnection;
using Objects.Atmospherics;


namespace Objects.Engineering
{
	public class ReactorBoiler : MonoBehaviour, IMultitoolMasterable, ICheckedInteractable<HandApply>, IServerDespawn
	{
		public decimal MaxPressureInput = 630000M;
		public decimal CurrentPressureInput = 0;
		public decimal OutputEnergy;
		public decimal TotalEnergyInput;

		public decimal Efficiency = 0.5M;

		public ReactorPipe ReactorPipe;

		public List<ReactorGraphiteChamber> Chambers;
		// Start is called before the first frame update

		#region Lifecycle

		public void Awake()
		{
			ReactorPipe = GetComponent<ReactorPipe>();
		}

		private void OnEnable()
		{
			if (CustomNetworkManager.Instance._isServer == false) return;

			UpdateManager.Add(CycleUpdate, 1);
		}

		private void OnDisable()
		{
			if (CustomNetworkManager.Instance._isServer == false) return;

			UpdateManager.Remove(CallbackType.PERIODIC_UPDATE, CycleUpdate);
		}

		/// <summary>
		/// is the function to denote that it will be pooled or destroyed immediately after this function is finished, Used for cleaning up anything that needs to be cleaned up before this happens
		/// </summary>
		void IServerDespawn.OnDespawnServer(DespawnInfo info)
		{
			Spawn.ServerPrefab(CommonPrefabs.Instance.Metal, this.GetComponent<RegisterObject>().WorldPositionServer,
				count: 15);
		}

		#endregion

		public void CycleUpdate()
		{
			//Maybe change equation later to something cool
			CurrentPressureInput = 0;
			CurrentPressureInput = (decimal)Mathf.Clamp(((ReactorPipe.pipeData.mixAndVolume.InternalEnergy -
			                                              (ReactorPipe.pipeData.mixAndVolume.WholeHeatCapacity * 293.15f))),
				(float)decimal.MinValue, (float)decimal.MaxValue);
			if (CurrentPressureInput > 0)
			{
				ReactorPipe.pipeData.mixAndVolume.InternalEnergy -= (float)(CurrentPressureInput * Efficiency);

				//Logger.Log("CurrentPressureInput " + CurrentPressureInput);
				if (CurrentPressureInput > MaxPressureInput)
				{
					//Logger.LogError(" ReactorBoiler !!!booommmm!!", Category.Editor);
					//Explosions.Explosion.StartExplosion(registerObject.LocalPosition, 800, registerObject.Matrix);
				}

				OutputEnergy = CurrentPressureInput * Efficiency;
			}
			else
			{
				OutputEnergy = 0;
			}
		}

		public bool WillInteract(HandApply interaction, NetworkSide side)
		{
			if (!DefaultWillInteract.Default(interaction, side)) return false;
			if (!Validations.HasItemTrait(interaction.UsedObject, CommonTraits.Instance.Welder)) return false;

			return true;
		}

		public void ServerPerformInteraction(HandApply interaction)
		{
			if (Validations.HasItemTrait(interaction.UsedObject, CommonTraits.Instance.Welder))
			{
				ToolUtils.ServerUseToolWithActionMessages(interaction, 10,
					"You start to deconstruct the ReactorBoiler..",
					$"{interaction.Performer.ExpensiveName()} starts to deconstruct the ReactorBoiler...",
					"You deconstruct the ReactorBoiler",
					$"{interaction.Performer.ExpensiveName()} deconstructs the ReactorBoiler.",
					() => { _ = Despawn.ServerSingle(gameObject); });
			}
		}

		#region Multitool Interaction

		public MultitoolConnectionType ConType => MultitoolConnectionType.BoilerTurbine;
		public bool MultiMaster => false;
		int IMultitoolMasterable.MaxDistance => int.MaxValue;

		#endregion
	}
}
