namespace EmailTestApplicationGUI
{
    partial class SMTPEmailTester
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            pnlSettings = new Panel();
            grpSmtp = new GroupBox();
            lblServer = new Label();
            txtServer = new TextBox();
            lblPort = new Label();
            txtPort = new TextBox();
            lblUsername = new Label();
            txtUsername = new TextBox();
            lblPassword = new Label();
            txtPassword = new TextBox();
            chkSSL = new CheckBox();
            chkAutoRetry = new CheckBox();
            grpEmail = new GroupBox();
            lblFrom = new Label();
            txtFrom = new TextBox();
            lblTo = new Label();
            txtTo = new TextBox();
            lblSubject = new Label();
            txtSubject = new TextBox();
            lblBody = new Label();
            txtBody = new TextBox();
            pnlButtons = new Panel();
            btnSend = new Button();
            btnClear = new Button();
            btnClearLog = new Button();
            grpLog = new GroupBox();
            rtbLog = new RichTextBox();
            progressBar = new ProgressBar();
            lblStatus = new Label();
            SuspendLayout();

            // ── Form ──────────────────────────────────────────────────
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(980, 720);
            Text = "SMTP Email Tester";
            MinimumSize = new Size(980, 720);
            Font = new Font("Segoe UI", 9F);
            BackColor = Color.FromArgb(240, 240, 240);

            // ── pnlSettings ──────────────────────────────────────────
            pnlSettings.Dock = DockStyle.Top;
            pnlSettings.Height = 310;
            pnlSettings.Padding = new Padding(6);

            // ── grpSmtp ───────────────────────────────────────────────
            grpSmtp.Text = "SMTP Server Settings";
            grpSmtp.Location = new Point(8, 6);
            grpSmtp.Size = new Size(450, 295);
            grpSmtp.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // lx=10, tx=140, tw=290, ly=22, gap=34
            lblServer.Text = "SMTP Server:";
            lblServer.Location = new Point(10, 22);
            lblServer.AutoSize = true;
            txtServer.Location = new Point(140, 20);
            txtServer.Size = new Size(290, 23);
            txtServer.Text = "smtp.example.com";

            lblPort.Text = "Port:";
            lblPort.Location = new Point(10, 56);
            lblPort.AutoSize = true;
            txtPort.Location = new Point(140, 54);
            txtPort.Size = new Size(80, 23);
            txtPort.Text = "587";

            lblUsername.Text = "Username:";
            lblUsername.Location = new Point(10, 90);
            lblUsername.AutoSize = true;
            txtUsername.Location = new Point(140, 88);
            txtUsername.Size = new Size(290, 23);

            lblPassword.Text = "Password:";
            lblPassword.Location = new Point(10, 124);
            lblPassword.AutoSize = true;
            txtPassword.Location = new Point(140, 122);
            txtPassword.Size = new Size(290, 23);
            txtPassword.PasswordChar = '●';

            chkSSL.Text = "Enable SSL/TLS";
            chkSSL.Location = new Point(140, 158);
            chkSSL.AutoSize = true;
            chkSSL.Checked = true;

            chkAutoRetry.Text = "Auto-retry with opposite SSL setting on failure";
            chkAutoRetry.Location = new Point(140, 192);
            chkAutoRetry.AutoSize = true;
            chkAutoRetry.Checked = true;

            grpSmtp.Controls.AddRange(new Control[] {
                lblServer, txtServer, lblPort, txtPort,
                lblUsername, txtUsername, lblPassword, txtPassword,
                chkSSL, chkAutoRetry });

            // ── grpEmail ──────────────────────────────────────────────
            grpEmail.Text = "Email";
            grpEmail.Location = new Point(470, 6);
            grpEmail.Size = new Size(490, 295);
            grpEmail.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // ex=10, etx=100, etw=370, ely=22, gap=34
            lblFrom.Text = "From:";
            lblFrom.Location = new Point(10, 22);
            lblFrom.AutoSize = true;
            txtFrom.Location = new Point(100, 20);
            txtFrom.Size = new Size(370, 23);
            txtFrom.Text = "sender@example.com";

            lblTo.Text = "To:";
            lblTo.Location = new Point(10, 56);
            lblTo.AutoSize = true;
            txtTo.Location = new Point(100, 54);
            txtTo.Size = new Size(370, 23);
            txtTo.Text = "recipient@example.com";

            lblSubject.Text = "Subject:";
            lblSubject.Location = new Point(10, 90);
            lblSubject.AutoSize = true;
            txtSubject.Location = new Point(100, 88);
            txtSubject.Size = new Size(370, 23);
            txtSubject.Text = "Test Email";

