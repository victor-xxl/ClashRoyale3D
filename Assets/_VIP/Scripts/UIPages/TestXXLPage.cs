using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class TestXXLPage : UIPage
{
	public WidgetID by;
	public Text text;

	
	public TestXXLPage() : base(UIType.Normal, UIMode.DoNothing, UICollider.None)
	{
	}

	protected override string uiPath => "TestXXLPage";

	protected override void OnAwake()
	{
		by = transform.Find("by").GetComponent<WidgetID>();
		text = transform.Find("by/Text").GetComponent<Text>();

		OnStart();
	}
}