using Godot;
using System;

public class Player : Area2D
{
	[Export] private float _rotationSpeed = 4.5f;
	[Export] private float _speed = 500;
	[Signal] public delegate void HitSignal();
	
	private CollisionPolygon2D _collisionPolygon;
	private Vector2 _velocity = Vector2.Zero;
	private Rect2 _mapLimit;
	
	private AnimationPlayer _burstAnimationPlayer;
	private Line2D _burstLine;

	private int _rotationDirection = 0;

	private const float Acceleration = 0.2f;
	private const float Friction = 0.02f;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Hide();
		_mapLimit = GetViewportRect();
		_burstAnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer_Burst");
		_burstLine = GetNode<Line2D>("Node_Player/Line_Burst");
		_collisionPolygon = GetNode<CollisionPolygon2D>("CollisionPolygon");

		Connect("body_entered", this, "OnPlayerBodyEntered");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(float delta)
	{
		Vector2 inputVelocity = HandlePlayerInput();
		HandlePlayerMovement(delta, inputVelocity);
	}

	public override void _Process(float delta)
	{
		HandlePlayerAnimation();
	}

	private Vector2 HandlePlayerInput()
	{
		Vector2 inputVelocity = Vector2.Zero;
		_rotationDirection = 0;
	  
		if (Input.IsActionPressed("ui_right"))
		{
			_rotationDirection += 1;
		}
		if (Input.IsActionPressed(("ui_left")))
		{
			_rotationDirection -= 1;
		}
		if (Input.IsActionPressed(("ui_down")))
		{
			inputVelocity = new Vector2(_speed, 0).Rotated(Rotation);
		}
		if (Input.IsActionPressed(("ui_up")))
		{
			inputVelocity = new Vector2(-_speed, 0).Rotated(Rotation);
		}

		return inputVelocity.Normalized() * _speed;
	}

	private void HandlePlayerMovement(float delta, Vector2 inputVelocity)
	{
		Rotation += _rotationDirection * _rotationSpeed * delta;

		_velocity = inputVelocity.Length() > 0 ? _velocity.LinearInterpolate(inputVelocity, Acceleration) : _velocity.LinearInterpolate(Vector2.Zero, Friction);
		Position += _velocity * delta;

		// Limits player position to map boundaries with an offset
		float xLimit = _mapLimit.End.x;
		float yLimit = _mapLimit.End.y;
		float offset = 20.0f;
		Position = new Vector2(Mathf.Clamp(Position.x, offset, xLimit - offset), Mathf.Clamp(Position.y, offset, yLimit - offset));
	}
	
	private void HandlePlayerAnimation()
	{
		if (_velocity.Length() >= 40)
		{
			_burstAnimationPlayer.Play("Burst Blink");
		}
		else
		{
			_burstLine.Visible = false;
			_burstAnimationPlayer.Stop();
		}
	}

	private void OnPlayerBodyEntered(object body)
	{
	  Hide();
	  EmitSignal("HitSignal");
	  _collisionPolygon.SetDeferred("disabled", false);
	}

	public void RestartPosition(Vector2 newPosition)
	{
	  Show();
	  this.Position = newPosition;
	  _collisionPolygon.Disabled = false;
	}
}


