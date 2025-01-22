using System;
using System.Windows.Forms;

namespace ProcessManager;

static class Program
{
    [STAThread]
    static void Main()
    {
        try
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
} 