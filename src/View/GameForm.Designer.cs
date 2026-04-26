namespace SnakeNet.View
{
    partial class GameForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            _scoreLabel2 = new System.Windows.Forms.Label();
            _livesLabel = new System.Windows.Forms.Label();
            _btnConnect = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // _scoreLabel2
            // 
            _scoreLabel2.AutoSize = true;
            _scoreLabel2.Font = new System.Drawing.Font("Consolas", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            _scoreLabel2.ForeColor = System.Drawing.Color.White;
            _scoreLabel2.Location = new System.Drawing.Point(510, 10);
            _scoreLabel2.Name = "_scoreLabel2";
            _scoreLabel2.Size = new System.Drawing.Size(180, 56);
            _scoreLabel2.TabIndex = 0;
            _scoreLabel2.Text = "0000";
            _scoreLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _livesLabel
            // 
            _livesLabel.AutoSize = true;
            _livesLabel.Font = new System.Drawing.Font("Consolas", 32F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            _livesLabel.ForeColor = System.Drawing.Color.LimeGreen;
            _livesLabel.Location = new System.Drawing.Point(510, 75);
            _livesLabel.Name = "_livesLabel";
            _livesLabel.Size = new System.Drawing.Size(120, 51);
            _livesLabel.TabIndex = 1;
            _livesLabel.Text = "❤ 3";
            _livesLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _btnConnect
            // 
            _btnConnect.Location = new System.Drawing.Point(510, 258);
            _btnConnect.Name = "_btnConnect";
            _btnConnect.Size = new System.Drawing.Size(110, 25);
            _btnConnect.TabIndex = 2;
            _btnConnect.Text = "Подключиться";
            _btnConnect.UseVisualStyleBackColor = true;
            // 
            // GameForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.Black;
            ClientSize = new System.Drawing.Size(680, 500);
            Controls.Add(_livesLabel);
            Controls.Add(_btnConnect);
            Controls.Add(_scoreLabel2);
            DoubleBuffered = true;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "GameForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "🐍 SnakeNet";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label _scoreLabel2;
        private System.Windows.Forms.Label _livesLabel;
        private System.Windows.Forms.Button _btnConnect;
    }
}