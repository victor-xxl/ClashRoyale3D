using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class LoginPage : UIPage
{
	public InputField accInput;
	public InputField pwdInput;
	public Button loginButton;
	public Slider genAccountSlider;

	
	public LoginPage() : base(UIType.Normal, UIMode.DoNothing, UICollider.None)
	{
	}

	protected override string uiPath => "LoginPage";

	protected override void OnAwake()
	{
		accInput = transform.Find("AccInput").GetComponent<InputField>();
		pwdInput = transform.Find("PwdInput").GetComponent<InputField>();
		loginButton = transform.Find("LoginButton").GetComponent<Button>();
		genAccountSlider = transform.Find("GenAccountSlider").GetComponent<Slider>();

		OnStart();
	}
}