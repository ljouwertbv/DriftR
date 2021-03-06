﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Driftr
{
    public abstract class RigidBody
    {
        // Linear properties.
        private Vector _position = new Vector(); // P
        private Vector _velocity = new Vector(); // A
        private Vector _forces = new Vector(); // F
        private float _mass; // M

        // Angular properties
        private float _angle;
        private float _angularVelocity;
        private float _torque;
        private float _inertia;

        // Graphical properties.
        private Vector _halfsize = new Vector();
        private Rectangle _rect;
        private Bitmap _vehicleBitmap;

        protected RigidBody()
        {
            // Set defaults to prevent dividing by zero.
            _mass = 1.0f;
            _inertia = 1.0f;
        }

        public virtual void Setup(Vector halfsize, float mass, Bitmap vehicleBitmap)
        {
            // Store the physical parameters.
            _halfsize = halfsize;
            _mass = mass;
            _vehicleBitmap = vehicleBitmap;

            _inertia = (1.0f / 12.0f) *
                       (halfsize.X * halfsize.X) *
                       (halfsize.Y * halfsize.Y) *
                       mass;

            // Generate the viewable rectangle.
            _rect.X = (int)-_halfsize.X;
            _rect.Y = (int)-_halfsize.Y;
            _rect.Width = (int)(_halfsize.X * 8.0f);
            _rect.Height = (int)(_halfsize.Y * 8.0f);
        }

        public void SetLocation(Vector position, float angle)
        {
            _position = position;
            _angle = angle;
        }

        public Vector Position
        {
            get
            {
                return _position;
            }
        }

        /// <summary>
        /// Gets the relative position of the vehicle based on the specified width, height and screen scale.
        /// </summary>
        public Vector RelativePosition(int width, int height, float screenScale)
        {
            float x = _position.X;
            float y = _position.Y;

            x *= screenScale;
            y *= screenScale;

            x += width / 2.0f;
            y = (height / 2.0f) - y;

            return new Vector(x, y);
        }

        protected void AddForce(Vector worldForce, Vector worldOffset)
        {
            // Add the linear force.
            _forces += worldForce;

            // Add its associated torque.
            _torque += worldOffset % worldForce;
        }

        public bool Collision { get; set; }

        public virtual void Update(float timeStep)
        {
            // Linear physics.
            Vector acceleration = _forces / _mass; // A = F / M

            if (Collision)
            {
                if (_velocity.Length > 20)
                {
                    _velocity /= 4;
                }
            }

            _velocity += acceleration * timeStep; // V = V + A * T
            _position += _velocity * timeStep; // P = P + V * T

            // Set the location of the rectangle to the relative position of the vehicle.
            var rel = RelativePosition(1000, 700, 3);
            _rect.Location = new Point((int)rel.X, (int)rel.Y);

            // Clear the forces.
            _forces = new Vector(0, 0);

            // Angular physics.
            float angAcceleration = _torque / _inertia;
            _angularVelocity += angAcceleration * timeStep; // AngV = AngV + A * T
            _angle += _angularVelocity * timeStep; // Angle = AngV * T

            // Clear torque.
            _torque = 0;
        }

        public void Draw(Graphics graphics, Size bufferSize)
        {
            Matrix matrix = graphics.Transform;

            graphics.TranslateTransform(_position.X, _position.Y);
            graphics.RotateTransform(_angle / (float)Math.PI * 180.0f);

            try
            {
                graphics.DrawRectangle(new Pen(Color.Transparent), _rect);
                graphics.DrawImage(_vehicleBitmap, -5, -5, 12, 12);
            }
            catch (StackOverflowException)
            {
                // Physics overflow..
            }

            graphics.Transform = matrix;
        }

        protected Vector RelativeToWorld(Vector relative)
        {
            var matrix = new Matrix();
            var vectors = new PointF[1];

            vectors[0].X = relative.X;
            vectors[0].Y = relative.Y;

            matrix.Rotate(_angle / (float)Math.PI * 180.0f);
            matrix.TransformVectors(vectors);

            return new Vector(vectors[0].X, vectors[0].Y);
        }

        protected Vector WorldToRelative(Vector world)
        {
            var matrix = new Matrix();
            var vectors = new PointF[1];

            vectors[0].X = world.X;
            vectors[0].Y = world.Y;

            matrix.Rotate(-_angle / (float)Math.PI * 180.0f);
            matrix.TransformVectors(vectors);

            return new Vector(vectors[0].X, vectors[0].Y);
        }

        protected Vector PointVelocity(Vector worldOffset)
        {
            var tangent = new Vector(-worldOffset.Y, worldOffset.X);
            return tangent * _angularVelocity + _velocity;
        }

        public Vector Velocity
        {
            get
            {
                return _velocity;
            }
        }

        public bool CollissionWith(Vehicle v)
        {
            //(X2' >= X1 && X1' <= X2) && (Y2' >= Y1 && Y1' <= Y2)

            return Rectangle.Intersect(_rect, v._rect) != Rectangle.Empty;
        }
    }
}
