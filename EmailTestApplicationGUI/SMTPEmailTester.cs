using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace EmailTestApplicationGUI
{
    public partial class SMTPEmailTester : Form
    {
        private CancellationTokenSource? _cts;

        public SMTPEmailTester()
        {
            InitializeComponent();
        }

        // ──────────────────────────────────────────────────────────────
        //  Send button
        // ──────────────────────────────────────────────────────────────
        private async void BtnSend_Click(object sender, EventArgs e)
        {
            if (btnSend.Tag is "running")
            {
                _cts?.Cancel();
                return;
            }

            if (!ValidateInputs()) return;

            SetSendingState(true);
            rtbLog.Clear();

            var cfg = BuildConfig();
            _cts = new CancellationTokenSource();

            try
            {
                await RunSendSequenceAsync(cfg, _cts.Token);
            }
            catch (OperationCanceledException)
            {
                AppendLog("⚠  Operation cancelled by user.", LogLevel.Warning);
            }
            finally
            {
                SetSendingState(false);
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  Main send sequence with auto-retry
        // ──────────────────────────────────────────────────────────────
        private async Task RunSendSequenceAsync(SmtpConfig cfg, CancellationToken ct)
        {
            AppendLog($"══════════════════════════════════════════", LogLevel.Info);
            AppendLog($"  SMTP Email Tester  —  {DateTime.Now:yyyy-MM-dd HH:mm:ss}", LogLevel.Info);
            AppendLog($"══════════════════════════════════════════", LogLevel.Info);
            AppendLog($"Host   : {cfg.Host}", LogLevel.Info);
            AppendLog($"Port   : {cfg.Port}", LogLevel.Info);
            AppendLog($"From   : {cfg.From}", LogLevel.Info);
            AppendLog($"To     : {cfg.To}", LogLevel.Info);
            AppendLog($"SSL    : {cfg.EnableSsl}", LogLevel.Info);

            // ── Step 1: Port connectivity check ───────────────────────
            AppendLog("", LogLevel.Info);
            AppendLog("── Step 1: Port connectivity check ──────────────", LogLevel.Header);
            bool portOpen = await CheckPortAsync(cfg.Host, cfg.Port, ct);
            if (!portOpen)
            {
                AppendLog($"✘ Port {cfg.Port} on {cfg.Host} appears BLOCKED or unreachable.", LogLevel.Error);
                AppendLog("  Possible causes:", LogLevel.Error);
                AppendLog("  • Firewall (local or network) is blocking outbound TCP on this port.", LogLevel.Error);
                AppendLog("  • The SMTP server is not listening on that port.", LogLevel.Error);
                AppendLog("  • ISP is blocking port 25 (common for residential connections).", LogLevel.Error);
                AppendLog("  Common SMTP ports: 25 (relay), 465 (SMTPS), 587 (STARTTLS).", LogLevel.Warning);

                if (chkAutoRetry.Checked)
                {
                    int[] fallbackPorts = GetFallbackPorts(cfg.Port);
                    foreach (int fp in fallbackPorts)
                    {
                        ct.ThrowIfCancellationRequested();
                        AppendLog($"↺  Auto-retry: probing port {fp}…", LogLevel.Retry);
                        bool fpOpen = await CheckPortAsync(cfg.Host, fp, ct);
                        if (fpOpen)
                        {
                            AppendLog($"✔  Port {fp} is reachable! Switching to port {fp}.", LogLevel.Success);
                            cfg = cfg with { Port = fp, EnableSsl = fp == 465 };
                            break;
                        }
                        AppendLog($"✘  Port {fp} is also blocked.", LogLevel.Error);
                    }
                }

                // Re-check after fallback attempts
                bool retry = await CheckPortAsync(cfg.Host, cfg.Port, ct);
                if (!retry)
                {
                    AppendLog("✘ No reachable port found. Cannot proceed.", LogLevel.Error);
                    UpdateStatus("Failed — port blocked.");
                    return;
                }
            }
            else
            {
                AppendLog($"✔  Port {cfg.Port} is open and reachable.", LogLevel.Success);
            }

            // ── Step 2: TLS/SSL capability probe ──────────────────────
            AppendLog("", LogLevel.Info);
            AppendLog("── Step 2: TLS capability probe ─────────────────", LogLevel.Header);
            await ProbeTlsAsync(cfg.Host, cfg.Port, ct);

            // ── Step 3: Attempt send ───────────────────────────────────
            AppendLog("", LogLevel.Info);
            AppendLog("── Step 3: Sending email ─────────────────────────", LogLevel.Header);
            bool sent = await TrySendAsync(cfg, ct);

            if (!sent && chkAutoRetry.Checked)
            {
                AppendLog("", LogLevel.Info);
                AppendLog("── Step 4: Auto-retry with opposite SSL setting ──", LogLevel.Header);
                var altCfg = cfg with { EnableSsl = !cfg.EnableSsl };
                AppendLog($"↺  Retrying with SSL={altCfg.EnableSsl}…", LogLevel.Retry);
                sent = await TrySendAsync(altCfg, ct);
            }

            AppendLog("", LogLevel.Info);
            if (sent)
            {
                AppendLog("══════════════════════════════════════════", LogLevel.Success);
                AppendLog("  ✔  Email sent successfully!", LogLevel.Success);
                AppendLog("══════════════════════════════════════════", LogLevel.Success);
                UpdateStatus("✔  Email sent successfully.");
            }
            else
            {
                AppendLog("══════════════════════════════════════════", LogLevel.Error);
                AppendLog("  ✘  All attempts failed. See log above.", LogLevel.Error);
                AppendLog("══════════════════════════════════════════", LogLevel.Error);
                UpdateStatus("✘  All attempts failed.");
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  Port probe
        // ──────────────────────────────────────────────────────────────
        private async Task<bool> CheckPortAsync(string host, int port, CancellationToken ct)
        {
            AppendLog($"  Probing TCP {host}:{port}…", LogLevel.Info);
            try
            {
                using var tcp = new TcpClient();
                var connectTask = tcp.ConnectAsync(host, port, ct).AsTask();
                var timeout     = Task.Delay(5000, ct);
                var winner      = await Task.WhenAny(connectTask, timeout);
                if (winner == timeout)
                {
                    AppendLog($"  ⏱  Connection timed out after 5 s.", LogLevel.Warning);
                    return false;
                }
                await connectTask; // propagate any socket exception
                AppendLog($"  TCP connection established.", LogLevel.Success);
                return true;
            }
            catch (SocketException ex)
            {
                AppendLog($"  Socket error: {ex.SocketErrorCode} — {ex.Message}", LogLevel.Error);
                return false;
            }
            catch (Exception ex)
            {
                AppendLog($"  Connection error: {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  TLS probe — reads SMTP banner + EHLO to discover STARTTLS,
        //  then optionally negotiates TLS to report the protocol version
        // ──────────────────────────────────────────────────────────────
        private async Task ProbeTlsAsync(string host, int port, CancellationToken ct)
        {
            bool isImplicitTls = port == 465;
            AppendLog($"  Mode: {(isImplicitTls ? "Implicit TLS (connect wrapped)" : "Plain/STARTTLS")}", LogLevel.Info);

            try
            {
                using var tcp = new TcpClient();
                await tcp.ConnectAsync(host, port, ct);

                Stream stream;
                SslStream? ssl = null;

                if (isImplicitTls)
                {
                    ssl    = new SslStream(tcp.GetStream(), false, CertificateValidationCallback);
                    stream = ssl;
                    await ssl.AuthenticateAsClientAsync(
                        new SslClientAuthenticationOptions
                        {
                            TargetHost             = host,
                            EnabledSslProtocols    = SslProtocols.None, // let OS choose
                            RemoteCertificateValidationCallback = CertificateValidationCallback
                        }, ct);
                    ReportTlsDetails(ssl);
                }
                else
                {
                    stream = tcp.GetStream();
                }

                using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);
                using var writer = new StreamWriter(stream, Encoding.ASCII, leaveOpen: true) { AutoFlush = true, NewLine = "\r\n" };

                // Read greeting
                string? greeting = await ReadSmtpResponseAsync(reader, ct);
                AppendLog($"  Server greeting: {greeting?.Trim()}", LogLevel.Info);

                // Send EHLO
                await writer.WriteLineAsync($"EHLO smtp-tester");
                string? ehloResponse = await ReadSmtpResponseAsync(reader, ct);
                AppendLog($"  EHLO response:", LogLevel.Info);
                foreach (var line in (ehloResponse ?? "").Split('\n', StringSplitOptions.RemoveEmptyEntries))
                    AppendLog($"    {line.Trim()}", LogLevel.Info);

                bool supportsStartTls = ehloResponse?.Contains("STARTTLS", StringComparison.OrdinalIgnoreCase) ?? false;

                if (!isImplicitTls)
                {
                    if (supportsStartTls)
                    {
                        AppendLog("  ✔  Server advertises STARTTLS.", LogLevel.Success);
                        // Upgrade
                        await writer.WriteLineAsync("STARTTLS");
                        string? startTlsResp = await ReadSmtpResponseAsync(reader, ct);
                        AppendLog($"  STARTTLS response: {startTlsResp?.Trim()}", LogLevel.Info);
                        if (startTlsResp?.StartsWith("220") ?? false)
                        {
                            ssl    = new SslStream(stream, false, CertificateValidationCallback);
                            stream = ssl;
                            await ssl.AuthenticateAsClientAsync(
                                new SslClientAuthenticationOptions
                                {
                                    TargetHost          = host,
                                    EnabledSslProtocols = SslProtocols.None,
                                    RemoteCertificateValidationCallback = CertificateValidationCallback
                                }, ct);
                            ReportTlsDetails(ssl);
                        }
                        else
                        {
                            AppendLog("  ⚠  STARTTLS upgrade was rejected by the server.", LogLevel.Warning);
                        }
                    }
                    else
                    {
                        AppendLog("  ⚠  Server does NOT advertise STARTTLS on this port.", LogLevel.Warning);
                        AppendLog("     Plain-text or implicit TLS (port 465) may be required.", LogLevel.Warning);
                    }
                }

                // Send QUIT
                try { await writer.WriteLineAsync("QUIT"); } catch { }
            }
            catch (AuthenticationException ax)
            {
                AppendLog($"  ✘ TLS handshake failed: {ax.Message}", LogLevel.Error);
                if (ax.InnerException != null)
                    AppendLog($"    Inner: {ax.InnerException.Message}", LogLevel.Error);
            }
            catch (Exception ex)
            {
                AppendLog($"  ⚠  TLS probe error: {ex.Message}", LogLevel.Warning);
            }
        }

        private void ReportTlsDetails(SslStream ssl)
        {
            AppendLog($"  ✔  TLS handshake successful.", LogLevel.Success);
            AppendLog($"  Protocol      : {ssl.SslProtocol}", LogLevel.Info);
            AppendLog($"  Cipher        : {ssl.CipherAlgorithm} ({ssl.CipherStrength} bit)", LogLevel.Info);
            AppendLog($"  Hash          : {ssl.HashAlgorithm} ({ssl.HashStrength} bit)", LogLevel.Info);
            AppendLog($"  Key exchange  : {ssl.KeyExchangeAlgorithm} ({ssl.KeyExchangeStrength} bit)", LogLevel.Info);

            var cert = ssl.RemoteCertificate;
            if (cert != null)
            {
                var x509 = new X509Certificate2(cert);
                AppendLog($"  Cert subject  : {x509.Subject}", LogLevel.Info);
                AppendLog($"  Cert issuer   : {x509.Issuer}", LogLevel.Info);
                AppendLog($"  Cert valid    : {x509.NotBefore:yyyy-MM-dd} → {x509.NotAfter:yyyy-MM-dd}", LogLevel.Info);
                bool expired = x509.NotAfter < DateTime.UtcNow;
                if (expired) AppendLog("  ⚠  Certificate is EXPIRED!", LogLevel.Error);
            }
        }

        private static bool CertificateValidationCallback(object sender, X509Certificate? cert, X509Chain? chain, SslPolicyErrors errors)
            => true; // accept all — this is a diagnostic tool

        // ──────────────────────────────────────────────────────────────
        //  SMTP send attempt
        // ──────────────────────────────────────────────────────────────
        private async Task<bool> TrySendAsync(SmtpConfig cfg, CancellationToken ct)
        {
            AppendLog($"  Attempting: host={cfg.Host} port={cfg.Port} ssl={cfg.EnableSsl}", LogLevel.Info);
            UpdateStatus($"Sending via {cfg.Host}:{cfg.Port} SSL={cfg.EnableSsl}…");

            try
            {
                using var client = new SmtpClient(cfg.Host, cfg.Port)
                {
                    EnableSsl            = cfg.EnableSsl,
                    DeliveryMethod       = SmtpDeliveryMethod.Network,
                    Timeout              = 15000,
                    UseDefaultCredentials = false
                };

                if (!string.IsNullOrWhiteSpace(cfg.Username))
                    client.Credentials = new NetworkCredential(cfg.Username, cfg.Password);

                using var msg = new MailMessage(cfg.From, cfg.To, cfg.Subject, cfg.Body);

                await client.SendMailAsync(msg, ct);
                AppendLog($"  ✔  SmtpClient.SendMailAsync completed without exception.", LogLevel.Success);
                return true;
            }
            catch (SmtpException ex)
            {
                AppendLog($"  ✘ SMTP error (status {ex.StatusCode}): {ex.Message}", LogLevel.Error);
                DiagnoseSmtpException(ex, cfg);
                return false;
            }
            catch (AuthenticationException ax)
            {
                AppendLog($"  ✘ TLS/Auth error: {ax.Message}", LogLevel.Error);
                if (ax.InnerException != null)
                    AppendLog($"    Inner: {ax.InnerException.Message}", LogLevel.Error);
                AppendLog("    → Try toggling SSL setting or use a different port.", LogLevel.Warning);
                return false;
            }
            catch (SocketException sx)
            {
                AppendLog($"  ✘ Socket error ({sx.SocketErrorCode}): {sx.Message}", LogLevel.Error);
                AppendLog("    → Port may be blocked or server unreachable.", LogLevel.Warning);
                return false;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                AppendLog($"  ✘ Unexpected error ({ex.GetType().Name}): {ex.Message}", LogLevel.Error);
                if (ex.InnerException != null)
                    AppendLog($"    Inner: {ex.InnerException.Message}", LogLevel.Error);
                return false;
            }
        }

        private void DiagnoseSmtpException(SmtpException ex, SmtpConfig cfg)
        {
            switch (ex.StatusCode)
            {
                case SmtpStatusCode.MustIssueStartTlsFirst:
                    AppendLog("    Diagnosis: Server requires STARTTLS before authentication.", LogLevel.Warning);
                    AppendLog("    → Enable SSL/TLS and use port 587.", LogLevel.Warning);
                    break;
                case SmtpStatusCode.ClientNotPermitted:
                    AppendLog("    Diagnosis: Client not permitted — relay denied or IP blocked.", LogLevel.Warning);
                    break;
                case SmtpStatusCode.MailboxBusy or SmtpStatusCode.MailboxUnavailable:
                    AppendLog("    Diagnosis: Recipient mailbox issue — check the To address.", LogLevel.Warning);
                    break;
                case SmtpStatusCode.GeneralFailure:
                    AppendLog("    Diagnosis: General failure — check credentials and server config.", LogLevel.Warning);
                    if (cfg.EnableSsl)
                        AppendLog("    → Server may not support SSL on this port; try disabling SSL.", LogLevel.Warning);
                    else
                        AppendLog("    → Server may require SSL; try enabling SSL.", LogLevel.Warning);
                    break;
                default:
                    AppendLog($"    Diagnosis: Status code {(int)ex.StatusCode} ({ex.StatusCode}).", LogLevel.Warning);
                    break;
            }

            if (ex.InnerException is AuthenticationException)
            {
                AppendLog("    TLS negotiation failed inside SmtpClient.", LogLevel.Error);
                AppendLog("    → The server likely doesn't support the requested TLS mode on this port.", LogLevel.Warning);
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  Helpers
        // ──────────────────────────────────────────────────────────────
        private static async Task<string?> ReadSmtpResponseAsync(StreamReader reader, CancellationToken ct)
        {
            var sb = new StringBuilder();
            string? line;
            while ((line = await reader.ReadLineAsync(ct)) != null)
            {
                sb.AppendLine(line);
                // Multi-line responses have a '-' after the code; single/last line has a space
                if (line.Length >= 4 && line[3] == ' ') break;
                if (line.Length < 4) break;
            }
            return sb.ToString();
        }

        private static int[] GetFallbackPorts(int failedPort) => failedPort switch
        {
            25  => [587, 465],
            587 => [465, 25],
            465 => [587, 25],
            _   => [587, 465, 25]
        };

        private SmtpConfig BuildConfig() => new(
            Host:      txtServer.Text.Trim(),
            Port:      int.Parse(txtPort.Text.Trim()),
            Username:  txtUsername.Text.Trim(),
            Password:  txtPassword.Text,
            EnableSsl: chkSSL.Checked,
            From:      txtFrom.Text.Trim(),
            To:        txtTo.Text.Trim(),
            Subject:   txtSubject.Text.Trim(),
            Body:      txtBody.Text
        );

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtServer.Text))  { ShowError("SMTP Server is required.");          return false; }
            if (!int.TryParse(txtPort.Text, out int p) || p is < 1 or > 65535)
                                                             { ShowError("Port must be a number 1–65535.");    return false; }
            if (string.IsNullOrWhiteSpace(txtFrom.Text))    { ShowError("From address is required.");         return false; }
            if (string.IsNullOrWhiteSpace(txtTo.Text))      { ShowError("To address is required.");           return false; }
            return true;
        }

        private void SetSendingState(bool sending)
        {
            btnSend.Tag       = sending ? "running" : null;
            btnSend.Text      = sending ? "■  Cancel" : "▶  Send Email";
            btnSend.BackColor = sending ? Color.FromArgb(160, 60, 60) : Color.FromArgb(0, 120, 215);
            progressBar.Visible = sending;
            btnClear.Enabled  = !sending;
            if (!sending) UpdateStatus("Done.");
        }

        private void UpdateStatus(string text)
        {
            if (lblStatus.InvokeRequired)
                lblStatus.Invoke(() => lblStatus.Text = text);
            else
                lblStatus.Text = text;
        }

        private enum LogLevel { Info, Header, Success, Warning, Error, Retry }

        private void AppendLog(string text, LogLevel level)
        {
            if (rtbLog.InvokeRequired) { rtbLog.Invoke(() => AppendLog(text, level)); return; }

            Color color = level switch
            {
                LogLevel.Header  => Color.Cyan,
                LogLevel.Success => Color.LimeGreen,
                LogLevel.Warning => Color.Yellow,
                LogLevel.Error   => Color.OrangeRed,
                LogLevel.Retry   => Color.Plum,
                _                => Color.LightGreen
            };

            int start = rtbLog.TextLength;
            rtbLog.AppendText(text + Environment.NewLine);
            rtbLog.Select(start, text.Length);
            rtbLog.SelectionColor = color;
            rtbLog.SelectionLength = 0;
            rtbLog.ScrollToCaret();
        }

        private static void ShowError(string msg) =>
            MessageBox.Show(msg, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        private void BtnClearLog_Click(object sender, EventArgs e) => rtbLog.Clear();

        private void BtnClear_Click(object sender, EventArgs e)
        {
            txtServer.Text  = string.Empty;
            txtPort.Text    = "587";
            txtUsername.Text = string.Empty;
            txtPassword.Text = string.Empty;
            txtFrom.Text    = string.Empty;
            txtTo.Text      = string.Empty;
            txtSubject.Text = string.Empty;
            txtBody.Text    = string.Empty;
            chkSSL.Checked  = true;
        }
    }

    // ── Config record ─────────────────────────────────────────────────
    internal record SmtpConfig(
        string Host,
        int    Port,
        string Username,
        string Password,
        bool   EnableSsl,
        string From,
        string To,
        string Subject,
        string Body);
}

