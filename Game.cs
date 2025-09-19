using Godot;
using System;
using System.Threading.Tasks;

public partial class Game : Node {
	// Modifier constants
	private const int NONE    = 0x0;
	private const int SHIFT   = 0x1;
	private const int CONTROL = 0x2;
	private const int ALT     = 0x4;

	// Materials
	private static readonly Material JULIA       = GD.Load<Material>("res://materials/julia.material");
	private static readonly Material MANDELBROT  = GD.Load<Material>("res://materials/mandelbrot.material");
	private static readonly Material DMANDELBROT = GD.Load<Material>("res://materials/mandelbrot.material");
	private static readonly string panelpath = "Panel";
	private const float scroll = 8f;
	private bool mouse_sets_seed = false;
	private bool mouse_sets_position = false;
	private Vector2 screen_size;
	private Control ui;
	private Label fps;
	private Texture2D Tex;
	
	
	public override void _Ready() {
		screen_size = GetViewport().GetVisibleRect().Size;
		GD.Print("ss", screen_size);
		GetViewport().SizeChanged += UpdateAspectRatio;
		UpdateAspectRatio();
		
		ui = FindChild("UI") as Control;
		fps = ui.FindChild("FPS") as Label;
		ui.Set("shader_mat", GetNode<TextureRect>(panelpath).Material);
		GetWindow().Mode = Window.ModeEnum.Windowed;
		GD.Print("wind", GetWindow().Mode);
	}

	public override void _Process(double delta) {
		if (fps != null)
			fps.Text = Engine.GetFramesPerSecond().ToString();
	}

	public void SetFractal(int fractal) {
		var panel = GetNode<TextureRect>(panelpath);
		if (fractal == 1)
			panel.Material = MANDELBROT;
		if (fractal == 2)
			panel.Material = JULIA;
		UpdateAspectRatio();
	}

	public void UpdateAspectRatio() {
		screen_size = GetViewport().GetVisibleRect().Size;
		var panel = (ShaderMaterial)(GetNode<TextureRect>(panelpath).Material);
		panel.SetShaderParameter("aspect_ratio", screen_size.X / screen_size.Y);
	}

	private bool CheckKey(InputEvent @event, Key key, int modifier = NONE) {
		if (@event is InputEventKey keyEvent && keyEvent.Pressed) {
			if (keyEvent.Keycode == key) {
				if ((modifier & SHIFT) != 0 && keyEvent.ShiftPressed) return true;
				else if ((modifier & CONTROL) != 0 && keyEvent.CtrlPressed) return true;
				else if ((modifier & ALT) != 0 && keyEvent.AltPressed) return true;
				else if (modifier == NONE) return true;
			}
		}
		return false;
	}