            lblBody.Text = "Body:";
            lblBody.Location = new Point(10, 124);
            lblBody.AutoSize = true;
            txtBody.Location = new Point(100, 122);
            txtBody.Size = new Size(370, 160);
            txtBody.Multiline = true;
            txtBody.ScrollBars = ScrollBars.Vertical;
            txtBody.Text = "This is a test email sent from SMTP Email Tester.";

            grpEmail.Controls.AddRange(new Control[] {
                lblFrom, txtFrom, lblTo, txtTo,
                lblSubject, txtSubject, lblBody, txtBody });

            pnlSettings.Controls.AddRange(new Control[] { grpSmtp, grpEmail });

            // ── pnlButtons ────────────────────────────────────────────
            pnlButtons.Dock = DockStyle.Top;
            pnlButtons.Height = 46;
            pnlButtons.Padding = new Padding(8, 6, 8, 6);

            btnSend.Text = "▶  Send Email";
            btnSend.Size = new Size(150, 34);
            btnSend.Location = new Point(8, 6);
            btnSend.BackColor = Color.FromArgb(0, 120, 215);
            btnSend.ForeColor = Color.White;
            btnSend.FlatStyle = FlatStyle.Flat;
            btnSend.FlatAppearance.BorderSize = 0;
            btnSend.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSend.Click += BtnSend_Click;

            btnClear.Text = "Clear Fields";
            btnClear.Size = new Size(110, 34);
            btnClear.Location = new Point(168, 6);
            btnClear.BackColor = Color.FromArgb(100, 100, 100);
            btnClear.ForeColor = Color.White;
            btnClear.FlatStyle = FlatStyle.Flat;
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Click += BtnClear_Click;

            btnClearLog.Text = "Clear Log";
            btnClearLog.Size = new Size(100, 34);
            btnClearLog.Location = new Point(288, 6);
            btnClearLog.BackColor = Color.FromArgb(150, 60, 60);
            btnClearLog.ForeColor = Color.White;
            btnClearLog.FlatStyle = FlatStyle.Flat;
            btnClearLog.FlatAppearance.BorderSize = 0;
            btnClearLog.Click += BtnClearLog_Click;

            pnlButtons.Controls.AddRange(new Control[] { btnSend, btnClear, btnClearLog });

            // ── progressBar ───────────────────────────────────────────
            progressBar.Dock = DockStyle.Top;
            progressBar.Height = 6;
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.Visible = false;

            // ── lblStatus ─────────────────────────────────────────────
            lblStatus.Dock = DockStyle.Top;
            lblStatus.Height = 22;
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            lblStatus.Padding = new Padding(6, 0, 0, 0);
            lblStatus.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            lblStatus.ForeColor = Color.Gray;
            lblStatus.Text = "Ready.";

            // ── grpLog ────────────────────────────────────────────────
            grpLog.Text = "Output Log";
            grpLog.Dock = DockStyle.Fill;
            grpLog.Padding = new Padding(6);

            rtbLog.Dock = DockStyle.Fill;
            rtbLog.ReadOnly = true;
            rtbLog.BackColor = Color.FromArgb(15, 15, 15);
            rtbLog.ForeColor = Color.LightGreen;
            rtbLog.Font = new Font("Consolas", 9.5F);
            rtbLog.ScrollBars = RichTextBoxScrollBars.Vertical;
            rtbLog.BorderStyle = BorderStyle.None;
            rtbLog.WordWrap = true;

            grpLog.Controls.Add(rtbLog);

            // ── Add to Form (order matters for Dock stacking) ─────────
            Controls.Add(grpLog);
            Controls.Add(lblStatus);
            Controls.Add(progressBar);
            Controls.Add(pnlButtons);
            Controls.Add(pnlSettings);

            ResumeLayout(false);
        }

        #endregion

        // ── Fields ────────────────────────────────────────────────────
        private Panel       pnlSettings;
        private GroupBox    grpSmtp;
        private Label       lblServer;
        private TextBox     txtServer;
        private Label       lblPort;
        private TextBox     txtPort;
        private Label       lblUsername;
        private TextBox     txtUsername;
        private Label       lblPassword;
        private TextBox     txtPassword;
        private CheckBox    chkSSL;
        private CheckBox    chkAutoRetry;
        private GroupBox    grpEmail;
        private Label       lblFrom;
        private TextBox     txtFrom;
        private Label       lblTo;
        private TextBox     txtTo;
        private Label       lblSubject;
        private TextBox     txtSubject;
        private Label       lblBody;
        private TextBox     txtBody;
        private Panel       pnlButtons;
        private Button      btnSend;
        private Button      btnClear;
        private Button      btnClearLog;
        private GroupBox    grpLog;
        private RichTextBox rtbLog;
        private ProgressBar progressBar;
        private Label       lblStatus;
    }
}
