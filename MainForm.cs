using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Linq;
using System.IO;

namespace ProcessManager
{
    public partial class MainForm : Form
    {
        private System.Windows.Forms.Timer refreshTimer = new System.Windows.Forms.Timer();
        private Dictionary<string, int> killAttempts = new Dictionary<string, int>();
        private HashSet<string> blacklistedDescriptions = new HashSet<string>();
        private HashSet<string> blacklistPatterns = new HashSet<string>();
        private string searchText = string.Empty;
        private const string CONFIG_FILE = "blacklist.config";

        public MainForm()
        {
            InitializeComponent();
            LoadBlacklistConfig();
            SetupTimer();
            RefreshProcessList();
        }


        private void LoadBlacklistConfig()
        {
            try
            {
                blacklistPatterns.Clear();
                if (File.Exists(CONFIG_FILE))
                {
                    var patterns = File.ReadAllLines(CONFIG_FILE)
                        .Where(line => !string.IsNullOrWhiteSpace(line))
                        .Select(line => line.Trim());

                    foreach (var pattern in patterns)
                    {
                        blacklistPatterns.Add(pattern);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading blacklist configuration: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupTimer()
        {
            refreshTimer.Interval = 60000; // Refresh every minute
            refreshTimer.Tick += RefreshTimer_Tick;
            refreshTimer.Start();
        }        
        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            RefreshProcessList();
        }

        private string GetProcessDescription(Process process, bool withId = true)
        {
            try
            {
                if (!process.HasExited)
                {
                    try
                    {
                        var mainModule = process.MainModule;
                        if (mainModule != null)
                        {
                            var fileVersionInfo = mainModule.FileVersionInfo;
                            if (!string.IsNullOrWhiteSpace(fileVersionInfo.FileDescription))
                            {
                                return withId ? $"{fileVersionInfo.FileDescription}:{process.Id}" : fileVersionInfo.FileDescription;
                            }
                        }
                    }
                    catch (System.ComponentModel.Win32Exception)
                    {
                        // Access denied to module information
                    }
                }
            }
            catch (InvalidOperationException)
            {
                // Process has exited
            }
            catch (Exception)
            {
                // Any other unexpected error
            }
            return withId ? $"{process.ProcessName}:{process.Id}" : process.ProcessName;
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            searchText = txtSearch.Text.Trim().ToLower();
            RefreshProcessList();
        }        
        private bool IsProcessMatchingPattern(string processName, string pattern)
        {
            if (pattern.Contains("*"))
            {
                var regex = new System.Text.RegularExpressions.Regex(
                    "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
                        .Replace("\\*", ".*") + "$",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                return regex.IsMatch(processName);
            }
            return processName.Equals(pattern, StringComparison.OrdinalIgnoreCase);
        }

        private void RefreshProcessList()
        {
            lstProcesses.Items.Clear();
            
            Process[] processes = Process.GetProcesses();
            var processGroups = processes.GroupBy(p => GetProcessDescription(p, false));

            // Kill processes that match patterns or are manually blacklisted
            foreach (var group in processGroups)
            {
                bool isBlacklisted = blacklistedDescriptions.Contains(group.Key) ||
                                     blacklistPatterns.Any(pattern => IsProcessMatchingPattern(group.Key, pattern));

                if (isBlacklisted)
                {
                    TryKillProcessesByDescription(group.Key);
                }
            }

            // Update process list display
            processGroups = Process.GetProcesses().GroupBy(p => GetProcessDescription(p, false));
            foreach (var group in processGroups.OrderBy(g => g.Key))
            {
                bool isBlacklisted = blacklistedDescriptions.Contains(group.Key) ||
                                     blacklistPatterns.Any(pattern => IsProcessMatchingPattern(group.Key, pattern));

                if (!isBlacklisted && 
                    (string.IsNullOrEmpty(searchText) || 
                     group.Key.ToLower().Contains(searchText)))
                {
                    string description = group.Key;
                    if (killAttempts.ContainsKey(description))
                    {
                        description += $" ({killAttempts[description]})";
                    }
                    var item = lstProcesses.Items.Add(description);
                    item.Tag = group.Key;
                }
            }

            RefreshBlacklist();
        }

        private void RefreshBlacklist()
        {
            lstBlacklist.Items.Clear();
            Process[] currentProcesses = Process.GetProcesses();
            var processGroups = currentProcesses.GroupBy(p => GetProcessDescription(p, false));
            var runningDescriptions = processGroups.ToDictionary(g => g.Key, g => g.Count());

            // Add pattern-based items
            foreach (string pattern in blacklistPatterns)
            {
                var matchingProcesses = processGroups
                    .Where(g => IsProcessMatchingPattern(g.Key, pattern))
                    .Sum(g => g.Count());

                string displayText = $"[Pattern] {pattern}";
                if (matchingProcesses > 0)
                {
                    displayText += $" (Running: {matchingProcesses})";
                }
                var item = lstBlacklist.Items.Add(displayText);
                item.Tag = pattern;
            }

            // Add manually selected items
            foreach (string description in blacklistedDescriptions.ToList())
            {
                string displayText;
                if (runningDescriptions.ContainsKey(description))
                {
                    int count = runningDescriptions[description];
                    displayText = $"{description} (Running: {count})";
                }
                else
                {
                    displayText = $"{description} (Killed)";
                }
                var item = lstBlacklist.Items.Add(displayText);
                item.Tag = description;
            }
        }

        private void btnAddToBlacklist_Click(object sender, EventArgs e)
        {
            if (lstProcesses.SelectedItems.Count > 0)
            {
                string description = lstProcesses.SelectedItems[0].Tag as string;
                if (!blacklistedDescriptions.Contains(description))
                {
                    blacklistedDescriptions.Add(description);
                    TryKillProcessesByDescription(description);
                    RefreshProcessList();
                }
            }
        }

        private void TryKillProcessesByDescription(string targetDescription)
        {
            try
            {
                foreach (var process in Process.GetProcesses())
                {
                    try
                    {
                        string currentDescription = GetProcessDescription(process, false);
                        if (currentDescription == targetDescription)
                        {
                            try
                            {
                                process.Kill();
                            }
                            catch
                            {
                                if (!killAttempts.ContainsKey(targetDescription))
                                {
                                    killAttempts[targetDescription] = 1;
                                }
                                else
                                {
                                    killAttempts[targetDescription]++;
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Skip if we can't access process information
                    }
                }
            }
            catch
            {
                // Handle any unexpected errors
            }
        }
    }
}