	public override void _UnhandledInput(InputEvent @event) {
		var panel = (ShaderMaterial)(GetNode<TextureRect>(panelpath).Material);
		if (@event is InputEventMouseMotion motionEvent) {
			var seedParam = panel.GetShaderParameter("seed");
			if (mouse_sets_seed && seedParam.VariantType != Variant.Type.Nil){
				Vector2 jseed = (Vector2)panel.GetShaderParameter("seed");
				float mouseSpeed = 0.25f;
				if (Input.IsKeyPressed(Key.Shift)) mouseSpeed = 0.0833f;
				if (Input.IsKeyPressed(Key.Ctrl)) mouseSpeed = 0.025f;

				jseed += mouseSpeed * motionEvent.Relative / (screen_size.Y * (float)panel.GetShaderParameter("scale"));
				
				panel.SetShaderParameter("seed", jseed);
				(ui.FindChild("SeedX") as LineEdit).Text = jseed.X.ToString();
				(ui.FindChild("SeedY") as LineEdit).Text = jseed.Y.ToString();
			}

			if (mouse_sets_position) {
				Vector2 fractalPos = new Vector2(motionEvent.Relative.X, -motionEvent.Relative.Y);
				fractalPos /= (screen_size.Y * (float)panel.GetShaderParameter("scale"));
				fractalPos += (Vector2)panel.GetShaderParameter("position");

				panel.SetShaderParameter("position", fractalPos);
				(ui.FindChild("PositionX") as LineEdit).Text = fractalPos.X.ToString();
				(ui.FindChild("PositionY") as LineEdit).Text = fractalPos.Y.ToString();
			}
		}
		else if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left){
			if (mouseButton.Pressed) {
				var pp = ui.FindChild("PickPanel") as Control;
				if (!((ui.Visible && ui.GetGlobalRect().HasPoint(mouseButton.Position))
					  || (pp.Visible && pp.GetGlobalRect().HasPoint(mouseButton.Position)))) {
					mouse_sets_position = true;
				}
			}
			else {
				mouse_sets_position = false;
			}
		}
		else if (@event.IsActionPressed("wheel_up")) {
			float scale = (float)panel.GetShaderParameter("scale") * (scroll + 1f) / scroll;
			panel.SetShaderParameter("scale", scale);
			(ui.FindChild("Scale") as LineEdit).Text = scale.ToString();

			Vector2 mousePos = GetViewport().GetMousePosition();
			mousePos = new Vector2((mousePos.X - 0.5f * screen_size.X) / screen_size.Y, 0.5f - mousePos.Y / screen_size.Y);
			Vector2 fractalPos = (Vector2)panel.GetShaderParameter("position");
			fractalPos -= mousePos / (scale * scroll);
			panel.SetShaderParameter("position", fractalPos);
			(ui.FindChild("PositionX") as LineEdit).Text = fractalPos.X.ToString();
			(ui.FindChild("PositionY") as LineEdit).Text = fractalPos.Y.ToString();
		}
		else if (@event.IsActionPressed("wheel_down")) {
			float scale = (float)panel.GetShaderParameter("scale") * scroll / (scroll + 1f);
			panel.SetShaderParameter("scale", scale);
			(ui.FindChild("Scale") as LineEdit).Text = scale.ToString();

			Vector2 mousePos = GetViewport().GetMousePosition();
			mousePos = new Vector2((mousePos.X - 0.5f * screen_size.X) / screen_size.Y, 0.5f - mousePos.Y / screen_size.Y);
			Vector2 fractalPos = (Vector2)panel.GetShaderParameter("position");
			fractalPos += mousePos / (scale * (scroll + 1f));
			panel.SetShaderParameter("position", fractalPos);
			(ui.FindChild("PositionX") as LineEdit).Text = fractalPos.X.ToString();
			(ui.FindChild("PositionY") as LineEdit).Text = fractalPos.Y.ToString();
		}
		else if (CheckKey(@event, Key.S)) {
			
			var seedParam = panel.GetShaderParameter("seed");
			if (seedParam.VariantType != Variant.Type.Nil) mouse_sets_seed = !mouse_sets_seed;
		}
		else if (CheckKey(@event, Key.P, SHIFT)) {
			float po = (float)panel.GetShaderParameter("power") - 0.1f;
			panel.SetShaderParameter("power", po);
			(ui.FindChild("Power") as LineEdit).Text = po.ToString();
		}
		else if (CheckKey(@event, Key.P)) {
			float po = (float)panel.GetShaderParameter("power") + 0.1f;
			panel.SetShaderParameter("power", po);
			(ui.FindChild("Power") as LineEdit).Text = po.ToString();
		}
		else if (CheckKey(@event, Key.I, SHIFT)) {
			float i = (float)panel.GetShaderParameter("iterations");
			if (i > 0) {
				panel.SetShaderParameter("iterations", i);
				(ui.FindChild("Iterations") as LineEdit).Text = i.ToString();
			}
		}
		else if (CheckKey(@event, Key.I)) {
			float i = (float)panel.GetShaderParameter("iterations") + 1f;
			panel.SetShaderParameter("iterations", i);
			(ui.FindChild("Iterations") as LineEdit).Text = i.ToString();
		}
		else if (CheckKey(@event, Key.G)) {
			bool smoothing = (bool)panel.GetShaderParameter("smoothing");
			panel.SetShaderParameter("smoothing", !smoothing);
			(ui.FindChild("Smoothing") as Button).ButtonPressed = (bool)panel.GetShaderParameter("smoothing");
		}
		else if (CheckKey(@event, Key.Escape) ||
				 (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Right && mb.Pressed)) {
			ui.Visible = !ui.Visible;
			(ui.FindChild("Mandelbrot Button") as Control).GrabFocus();
		}
		else if (CheckKey(@event, Key.U)) {
			_ = TogglePauseAsync();
		}
		else if (CheckKey(@event, Key.Enter)) ui.ReleaseFocus();
		else if (CheckKey(@event, Key.Q, CONTROL)) GetTree().Quit();
	}
	private async Task TogglePauseAsync() {
		var panel = (ShaderMaterial)(GetNode<TextureRect>(panelpath).Material);
		bool paused = (bool)panel.GetShaderParameter("paused");
		if (!paused) {
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			var fractalViewport = GetNode<TextureRect>(panelpath);
			var img = fractalViewport.GetTexture();
			panel.SetShaderParameter("Tex", img);
		}
		panel.SetShaderParameter("paused", !paused);
	}
}
