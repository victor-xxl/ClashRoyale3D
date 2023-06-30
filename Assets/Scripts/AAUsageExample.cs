﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityRoyale
{
	public class AAUsageExample : MonoBehaviour
	{
		public AssetReferenceGameObject refObject;
		public AssetReference scene;

		void Start()
		{
			refObject.InstantiateAsync(Vector3.zero, Quaternion.identity, null).Completed += OnAssetInstantiated;
		}

		private void OnAssetInstantiated(AsyncOperationHandle<GameObject> asyncOp)
		{
			Debug.Log(asyncOp.Result.name + " loaded.");
		}
	}
}