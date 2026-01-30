using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TokensBegone
{
    public class MainForm : Form
    {
        private Label titleLabel;
        private Label summonerLabel;
        private Label statusLabel;
        private Button removeButton;
        private Button refreshButton;
        private Button legalButton;
        private Button githubButton;
        private Panel starPanel;
        private string? authToken;
        private int port;
        private HttpClient? httpClient;

        public MainForm()
        {
            InitializeComponent();
            ConnectToClient();
        }

        private void InitializeComponent()
        {
            this.Text = "TokensBegone";
            this.Size = new Size(300, 200);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);

            // Try to load icon
            try
            {
                string iconPath = Path.Combine(AppContext.BaseDirectory, "app_icon.ico");
                if (File.Exists(iconPath))
                    this.Icon = new Icon(iconPath);
            }
            catch { }

            // Title
            titleLabel = new Label
            {
                Text = "TokensBegone",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(85, 10)
            };

            // Summoner label
            summonerLabel = new Label
            {
                Text = "Connecting...",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.LightGray,
                AutoSize = true,
                Location = new Point(10, 45)
            };

            // Status label
            statusLabel = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.LightGray,
                AutoSize = true,
                Location = new Point(10, 65)
            };

            // Remove button
            removeButton = new Button
            {
                Text = "Remove Tokens",
                Size = new Size(180, 30),
                Location = new Point(10, 90),
                BackColor = Color.FromArgb(200, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            removeButton.Click += RemoveButton_Click;

            // Refresh button
            refreshButton = new Button
            {
                Text = "Refresh",
                Size = new Size(75, 30),
                Location = new Point(200, 90),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            refreshButton.Click += (s, e) => ConnectToClient();

            // Legal button
            legalButton = new Button
            {
                Text = "Legal",
                Size = new Size(80, 25),
                Location = new Point(10, 130),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            legalButton.Click += (s, e) => MessageBox.Show(
                "This tool uses Riot's official LCU API.\n\n" +
                "It only modifies challenge token display settings\n" +
                "which is a normal client feature.\n\n" +
                "No game files are modified.\n" +
                "Use at your own discretion.",
                "Legal Notice",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            // GitHub button
            githubButton = new Button
            {
                Text = "GitHub",
                Size = new Size(80, 25),
                Location = new Point(100, 130),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            githubButton.Click += (s, e) => Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/holyghostlol/TokensBegone",
                UseShellExecute = true
            });

            // Star panel (acts as button)
            starPanel = new Panel
            {
                Size = new Size(80, 25),
                Location = new Point(190, 130),
                BackColor = Color.FromArgb(60, 60, 60),
                Cursor = Cursors.Hand
            };
            var starLabel = new Label
            {
                Text = "Star",
                ForeColor = Color.Gold,
                Font = new Font("Segoe UI", 9),
                AutoSize = false,
                Size = new Size(80, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };
            starPanel.Controls.Add(starLabel);
            starPanel.Click += Star_Click;
            starLabel.Click += Star_Click;

            // Credit
            var creditLabel = new Label
            {
                Text = "Made with love by holyghostlol",
                Font = new Font("Segoe UI", 7),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(75, 160)
            };

            this.Controls.AddRange(new Control[] {
                titleLabel, summonerLabel, statusLabel,
                removeButton, refreshButton, legalButton, githubButton, starPanel, creditLabel
            });
        }

        private void Star_Click(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "Thank you!\n\nPlease star the repo on GitHub to support the project.",
                "Support",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/holyghostlol/TokensBegone",
                UseShellExecute = true
            });
        }

        private void ConnectToClient()
        {
            try
            {
                // Get LCU credentials from process
                var process = Process.GetProcessesByName("LeagueClientUx").FirstOrDefault();
                if (process == null)
                {
                    summonerLabel.Text = "League Client not found!";
                    statusLabel.Text = "Please open League of Legends";
                    return;
                }

                // Get command line using WMI
                string? cmdLine = GetCommandLine(process.Id);
                if (cmdLine == null)
                {
                    summonerLabel.Text = "Could not get client info";
                    return;
                }

                // Parse port and auth token
                var portMatch = Regex.Match(cmdLine, @"--app-port=(\d+)");
                var authMatch = Regex.Match(cmdLine, @"--remoting-auth-token=([\w-]+)");

                if (!portMatch.Success || !authMatch.Success)
                {
                    summonerLabel.Text = "Could not parse client credentials";
                    return;
                }

                port = int.Parse(portMatch.Groups[1].Value);
                authToken = authMatch.Groups[1].Value;

                // Create HTTP client with auth
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true
                };
                httpClient = new HttpClient(handler);
                
                var authBytes = Encoding.ASCII.GetBytes($"riot:{authToken}");
                httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));

                // Get summoner info
                GetSummonerInfo();
            }
            catch (Exception ex)
            {
                summonerLabel.Text = $"Error: {ex.Message}";
            }
        }

        private string? GetCommandLine(int processId)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = $"-Command \"(Get-CimInstance Win32_Process -Filter 'ProcessId={processId}').CommandLine\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                
                using var proc = Process.Start(startInfo);
                if (proc == null) return null;
                
                string output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();
                return output;
            }
            catch
            {
                return null;
            }
        }

        private async void GetSummonerInfo()
        {
            try
            {
                var response = await httpClient!.GetAsync($"https://127.0.0.1:{port}/lol-summoner/v1/current-summoner");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    string name;
                    if (root.TryGetProperty("gameName", out var gameName) && 
                        root.TryGetProperty("tagLine", out var tagLine))
                    {
                        name = $"{gameName.GetString()}#{tagLine.GetString()}";
                    }
                    else
                    {
                        name = root.GetProperty("displayName").GetString() ?? "Unknown";
                    }

                    summonerLabel.Text = $"Logged in as: {name}";
                    statusLabel.Text = "Ready to remove tokens";
                    removeButton.Enabled = true;
                }
                else
                {
                    summonerLabel.Text = "Could not get summoner info";
                }
            }
            catch (Exception ex)
            {
                summonerLabel.Text = $"Error: {ex.Message}";
            }
        }

        private async void RemoveButton_Click(object? sender, EventArgs e)
        {
            try
            {
                removeButton.Enabled = false;
                statusLabel.Text = "Removing tokens...";

                var content = new StringContent(
                    "{\"challengeIds\":[]}",
                    Encoding.UTF8,
                    "application/json");

                var response = await httpClient!.PostAsync(
                    $"https://127.0.0.1:{port}/lol-challenges/v1/update-player-preferences",
                    content);

                if (response.IsSuccessStatusCode)
                {
                    statusLabel.Text = "Tokens removed successfully!";
                    statusLabel.ForeColor = Color.LightGreen;
                }
                else
                {
                    statusLabel.Text = $"Failed: {response.StatusCode}";
                    statusLabel.ForeColor = Color.Red;
                    removeButton.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Error: {ex.Message}";
                statusLabel.ForeColor = Color.Red;
                removeButton.Enabled = true;
            }
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
