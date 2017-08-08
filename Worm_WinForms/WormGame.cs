using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace Worm_WinForms
{
    public partial class WormGame : Form
    {
        #region Константы
        /// <summary>
        /// Игровые константы
        /// </summary>
        private struct Constants
        {
            /// <summary>
            /// Интервал обновлений в секунду
            /// </summary> 
            public const float Interval = 16.0f;

            /// <summary>
            /// Начальная скорость
            /// </summary>
            public const float NormSpeed = 60.0f;

            /// <summary>
            /// Максимальная скорость
            /// </summary>
            public const int MaxSpeed = 100;
        }
        #endregion

        #region Переменные
        private Size _fieldSize;
        private readonly List<Square> _worm = new List<Square>();
        private bool _gameover;
        private int _score;
        private readonly Timer _gameLoop = new Timer();
        private readonly Timer _wormLoop = new Timer();
        private ProgressBar _lifeBar;
        private ProgressBar _speedBar;
        private Square _food;
        private Label _lifeLabel;
        private Label _scoreLabel;
        private Label _speedLabel;
        private Label _scoreLabel2;
        private Label _connectionStatus;
        private Button _btnConnect;
        public PictureBox GameField;

        public int Direction { get; set; }
        public IContainer components;

        #endregion

        #region Конструктор
        public WormGame(Size size)
        {
            InitializeComponent();
            _fieldSize = size;
            _gameLoop.Tick += Update;
            _wormLoop.Tick += UpdateWorm;
            _gameLoop.Interval = (int)Constants.Interval;
            _speedBar.Value = (int)Constants.NormSpeed;
            _gameLoop.Start();
            _wormLoop.Start();
            StartGame(true);
        }

        private static void Update(object sender, EventArgs e)
        {
        }

        #endregion

        #region События формы
        private void WormGame_KeyDown(object sender, KeyEventArgs e)
        {
            Input.ChangeState(e.KeyCode, true);
        }

        private void WormGame_KeyUp(object sender, KeyEventArgs e)
        {
            Input.ChangeState(e.KeyCode, false);
        }

        private void pbField_Paint(object sender, PaintEventArgs e)
        {
            Draw(e.Graphics);
        }

        private void Draw(Graphics canvas)
        {
            var font = new Font(Font, FontStyle.Bold);
            if (_gameover)
            {
                var message = canvas.MeasureString("LIFE: ", font);
                canvas.DrawString("LIFE: ", font, Brushes.Aqua, new PointF(140 - message.Width / 2, 20));
                canvas.DrawString(_lifeBar.Value.ToString(), font, Brushes.Aqua, new PointF(160, 20));
                message = canvas.MeasureString("Total score is " + _score, font);
                canvas.DrawString("Total score is " + _score, font, Brushes.Aqua,
                    new PointF(140 - message.Width / 2, 70));
                message = canvas.MeasureString("ENTER - kill the enemy", font);
                canvas.DrawString("ENTER - kill the enemy", font, Brushes.Aqua,
                    new PointF(140 - message.Width / 2, 120));
                message = canvas.MeasureString("ESCAPE to exit game", font);
                canvas.DrawString("ESCAPE - exit game", font, Brushes.Aqua,
                    new PointF(140 - message.Width / 2, 220));
            }
            else
            {
                // рисуем приз
                _scoreLabel2.Text = _score.ToString("####");
                //canvas.FillRectangle(new SolidBrush(Color.FromArgb(255, 0, 160, 220)),
                //    new Rectangle(_food.X * 16, _food.Y * 16, 16, 16));
                // рисуем червя
                for (var i = 0; i < _worm.Count; i++)
                {
                    var currentPart = _worm[i];
                    canvas.FillRectangle(new SolidBrush(i == 0 ? Color.FromArgb(255, 118, 0) :
                        Color.FromArgb(220, 20, 206, 17)), 
                        new Rectangle(currentPart.X * 16+1, currentPart.Y * 16+1, 14, 14));
                }
            }
        }
        #endregion

        #region Auto generated code

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">True if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WormGame));
            this._lifeBar = new System.Windows.Forms.ProgressBar();
            this._speedBar = new System.Windows.Forms.ProgressBar();
            this._lifeLabel = new System.Windows.Forms.Label();
            this._scoreLabel = new System.Windows.Forms.Label();
            this._speedLabel = new System.Windows.Forms.Label();
            this._scoreLabel2 = new System.Windows.Forms.Label();
            this._connectionStatus = new System.Windows.Forms.Label();
            this._btnConnect = new System.Windows.Forms.Button();
            this.GameField = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.GameField)).BeginInit();
            this.SuspendLayout();
            // 
            // _lifeBar
            // 
            this._lifeBar.BackColor = System.Drawing.Color.Honeydew;
            this._lifeBar.Cursor = System.Windows.Forms.Cursors.Arrow;
            this._lifeBar.ForeColor = System.Drawing.Color.MintCream;
            this._lifeBar.Location = new System.Drawing.Point(44, 40);
            this._lifeBar.Margin = new System.Windows.Forms.Padding(0);
            this._lifeBar.Maximum = 10;
            this._lifeBar.Name = "_lifeBar";
            this._lifeBar.Size = new System.Drawing.Size(49, 10);
            this._lifeBar.Step = 1;
            this._lifeBar.TabIndex = 2;
            this._lifeBar.Value = 3;
            // 
            // _speedBar
            // 
            this._speedBar.BackColor = System.Drawing.Color.Aquamarine;
            this._speedBar.Location = new System.Drawing.Point(44, 24);
            this._speedBar.MarqueeAnimationSpeed = 10;
            this._speedBar.Name = "_speedBar";
            this._speedBar.Size = new System.Drawing.Size(49, 10);
            this._speedBar.Step = 1;
            this._speedBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this._speedBar.TabIndex = 3;
            this._speedBar.Value = 6;
            // 
            // _lifeLabel
            // 
            this._lifeLabel.BackColor = System.Drawing.Color.Transparent;
            this._lifeLabel.Font = new System.Drawing.Font("Gabriola", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lifeLabel.Location = new System.Drawing.Point(3, 34);
            this._lifeLabel.Name = "_lifeLabel";
            this._lifeLabel.Size = new System.Drawing.Size(35, 16);
            this._lifeLabel.TabIndex = 4;
            this._lifeLabel.Text = "LIFES";
            this._lifeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _scoreLabel
            // 
            this._scoreLabel.Font = new System.Drawing.Font("Gabriola", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this._scoreLabel.Location = new System.Drawing.Point(3, 3);
            this._scoreLabel.Margin = new System.Windows.Forms.Padding(0);
            this._scoreLabel.Name = "_scoreLabel";
            this._scoreLabel.Size = new System.Drawing.Size(35, 15);
            this._scoreLabel.TabIndex = 5;
            this._scoreLabel.Text = "SCORE";
            this._scoreLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _speedLabel
            // 
            this._speedLabel.Font = new System.Drawing.Font("Gabriola", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this._speedLabel.Location = new System.Drawing.Point(3, 18);
            this._speedLabel.Margin = new System.Windows.Forms.Padding(0);
            this._speedLabel.Name = "_speedLabel";
            this._speedLabel.Size = new System.Drawing.Size(35, 13);
            this._speedLabel.TabIndex = 6;
            this._speedLabel.Text = "SPEED";
            this._speedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _scoreLabel2
            // 
            this._scoreLabel2.Font = new System.Drawing.Font("Gadugi", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._scoreLabel2.Location = new System.Drawing.Point(41, 3);
            this._scoreLabel2.Name = "_scoreLabel2";
            this._scoreLabel2.Size = new System.Drawing.Size(52, 18);
            this._scoreLabel2.TabIndex = 7;
            this._scoreLabel2.Text = "0000";
            this._scoreLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _connectionStatus
            // 
            this._connectionStatus.AutoSize = true;
            this._connectionStatus.ForeColor = System.Drawing.Color.Red;
            this._connectionStatus.Location = new System.Drawing.Point(8, 240);
            this._connectionStatus.Name = "_connectionStatus";
            this._connectionStatus.Size = new System.Drawing.Size(78, 13);
            this._connectionStatus.TabIndex = 8;
            this._connectionStatus.Text = "Not connected";
            // 
            // _btnConnect
            // 
            this._btnConnect.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this._btnConnect.ForeColor = System.Drawing.Color.Transparent;
            this._btnConnect.Image = global::Worm_WinForms.Properties.Resources.button_1281;
            this._btnConnect.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this._btnConnect.Location = new System.Drawing.Point(7, 214);
            this._btnConnect.Name = "_btnConnect";
            this._btnConnect.Size = new System.Drawing.Size(79, 23);
            this._btnConnect.TabIndex = 13;
            this._btnConnect.TabStop = false;
            this._btnConnect.Text = "Connect";
            this._btnConnect.UseVisualStyleBackColor = true;
            // 
            // GameField
            // 
            this.GameField.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.GameField.BackgroundImage = global::Worm_WinForms.Properties.Resources.grass_texture256;
            this.GameField.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.GameField.Location = new System.Drawing.Point(96, 3);
            this.GameField.Margin = new System.Windows.Forms.Padding(0);
            this.GameField.Name = "GameField";
            this.GameField.Size = new System.Drawing.Size(256, 256);
            this.GameField.TabIndex = 0;
            this.GameField.TabStop = false;
            this.GameField.Paint += new System.Windows.Forms.PaintEventHandler(this.pbField_Paint);
            // 
            // WormGame
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.SteelBlue;
            this.ClientSize = new System.Drawing.Size(355, 262);
            this.Controls.Add(this._btnConnect);
            this.Controls.Add(this._connectionStatus);
            this.Controls.Add(this._scoreLabel2);
            this.Controls.Add(this._speedLabel);
            this.Controls.Add(this._scoreLabel);
            this.Controls.Add(this._lifeLabel);
            this.Controls.Add(this._speedBar);
            this.Controls.Add(this._lifeBar);
            this.Controls.Add(this.GameField);
            this.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "WormGame";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "mainWindow";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.WormGame_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.WormGame_KeyUp);
            ((System.ComponentModel.ISupportInitialize)(this.GameField)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        #endregion
    }
}
