using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace ConsoleApplication1 {
    public class Ball {
        public int x;
        public int y;
        public int radius;

        public int screen_width;
        public int screen_height;
        
        public CircleShape cs;

        public Vector2f gravity = new Vector2f(0.0f, 0.005f);
        public Vector2f speed;
        public double speed_modifier = 1d / 100d;
        
        public const int min = 8;
        public const int max = 14;

        public const int divider_min = 1;
        public const int divider_max = 20;
        
        private float oldx;
        private float oldy;

        public bool PAUSED = false;

        private int seed;

        private Vector2f center;
        
        public Ball(int radius, int x, int y, int screen_width, int screen_height, int seed = 1) {
            this.seed = new Random(seed).Next(0, 100);
            this.screen_height = screen_height;
            this.screen_width = screen_width;

            center = new Vector2f(radius, radius);
            
            this.x = x;
            this.y = y;
            this.radius = radius;
            cs = new CircleShape(radius);
            cs.Position = new Vector2f(x, y);
            Color c = new Color((byte) get_random(0, 255), (byte) get_random(0, 255), (byte) get_random(0, 255));
            cs.FillColor = c;
            //change_speedX(-5, 5);
        }

        private double distance_between(Ball b1) {
            return Math.Sqrt(Math.Pow(cs.Position.X - b1.cs.Position.X, 2) + Math.Pow(cs.Position.Y - b1.cs.Position.Y, 2));
        }

        private void change_speedY(int min, int max) {
            speed = new Vector2f(speed.X, (get_random(min, max)));
        }
        private void change_speedX(int min, int max) {
            speed = new Vector2f(get_random(min, max), speed.Y);
        }

        private void hit_wall() {
            speed = new Vector2f(speed.X, speed.Y);
        }
        
        private int get_random(int lower, int higher) {
            seed += 1;
            return new Random(DateTime.Now.Millisecond * seed).Next(lower, higher);
        }

        public void change_color() {
            Color c = new Color((byte) get_random(0, 255), (byte) get_random(0, 255), (byte) get_random(0, 255));
            cs.FillColor = c;
        }

        public void change_gravity(float delta) {
            gravity = new Vector2f(0.0f, gravity.Y + (delta / 100.0f));
        }
        public void reset_gravity() {
            gravity = new Vector2f(0.0f, 0.01f);;
        }

        public void moveY(float delta) {
            if (!(cs.Position.Y > screen_height - (2 * cs.Radius)) && !(cs.Position.Y < 0)) {
                cs.Position = new Vector2f(cs.Position.X, cs.Position.Y + delta);
            }
        }
        public void moveX(float delta) {
            if (!(cs.Position.X > screen_width - (2 * cs.Radius)) && !(cs.Position.X < 0)) {
                cs.Position = new Vector2f(cs.Position.X + delta, cs.Position.Y);
            }
        }

        private float pull_strength = 0.0001f;
        public void point_to_mouse(float mousex, float mousey) {
            speed += new Vector2f((float)Math.Sqrt(Math.Pow(cs.Position.X, 2) - Math.Pow(mousex, 2)), (float)Math.Sqrt(Math.Pow(cs.Position.Y, 2) - Math.Pow(mousey, 2))) * pull_strength;
        }
        public void update() {
            if (PAUSED) { return; }
            if (cs.Position.Y > screen_height - (2 * cs.Radius)) {
                change_speedY(-max, -min);
                if (get_random(0, 2) == 1) {
                    change_speedX(-max, -min);
                } else {
                    change_speedX(min, max);
                }
            }
            else if (cs.Position.Y < 0) {
                change_speedY(min, max);
                if (get_random(0, 2) == 1) {
                    change_speedX(-max, -min);
                } else {
                    change_speedX(min, max);
                }
            }
            else if (cs.Position.X > screen_width - (2 * cs.Radius)) {
                change_speedX(-max, -min);
                if (get_random(0, 2) == 1) {
                    change_speedY(-max, -min);
                } else {
                    change_speedY(min, max);
                }
            }
            else if (cs.Position.X < 0) {
                change_speedX(min, max);
                if (get_random(0, 2) == 1) {
                    change_speedY(-max, -min);
                } else {
                    change_speedY(min, max);
                }
            }
            speed += gravity;
            cs.Position = new Vector2f(cs.Position.X + (float)(speed.X * speed_modifier), cs.Position.Y + (float)(speed.Y * speed_modifier));
        }

        CircleShape get_cs() {
            return cs;
        }
    }

    public class Program {

        const int width = 1920;
        const int height = 1080;
        
        public static List<Ball> balls = new List<Ball>();
        public static RenderWindow win = new RenderWindow(new VideoMode(width, height), "hey", Styles.Fullscreen);


        static void Main() {

            win.SetKeyRepeatEnabled(true);

            win.Closed += (sender, e) => { win.Close(); };
            win.MouseButtonReleased += (sender, e) => {
                if (e.Button == Mouse.Button.Left) {
                    foreach (Ball ball in balls) {
                        ball.speed = new Vector2f(0.0f, 0.0f);
                    }
                } else if (e.Button == Mouse.Button.Right) {
                    foreach (Ball ball in balls) {
                        ball.change_color();
                    }
                } else if (e.Button == Mouse.Button.Middle) {
                    foreach (Ball ball in balls) {
                        ball.reset_gravity();
                    }
                }
            };
            win.MouseWheelScrolled += (sender, e) => {
                foreach (Ball ball in balls) {
                    ball.change_gravity(-e.Delta);
                }
            };

            bool frozen = false;
            
            win.KeyPressed += (sender, e) => {
                if (e.Code == Keyboard.Key.Space && !frozen) {
                    frozen = true;
                    foreach (Ball ball in balls) {
                        ball.PAUSED = frozen = true;
                    }
                }
                if (e.Code == Keyboard.Key.W) {
                    if (!frozen) {
                        frozen = true;
                        foreach (Ball ball in balls) {
                            ball.PAUSED = true;
                        }
                    }
                    foreach (Ball ball in balls) {
                        ball.moveY(-5.0f);
                    }
                }
                if (e.Code == Keyboard.Key.S) {
                    if (!frozen) {
                        frozen = true;
                        foreach (Ball ball in balls) {
                            ball.PAUSED = true;
                        }
                    }
                    foreach (Ball ball in balls) {
                        ball.moveY(5.0f);
                    }
                }
                if (e.Code == Keyboard.Key.A) {
                    if (!frozen) {
                        frozen = true;
                        foreach (Ball ball in balls) {
                            ball.PAUSED = true;
                        }
                    }
                    foreach (Ball ball in balls) {
                        ball.moveX(-5.0f);
                    }
                }
                if (e.Code == Keyboard.Key.D) {
                    if (!frozen) {
                        frozen = true;
                        foreach (Ball ball in balls) {
                            ball.PAUSED = true;
                        }
                    }
                    foreach (Ball ball in balls) {
                        ball.moveX(5.0f);
                    }
                }
            };
            win.KeyReleased += (sender, e) => {
                if (e.Code == Keyboard.Key.Space && frozen) {
                    frozen = false;
                    foreach (Ball ball in balls) {
                        ball.PAUSED = false;
                    }
                }
                if (e.Code == Keyboard.Key.W) {
                    frozen = false;
                    foreach (Ball ball in balls) {
                        ball.PAUSED = false;
                    }
                }
                if (e.Code == Keyboard.Key.S) {
                    frozen = false;
                    foreach (Ball ball in balls) {
                        ball.PAUSED = false;
                    }
                }
                if (e.Code == Keyboard.Key.A) {
                    frozen = false;
                    foreach (Ball ball in balls) {
                        ball.PAUSED = false;
                    }
                }
                if (e.Code == Keyboard.Key.D) {
                    frozen = false;
                    foreach (Ball ball in balls) {
                        ball.PAUSED = false;
                    }
                }
            };
            
            win.SetActive();
            
            int missedFrames = 0;
            int missedFramesCap = 10;
            int ballCap = 80000;
            while (win.IsOpen) {
                if (balls.Count < ballCap) {
                    balls.Add(new Ball(5, width / 2, 10, width, height, balls.Count));
                }
                win.DispatchEvents();
                missedFrames += 1;
                if (missedFrames > missedFramesCap) {
                    win.Clear();
                }
                foreach (Ball ball in balls) {
                    ball.update();
                    if (missedFrames > missedFramesCap) {
                        win.Draw(ball.cs);
                    }
                }
                if (missedFrames > missedFramesCap) {
                    missedFrames = 0;
                }
                win.Display();

            }
        }
    }
}