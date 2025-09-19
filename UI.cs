using Godot;
using System;

public partial class UI : Control {
	private static readonly string panelpath = "Panel";
	private ShaderMaterial shader_mat;
	private Button mandel;
	private Button julia;
	private LineEdit power;
	private LineEdit itera;
	private LineEdit scalee;
	private LineEdit posx;
	private LineEdit posy;
	private LineEdit seedx;
	private LineEdit seedy;
	private CheckButton smooth;
	private HSlider Rfreq;
	private HSlider Gfreq;
	private HSlider Bfreq;
	private HSlider Rphase;
	private HSlider Gphase;
	private HSlider Bphase;
	//private string picker_name = "";

	public override async void _Ready() {
		mandel = FindChild("Mandelbrot Button") as Button;
		julia = FindChild("Julia Button") as Button;
		power = FindChild("Power") as LineEdit;
		itera = FindChild("Iterations") as LineEdit;
		scalee = FindChild("Scale") as LineEdit;
		posx = FindChild("PositionX") as LineEdit;
		posy = FindChild("PositionY") as LineEdit;
		seedx = FindChild("SeedX") as LineEdit;
		seedy = FindChild("SeedY") as LineEdit;
		smooth = FindChild("Smoothing") as CheckButton;
		Rfreq = FindChild("RedFrequency") as HSlider;
		Gfreq = FindChild("GreenFrequency") as HSlider;
		Bfreq = FindChild("BlueFrequency") as HSlider;
		Rphase = FindChild("RedPhase") as HSlider;
		Gphase = FindChild("GreenPhase") as HSlider;
		Bphase = FindChild("BluePhase") as HSlider;
		// Wait for parent to be ready
		await ToSignal(GetParent(), Node.SignalName.Ready);
		UpdateUI();
		mandel.Pressed += OnMandelbrotButtonpressed;
		julia.Pressed += OnJuliaButtonpressed;
		power.TextChanged += OnPowerTextChanged;
		itera.TextChanged += OnIteraTextChanged;
		scalee.TextChanged += OnScaleeTextChanged;
		posx.TextChanged += OnPosXTextChanged;
		posy.TextChanged += OnPosYTextChanged;
		seedx.TextChanged += OnSeedXTextChanged;
		seedy.TextChanged += OnSeedYTextChanged;
		smooth.Toggled += OnSmoothToggled;
		Rfreq.ValueChanged += OnRFreqChanged;
		Gfreq.ValueChanged += OnGFreqChanged;
		Bfreq.ValueChanged += OnBFreqChanged;
		Rphase.ValueChanged += OnRPhaseChanged;
		Gphase.ValueChanged += OnGPhaseChanged;
		Bphase.ValueChanged += OnBPhaseChanged;
		(FindChild("Julia Button") as Button).GrabFocus();
	}

	public void SetColorScheme() {
		shader_mat.SetShaderParameter("red_frequency", ((HSlider)FindChild("RedFrequency")).Value);
		shader_mat.SetShaderParameter("red_phase", ((HSlider)FindChild("RedPhase")).Value);
		shader_mat.SetShaderParameter("green_frequency", ((HSlider)FindChild("GreenFrequency")).Value);
		shader_mat.SetShaderParameter("green_phase", ((HSlider)FindChild("GreenPhase")).Value);
		shader_mat.SetShaderParameter("blue_frequency", ((HSlider)FindChild("BlueFrequency")).Value);
		shader_mat.SetShaderParameter("blue_phase", ((HSlider)FindChild("BluePhase")).Value);
		shader_mat.SetShaderParameter("smoothing", ((CheckButton)FindChild("Smoothing")).ButtonPressed);
	}

