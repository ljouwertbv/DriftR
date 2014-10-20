﻿    using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Driftr
{
    public partial class Driftr : Form
    {
        private const float screenScale = 3.0F;
        private Graphics _graphics;
        private Bitmap _backbuffer;
        private Size _bufferSize;
        private readonly GameTimer _timer = new GameTimer();

        private bool _left = false, _right = false, _up = false, _down = false;

        private float _steering = 0;    // -1.0 is left, 0 is center,  1.0 is right.
        private float _throttle = 0;    // 0 is coasting, 1 is full throttle.
        private float _brakes;  // 0 is no breaks, 1 is full breaks.

        private readonly Vehicle _vehicle = new Vehicle();

        public Driftr()
        {
            InitializeComponent();
            Application.Idle += Application_Idle;

            screen.Paint += screen_Paint;
            KeyUp += Driftr_KeyUp;
            KeyDown += Driftr_KeyDown;

            Init(screen.Size);
        }

        private void Init(Size size)
        {
            _bufferSize = size;
            _backbuffer = new Bitmap(_bufferSize.Width, _bufferSize.Height);
            _graphics = Graphics.FromImage(_backbuffer);

            _timer.GetETime();

            _vehicle.Setup(new Vector(3, 8) / 2.0F, 5, Color.Red);
            _vehicle.SetLocation(new Vector(0, 0), 0);
        }

        private void Render(Graphics g)
        {
            _graphics.Clear(Color.Black);
            _graphics.ResetTransform();
            _graphics.ScaleTransform(screenScale, -screenScale);
            _graphics.TranslateTransform(
                _bufferSize.Width / 2.0F / screenScale, 
                -_bufferSize.Height / 2.0f / screenScale);

            DrawScreen();

            g.DrawImage(
                _backbuffer, 
                new Rectangle(0, 0, _bufferSize.Width, _bufferSize.Height), 
                0,
                0,
                _bufferSize.Width, 
                _bufferSize.Height,
                GraphicsUnit.Pixel);
        }

        private void DrawScreen()
        {
            _vehicle.Draw(_graphics, _bufferSize);
        }

        private void DoFrame()
        {
            float etime = _timer.GetETime();

            ProcessInput();

            // Apply vehicle controls.
            _vehicle.SetSteering(_steering);
            _vehicle.SetThrottle(_throttle, true);
            _vehicle.SetBrakes(_brakes);

            _vehicle.Update(etime);

            ConstrainVehicle();

            screen.Invalidate();
        }

        private void ConstrainVehicle()
        {
            Vector position = _vehicle.Position;
            var screenSize = new Vector(screen.Width / screenScale, screen.Height / screenScale);

            while (position.X > screenSize.X / 2.0f)
            {
                position.X -= screenSize.X;
            }
            while (position.Y > screenSize.Y / 2.0f)
            {
                position.Y -= screenSize.Y;
            }
            while (position.X < -screenSize.X / 2.0f)
            {
                position.X += screenSize.X;
            }
            while (position.Y < -screenSize.Y / 2.0f)
            {
                position.Y += screenSize.Y;
            }
        }

        private void ProcessInput()
        {
            if (_left)
            {
                _steering = -1;
            }
            else if (_right)
            {
                _steering = 1;
            }
            else
            {
                _steering = 0;
            }

            if (_up)
            {
                _throttle = 5;
            }
            else
            {
                _throttle = 0;
            }

            if (_down)
            {
                _brakes = 1;
            }
            else
            {
                _brakes = 0;
            }
        }

        private void screen_Paint(object sender, PaintEventArgs e)
        {
            Render(e.Graphics);
        }

        private void Driftr_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    _left = true;
                    break;
                case Keys.Right:
                    _right = true;
                    break;
                case Keys.Up:
                    _up = true;
                    break;
                case Keys.Down:
                    _down = true;
                    break;
                default:
                    return;
            }

            e.Handled = true;
        }

        private void Driftr_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    _left = false;
                    break;
                case Keys.Right:
                    _right = false;
                    break;
                case Keys.Up:
                    _up = false;
                    break;
                case Keys.Down:
                    _down = false;
                    break;
                default:
                    return;
            }

            e.Handled = true;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            DoFrame();
        }

    }
}