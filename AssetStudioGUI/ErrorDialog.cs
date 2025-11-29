using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AssetStudioGUI
{
    public partial class ErrorDialog : Form
    {
        private TextBox errorTextBox = null!;
        private Button okButton = null!;
        private Button copyButton = null!;

        public ErrorDialog(string errorMessage)
        {
            InitializeComponent();
            errorTextBox.Text = errorMessage;
        }

        private void InitializeComponent()
        {
            this.Text = "Error";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(400, 300);

            // TextBox для ошибки
            errorTextBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9),
                BackColor = SystemColors.Window,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Panel для кнопок
            var buttonPanel = new Panel
            {
                Height = 50,
                Dock = DockStyle.Bottom,
                Padding = new Padding(10)
            };

            // Кнопка Copy
            copyButton = new Button
            {
                Text = "Copy",
                Size = new Size(75, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            copyButton.Click += CopyButton_Click;

            // Кнопка OK
            okButton = new Button
            {
                Text = "OK",
                Size = new Size(75, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                DialogResult = DialogResult.OK
            };

            buttonPanel.Controls.Add(copyButton);
            buttonPanel.Controls.Add(okButton);

            // Layout
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                Padding = new Padding(0)
            };
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            mainPanel.Controls.Add(errorTextBox, 0, 0);
            mainPanel.Controls.Add(buttonPanel, 0, 1);

            this.Controls.Add(mainPanel);

            // Установка позиций кнопок после загрузки формы
            this.Load += (s, e) =>
            {
                UpdateButtonPositions(buttonPanel);
            };

            // Обновление позиций кнопок при изменении размера формы
            this.Resize += (s, e) =>
            {
                UpdateButtonPositions(buttonPanel);
            };

            buttonPanel.Resize += (s, e) =>
            {
                UpdateButtonPositions(buttonPanel);
            };
        }

        private void UpdateButtonPositions(Panel panel)
        {
            if (copyButton != null && okButton != null && panel != null)
            {
                int buttonWidth = 75;
                int spacing = 10;
                int rightMargin = 10;
                int topMargin = 10;

                okButton.Location = new Point(panel.Width - buttonWidth - rightMargin, topMargin);
                copyButton.Location = new Point(panel.Width - buttonWidth * 2 - spacing - rightMargin, topMargin);
            }
        }

        private void CopyButton_Click(object? sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(errorTextBox.Text);
                MessageBox.Show("Error message copied to clipboard!", "Copy", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to copy to clipboard: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