	public void UpdateUI() {
		((HSlider)FindChild("RedFrequency")).Value = (double)shader_mat.GetShaderParameter("red_frequency");
		((HSlider)FindChild("RedPhase")).Value = (double)shader_mat.GetShaderParameter("red_phase");
		((HSlider)FindChild("GreenFrequency")).Value = (double)shader_mat.GetShaderParameter("green_frequency");
		((HSlider)FindChild("GreenPhase")).Value = (double)shader_mat.GetShaderParameter("green_phase");
		((HSlider)FindChild("BlueFrequency")).Value = (double)shader_mat.GetShaderParameter("blue_frequency");
		((HSlider)FindChild("BluePhase")).Value = (double)shader_mat.GetShaderParameter("blue_phase");
		((CheckButton)FindChild("Smoothing")).ButtonPressed = (bool)shader_mat.GetShaderParameter("smoothing");

		(FindChild("Scale") as LineEdit).Text = ((float)shader_mat.GetShaderParameter("scale")).ToString();
		Vector2 pos = (Vector2)shader_mat.GetShaderParameter("position");
		(FindChild("PositionX") as LineEdit).Text = pos.X.ToString();
		(FindChild("PositionY") as LineEdit).Text = pos.Y.ToString();
		var seedParam = shader_mat.GetShaderParameter("seed");
		if (seedParam.VariantType != Variant.Type.Nil) {
			Vector2 seed = (Vector2)seedParam;
			seedx.Text = seed.X.ToString();
			seedy.Text = seed.Y.ToString();
		}

		(FindChild("Power") as LineEdit).Text = ((float)shader_mat.GetShaderParameter("power")).ToString();
		(FindChild("Iterations") as LineEdit).Text = ((float)shader_mat.GetShaderParameter("iterations")).ToString();
	}
	private void OnMandelbrotButtonpressed() {
		GetParent().Call("SetFractal", 1);
		shader_mat = (ShaderMaterial)(GetNode<TextureRect>("../Panel").Material);
		SetColorScheme();
		UpdateUI();
		((LineEdit)FindChild("SeedX")).Editable = false;
		((LineEdit)FindChild("SeedY")).Editable = false;
	}

	private void OnJuliaButtonpressed() {
		GetParent().Call("SetFractal", 2);
		shader_mat = (ShaderMaterial)(GetNode<TextureRect>("../Panel").Material);
		SetColorScheme();
		UpdateUI();
		((LineEdit)FindChild("SeedX")).Editable = true;
		((LineEdit)FindChild("SeedY")).Editable = true;
	}

	private void OnPowerTextChanged(string newText) {
		if (float.TryParse(newText, out float value))
		shader_mat.SetShaderParameter("power", value);
	}
	private void OnIteraTextChanged(string newText) {
		if (float.TryParse(newText, out float value))
			shader_mat.SetShaderParameter("iterations", value);
	}
	private void OnScaleeTextChanged(string newText) {
		if (float.TryParse(newText, out float value))
			shader_mat.SetShaderParameter("scale", value);
	}
	private void OnPosXTextChanged(string newText) {
		if (float.TryParse(newText, out float value))
			shader_mat.SetShaderParameter("pos_x", value);
	}
	private void OnPosYTextChanged(string newText) {
		if (float.TryParse(newText, out float value))
			shader_mat.SetShaderParameter("pos_y", value);
	}
	private void OnSeedXTextChanged(string newText) {
		if (float.TryParse(newText, out float value))
			shader_mat.SetShaderParameter("seed_x", value);
	}
	private void OnSeedYTextChanged(string newText) {
		if (float.TryParse(newText, out float value))
			shader_mat.SetShaderParameter("seed_y", value);
	}
	private void OnSmoothToggled(bool pressed) {
		pressed = !pressed;
		shader_mat.SetShaderParameter("smoothing", !pressed);}
	private void OnRFreqChanged(double value) {
		shader_mat.SetShaderParameter("red_frequency", (float)value);}
	private void OnGFreqChanged(double value) {
		shader_mat.SetShaderParameter("green_frequency", (float)value);}
	private void OnBFreqChanged(double value) {
		shader_mat.SetShaderParameter("blue_frequency", (float)value);}
	private void OnRPhaseChanged(double value) {
		shader_mat.SetShaderParameter("red_phase", (float)value);}
	private void OnGPhaseChanged(double value) {
		shader_mat.SetShaderParameter("green_phase", (float)value);}
	private void OnBPhaseChanged(double value) {
		shader_mat.SetShaderParameter("blue_phase", (float)value);}
}
