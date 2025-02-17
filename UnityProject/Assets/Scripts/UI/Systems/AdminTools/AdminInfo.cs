﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mirror;
using UnityEngine;

namespace AdminTools
{
	/// <summary>
	/// Add Admin info to any networked obj that needs to have information displayed
	/// on the admins overlay canvas. Add the IAdminInfo interface to each component
	/// where information needs to be gathered from. This component will then handle
	/// the communication to the admin clients
	/// </summary>
	public class AdminInfo : NetworkBehaviour
	{
		private IAdminInfo[] adminInfos;
		[Tooltip("The position offset from the center of the tracked object")]
		[SerializeField] private Vector2 offsetPosition = Vector2.zero;
		[Tooltip("Give the server obj time to init before sending overlay data")]
		[SerializeField] private float waitToInit = 2f;

		public Vector2 OffsetPosition => offsetPosition;

		public string StringInfo
		{
			get
			{
				if (adminInfos == null || adminInfos.Length == 0)
				{
					return "";
				}
				else
				{
					var builder = new StringBuilder();

					foreach (var info in adminInfos)
					{
						builder.AppendLine(info.AdminInfoString());
					}

					return builder.ToString();
				}
			}
		}

		public override void OnStartServer()
		{
			base.OnStartServer();
			adminInfos = GetComponents<IAdminInfo>();
			StartCoroutine(WaitToSet());
		}

		IEnumerator WaitToSet()
		{
			yield return WaitFor.Seconds(waitToInit);
			AdminOverlay.ServerAddInfoPanel(netId, this);
		}
	}
}
