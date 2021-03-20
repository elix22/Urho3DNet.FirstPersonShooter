using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Urho3DNet.FirstPersonShooter
{
    public class FPSApplication : Application
    {
        /// Scene.
        private readonly SharedPtr<Scene> _scene = new SharedPtr<Scene>(null);

        /// Camera scene node.
        private readonly SharedPtr<Viewport> _viewport = new SharedPtr<Viewport>(null);

        private Input _input;
        private Node _level;
        private Node _player;
        private readonly float _mouseSensetivity = 0.22f;
        private ClassicFPSCharacter _character;
        private List<Node> _spawnPoints;

        public FPSApplication(Context context) : base(context)
        {
            context.RegisterFactory<ClassicFPSCharacter>();
        }

        protected Viewport Viewport
        {
            get => _viewport?.Value;
            set => _viewport.Value = value;
        }

        protected Scene Scene
        {
            get => _scene?.Value;
            set => _scene.Value = value;
        }

        public override void Setup()
        {
            var windowed = Debugger.IsAttached;
            EngineParameters[Urho3D.EpFullScreen] = !windowed;
            if (windowed) EngineParameters[Urho3D.EpWindowResizable] = true;
            EngineParameters[Urho3D.EpWindowTitle] = "First Person Shooter Demo";
            EngineParameters[Urho3D.EpFrameLimiter] = true;
            EngineParameters[Urho3D.EpVsync] = true;
        }


        public override void Start()
        {
            // Execute base class startup
            base.Start();

            _input = Context.Input;

            //if (touchEnabled_)
            //    touch_ = new Touch(Context, TOUCH_SENSITIVITY);

            // Create static scene content
            CreateScene();


            //// Create the UI content
            //CreateInstructions();

            // Subscribe to necessary events
            SubscribeToEvents();

            // Set the mouse mode to use in the sample
            InitMouseMode(MouseMode.MmRelative);
        }

        public override void Stop()
        {
            _viewport.Dispose();
            _scene.Dispose();
            base.Stop();
        }

        protected void InitMouseMode(MouseMode mode)
        {
            var input = Context.Input;

            var console = GetSubsystem<Console>();

            Context.Input.SetMouseMode(mode);
            if (console != null && console.IsVisible)
                Context.Input.SetMouseMode(MouseMode.MmAbsolute, true);
        }

        private void SubscribeToEvents()
        {
            // Subscribe to Update event for setting the character controls before physics simulation
            SubscribeToEvent(E.Update, HandleUpdate);

            // Unsubscribe the SceneUpdate event from base class as the camera node is being controlled in HandlePostUpdate() in this sample
            UnsubscribeFromEvent(E.SceneUpdate);

            SubscribeToEvent(E.KeyDown, HandleKeyDown);
            SubscribeToEvent(E.KeyUp, HandleKeyUp);
            SubscribeToEvent(E.MouseMove, HandleMouseMove);
        }

        private void HandleUpdate(VariantMap args)
        {
            var timestep = args[E.Update.TimeStep].Float;


            var mouseMove = _input.MouseMove;
            _character.Rotate(mouseMove.X * _mouseSensetivity, mouseMove.Y * _mouseSensetivity, 0);
        }

        private void HandleMouseMove(VariantMap obj)
        {
        }

        private void HandleKeyUp(VariantMap obj)
        {
            var keycode = (Key) obj[E.KeyUp.Key].Int;

            switch (keycode)
            {
                case Key.KeyUp:
                case Key.KeyW:
                    _character.Forward = 0.0f;
                    break;
                case Key.KeyDown:
                case Key.KeyS:
                    _character.Backward = 0.0f;
                    break;
                case Key.KeyLeft:
                case Key.KeyA:
                    _character.Left = 0.0f;
                    break;
                case Key.KeyRight:
                case Key.KeyD:
                    _character.Right = 0.0f;
                    break;
                case Key.KeySpace:
                    _character.Jump = false;
                    break;
            }
        }

        private void HandleKeyDown(VariantMap obj)
        {
            var keycode = (Key) obj[E.KeyDown.Key].Int;

            switch (keycode)
            {
                case Key.KeyUp:
                case Key.KeyW:
                    _character.Forward = 1.0f;
                    break;
                case Key.KeyDown:
                case Key.KeyS:
                    _character.Backward = 1.0f;
                    break;
                case Key.KeyLeft:
                case Key.KeyA:
                    _character.Left = 1.0f;
                    break;
                case Key.KeyRight:
                case Key.KeyD:
                    _character.Right = 1.0f;
                    break;
                case Key.KeySpace:
                    _character.Jump = true;
                    break;
            }
        }

        private void CreateScene()
        {
            var cache = GetSubsystem<ResourceCache>();

            Scene = new Scene(Context);

            Scene.LoadXML("Scenes/Map_v2.xml");

            _player = Scene.CreateChild();

            _spawnPoints = Scene.GetChildrenWithTag("SpawnPoint", true).ToList();
            _player.Position = _spawnPoints[0].WorldPosition;
            _character = _player.CreateComponent<ClassicFPSCharacter>();
            var camera = _character.Camera;
            camera.Fov = 80;
            var gun = camera.Node.CreateChild("Gun");
            gun.LoadXML("Weapons/AdaptiveCombatRifle/ACRRifle.xml");
            gun.Position = new Vector3(0.104308f, -0.19f, 0.227534f);
            gun.Rotation = new Quaternion(0.5f, -0.5f, 0.5f, 0.5f);

            foreach (var spawnPoint in _spawnPoints.Skip(1))
            {
                var enemy = Scene.CreateChild();
                enemy.LoadXML("Objects/Enemy.xml");
                enemy.WorldPosition = spawnPoint.WorldPosition;
                var model = enemy.GetComponent<AnimatedModel>(true);
                var states = model.AnimationStates;
                var a = model.Node.GetOrCreateComponent<AnimationController>();
                a.Play(states[0].Animation.Name, 0, true);
            }

            Viewport = new Viewport(Context, Scene, camera);
            Context.Renderer.SetViewport(0, Viewport);
        }
    }
}