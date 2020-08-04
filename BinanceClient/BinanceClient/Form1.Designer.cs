namespace BinanceClient
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            this.AutoUnloadCheckBox = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.TimeoutTextBox = new System.Windows.Forms.TextBox();
            this.UnloadedInfoTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.UnloadButton = new System.Windows.Forms.Button();
            this.SymbolsComboBox = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.binanceClientBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.binanceClientBindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            this.StartTime = new System.Windows.Forms.DateTimePicker();
            this.EndTime = new System.Windows.Forms.DateTimePicker();
            this.AutoUnloadButton = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.FromDBButton = new System.Windows.Forms.Button();
            this.fullCB = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.binanceClientBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.binanceClientBindingSource1)).BeginInit();
            this.SuspendLayout();
            // 
            // AutoUnloadCheckBox
            // 
            this.AutoUnloadCheckBox.AutoSize = true;
            this.AutoUnloadCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.AutoUnloadCheckBox.Location = new System.Drawing.Point(16, 179);
            this.AutoUnloadCheckBox.Name = "AutoUnloadCheckBox";
            this.AutoUnloadCheckBox.Size = new System.Drawing.Size(228, 24);
            this.AutoUnloadCheckBox.TabIndex = 0;
            this.AutoUnloadCheckBox.Text = "Автоматическая выгрузка";
            this.AutoUnloadCheckBox.UseVisualStyleBackColor = true;
            this.AutoUnloadCheckBox.CheckedChanged += new System.EventHandler(this.AutoUnloadCheckBox_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(130, 20);
            this.label1.TabIndex = 3;
            this.label1.Text = "Время выгрузки";
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label3.Location = new System.Drawing.Point(22, 216);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(117, 81);
            this.label3.TabIndex = 7;
            this.label3.Text = "Длина таймаута\r\n(в секундах)";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TimeoutTextBox
            // 
            this.TimeoutTextBox.Location = new System.Drawing.Point(48, 300);
            this.TimeoutTextBox.Name = "TimeoutTextBox";
            this.TimeoutTextBox.Size = new System.Drawing.Size(58, 20);
            this.TimeoutTextBox.TabIndex = 8;
            this.TimeoutTextBox.Text = "5";
            // 
            // UnloadedInfoTextBox
            // 
            this.UnloadedInfoTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.UnloadedInfoTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.UnloadedInfoTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.UnloadedInfoTextBox.Location = new System.Drawing.Point(250, 32);
            this.UnloadedInfoTextBox.Multiline = true;
            this.UnloadedInfoTextBox.Name = "UnloadedInfoTextBox";
            this.UnloadedInfoTextBox.ReadOnly = true;
            this.UnloadedInfoTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.UnloadedInfoTextBox.Size = new System.Drawing.Size(492, 294);
            this.UnloadedInfoTextBox.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label4.Location = new System.Drawing.Point(246, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(174, 20);
            this.label4.TabIndex = 10;
            this.label4.Text = "Выгруженные данные";
            // 
            // UnloadButton
            // 
            this.UnloadButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.UnloadButton.Location = new System.Drawing.Point(145, 94);
            this.UnloadButton.Name = "UnloadButton";
            this.UnloadButton.Size = new System.Drawing.Size(95, 29);
            this.UnloadButton.TabIndex = 11;
            this.UnloadButton.Text = "Выгрузить";
            this.UnloadButton.UseVisualStyleBackColor = true;
            this.UnloadButton.Click += new System.EventHandler(this.UnloadButton_Click);
            // 
            // SymbolsComboBox
            // 
            this.SymbolsComboBox.FormattingEnabled = true;
            this.SymbolsComboBox.Location = new System.Drawing.Point(145, 64);
            this.SymbolsComboBox.Name = "SymbolsComboBox";
            this.SymbolsComboBox.Size = new System.Drawing.Size(95, 21);
            this.SymbolsComboBox.Sorted = true;
            this.SymbolsComboBox.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label5.Location = new System.Drawing.Point(12, 62);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(127, 20);
            this.label5.TabIndex = 13;
            this.label5.Text = "Валютная пара";
            // 
            // binanceClientBindingSource
            // 
            this.binanceClientBindingSource.DataSource = typeof(Binance.Net.BinanceClient);
            // 
            // binanceClientBindingSource1
            // 
            this.binanceClientBindingSource1.DataSource = typeof(Binance.Net.BinanceClient);
            // 
            // StartTime
            // 
            this.StartTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.StartTime.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.StartTime.Location = new System.Drawing.Point(16, 32);
            this.StartTime.Name = "StartTime";
            this.StartTime.Size = new System.Drawing.Size(100, 23);
            this.StartTime.TabIndex = 15;
            // 
            // EndTime
            // 
            this.EndTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.EndTime.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.EndTime.Location = new System.Drawing.Point(145, 32);
            this.EndTime.Name = "EndTime";
            this.EndTime.Size = new System.Drawing.Size(95, 23);
            this.EndTime.TabIndex = 16;
            // 
            // AutoUnloadButton
            // 
            this.AutoUnloadButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.AutoUnloadButton.Location = new System.Drawing.Point(145, 216);
            this.AutoUnloadButton.Name = "AutoUnloadButton";
            this.AutoUnloadButton.Size = new System.Drawing.Size(95, 104);
            this.AutoUnloadButton.TabIndex = 17;
            this.AutoUnloadButton.Text = "Начать авто. выгрузку";
            this.AutoUnloadButton.UseVisualStyleBackColor = true;
            this.AutoUnloadButton.Click += new System.EventHandler(this.AutoUnloadButton_Click);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // FromDBButton
            // 
            this.FromDBButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.FromDBButton.Location = new System.Drawing.Point(16, 94);
            this.FromDBButton.Name = "FromDBButton";
            this.FromDBButton.Size = new System.Drawing.Size(100, 29);
            this.FromDBButton.TabIndex = 11;
            this.FromDBButton.Text = "Показать";
            this.FromDBButton.UseVisualStyleBackColor = true;
            this.FromDBButton.Click += new System.EventHandler(this.FromDBButton_Click);
            // 
            // fullCB
            // 
            this.fullCB.AutoSize = true;
            this.fullCB.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.fullCB.Location = new System.Drawing.Point(16, 147);
            this.fullCB.Name = "fullCB";
            this.fullCB.Size = new System.Drawing.Size(151, 24);
            this.fullCB.TabIndex = 18;
            this.fullCB.Text = "Полные Данные";
            this.fullCB.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(753, 338);
            this.Controls.Add(this.fullCB);
            this.Controls.Add(this.AutoUnloadButton);
            this.Controls.Add(this.EndTime);
            this.Controls.Add(this.StartTime);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.SymbolsComboBox);
            this.Controls.Add(this.FromDBButton);
            this.Controls.Add(this.UnloadButton);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.UnloadedInfoTextBox);
            this.Controls.Add(this.TimeoutTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.AutoUnloadCheckBox);
            this.Name = "Form1";
            this.Text = "BinanceClient";
            ((System.ComponentModel.ISupportInitialize)(this.binanceClientBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.binanceClientBindingSource1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox AutoUnloadCheckBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox TimeoutTextBox;
        private System.Windows.Forms.TextBox UnloadedInfoTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button UnloadButton;
        private System.Windows.Forms.ComboBox SymbolsComboBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.BindingSource binanceClientBindingSource;
        private System.Windows.Forms.BindingSource binanceClientBindingSource1;
        private System.Windows.Forms.DateTimePicker StartTime;
        private System.Windows.Forms.DateTimePicker EndTime;
        private System.Windows.Forms.Button AutoUnloadButton;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button FromDBButton;
        private System.Windows.Forms.CheckBox fullCB;
    }
}

